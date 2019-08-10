using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Transitions;
using Android.Support.V7.Widget;
using Plugin.SharedTransitions;
using Plugin.SharedTransitions.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Context = Android.Content.Context;
using View = Android.Views.View;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

[assembly: ExportRenderer(typeof(SharedTransitionNavigationPage), typeof(SharedTransitionNavigationPageRenderer))]

namespace Plugin.SharedTransitions.Platforms.Android
{

    /*
     * IMPORTANT NOTES:
     * Read the dedicate comments in code for more info about those fixes.
     *
     * Pop a controller with transitions groups:
     * Fix to allow the group to be set wit hbinding
     *
     * Pop to root mess:
     * When we make a PopToRoot, we need to "play" with fragments
     * in order to get the right UI
     */

    /// <summary>
    /// Platform Renderer for the NavigationPage responsible to manage the Shared Transitions
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SharedTransitionNavigationPageRenderer : NavigationPageRenderer
    {
        readonly FragmentManager _fragmentManager;
        bool _popToRoot;
        string _selectedGroup;

        BackgroundAnimation _backgroundAnimation;
        int _TransitionDuration;

        SharedTransitionNavigationPage NavPage => Element as SharedTransitionNavigationPage;

        Page _propertiesContainer;
        private Page PropertiesContainer
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

        public SharedTransitionNavigationPageRenderer(Context context) : base(context)
        {
            _fragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager;
        }

        protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
        {
            if (_popToRoot || Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                base.SetupPageTransition(transaction, isPush);
            }
            else
            {
                //In Android the mapping logic is inverse compared to IOS
                //When we are here, the destination page is not yet attached so we dont know if there are transitions
                //We need to setup transitions only for what we know here, starting from sourcepage
                var fragmentToHide = _fragmentManager.Fragments.Last();

                //When we PUSH a page, we arrive here that the destination is already the current page in NavPage
                //During the override we set the propertiescontainer to the page where the push started
                //So we reflect the TransitionStacks accoringly
                var sourcePage = isPush
                    ? PropertiesContainer
                    : NavPage.CurrentPage;

                var destinationPage = isPush
                    ? NavPage.CurrentPage
                    : PropertiesContainer;

                //return the tag map filtering by group (if specified) during push
                var transitionStackFrom = NavPage.TransitionMap.GetMap(sourcePage, isPush ? _selectedGroup : null);

                //Get the views who need the transitionName, based on the tags in destination page
                foreach (var transitionFromMap in transitionStackFrom)
                {
                    var fromView = fragmentToHide.View.FindViewById(transitionFromMap.NativeViewId);
                    if (fromView == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"The source ViewId {transitionFromMap.NativeViewId} has no corrisponding Navive Views in tree");
                        continue;
                    }

                    //group management for pop:
                    //TODO: Rethink this mess... it works but is superduper ugly 
                    var correspondingTag = destinationPage.Id + "_" + transitionFromMap.TransitionName.Replace(sourcePage.Id + "_","");
                    if (!string.IsNullOrEmpty(_selectedGroup) && _selectedGroup != "0" && !isPush)
                        correspondingTag += "_" + _selectedGroup;

                    transaction.AddSharedElement(fromView, correspondingTag);
                }

                //This is needed to make shared transitions works with hide & add fragments instead of .replace
                transaction.SetAllowOptimization(true);
                FinalizePageTransition(transaction, isPush);
            }
        }

        public override void AddView(View child)
        {
            if (!(child is Toolbar) && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var fragments = _fragmentManager.Fragments;
                var fragmentToShow = fragments.Last();

                var navigationTransition = TransitionInflater.From(Context)
                                                             .InflateTransition(Resource.Transition.navigation_transition)
                                                             .SetDuration(_TransitionDuration);

                fragmentToShow.SharedElementEnterTransition = navigationTransition;

                if (fragments.Count > 1)
                {
                    //Switch the current here for all the page except the first.
                    //So we can read the properties for subsequent transitions from the page we are leaving
                    PropertiesContainer = Element.CurrentPage;
                }
            } else if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                System.Diagnostics.Debug.WriteLine($"Shared transitions are supported starting from Android Lollipop");
            }
            base.AddView(child);
        }

        protected override async Task<bool> OnPushAsync(Page page, bool animated)
        {
            //At the very start of the navigationpage push occour inflating the first view
            //We save it immediately so we can access the Navigation options needed for the first transaction
            if (Element.Navigation.NavigationStack.Count == 1)
                PropertiesContainer = page;

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

            return await base.OnPushAsync(page, animated);
        }

        protected override async Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            //We need to take the transition configuration from the destination page
            //At this point the pop is not started so we need to go back in the stack
            PropertiesContainer = ((INavigationPageController)Element).Peek(1);
            return await base.OnPopViewAsync(page, animated); 
        }

        //During PopToRoot we skip everything and make the default animation
        protected override async Task<bool> OnPopToRootAsync(Page page, bool animated)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var fragments = _fragmentManager.Fragments;
                var t = _fragmentManager.BeginTransaction();

                /*
                 * IMPORTANT!
                 *
                 * we need Detach->Attach to recreate the first fragment ui
                 * Our shared transactions use SetReorderingAllowed that cause mess when popping directly to root 
                 * The only way to be sure to display correctly the rootpage is to recreate his ui.
                 *
                 * NOTE: we don't use "remove" here so we can maintain the state of the root view
                 */
                
                t.Detach(fragments.First());
                t.Attach(fragments.First());

                t.CommitAllowingStateLoss();
            }
            _popToRoot = true;
            var result = await base.OnPopToRootAsync(page, animated);
            _popToRoot = false;

            return result;
        }

        protected override int TransitionDuration
        {
            get => _TransitionDuration;
            set => _TransitionDuration = value;
        }

        void FinalizePageTransition(FragmentTransaction transaction, bool isPush)
        {
            switch (_backgroundAnimation)
            {
                case BackgroundAnimation.None:
                    return;
                case BackgroundAnimation.Fade:
                    transaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out,
                        Resource.Animation.fade_out, Resource.Animation.fade_in);
                    break;
                case BackgroundAnimation.Flip:
                    transaction.SetCustomAnimations(Resource.Animation.flip_in, Resource.Animation.flip_out,
                        Resource.Animation.flip_out, Resource.Animation.flip_in);
                    break;
                case BackgroundAnimation.SlideFromLeft:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                            Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                            Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }
                    break;
                case BackgroundAnimation.SlideFromRight:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                            Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                            Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }
                    break;
                case BackgroundAnimation.SlideFromTop:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                            Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                            Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }
                    break;
                case BackgroundAnimation.SlideFromBottom:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                            Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                            Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }
                    break;
                default:
                    transaction.SetTransition((int) FragmentTransit.None);
                    return;
            }
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
            _backgroundAnimation = SharedTransitionNavigationPage.GetBackgroundAnimation(PropertiesContainer);
        }

        void UpdateTransitionDuration()
        {
            TransitionDuration = (int)SharedTransitionNavigationPage.GetTransitionDuration(PropertiesContainer);
        }

        void UpdateSelectedGroup()
        {
            _selectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
        }
    }
}
