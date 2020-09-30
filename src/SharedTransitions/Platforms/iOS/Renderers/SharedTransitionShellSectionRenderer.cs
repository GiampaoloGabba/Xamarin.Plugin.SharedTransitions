using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

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
	/// Platform Renderer for ShellSection responsible to manage the Shared Transitions
	/// </summary>
	public sealed class SharedTransitionShellSectionRenderer : ShellSectionRenderer, ITransitionRenderer
	{
		public double TransitionDuration { get; set; }
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
		public UIScreenEdgePanGestureRecognizer EdgeGestureRecognizer { get; set; }
		public bool DisableTransition { get; set; }
		public string SelectedGroup { get; set; }
		public ITransitionMapper TransitionMap { get; set; }
		public UIPercentDrivenInteractiveTransition PercentDrivenInteractiveTransition { get; set; }
		public event EventHandler<EdgeGesturePannedArgs> OnEdgeGesturePanned;

		readonly InteractiveTransitionRecognizer _interactiveTransitionRecognizer;
		readonly IShellContext _shellContext;
		bool _isPush;
		int _consequentPops;

		public SharedTransitionShellSectionRenderer(IShellContext shellContext) : base(shellContext)
		{
			_shellContext = shellContext;
			Delegate = new SharedTransitionDelegate(Delegate, this);
			TransitionMap = ((ISharedTransitionContainer) shellContext.Shell).TransitionMap;
			_interactiveTransitionRecognizer = new InteractiveTransitionRecognizer(this);
		}

		[Export("navigationBar:shouldPopItem:")]
		public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
		{
			//The base renderer is returning false when doing a pop with the back button
			//because is calling PopViewController manually
			//We need to return true to get the transition working and check for multiple execution in PopViewController
			_consequentPops = 0;
			base.ShouldPopItem(navigationBar, item);
			return true;
		}

		//NOT CALLED BY:
		//Navigationbar button
		//Called when tapping the same tabicon when there is a tabbedpage
		protected override async void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			_consequentPops = 0;

			if (e.RequestType == NavigationRequestType.Pop)
			{
				PopViewController(e.Animated);
			}
			else if (e.RequestType == NavigationRequestType.Push)
			{
				_isPush = true;
				UpdatePropertyContainer(false);

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
			}

			base.OnNavigationRequested(sender, e);
		}

		//Called by navigation bar button
		public override  UIViewController PopViewController(bool animated)
		{
			// this means the pop is already done, nothing we can do
			if (_consequentPops > 0)
			{
				return null;
			}
			_consequentPops++;

			_isPush = false;
			//at this point, currentitem is already set to the new page, wich contains our properties
			if (ShellSection != null)
				UpdatePropertyContainer(true);

			return base.PopViewController(animated);
		}

		//Called when tapping the same tabicon when there is a tabbedpage
		public override UIViewController[] PopToRootViewController(bool animated)
		{
			//Fix for tap on same tab to go back to the first view
			//Without this, events on the first view are invoked even when they shouldnt
			_isPush = false;
			return base.PopToRootViewController(animated);
		}

		protected override void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			_isPush = false;

			//Check if is a "true" PopToRoot or only a GotoAsync("../") to the first page
			//I HOPE that the back navigation is always ..
			//<rant>
			//FFS everything is f#####g internal in forms! Even the real Location requested by user!
			//getting exhausted by this....
			//</rant>
			var shell = (SharedTransitionShell)_shellContext.Shell;
			if (shell.LastNavigating.Location.ToString() == "..")
			{
				e.RequestType = NavigationRequestType.Pop;
				e.Animated    = true;
				OnPopRequested(e);
			}
			else
			{
				//During PopToRoot we skip everything and make the default animation
				DisableTransition = true;
				base.OnPopToRootRequested(e);
				DisableTransition = false;
			}
		}

		/// <summary>
		/// Add our custom EdgePanGesture
		/// </summary>
		public void AddInteractiveTransitionRecognizer()
		{
			_interactiveTransitionRecognizer.AddInteractiveTransitionRecognizer(ShellSection.Stack);
		}

		/// <summary>
		/// Remove our custom EdgePanGesture
		/// </summary>
		public void RemoveInteractiveTransitionRecognizer()
		{
			_interactiveTransitionRecognizer.RemoveInteractiveTransitionRecognizer();
		}

		/// <summary>
		/// Set the page we are using to read transition properties
		/// </summary>
		void UpdatePropertyContainer(bool isPop)
		{
			PropertiesContainer = ((IShellContentController) ShellSection.CurrentItem)?.Page;
			LastPageInStack     = ShellSection.Stack?.Last();

			//Fix bug with new shellrenderer where we have the first page null in stack
			if (LastPageInStack == null && isPop)
			{
				var pageDisplayedInfo = typeof(ShellSection).GetTypeInfo().GetDeclaredProperty("DisplayedPage");
				if (pageDisplayedInfo != null)
					LastPageInStack = (Page) pageDisplayedInfo.GetValue(ShellSection);
			}
		}

		void HandleChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SharedTransitionShell.BackgroundAnimationProperty.PropertyName)
			{
				UpdateBackgroundTransition();
			}
			else if (e.PropertyName == SharedTransitionShell.TransitionDurationProperty.PropertyName)
			{
				UpdateTransitionDuration();
			}
			else if (e.PropertyName == SharedTransitionShell.TransitionSelectedGroupProperty.PropertyName)
			{
				UpdateSelectedGroup();
			}
		}

		void UpdateBackgroundTransition()
		{
			BackgroundAnimation = SharedTransitionShell.GetBackgroundAnimation(PropertiesContainer);
		}

		void UpdateTransitionDuration()
		{
			TransitionDuration = (double) SharedTransitionShell.GetTransitionDuration(PropertiesContainer) / 1000;
		}

		void UpdateSelectedGroup()
		{
			SelectedGroup = SharedTransitionShell.GetTransitionSelectedGroup(PropertiesContainer);
		}

		/// <summary>
		/// Event fired when the EdgeGesture is working.
		/// Useful to commanding additional animations attached to the transition
		/// </summary>
		public void EdgeGesturePanned(EdgeGesturePannedArgs e)
		{
			EventHandler<EdgeGesturePannedArgs> handler = OnEdgeGesturePanned;
			handler?.Invoke(this, e);
		}

		public void SharedTransitionStarted()
		{
			((ISharedTransitionContainer) _shellContext.Shell).SendTransitionStarted(TransitionArgs());
		}

		public void SharedTransitionEnded()
		{
			((ISharedTransitionContainer) _shellContext.Shell).SendTransitionEnded(TransitionArgs());
		}

		public void SharedTransitionCancelled()
		{
			((ISharedTransitionContainer) _shellContext.Shell).SendTransitionCancelled(TransitionArgs());
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
