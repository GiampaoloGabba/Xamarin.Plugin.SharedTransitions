using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;

#if __ANDROID_29__
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
#else
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
#endif

namespace Plugin.SharedTransitions.Platforms.Android
{
	/*
	 * IMPORTANT NOTES:
	 * Read the dedicate comments in code for more info about those fixes.
	 *
	 * Pop to root mess:
	 * When we make a PopToRoot, we need to "play" with fragments
	 * in order to get the right UI
	 */
	public class NavigationTransition
	{
		private readonly ITransitionRenderer _renderer;

		public NavigationTransition(ITransitionRenderer renderer)
		{
			_renderer = renderer;
		}

		public void SetupPageTransition(FragmentTransaction transaction, bool isPush)
		{
			//When we are here, the destination page is not yet attached so we dont know if there are transitions
            //We need to setup transitions only for what we know here, starting from sourcepage
            Fragment fragmentToHide = null;

            if (!_renderer.IsInTabbedPage)
	            fragmentToHide = _renderer.SupportFragmentManager.Fragments.Last();

	        bool animationFound = false;

            //When we PUSH a page, we arrive here that the destination is already the current page in NavPage
            //During the override we set the propertiescontainer to the page where the push started
            //So we reflect the TransitionStacks accoringly
            var sourcePage = isPush
                ? _renderer.PropertiesContainer
                : _renderer.LastPageInStack;

            var destinationPage = isPush
                ? _renderer.LastPageInStack
                : _renderer.PropertiesContainer;

            //return the tag map filtering by group (if specified) during push
            var transitionStackFrom = _renderer.TransitionMap.GetMap(sourcePage, isPush ? _renderer.SelectedGroup : null);
            var transitionStackTo   = _renderer.TransitionMap.GetMap(destinationPage, null ,true);

            //Get the views who need the transitionName, based on the tags in destination page
            foreach (var transitionFromMap in transitionStackFrom)
            {
	            var fromView = (View) transitionFromMap.NativeView;

	            if (fromView == null)
	            {
		            System.Diagnostics.Debug.WriteLine($"The source ViewId for {transitionFromMap.TransitionName} has no corresponding Native Views in tree and has been cleared");
		            Transition.RemoveTransition(transitionFromMap.View, sourcePage);
		            continue;
	            }
                
                //fix for tabbedpage and masterdetail
                //In those cases we have fragments for all the pages, so we need the right fragments containing this view
                if (fragmentToHide == null)
                {
	                foreach (var fragment in _renderer.SupportFragmentManager.Fragments)
	                {
		                if (fragment.View.FindViewById(fromView.Id) != null)
		                {
			                fragmentToHide = fragment.ChildFragmentManager.Fragments.Last();
			                break;
		                }
	                }
                }

                //group management for pop:
                //TODO: Rethink this mess... it works but is superduper ugly 
                var correspondingTag = destinationPage.Id + "_" + transitionFromMap.TransitionName.Replace(sourcePage.Id + "_","");
                if (!string.IsNullOrEmpty(_renderer.SelectedGroup) && _renderer.SelectedGroup != "0" && !isPush)
                    correspondingTag += "_" + _renderer.SelectedGroup;

                //during pop we need to be sure that a transition exists so we can set SetAllowOptimization to true
                //Without active shared transition, the allowOptimization need to stay false or pop will not work
				//With shell, allowOptimization is always off on pop
                if (!isPush && animationFound == false && !(_renderer is SharedTransitionShellItemRenderer) &&
                    transitionStackTo.FirstOrDefault(x => x.TransitionName == transitionFromMap.TransitionName) != null)
	                animationFound = true;

                transaction.AddSharedElement(fromView, correspondingTag);
            }

            //During push we always set the optimization, is harmless even if we dont have transitions
            //During pop we need to check or we risk to break the pop (i love rock anyway....)
            if (animationFound || isPush)
	            //This is needed to make shared transitions works with hide & add fragments instead of .replace
                transaction.SetAllowOptimization(true);

            //This is needed to retain the transition duration for backwards transitions
            //Miss this and they will ignore our custom duration!
            
            if (fragmentToHide != null)
				fragmentToHide.SharedElementEnterTransition = _renderer.InflateTransitionInContext();

            AnimateBackground(transaction, isPush);
        }

		public void HandlePopToRoot()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				var fragments   = _renderer.SupportFragmentManager.Fragments;
				var transaction = _renderer.SupportFragmentManager.BeginTransaction();

				/*
				 * IMPORTANT!
				 *
				 * we need Detach->Attach to recreate the first fragment ui
				 * Our shared transactions use SetReorderingAllowed that cause mess when popping directly to root 
				 * The only way to be sure to display correctly the rootpage is to recreate his ui.
				 *
				 * NOTE: we don't use "remove" here so we can maintain the state of the root view
				 */
				transaction.Detach(fragments.First());
				transaction.Attach(fragments.First());
				transaction.CommitAllowingStateLoss();
			}
		}

        /// <summary>
        /// Animate the background based on user choices
        /// </summary>
        void AnimateBackground(FragmentTransaction transaction, bool isPush)
        {
            switch (_renderer.BackgroundAnimation)
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
	}
}
