using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using CoreAnimation;
using CoreGraphics;
using UIKit;

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
        readonly UIScreenEdgePanGestureRecognizer _edgeGestureRecognizer;
        readonly List<CAShapeLayer> _masksToAnimate;
        IUIViewControllerContextTransitioning _transitionContext; 
        UIViewController _fromViewController;
        UIViewController _toViewController;
        double _transitionDuration;

        public NavigationTransition(List<(UIView ToView, UIView FromView)> viewsToAnimate, UINavigationControllerOperation operation, SharedTransitionNavigationRenderer navigationPage, UIScreenEdgePanGestureRecognizer edgeGestureRecognizer)
        {
            _navigationPage        = navigationPage;
            _operation             = operation;
            _edgeGestureRecognizer = edgeGestureRecognizer;
            _masksToAnimate        = new List<CAShapeLayer>();

            //Auto z-index, to avoid mess when animating multiple views, layouts ecc.
            //TODO: Create a property for custom z-index
            _viewsToAnimate = viewsToAnimate.OrderBy(x => x.FromView is UIControl || x.FromView is UILabel || x.FromView is UIImageView).ToList();
        }

        /// <summary>
        /// Setup the animation transitions between elements (also animate the background)
        /// </summary>
        /// <param name="transitionContext">The transition context</param>
        public override async void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
        {
            _transitionContext  = transitionContext;
            _fromViewController = _transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);
            _toViewController   = _transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);
            _transitionDuration = TransitionDuration(transitionContext);

            var containerView   = _transitionContext.ContainerView;

            // This needs to be added to the view hierarchy for the destination frame to be correct,
            // but we don't want it visible yet.
            containerView.InsertSubview(_toViewController.View, 0);
            _toViewController.View.Alpha = 0;

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
                else
                {
                    /*
                     * IMPORTANT
                     *
                     * DAMNIT! this little things made me lost a LOT of time. Me n00b.
                     * So.. dont use fromView.Frame here or layout will go crazy!
                     */
                    fromViewFrame    = fromView.Bounds;
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
                    CABasicAnimation maskLayerAnimation = CreateMaskTransition(fromMask, toMask);
                    fromMask.AddAnimation(maskLayerAnimation, "path");

                    //Handling the mask transition with the Interactive gesture for pop
                    //Warning: We need to watch for began and changed because sometimes began is not fired
                    if (_edgeGestureRecognizer.State == UIGestureRecognizerState.Began ||
                        _edgeGestureRecognizer.State == UIGestureRecognizerState.Changed)
                    {
                        fromMask.Speed = 0;
                        _masksToAnimate.Add(fromMask);
                    }
                }

                UIView.Animate(_transitionDuration,0, UIViewAnimationOptions.CurveEaseInOut|UIViewAnimationOptions.AllowUserInteraction, () =>
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
                _fromViewController.View.Alpha = 0;

            if (_masksToAnimate.Any())
                _navigationPage.EdgeGesturePanned += NavigationPageOnEdgeGesturePanned;

            AnimateBackground();
        }

        /// <summary>
        /// Handle the main EdgePanGesture on the NavigationPage used to swipe back
        /// </summary>
        private void NavigationPageOnEdgeGesturePanned(object sender, EdgeGesturePannedArgs args)
        {
            //Control the animation on the upper mask layer
            double offset = _transitionDuration * args.Percent;
            foreach (CAShapeLayer fromMask in _masksToAnimate)
            {
                switch (args.State)
                {
                    case UIGestureRecognizerState.Changed:
                        fromMask.TimeOffset = offset;
                        break;

                    case UIGestureRecognizerState.Ended:
                        fromMask.Speed = args.FinishTransitionOnEnd ? 1 : -1;
                        //this do the magic... forget this and it's a mess :)
                        fromMask.BeginTime = CAAnimation.CurrentMediaTime();
                        break;
                }   
            }

            if (args.State == UIGestureRecognizerState.Ended ||
                args.State == UIGestureRecognizerState.Cancelled ||
                args.State == UIGestureRecognizerState.Failed)
            {
                _navigationPage.EdgeGesturePanned -= NavigationPageOnEdgeGesturePanned;
            }
        }

        /// <summary>
        /// Create an animate transition between two masks
        /// </summary>
        /// <param name="fromMask">Starting mask</param>
        /// <param name="toMask">Ending mask</param>
        CABasicAnimation CreateMaskTransition(CAShapeLayer fromMask, CAShapeLayer toMask)
        {
            var maskLayerAnimation = CABasicAnimation.FromKeyPath("path");
            maskLayerAnimation.SetFrom(fromMask.Path);
            maskLayerAnimation.SetTo(toMask.Path);
            maskLayerAnimation.Duration = _transitionDuration;
            maskLayerAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);

            //Avoid "flashing" at the end of the animation
            maskLayerAnimation.FillMode = CAFillMode.Forwards;
            maskLayerAnimation.RemovedOnCompletion = false;

            return maskLayerAnimation;
        }

        /// <summary>
        /// Animate the background based on user choices
        /// </summary>
        void AnimateBackground()
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;
            var backgroundAnimation = _navigationPage.BackgroundAnimation;

            //fix animation for push & pop
            //TODO rework this, I dont have time now :P
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
                        ? _transitionDuration
                        : 0;

                    UIView.Animate(0, delay, UIViewAnimationOptions.TransitionNone, () =>
                    {
                        _toViewController.View.Alpha = 1;
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.Fade:
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        _toViewController.View.Alpha = 1;
                        _fromViewController.View.Alpha = 0;
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.Flip:
                    var m34 = (nfloat)(-1 * 0.001);
                    var initialTransform = CATransform3D.Identity;
                    initialTransform.m34 = m34;
                    initialTransform = initialTransform.Rotate((nfloat)(1 * Math.PI * 0.5), 0.0f, 1.0f, 0.0f);

                    _fromViewController.View.Alpha = 0;
                    _toViewController.View.Alpha = 0;
                    _toViewController.View.Layer.Transform = initialTransform;
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveEaseInOut,
                        () =>
                        {
                            _toViewController.View.Layer.AnchorPoint = new CGPoint((nfloat)0.5, 0.5f);
                            var newTransform = CATransform3D.Identity;
                            newTransform.m34 = m34;
                            _toViewController.View.Layer.Transform = newTransform;
                            _toViewController.View.Alpha = 1;
                        }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.SlideFromBottom:
                    _toViewController.View.Alpha = 1;
                    _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X, _toViewController.View.Center.Y + screenWidth);
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _fromViewController.View.Center = new CGPoint(_fromViewController.View.Center.X, _fromViewController.View.Center.Y - screenWidth);
                        _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X, _toViewController.View.Center.Y - screenWidth);
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.SlideFromLeft:
                    _toViewController.View.Alpha = 1;
                    _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X - screenWidth, _toViewController.View.Center.Y);
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _fromViewController.View.Center = new CGPoint(_fromViewController.View.Center.X + screenWidth, _fromViewController.View.Center.Y);
                        _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X + screenWidth, _toViewController.View.Center.Y);
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.SlideFromRight:
                    _toViewController.View.Alpha = 1;
                    _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X + screenWidth, _toViewController.View.Center.Y);
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _fromViewController.View.Center = new CGPoint(_fromViewController.View.Center.X - screenWidth, _fromViewController.View.Center.Y);
                        _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X - screenWidth, _toViewController.View.Center.Y);
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;

                case BackgroundAnimation.SlideFromTop:
                    _toViewController.View.Alpha = 1;
                    _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X, _toViewController.View.Center.Y - screenWidth);
                    UIView.Animate(_transitionDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _fromViewController.View.Center = new CGPoint(_fromViewController.View.Center.X, _fromViewController.View.Center.Y + screenWidth);
                        _toViewController.View.Center = new CGPoint(_toViewController.View.Center.X, _toViewController.View.Center.Y + screenWidth);
                    }, FixCompletionForSwipeAndPopToRoot);
                    break;
            }
        }

        /// <summary>
        /// Fix the animation completion for swipe back and pop to root
        /// </summary>
        void FixCompletionForSwipeAndPopToRoot()
        {
            // Fix 1 for swipe + pop to root
            _fromViewController.View.Alpha = 1;
            _transitionContext.CompleteTransition(!_transitionContext.TransitionWasCancelled);

            // Fix 2 for swipe + pop to root
            if (_transitionContext.TransitionWasCancelled)
                _fromViewController.View.Alpha = 1;
        }

        /// <summary>
        /// The duration of the transition
        /// </summary>
        /// <param name="transitionContext">The transition context.</param>
        public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
        {
            return _navigationPage.TransitionDuration;
        }
    }
}