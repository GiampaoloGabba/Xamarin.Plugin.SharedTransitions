using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    /// <summary>
    /// Set the navigation transition for the NavigationPage
    /// </summary>
    internal class NavigationTransition : UIViewControllerAnimatedTransitioning
    {
        /*
         * IMPORTANT NOTES:
         * Read the dedicate comments in code for more info about those fixes.
         *
         * Frame management for layout and boxview!
         * Use the to bounds, not the frame! 
         *
         * Task management in pop before make some changes to the snapshot:
         * Necessary to get the transition to work properly
         *
         * Get the true image bounds with aspectwidth:
         * In order to avoid deformations during transitions 
         * when the frame has different size than the contained image
         */

        readonly SharedTransitionNavigationRenderer _navigationPage;
        readonly List<(UIView ToView, UIView FromView)> _viewsToAnimate;
        readonly UINavigationControllerOperation _operation;

        public NavigationTransition(List<(UIView ToView, UIView FromView)> viewsToAnimate, UINavigationControllerOperation operation, SharedTransitionNavigationRenderer navigationPage)
        {
            _navigationPage = navigationPage;
            //Auto z-index, to avoid mess when animating multiple views, layouts ecc.
            _viewsToAnimate = viewsToAnimate.OrderBy(x => x.FromView is UIControl || x.FromView is UILabel || x.FromView is UIImageView).ToList();
            _operation = operation;
        }

        /// <summary>
        /// Animates the transition.
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        public override async void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
        {
            var containerView      = transitionContext.ContainerView;
            var fromViewController = transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);
            var toViewController   = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);

            // This needs to be added to the view hierarchy for the destination frame to be correct,
            // but we don't want it visible yet.
            containerView.InsertSubview(toViewController.View, 0);
            toViewController.View.Alpha = 0;

            //iterate the destination views, this has two benefits:
            //1) We are sure to dont start transitions with views only in the start controller
            //2) With dynamic transitions (listview) we dont need to iterate al the tags we dont need
            foreach (var viewToAnimate in _viewsToAnimate)
            {
                var toView   = viewToAnimate.ToView;
                var fromView = viewToAnimate.FromView;

                if (toView == null || fromView == null)
                {
                    Debug.WriteLine($"At this point we must have the 2 views to animate! One or both is missing");
                    break;
                }
                
                UIView fromViewSnapshot;
                CGRect fromViewFrame;

                if (fromView is UIControl || fromView is UILabel )
                {
                    //For buttons and labels just copy the view to preserve a good transition
                    //Using normal snapshot with labels and buttons may cause streched and deformed images
                    fromViewFrame    = fromView.Frame;
                    fromViewSnapshot = fromView.CopyView();
                }
                else if (fromView is UIImageView fromImageView)
                {
                    if (fromImageView.ContentMode == UIViewContentMode.ScaleAspectFit)
                    {
                        //Take a simple snapshot, saving resources, only of the visible frame, 
                        fromViewFrame    = fromImageView.GetImageFrame();
                        fromViewSnapshot = fromView.ResizableSnapshotView(fromViewFrame, false, UIEdgeInsets.Zero);
                    }
                    else
                    {
                        //The only way to do good transitions with *Fit aspect is just to copy the view and animate the frame/image
                        fromViewFrame    = fromView.Frame;
                        fromViewSnapshot = fromView.CopyView();
                    }
                }
                else if (fromView is VisualElementRenderer<BoxView>)
                {
                    /*
                     * IMPORTANT
                     *
                     * DAMNIT! this little things made me lost a LOT of time. Me n00b.
                     * So.. dont use fromView.Frame here or layout will go crazy!
                     */
                    fromViewFrame    = fromView.Bounds;
                    fromViewSnapshot = fromView.SnapshotView(false);
                }
                else
                {
                    /*
                     * IMPORTANT
                     *
                     * DAMNIT! this little things made me lost a LOT of time. Me n00b.
                     * So.. dont use fromView.Frame here or layout will go crazy!
                     */
                    fromViewFrame = fromView.Bounds;
                    fromViewSnapshot = fromView.CopyView(softCopy: true);
                }

                containerView.AddSubview(fromViewSnapshot);
                fromViewSnapshot.Frame = fromView.ConvertRectToView(fromViewFrame, containerView);

                /*
                 * IMPORTANT
                 * We need to Yield the task in order to exclude the following changes
                 * before the transition starts. Needed only on push, dont try this in pop
                 * or the custom edge gesture will not work!
                 */

                if (_operation == UINavigationControllerOperation.Push)
                {
                    //without this flickering could occour
                    fromViewSnapshot.Hidden = true;
                    await Task.Yield();
                }

                fromViewSnapshot.Hidden = false;
                toView.Hidden           = true;
                fromView.Hidden         = true;

                CGRect toFrame;
                if (toView is UIImageView toImageView)
                    toFrame = toImageView.ConvertRectToView(toImageView.GetImageFrame(), containerView);
                else
                    toFrame = toView.ConvertRectToView(toView.Bounds, containerView);

                //Mask animation (for shape/corner radius)
                var toMask = toView.Layer.GetMask(toView.Bounds);
                if (toMask != null && fromViewSnapshot.Layer.Mask is CAShapeLayer fromMask)
                {
                    var maskLayerAnimation = CreateMaskTransition(transitionContext, fromMask, toMask);
                    fromViewSnapshot.Layer.Mask.AddAnimation(maskLayerAnimation, "path");
                }

                UIView.Animate(TransitionDuration(transitionContext),0, UIViewAnimationOptions.CurveEaseInOut, () =>
                {
                    //set the main properties to animate
                    fromViewSnapshot.Frame = toFrame;
                    fromViewSnapshot.Alpha = 1;
                    fromViewSnapshot.Layer.CornerRadius = toView.Layer.CornerRadius;

                }, () =>
                {
                    toView.Hidden   = false;
                    fromView.Hidden = false;
                    fromViewSnapshot.RemoveFromSuperview();
                });
            }

            //Avoid flickering during push and display a right pop
            if (_operation == UINavigationControllerOperation.Pop)
                fromViewController.View.Alpha = 0;

            FinalizeTransition(transitionContext, fromViewController, toViewController);
        }

        CABasicAnimation CreateMaskTransition(IUIViewControllerContextTransitioning transitionContext, CAShapeLayer fromMask, CAShapeLayer toMask)
        {
            var maskLayerAnimation = CABasicAnimation.FromKeyPath("path");
            maskLayerAnimation.SetFrom(fromMask.Path);
            maskLayerAnimation.SetTo(toMask.Path);
            maskLayerAnimation.Duration       = TransitionDuration(transitionContext);
            maskLayerAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
            return maskLayerAnimation;
        }

        void FinalizeTransition(IUIViewControllerContextTransitioning transitionContext, UIViewController fromViewController, UIViewController toViewController)
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;
            var backgroundAnimation = _navigationPage.BackgroundAnimation;

            //fix animation for push & pop
            //TODO rework this better i have no time now :P
            if (_operation == UINavigationControllerOperation.Pop)
            {
                if (backgroundAnimation == BackgroundAnimation.SlideFromBottom)
                {
                    backgroundAnimation = BackgroundAnimation.SlideFromTop;
                } 
                else if (backgroundAnimation == BackgroundAnimation.SlideFromTop)
                {
                    backgroundAnimation = BackgroundAnimation.SlideFromBottom;
                } 
                else if (backgroundAnimation == BackgroundAnimation.SlideFromRight)
                {
                    backgroundAnimation = BackgroundAnimation.SlideFromLeft;
                } 
                else if (backgroundAnimation == BackgroundAnimation.SlideFromLeft)
                {
                    backgroundAnimation = BackgroundAnimation.SlideFromRight;
                }
            }

            switch (backgroundAnimation)
            {
                case BackgroundAnimation.None:

                    var delay = _operation == UINavigationControllerOperation.Push
                        ? TransitionDuration(transitionContext)
                        : 0;

                    UIView.Animate(0, delay, UIViewAnimationOptions.TransitionNone, () =>
                    {
                        toViewController.View.Alpha = 1;
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;

                case BackgroundAnimation.Fade:
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        toViewController.View.Alpha = 1;
                        fromViewController.View.Alpha = 0;
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;

                case BackgroundAnimation.Flip:
                    var m34 = (nfloat)(-1 * 0.001);
                    var initialTransform = CATransform3D.Identity;
                    initialTransform.m34 = m34;
                    initialTransform = initialTransform.Rotate((nfloat)(1 * Math.PI * 0.5), 0.0f, 1.0f, 0.0f);

                    fromViewController.View.Alpha = 0;
                    toViewController.View.Alpha = 0;
                    toViewController.View.Layer.Transform = initialTransform;
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseInOut,
                        () =>
                        {
                            toViewController.View.Layer.AnchorPoint = new CGPoint((nfloat)0.5, 0.5f);
                            var newTransform = CATransform3D.Identity;
                            newTransform.m34 = m34;
                            toViewController.View.Layer.Transform = newTransform;
                            toViewController.View.Alpha = 1;
                        }, () =>
                        {
                            FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                        });
                    break;

                case BackgroundAnimation.SlideFromBottom:
                    toViewController.View.Alpha = 1;
                    toViewController.View.Center = new CGPoint(toViewController.View.Center.X, toViewController.View.Center.Y + screenWidth);
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        fromViewController.View.Center = new CGPoint(fromViewController.View.Center.X, fromViewController.View.Center.Y - screenWidth);
                        toViewController.View.Center = new CGPoint(toViewController.View.Center.X, toViewController.View.Center.Y - screenWidth);
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;

                case BackgroundAnimation.SlideFromLeft:
                    toViewController.View.Alpha = 1;
                    toViewController.View.Center = new CGPoint(toViewController.View.Center.X - screenWidth, toViewController.View.Center.Y);
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        fromViewController.View.Center = new CGPoint(fromViewController.View.Center.X + screenWidth, fromViewController.View.Center.Y);
                        toViewController.View.Center = new CGPoint(toViewController.View.Center.X + screenWidth, toViewController.View.Center.Y);
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;

                case BackgroundAnimation.SlideFromRight:
                    toViewController.View.Alpha = 1;
                    toViewController.View.Center = new CGPoint(toViewController.View.Center.X + screenWidth, toViewController.View.Center.Y);
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        fromViewController.View.Center = new CGPoint(fromViewController.View.Center.X - screenWidth, fromViewController.View.Center.Y);
                        toViewController.View.Center = new CGPoint(toViewController.View.Center.X - screenWidth, toViewController.View.Center.Y);
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;

                case BackgroundAnimation.SlideFromTop:
                    toViewController.View.Alpha = 1;
                    toViewController.View.Center = new CGPoint(toViewController.View.Center.X, toViewController.View.Center.Y - screenWidth);
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        fromViewController.View.Center = new CGPoint(fromViewController.View.Center.X, fromViewController.View.Center.Y + screenWidth);
                        toViewController.View.Center = new CGPoint(toViewController.View.Center.X, toViewController.View.Center.Y + screenWidth);
                    }, () =>
                    {
                        FixCompletionForSwipeAndPopToRoot(transitionContext, fromViewController);
                    });
                    break;
            }
        }

        void FixCompletionForSwipeAndPopToRoot(IUIViewControllerContextTransitioning transitionContext, UIViewController fromViewController)
        {
            // Fix 1 for swipe + pop to root
            fromViewController.View.Alpha = 1;
            transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

            // Fix 2 for swipe + pop to root
            if (transitionContext.TransitionWasCancelled)
                fromViewController.View.Alpha = 1;
        }

        /// <summary>
        /// The duration of the transition
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        /// <returns></returns>
        public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
        {
            return _navigationPage.TransitionDuration;
        }
    }
}