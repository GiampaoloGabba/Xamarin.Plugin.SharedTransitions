using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Support.Transitions;
using Android.Transitions;
using Android.Support.V7.Widget;
using Plugin.SharedTransitions;
using Plugin.SharedTransitions.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using View = Android.Views.View;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

[assembly: ExportRenderer(typeof(SharedTransitionNavigationPage), typeof(SharedTransitionNavigationPageRenderer))]

namespace Plugin.SharedTransitions.Platforms.Android
{
    /// <summary>
    /// Platform Renderer for the NavigationPage responsible to manage the Shared Transitions
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SharedTransitionNavigationPageRenderer : NavigationPageRenderer
    {
        readonly FragmentManager _fragmentManager;
        bool _popToRoot;
        int _selectedGroup;

        BackgroundAnimation _backgroundAnimation;
        long _sharedTransitionDuration;

        SharedTransitionNavigationPage NavPage => Element as SharedTransitionNavigationPage;

        Page _propertiesContainer;
        private Page PropertiesContainer
        {
            get => _propertiesContainer;
            set
            {
                if (_propertiesContainer == value)
                    return;

                if (_propertiesContainer != null)
                    _propertiesContainer.PropertyChanged -= PropertiesContainerOnPropertyChanged;

                _propertiesContainer = value;

                if (_propertiesContainer != null)
                {
                    _propertiesContainer.PropertyChanged += PropertiesContainerOnPropertyChanged;
                    UpdateBackgroundTransition();
                    UpdateSharedTransitionDuration();
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
            if (_popToRoot)
            {
                base.SetupPageTransition(transaction, isPush);
            }
            else
            {
                //In Android the mapping logic is inverse compared to IOS
                //When we are here, the destination page is not yet attached so we dont know if there are tags
                //We need to setup transitions only for what we know here, starting from sourcepage
                var fragmentToHide = _fragmentManager.Fragments.Last();

                //this is due the overridden management of pop and push
                var sourcePage = isPush
                    ? PropertiesContainer
                    : NavPage.CurrentPage;

                //this is needed to remap the tag
                var destinationPage = isPush
                    ? NavPage.CurrentPage
                    : PropertiesContainer;

                //When pushing, take only the tags with the selected group (if any).
                //This is to avoid to search al the views in a listview (if any)
                var mapStack = NavPage.TagMap.GetMap(sourcePage, isPush ? _selectedGroup : 0);

                //Get the views who need the transitionName, based on the tags presnete in destination page
                foreach (var tagMap in mapStack)
                {
                    var fromView = fragmentToHide.View.FindViewById(tagMap.ViewId);
                    if (fromView != null)
                    {
                        var correspondingTag = Transition.GetUniqueTag(tagMap.Tag, _selectedGroup, isPush);
                        transaction.AddSharedElement(fromView, $"{destinationPage.Id}_transition_{correspondingTag}");
                    }
                }

                //This is needed to make shared transitions works with hide & add fragments instead of .replace
                transaction.SetAllowOptimization(true);

                if (_backgroundAnimation == BackgroundAnimation.Fade || _popToRoot)
                    transaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out,
                        Resource.Animation.fade_in, Resource.Animation.fade_out);
                else
                    transaction.SetTransition((int)FragmentTransit.None);
            }
        }
        
        public override void AddView(View child)
        {
            if (!(child is Toolbar))
            {
                var fragments = _fragmentManager.Fragments;
                var fragmentToShow = fragments.Last();

                var navigationTransition = TransitionInflater.From(Context).InflateTransition(Resource.Transition.navigation_transition)
                    .SetDuration(_sharedTransitionDuration);

                fragmentToShow.SharedElementEnterTransition = navigationTransition;

                if (fragments.Count > 1)
                {
                    //Switch the current here for all the page except the first.
                    //So we can read the properties for subsequent transitions from the page we leaving
                    PropertiesContainer = Element.CurrentPage;
                }
            }
            base.AddView(child);
        }

        protected override async Task<bool> OnPushAsync(Page page, bool animated)
        {
            //At the very start of the navigationpage push occour inflating the first view
            //We save it immediately so we can access the Navigation options needed for the first transaction
            if (Element.Navigation.NavigationStack.Count == 1)
                PropertiesContainer = page;

            return await base.OnPushAsync(page, animated); ;
        }

        protected override async Task<bool> OnPopViewAsync(Page page, bool animated)
        {

            //We need to take the transition configuration from the destination page
            //At this point the pop is not started so we need to go back in the stack
            Page pageToShow = ((INavigationPageController)Element).Peek(1);
            if (pageToShow == null)
                return await Task.FromResult(false);

            PropertiesContainer = pageToShow;

            //This is ugly but is needed!
            //If we press the back button very fast when we have more than 2 fragments in the stack,
            //unexpected behaviours can happen during pop (this is due to SetReorderingAllowed and base renderer not using fragment backstack).
            //So we need to add a small delay for fast pop clicks starting the third fragment on stack.
            if (_fragmentManager.Fragments.Count > 2)
                await Task.Delay(100);

            return await base.OnPopViewAsync(page, animated); ;
        }

        //During PopToRoot we skip everything and make the default animation
        protected override async Task<bool> OnPopToRootAsync(Page page, bool animated)
        {
            //In poproot we need to replace the last fragment with the first
            var fragments = _fragmentManager.Fragments;
            var t = _fragmentManager.BeginTransaction();

            //we neeed this to recreate the first fragment ui
            //Outs shared transactions use SetReorderingAllowed that cause mess when popping to root multiple situations
            //The only way to be sure to display correctly the rootpage is to recreate his ui.
            //Note: we dont use "remove" here so we can maintain the state of the root view
            t.Detach(fragments.First());
            t.Attach(fragments.First());

            t.CommitAllowingStateLoss();
            
            _popToRoot = true;
            var result = await base.OnPopToRootAsync(page, animated);
            _popToRoot = false;

            return result;
        }

        protected override int TransitionDuration
        {
            //_sharedTransitionDuration + 100 is a fix to prevent bad behaviours on pop (due to SetReorderingAllowed)
            //after the transition end, we need to wait a bit before telling the rendere that we are done
            //Not needed in PopToRoot
            get => _popToRoot ? base.TransitionDuration : (int) _sharedTransitionDuration + 100;
            set => _sharedTransitionDuration = value;
        }

        void PropertiesContainerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SharedTransitionNavigationPage.BackgroundAnimationProperty.PropertyName)
            {
                UpdateBackgroundTransition();
            }
            else if (e.PropertyName == SharedTransitionNavigationPage.SharedTransitionDurationProperty.PropertyName)
            {
                UpdateSharedTransitionDuration();
            }
            else if (e.PropertyName == SharedTransitionNavigationPage.SelectedTagGroupProperty.PropertyName)
            {
                UpdateSelectedGroup();
            }
        }

        void UpdateBackgroundTransition()
        {
            _backgroundAnimation = SharedTransitionNavigationPage.GetBackgroundAnimation(PropertiesContainer);
        }

        void UpdateSharedTransitionDuration()
        {
            TransitionDuration = (int)SharedTransitionNavigationPage.GetSharedTransitionDuration(PropertiesContainer);
        }

        void UpdateSelectedGroup()
        {
            _selectedGroup = SharedTransitionNavigationPage.GetSelectedTagGroup(PropertiesContainer);
        }
    }
}
