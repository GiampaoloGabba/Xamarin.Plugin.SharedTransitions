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
    public class SharedTransitionNavigationRenderer : NavigationRenderer, IUINavigationControllerDelegate, IUIGestureRecognizerDelegate
    {
        public double TransitionDuration { get; set; }
        public BackgroundAnimation BackgroundAnimation { get; set; }
        string _selectedGroup;
        private UIScreenEdgePanGestureRecognizer _interactiveTransitionGestureRecognizer;

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

        UIPercentDrivenInteractiveTransition _percentDrivenInteractiveTransition;
        SharedTransitionNavigationPage NavPage => Element as SharedTransitionNavigationPage;
        bool _popToRoot;

        public SharedTransitionNavigationRenderer() : base()
        {
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
                    //When we PUSH a page, we arrive here that the destination is already the current page in NavPage
                    //During the override we set the PropertiesContainer to the page where the push started
                    //So we reflect the TransitionStacks accoringly
                    transitionStackFrom = NavPage.TransitionMap.GetMap(PropertiesContainer, _selectedGroup);
                    transitionStackTo   = NavPage.TransitionMap.GetMap(NavPage.CurrentPage);
                }
                else
                {
                    //During POP, everyting is fine and clear
                    transitionStackFrom = NavPage.TransitionMap.GetMap(NavPage.CurrentPage);
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
                             * Using ListView/Collection cause the first item to be created two times, but then one of them is discarded
                             * without calling the Detach method from our effect. So we need to find the right element!
                             */
                            
                            foreach (var nativeView in transitionStackFrom.Where(x => x.TransitionName == transitionToMap.TransitionName))
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
                                    ? NavPage.CurrentPage
                                    : PropertiesContainer, transitionToMap.NativeViewId);

                            Debug.WriteLine($"The destination ViewId {transitionToMap.NativeViewId} has no corrisponding Navive Views in tree and has been removed");
                        }
                    }
                }

                //No view to animate = standard push & pop
                if (viewsToAnimate.Any())
                {
                    //deactivate normal pop gesture and activate the custom one suited for the shared transitions
                    if (operation == UINavigationControllerOperation.Push)
                    {
                        AddInteractiveTransitionRecognizer();
                    }
                    return new NavigationTransition(viewsToAnimate, operation, this);
                }
            }

            /*
             * IMPORTANT!
             *
             * standard push & pop
             * i dont use my custom edgeswipe because it does not play well with standard pop
             * doing this work here, is good for push and doing the check on new the page
             * when doing the custom, interactive, pop i need to double check the custom gesture
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
        protected override async Task<bool> OnPopToRoot(Page page, bool animated)
        {
            _popToRoot = true;
            var result = await base.OnPopToRoot(page, true);
            _popToRoot = false;

            return result;
        }

        public override UIViewController PopViewController(bool animated)
        {
            //We need to take the transition configuration from the destination page
            //At this point the pop is not started so we need to go back in the stack
            var pageCount = Element.Navigation.NavigationStack.Count;
            if (pageCount > 1)
                PropertiesContainer = Element.Navigation.NavigationStack[pageCount - 2];

            return base.PopViewController(animated); ;
        }
        
        public override async void PushViewController(UIViewController viewController, bool animated)
        {
            //We need to take the transition configuration from the page we are leaving page
            //At this point the current page in the navigation stack is already set with the page we are pusing
            //So we need to go back in the stack to retrieve what we want
            var pageCount = Element.Navigation.NavigationStack.Count;
            PropertiesContainer = pageCount > 1 
                ? Element.Navigation.NavigationStack[pageCount - 2] 
                : NavPage.CurrentPage;

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
            var mapStack = NavPage.TransitionMap.GetMap(PropertiesContainer, true);
            if (mapStack.Count > 0 && mapStack.Any(x=>!string.IsNullOrEmpty(x.TransitionGroup)))
                await Task.Yield();

            base.PushViewController(viewController, animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            InteractivePopGestureRecognizer.Delegate = this;
        }

        void AddInteractiveTransitionRecognizer()
        {
            InteractivePopGestureRecognizer.Enabled = false;
            if (!View.GestureRecognizers.Contains(_interactiveTransitionGestureRecognizer))
            {
                //Add PanGesture on left edge to POP page
                _interactiveTransitionGestureRecognizer = new UIScreenEdgePanGestureRecognizer {Edges = UIRectEdge.Left};
                _interactiveTransitionGestureRecognizer.AddTarget(() => InteractiveTransitionRecognizerAction(_interactiveTransitionGestureRecognizer));
                View.AddGestureRecognizer(_interactiveTransitionGestureRecognizer);
            }
            else
            {
                _interactiveTransitionGestureRecognizer.Enabled = true;
            }
        }

        void RemoveInteractiveTransitionRecognizer()
        {
            if (_interactiveTransitionGestureRecognizer != null && 
                View.GestureRecognizers.Contains(_interactiveTransitionGestureRecognizer))
            {
                _interactiveTransitionGestureRecognizer.Enabled = false;
                InteractivePopGestureRecognizer.Enabled = true;
            }
            InteractivePopGestureRecognizer.Enabled = true;
        }

        void InteractiveTransitionRecognizerAction(UIScreenEdgePanGestureRecognizer sender)
        {
            var percent = sender.TranslationInView(sender.View).X / sender.View.Frame.Width;

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
                    if (percent > 0.5 || sender.VelocityInView(sender.View).X > 300)
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

                        var pageCount = Element.Navigation.NavigationStack.Count;
                        if (pageCount > 2 && NavPage.TransitionMap.GetMap(Element.Navigation.NavigationStack[pageCount - 3]).Count==0)
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
