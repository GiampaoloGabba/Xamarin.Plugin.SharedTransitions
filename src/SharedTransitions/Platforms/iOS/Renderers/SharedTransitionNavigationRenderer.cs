using System;
using System.ComponentModel;
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
     * Pop a controller with transitions groups:
     * Fix to allow the group to be set wit hbinding
     */

    /// <summary>
    /// Platform Renderer for NavigationPage responsible to manage the Shared Transitions
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SharedTransitionNavigationRenderer : NavigationRenderer, ITransitionRenderer
    {
        public double TransitionDuration  { get; set; }
        public BackgroundAnimation BackgroundAnimation { get; set; }

        /// <summary>
        /// Track the page we need to get the custom properties for the shared transitions
        /// </summary>
        Page _propertiesContainer;
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
        public ITransitionMapper TransitionMap { get; set; }
        public UIScreenEdgePanGestureRecognizer EdgeGestureRecognizer { get; set; }
        public UIPercentDrivenInteractiveTransition PercentDrivenInteractiveTransition { get; set; }
        public bool DisableTransition { get; set; }
        public event EventHandler<EdgeGesturePannedArgs> OnEdgeGesturePanned;
        public string SelectedGroup { get; set; }

        private readonly InteractiveTransitionRecognizer _interactiveTransitionRecognizer;

        public SharedTransitionNavigationRenderer()
        {
	        Delegate = new SharedTransitionDelegate(Delegate,this);
	        _interactiveTransitionRecognizer = new InteractiveTransitionRecognizer(this);
        }

        //During PopToRoot we skip everything and make the default animation
        protected override async Task<bool> OnPopToRoot(Page page, bool animated)
        {
            DisableTransition = true;
            var result = await base.OnPopToRoot(page, true);
            DisableTransition = false;

            return result;
        }

        protected override async Task<bool> OnPushAsync(Page page, bool animated)
        {
	        LastPageInStack = page;
	        UpdatePropertyContainer();

	        return await base.OnPushAsync(page, animated);
        }

        public override UIViewController PopViewController(bool animated)
        {
	        LastPageInStack = Element.Navigation.NavigationStack.Last();
	        UpdatePropertyContainer();

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
             * After a lot of test it seems that with Task.Yield we have basicaly the same performance as without
             * This add no more than 5ms to the navigation i think is largely acceptable
             */
            if (PropertiesContainer != null)
            {
	            var mapStack = TransitionMap?.GetMap(PropertiesContainer, null, true);
	            if (mapStack?.Count > 0 && mapStack.Any(x=>!string.IsNullOrEmpty(x.TransitionGroup)))
		            await Task.Yield();
            }

            base.PushViewController(viewController, animated);
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
	        if (e.NewElement != null)
		        TransitionMap = ((ISharedTransitionContainer) Element).TransitionMap;

	        base.OnElementChanged(e);
        }

        /// <summary>
        /// Add our custom EdgePanGesture
        /// </summary>
        public void AddInteractiveTransitionRecognizer()
        {
	        _interactiveTransitionRecognizer.AddInteractiveTransitionRecognizer(Element.Navigation.NavigationStack);
        }

        /// <summary>
        /// Remove our custom EdgePanGesture
        /// </summary>
        public void RemoveInteractiveTransitionRecognizer()
        {
	        _interactiveTransitionRecognizer.RemoveInteractiveTransitionRecognizer();
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

        /// <summary>
        /// Set the page we are using to read transition properties
        /// </summary>
        void UpdatePropertyContainer()
        {
	        var pageCount = Element.Navigation.NavigationStack.Count;
	        if (pageCount > 1)
	        {
		        PropertiesContainer = ((INavigationPageController)Element).Peek(1);
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

        public void EdgeGesturePanned(EdgeGesturePannedArgs e)
        {
            EventHandler<EdgeGesturePannedArgs> handler = OnEdgeGesturePanned;
            handler?.Invoke(this, e);
        }

        public void SharedTransitionStarted()
        {
            ((ISharedTransitionContainer) Element).SendTransitionStarted();
        }

        public void SharedTransitionEnded()
        {
            ((ISharedTransitionContainer) Element).SendTransitionEnded();
        }

        public void SharedTransitionCancelled()
        {
            ((ISharedTransitionContainer) Element).SendTransitionCancelled();
        }
    }
}
