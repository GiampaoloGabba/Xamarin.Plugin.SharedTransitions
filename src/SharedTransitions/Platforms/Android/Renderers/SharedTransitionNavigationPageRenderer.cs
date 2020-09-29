using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.OS;
using Plugin.SharedTransitions;
using Plugin.SharedTransitions.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Context = Android.Content.Context;
using View = Android.Views.View;
using Plugin.SharedTransitions.Platforms.Android.Extensions;

#if __ANDROID_29__
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using SupportTransitions = AndroidX.Transitions;
#else
using Android.Support.V7.Widget;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using SupportTransitions = Android.Support.Transitions;
#endif

[assembly: ExportRenderer(typeof(SharedTransitionNavigationPage), typeof(SharedTransitionNavigationPageRenderer))]
namespace Plugin.SharedTransitions.Platforms.Android
{
    /*
     * IMPORTANT NOTES:
     * Read the dedicate comments in code for more info about those fixes.
     *
     * Pop a controller with transitions groups:
     * Fix to allow the group to be set with binding
     *
     */

    /// <summary>
    /// Platform Renderer for the NavigationPage responsible to manage the Shared Transitions
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SharedTransitionNavigationPageRenderer : NavigationPageRenderer, ITransitionRenderer
    {
        public FragmentManager SupportFragmentManager { get; set; }
        public string SelectedGroup { get; set; }
        public BackgroundAnimation BackgroundAnimation { get; set; }
        public ITransitionMapper TransitionMap { get; set; }
        public bool IsInTabbedPage {get; set; }

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
                    _propertiesContainer.PropertyChanged -= PropertiesContainerOnPropertyChanged;

                _propertiesContainer = value;

                if (_propertiesContainer != null)
                {
	                _propertiesContainer.PropertyChanged += PropertiesContainerOnPropertyChanged;
                    UpdateBackgroundTransition();
                    UpdateTransitionDuration();
                    UpdateSelectedGroup();
                }
            }
        }
        public Page LastPageInStack { get; set; }

        /// <summary>
        /// Apply the custom transition in context
        /// </summary>
        public SupportTransitions.Transition InflateTransitionInContext()
        {
            return SupportTransitions.TransitionInflater.From(Context)
                                     .InflateTransition(Resource.Transition.navigation_transition)
                                     .SetDuration(_transitionDuration)
                                     .AddListener(new NavigationTransitionListener(this));
        }

        bool _isPush;
        bool _popToRoot;
        int _transitionDuration;
        readonly NavigationTransition _navigationTransition;

        public SharedTransitionNavigationPageRenderer(Context context) : base(context)
        {
            SupportFragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager;

            //We need the last FragmentManager in Hyerarchy (for tabbed/masterpage)
            _navigationTransition = new NavigationTransition(this);
        }

        protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
        {
            if (_popToRoot || Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
	            base.SetupPageTransition(transaction, isPush);
            }
            else
            {
	            LastPageInStack = Element.CurrentPage;
	            _navigationTransition.SetupPageTransition(transaction,isPush);
            }
        }

        public override void AddView(View child)
        {
            base.AddView(child);

	        if (!(child is Toolbar) && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
	        {
                if (((SharedTransitionNavigationPage) Element).Parent is TabbedPage ||
                    ((SharedTransitionNavigationPage) Element).Parent is MasterDetailPage)
                {
	                IsInTabbedPage = true;
                    var fragment = child.ParentFragment(SupportFragmentManager);

                    if (fragment != null)
                        fragment.SharedElementEnterTransition = InflateTransitionInContext();
                }
                else
		        {
			        SupportFragmentManager.Fragments.Last().SharedElementEnterTransition = InflateTransitionInContext();
		        }
	        }
        }

        protected override async Task<bool> OnPushAsync(Page page, bool animated)
        {
            _isPush = true;

            //At the very start of the navigationpage push occour inflating the first view
            //We save it immediately so we can access the Navigation options needed for the first transaction
            PropertiesContainer = Element.Navigation.NavigationStack.Count == 1
	            ? page
	            : ((INavigationPageController) Element).Peek(1);

            /*
             * IMPORTANT!
             *
             * Fix for TransitionGroup selected with binding (ONLY if we have a transition with groups registered)
             * The binding system is a bit too slow and the Group Property get valorized after the navigation occours
             * I dont know how to solve this in an elegant way. If we set the value directly in the page it may works
             * After a lot of test it seems that with Task.Yield we have basicaly the same performance as without
             * This add no more than 5ms to the navigation i think is largely acceptable
             */
            var mapStack = TransitionMap.GetMap(PropertiesContainer, null, true);
            if (mapStack?.Count > 0 && mapStack.Any(x=>!string.IsNullOrEmpty(x.TransitionGroup)))
                await Task.Yield();

            return await base.OnPushAsync(page, animated);
        }

        protected override async Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            _isPush = false;

            //We need to take the transition configuration from the destination page
            //At this point the pop is not started so we need to go back in the stack
            PropertiesContainer = ((INavigationPageController)Element).Peek(1);

            /*
             * Without animation, we need Detach->Attach to recreate the fragment ui
             * Because wh are using SetReorderingAllowed  that cause mess when popping without animation or PopToRoot
             * NOTE: we don't use "remove" here so we can maintain the state of the root view
             */
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && !animated &&
                TransitionMap.TransitionStack.Any(x => x.Page == PropertiesContainer))
            {
                RecreateFragment(PropertiesContainer);
            }

            return await base.OnPopViewAsync(page, animated);
        }

        //During PopToRoot we skip everything and make the default animation
        protected override async Task<bool> OnPopToRootAsync(Page page, bool animated)
        {
            _isPush = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                RecreateFragment(((INavigationPageController) Element).Pages.FirstOrDefault());

            _popToRoot = true;
            var result = await base.OnPopToRootAsync(page, animated);
            _popToRoot = false;

            return result;
        }

        protected override int TransitionDuration
        {
            get => _transitionDuration;
            set => _transitionDuration = value;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
        {
	        if (e.NewElement != null)
		        TransitionMap = ((ISharedTransitionContainer) Element).TransitionMap;

	        base.OnElementChanged(e);
        }

        void PropertiesContainerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            TransitionDuration = (int)SharedTransitionNavigationPage.GetTransitionDuration(PropertiesContainer);
        }

        void UpdateSelectedGroup()
        {
            SelectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
        }

        public void SharedTransitionStarted()
        {
            ((ISharedTransitionContainer) Element).SendTransitionStarted(TransitionArgs());
        }

        public void SharedTransitionEnded()
        {
            ((ISharedTransitionContainer) Element).SendTransitionEnded(TransitionArgs());
        }

        public void SharedTransitionCancelled()
        {
            ((ISharedTransitionContainer) Element).SendTransitionCancelled(TransitionArgs());
        }

        void RecreateFragment(Page page)
        {
            if (page == null) return;
            //We need a bit of reflection to find the fragment associated to the page we need to display
            try
            {
                var getPageFragment = typeof(NavigationPageRenderer).GetTypeInfo().GetDeclaredMethod("GetPageFragment");
                if (getPageFragment != null)
                {
                    var fragmentToDisplay = (Fragment)getPageFragment.Invoke(this, new object[] { page });
                    var transaction       = SupportFragmentManager.BeginTransaction();
                    transaction.Detach(fragmentToDisplay);
                    transaction.Attach(fragmentToDisplay);
                    transaction.CommitAllowingStateLoss();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        SharedTransitionEventArgs TransitionArgs()
        {
            if (_isPush)
            {
                return new SharedTransitionEventArgs
                {
                    PageFrom     = PropertiesContainer,
                    PageTo       = LastPageInStack,
                    NavOperation = NavOperation.Push
                };
            }

            return new SharedTransitionEventArgs
            {
                PageFrom     = LastPageInStack,
                PageTo       = PropertiesContainer,
                NavOperation = NavOperation.Pop
            };
        }
    }
}
