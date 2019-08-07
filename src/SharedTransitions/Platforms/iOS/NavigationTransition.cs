using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
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
        readonly SharedTransitionNavigationRenderer _navigationPage;
        readonly List<(UIView ToView, UIView FromView)> _viewsToAnimate;
        readonly UINavigationControllerOperation _operation;

        public NavigationTransition(List<(UIView ToView, UIView FromView)> viewsToAnimate, UINavigationControllerOperation operation, SharedTransitionNavigationRenderer navigationPage)
        {
            _navigationPage = navigationPage;
            _viewsToAnimate = viewsToAnimate;
            _operation = operation;
        }

        /// <summary>
        /// Animates the transition.
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        public override async void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
        {
            var containerView = transitionContext.ContainerView;
            var fromViewController = transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);
            var toViewController = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);

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
                    break;

                UIView fromViewSnapshot;
                CGRect fromViewFrame = fromView.Frame;

                if (fromView is UIControl || fromView is UILabel )
                {
                    //For buttons and labels just copy the view to preserve a good transition
                    //Using normal snapshot with labels and buttons may cause streched and deformed images
                    fromViewSnapshot = (UIView)NSKeyedUnarchiver.UnarchiveObject(NSKeyedArchiver.ArchivedDataWithRootObject(fromView));
                }
                else if (fromView is UIImageView fromImageView)
                {
                    //Get the snapshot based on the real image size, not his containing frame!
                    //This is needed to avoid deformations with image aspectfit
                    //where the container frame can a have different size from the contained image
                    fromViewFrame = fromImageView.GetImageFrame();
                    fromViewSnapshot = fromView.ResizableSnapshotView(fromViewFrame, false, UIEdgeInsets.Zero);
                }
                else if (fromView is VisualElementRenderer<BoxView>)
                {
                    fromViewSnapshot = fromView.SnapshotView(false);
                }
                else
                {
                    fromViewSnapshot = new UIView
                    {
                        AutoresizingMask = UIViewAutoresizing.All,
                        ContentMode      = UIViewContentMode.ScaleToFill,
                        Alpha            = fromView.Alpha,
                        BackgroundColor  = fromView.BackgroundColor
                    };

                    fromViewSnapshot.Layer.CornerRadius    = fromView.Layer.CornerRadius;
                    fromViewSnapshot.Layer.MasksToBounds   = fromView.Layer.MasksToBounds;
                    fromViewSnapshot.Layer.BorderWidth     = fromView.Layer.BorderWidth ;
                    fromViewSnapshot.Layer.BorderColor     = fromView.Layer.BorderColor;
                    fromViewSnapshot.Layer.BackgroundColor = fromView.Layer.BackgroundColor ?? fromView.BackgroundColor.CGColor;
                }

                //minor perf gain
                //fromViewSnapshot.Opaque = true;
                containerView.AddSubview(fromViewSnapshot);
                fromViewSnapshot.Frame = fromView.ConvertRectToView(fromViewFrame, containerView);
                
                // Without this, the snapshots will include the following "recent" changes
                // Needed only on push. So pop can use the interaction (pangesture)
                if (_operation == UINavigationControllerOperation.Push)
                    await Task.Yield();

                toView.Alpha = 0;
                fromView.Alpha = 0;

                //If UIIMage, preserve aspect ratio on destination
                CGRect toFrame;
                if (toView is UIImageView toImageView)
                {
                    toFrame = toImageView.ConvertRectToView(toImageView.GetImageFrame(), containerView);
                }
                else
                {
                    toFrame = toView.ConvertRectToView(toView.Frame, containerView);
                }

                UIView.Animate(TransitionDuration(transitionContext),0, UIViewAnimationOptions.CurveEaseInOut, () =>
                {
                    fromViewSnapshot.Frame = toFrame;
                    fromViewSnapshot.Layer.CornerRadius = toView.Layer.CornerRadius;
                    fromViewSnapshot.BackgroundColor = toView.BackgroundColor;
                    fromViewSnapshot.Layer.BackgroundColor = toView.Layer.BackgroundColor;
                    fromViewSnapshot.Alpha = 1;
                }, () =>
                {
                    toView.Alpha = 1;
                    fromView.Alpha = 1;
                    fromViewSnapshot.RemoveFromSuperview();
                });
            }

            //containerView.InsertSubview(toViewController.View, 1);

            var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;

            fromViewController.View.Alpha = 0;

            switch (_navigationPage.BackgroundAnimation)
            {
                case BackgroundAnimation.None:
                    UIView.Animate(0, 0, UIViewAnimationOptions.TransitionNone, () =>
                    {
                        toViewController.View.Alpha = 1;
                    }, () => { transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled); });
                    break;

                case BackgroundAnimation.Fade:
                    UIView.Animate(TransitionDuration(transitionContext), 0, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        toViewController.View.Alpha = 1;
                        fromViewController.View.Alpha = 0;
                    }, () =>
                    {
                        // Fix 1 for swipe + pop to root
                        fromViewController.View.Alpha = 1;
                        transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                        // Fix 2 for swipe + pop to root
                        if (transitionContext.TransitionWasCancelled)
                            fromViewController.View.Alpha = 1;
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
                            // Fix 1 for swipe + pop to root
                            fromViewController.View.Alpha = 1;
                            transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                            // Fix 2 for swipe + pop to root
                            if (transitionContext.TransitionWasCancelled)
                                fromViewController.View.Alpha = 1;
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
                        // Fix 1 for swipe + pop to root
                        fromViewController.View.Alpha = 1;
                        transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                        // Fix 2 for swipe + pop to root
                        if (transitionContext.TransitionWasCancelled)
                            fromViewController.View.Alpha = 1;
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
                        // Fix 1 for swipe + pop to root
                        fromViewController.View.Alpha = 1;
                        transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                        // Fix 2 for swipe + pop to root
                        if (transitionContext.TransitionWasCancelled)
                            fromViewController.View.Alpha = 1;
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
                        // Fix 1 for swipe + pop to root
                        fromViewController.View.Alpha = 1;
                        transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                        // Fix 2 for swipe + pop to root
                        if (transitionContext.TransitionWasCancelled)
                            fromViewController.View.Alpha = 1;
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
                        // Fix 1 for swipe + pop to root
                        fromViewController.View.Alpha = 1;
                        transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);

                        // Fix 2 for swipe + pop to root
                        if (transitionContext.TransitionWasCancelled)
                            fromViewController.View.Alpha = 1;
                    });
                    break;
            }
        }

        /// <summary>
        /// The duration of the transition
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        /// <returns></returns>
        public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
        {
            return _navigationPage.SharedTransitionDuration;
        }
    }
}