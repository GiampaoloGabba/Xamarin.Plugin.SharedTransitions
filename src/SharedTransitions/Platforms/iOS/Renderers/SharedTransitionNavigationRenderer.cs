using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Plugin.SharedTransitions;
using Plugin.SharedTransitions.Platforms.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(SharedTransitionNavigationPage), typeof(SharedTransitionNavigationRenderer))]

namespace Plugin.SharedTransitions.Platforms.iOS
{
    /*
     * IMPORTANT NOTES:
     * Read the dedicate comments in code for more info about those fixes.
     *
     * Listview/collection view hidden item:
     * Fix First item is created two times, then discarded and Detach not called
     *
     * MapStack cleaning:
     * Clean here instead of the shared project
     * for dynamic transitions with virtualization
     *
     * Pop a controller with transitions groups:
     * Fix to allow the group to be set wit hbinding
     *
     * Custom edge gesture recognizer:
     * I need to enable/disable the standard edge swipe when needed
     * because the custom one works well with transition but not so much without
     */

    /// <summary>
    /// Platform Renderer for the NavigationPage responsible to manage the Shared Transitions
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SharedTransitionNavigationRenderer : NavigationRenderer, ITransitionRenderer
    {
	    public event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;
        public double TransitionDuration  { get; set; }
        public BackgroundAnimation BackgroundAnimation { get; set; }

        /// <summary>
        /// Track the page we need to get the custom properties for the shared transitions
        /// </summary>
        Page _propertiesContainer;

        private ISharedTransitionContainer _navPage;

        public Page PropertiesContainer
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

        public Page LastPageInStack { get; set; }

        public ISharedTransitionContainer NavPage
        {
	        get => _navPage ?? Element as SharedTransitionNavigationPage;
	        set => _navPage = value;
        }

        public UIScreenEdgePanGestureRecognizer EdgeGestureRecognizer { get; set; }
        public UIPercentDrivenInteractiveTransition PercentDrivenInteractiveTransition { get; set; }
        public bool PopToRoot { get; set; }
        public string SelectedGroup { get; set; }

        public SharedTransitionNavigationRenderer()
        {
	        Delegate = new SharedTransitionDelegate(Delegate,this);
        }

        //During PopToRoot we skip everything and make the default animation
        protected override async Task<bool> OnPopToRoot(Page page, bool animated)
        {
            PopToRoot = true;
            var result = await base.OnPopToRoot(page, true);
            PopToRoot = false;

            return result;
        }

        protected override async Task<bool> OnPushAsync(Page page, bool animated)
        {
	        LastPageInStack = page;

	        //We need to take the transition configuration from the page we are leaving
	        //At this point the current page in the navigation stack is already set with the page we are pusing
	        //So we need to go back in the stack to retrieve what we want
	        var pageCount = Element.Navigation.NavigationStack.Count;
	        if (pageCount > 1)
		        PropertiesContainer = Element.Navigation.NavigationStack[pageCount - 2];

	        return await base.OnPushAsync(page, animated);
        }

        public override UIViewController PopViewController(bool animated)
        {
	        LastPageInStack = Element.Navigation.NavigationStack.Last();

	        //We need to take the transition configuration from the destination page
	        //At this point the pop is not started so we need to go back in the stack
	        var pageCount = Element.Navigation.NavigationStack.Count;
	        if (pageCount > 1)
		        PropertiesContainer = Element.Navigation.NavigationStack[pageCount - 2];

	        return base.PopViewController(animated); ;
        }

        
        public override async void PushViewController(UIViewController viewController, bool animated)
        {
            /*
             * IMPORTANT!
             *
             * Fix for TransitionGroup selected with binding (ONLY if we have a transition with groups registered)
             * The binding system is a bit too slow and the Group Property get valorized after the navigation occours
             * I dont know how to solve this in an elegant way. If we set the value directly in the page it may works
             * buyt is not ideal cause i want this full compatible with binding and mvvm
             * We can use Yield the task or a small delay like Task.Delay(10) or Task.Delay(5).
             * On faster phones Task.Delay(1) work, but i wouldnt trust it in slower phones :)
             *
             * After a lot of test it seems that with Task.Yield we have basicaly the same performance as without
             * This add no more than 5ms to the navigation i think is largely acceptable
             */
            if (PropertiesContainer != null)
            {
	            var mapStack = NavPage.TransitionMap?.GetMap(PropertiesContainer, null, true);
	            if (mapStack.Count > 0 && mapStack.Any(x=>!string.IsNullOrEmpty(x.TransitionGroup)))
		            await Task.Yield();
            }

            base.PushViewController(viewController, animated);
        }

        /// <summary>
        /// Add our custom EdgePanGesture
        /// </summary>
        public void AddInteractiveTransitionRecognizer()
        {
            InteractivePopGestureRecognizer.Enabled = false;
            if (!View.GestureRecognizers.Contains(EdgeGestureRecognizer))
            {
                //Add PanGesture on left edge to POP page
                EdgeGestureRecognizer = new UIScreenEdgePanGestureRecognizer {Edges = UIRectEdge.Left};
                EdgeGestureRecognizer.AddTarget(() => InteractiveTransitionRecognizerAction(EdgeGestureRecognizer));
                View.AddGestureRecognizer(EdgeGestureRecognizer);
            }
            else
            {
                EdgeGestureRecognizer.Enabled = true;
            }
        }

        /// <summary>
        /// Remove our custom EdgePanGesture
        /// </summary>
        public void RemoveInteractiveTransitionRecognizer()
        {
            if (EdgeGestureRecognizer != null && 
                View.GestureRecognizers.Contains(EdgeGestureRecognizer))
            {
                EdgeGestureRecognizer.Enabled = false;
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
                    PercentDrivenInteractiveTransition = new UIPercentDrivenInteractiveTransition();
                    PopViewController(true);
                    break;

                case UIGestureRecognizerState.Changed:
                    PercentDrivenInteractiveTransition.UpdateInteractiveTransition(percent);
                    break;

                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    PercentDrivenInteractiveTransition.CancelInteractiveTransition();
                    PercentDrivenInteractiveTransition = null;
                    break;

                case UIGestureRecognizerState.Ended:
                    if (finishTransitionOnEnd)
                    {
                        PercentDrivenInteractiveTransition.FinishInteractiveTransition();
                        /*
                         * IMPORTANT!
                         *
                         * at the end of this transition, we need to check if we want a normal pop gesture or the custom one for the new page
                         * as we said before, the custom pop gesture doesnt play well with "normal" pages.
                         * So, at the end of the transition, we check if a page exists before the one we are opening and then check the mapstack
                         * If the previous page of the pop destination doesnt have shared transitions, we remove our custom gesture
                         */

                        var pageCount = Element.Navigation.NavigationStack.Count;
                        if (pageCount > 2 && NavPage.TransitionMap.GetMap(Element.Navigation.NavigationStack[pageCount - 3],null).Count==0)
                            RemoveInteractiveTransitionRecognizer();
                    }
                    else
                    {
                        PercentDrivenInteractiveTransition.CancelInteractiveTransition();
                    }
                    PercentDrivenInteractiveTransition = null;
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
            SelectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
        }
    }
}
