using Android.Transitions;
using Android.Support.V4.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Plugin.SharedTransitions.Platforms.Android
{
	public class SharedTransitionShellItemRenderer: ShellItemRenderer, ITransitionRenderer
	{
		public FragmentManager FragmentManager { get; set; }
		public string SelectedGroup { get; set; }
		public BackgroundAnimation BackgroundAnimation { get; set; }
		public Page PropertiesContainer { get; set; }
		public Page LastPageInStack { get; set; }
		public ITransitionMapper TransitionMap { get; set; }

		/// <summary>
		/// Apply the custom transition in context
		/// </summary>
		public global::Android.Transitions.Transition InflateTransitionInContext()
		{
			return TransitionInflater.From(Context)
				.InflateTransition(Resource.Transition.navigation_transition)
				.SetDuration(_transitionDuration);
		}

		bool _popToRoot;
		int _transitionDuration;
		private readonly NavigationTransition _navigationTransition;

		public SharedTransitionShellItemRenderer(IShellContext shellContext) : base(shellContext)
		{
			FragmentManager       = ChildFragmentManager;
			_navigationTransition = new NavigationTransition(this);
		}


	}
}
