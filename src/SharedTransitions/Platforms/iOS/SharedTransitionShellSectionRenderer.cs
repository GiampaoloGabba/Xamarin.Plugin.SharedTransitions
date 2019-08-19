using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public class SharedTransitionShellSectionRenderer : ShellSectionRenderer, ITransitionRenderer, IUINavigationControllerDelegate, IUIGestureRecognizerDelegate
	{
        public event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;
        public double TransitionDuration { get; set; }
        public BackgroundAnimation BackgroundAnimation { get; set; }

        /// <summary>
        /// Track the page we need to get the custom properties for the shared transitions
        /// </summary>
        Page _propertiesContainer;
        Page PropertiesContainer
        {
            get => _propertiesContainer;
            set
            {
                if (_propertiesContainer == value)
                    return;

                //container has a different value from the one we are passing.
                //We need to unsubscribe event, set the new value, then resubscribe for the new container
                if (_propertiesContainer != null)
                    _propertiesContainer.PropertyChanged -= HandleChildPropertyChanged;

                _propertiesContainer = value;

                if (_propertiesContainer != null)
                    _propertiesContainer.PropertyChanged += HandleChildPropertyChanged;

                UpdateBackgroundTransition();
                UpdateTransitionDuration();
                UpdateSelectedGroup();
            }
        }

        UIScreenEdgePanGestureRecognizer _edgeGestureRecognizer;
        UIPercentDrivenInteractiveTransition _percentDrivenInteractiveTransition;
        bool _popToRoot;
        string _selectedGroup;
        readonly IShellContext _context;
        private SharedTransitionShell NavPage;

        public SharedTransitionShellSectionRenderer(IShellContext context) : base(context)
		{
			_context = context;
			NavPage = (SharedTransitionShell)_context.Shell;
			Delegate = this;
		}

		[Export("navigationController:animationControllerForOperation:fromViewController:toViewController:")]
        public IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation(UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
        {
            if (!_popToRoot)
            {
                //At this point the property TargetPage refers to the view we are pushing or popping
                //This view is not yet visible in our app but the variable is already set
                var viewsToAnimate = new List<(UIView ToView, UIView FromView)>();

                IReadOnlyList<TransitionDetail> transitionStackTo;
                IReadOnlyList<TransitionDetail> transitionStackFrom;

                if (operation == UINavigationControllerOperation.Push)
                {
                    transitionStackFrom = NavPage.TransitionMap.GetMap(PropertiesContainer, _selectedGroup);
                    transitionStackTo   = NavPage.TransitionMap.GetMap(ShellSection.Stack.Last(), null);
                }
                else
                {
                    //During POP, everyting is fine and clear
                    transitionStackFrom = NavPage.TransitionMap.GetMap(ShellSection.Stack.Last(), null);
                    transitionStackTo   = NavPage.TransitionMap.GetMap(PropertiesContainer, _selectedGroup);
                }

                if (transitionStackFrom != null)
                {
                    //Get all the views with transitions in the destination page
                    //With this, we are sure to dont start transitions with no mathing transitions in destination
                    foreach (var transitionToMap in transitionStackTo)
                    {
                        var toView = toViewController.View.ViewWithTag(transitionToMap.NativeViewId);
                        if (toView != null)
                        {
                            //get the matching transition: we store the destination view and the corrispondent transition in the source view,
                            //so we can match them during transition.

                            /*
                             * IMPORTANT
                             *
                             * Using ListView/Collection, the first item is created two times, but then one of them is discarded
                             * without calling the Detach method from our effect. So we need to find the right element!
                             */
                            
                            foreach (var nativeView in transitionStackFrom.Where(x => x.TransitionName == transitionToMap.TransitionName).OrderByDescending(x=>x.NativeViewId))
                            {
                                var fromView = fromViewController.View.ViewWithTag(nativeView.NativeViewId);
                                if (fromView != null)
                                {
                                    viewsToAnimate.Add((toView, fromView));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            /*
                             * IMPORTANT:
                             *
                             * FIX for collectionview element recycling... or similar controls/virtualizations.
                             * I cant clean the mapstack in the shared project cause its managed by binding and attached properties
                             * aaaand.... they are slow to execute and leave the mapstack corrupted!
                             * This is the only way i found to have a reliable, clean mapstack.
                             */

                            NavPage.TransitionMap.Remove(
                                operation == UINavigationControllerOperation.Push
                                    ? PropertiesContainer
                                    : ShellSection.Stack.Last(), transitionToMap.NativeViewId);

                            Debug.WriteLine($"The destination ViewId {transitionToMap.NativeViewId} has no corrisponding Navive Views in tree and has been removed");
                        }
                    }
                }

                //IF we have views to animate, proceed with custom transition and edge gesture
                //No view to animate = standard push & pop
                if (viewsToAnimate.Any())
                {
                    //deactivate normal pop gesture and activate the custom one suited for the shared transitions
                    if (operation == UINavigationControllerOperation.Push)
                        AddInteractiveTransitionRecognizer();

                    return new NavigationTransition(viewsToAnimate, operation, this, _edgeGestureRecognizer);
                }
            }

            /*
             * IMPORTANT!
             *
             * standard push & pop
             * i dont use my custom edgeswipe because it does not play well with standard pop
             * Doing this work here, is good for push.
             * When doing the custom, interactive, pop i need to double check the custom gesture later
             * (see comments in UIGestureRecognizerState.Ended)
             */

            RemoveInteractiveTransitionRecognizer();
            return null;
        }

        [Export("navigationController:interactionControllerForAnimationController:")]
        public IUIViewControllerInteractiveTransitioning GetInteractionControllerForAnimationController(UINavigationController navigationController, IUIViewControllerAnimatedTransitioning animationController)
        {
            return _percentDrivenInteractiveTransition;
        }

        //During PopToRoot we skip everything and make the default animation
        protected override void OnPopToRootRequested(NavigationRequestedEventArgs e)
        {
            _popToRoot = true;
            base.OnPopToRootRequested(e);
            _popToRoot = false;
        }

        public override UIViewController PopViewController(bool animated)
        {
			//at this point, currentitem is already set to the new page, wich contains our properties
            PropertiesContainer = ((IShellContentController) ShellSection.CurrentItem).Page;
            return base.PopViewController(animated); ;
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {
	        PropertiesContainer = ((IShellContentController)ShellSection.CurrentItem).Page;
            base.PushViewController(viewController, animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            InteractivePopGestureRecognizer.Delegate = this;
        }

        /// <summary>
        /// Add our custom EdgePanGesture
        /// </summary>
        void AddInteractiveTransitionRecognizer()
        {
            InteractivePopGestureRecognizer.Enabled = false;
            if (!View.GestureRecognizers.Contains(_edgeGestureRecognizer))
            {
                //Add PanGesture on left edge to POP page
                _edgeGestureRecognizer = new UIScreenEdgePanGestureRecognizer {Edges = UIRectEdge.Left};
                _edgeGestureRecognizer.AddTarget(() => InteractiveTransitionRecognizerAction(_edgeGestureRecognizer));
                View.AddGestureRecognizer(_edgeGestureRecognizer);
            }
            else
            {
                _edgeGestureRecognizer.Enabled = true;
            }
        }

        /// <summary>
        /// Remove our custom EdgePanGesture
        /// </summary>
        void RemoveInteractiveTransitionRecognizer()
        {
            if (_edgeGestureRecognizer != null && 
                View.GestureRecognizers.Contains(_edgeGestureRecognizer))
            {
                _edgeGestureRecognizer.Enabled = false;
                InteractivePopGestureRecognizer.Enabled = true;
            }
            InteractivePopGestureRecognizer.Enabled = true;
        }

        /// <summary>
        ///  Handle the custom EdgePanGesture to control the Shared Transition state
        /// </summary>
        void InteractiveTransitionRecognizerAction(UIScreenEdgePanGestureRecognizer sender)
        {
            var percent = sender.TranslationInView(sender.View).X / sender.View.Frame.Width;
            var finishTransitionOnEnd = percent > 0.5 || sender.VelocityInView(sender.View).X > 300;

            OnEdgeGesturePanned(new EdgeGesturePannedArgs
            {
                State = sender.State,
                Percent = percent,
                FinishTransitionOnEnd = finishTransitionOnEnd
            });

            switch (sender.State)
            {
                case UIGestureRecognizerState.Began:
                    _percentDrivenInteractiveTransition = new UIPercentDrivenInteractiveTransition();
                    PopViewController(true);
                    break;

                case UIGestureRecognizerState.Changed:
                    _percentDrivenInteractiveTransition.UpdateInteractiveTransition(percent);
                    break;

                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    _percentDrivenInteractiveTransition.CancelInteractiveTransition();
                    _percentDrivenInteractiveTransition = null;
                    break;

                case UIGestureRecognizerState.Ended:
                    if (finishTransitionOnEnd)
                    {
                        _percentDrivenInteractiveTransition.FinishInteractiveTransition();
                        /*
                         * IMPORTANT!
                         *
                         * at the end of this transition, we need to check if we want a normal pop gesture or the custom one for the new page
                         * as we said before, the custom pop gesture doesnt play well with "normal" pages.
                         * So, at the end of the transition, we check if a page exists before the one we are opening and then check the mapstack
                         * If the previous page of the pop destination doesnt have shared transitions, we remove our custom gesture
                         */

                        var pageCount = ShellSection.Stack.Count;
                        if (pageCount > 2 && NavPage.TransitionMap.GetMap(ShellSection.Stack[pageCount - 3],null).Count==0)
                            RemoveInteractiveTransitionRecognizer();
                    }
                    else
                    {
                        _percentDrivenInteractiveTransition.CancelInteractiveTransition();
                    }
                    _percentDrivenInteractiveTransition = null;
                    break;
            }
        }

        /// <summary>
        /// Event fired when the EdgeGesture is working.
        /// Useful to commanding additional animations attached to the transition
        /// </summary>
        void OnEdgeGesturePanned(EdgeGesturePannedArgs e)
        {
            EventHandler<EdgeGesturePannedArgs> handler = EdgeGesturePanned;
            handler?.Invoke(this, e);
        }

        void HandleChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SharedTransitionNavigationPage.BackgroundAnimationProperty.PropertyName)
            {
                UpdateBackgroundTransition();
            }
            else if (e.PropertyName == SharedTransitionNavigationPage.TransitionDurationProperty.PropertyName)
            {
                UpdateTransitionDuration();
            }
            else if (e.PropertyName == SharedTransitionNavigationPage.TransitionSelectedGroupProperty.PropertyName)
            {
                UpdateSelectedGroup();
            }
        }

        void UpdateBackgroundTransition()
        {
            BackgroundAnimation = SharedTransitionNavigationPage.GetBackgroundAnimation(PropertiesContainer);
        }

        void UpdateTransitionDuration()
        {
            TransitionDuration = (double) SharedTransitionNavigationPage.GetTransitionDuration(PropertiesContainer) / 1000;
        }

        void UpdateSelectedGroup()
        {
            _selectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
        }
	}
}
