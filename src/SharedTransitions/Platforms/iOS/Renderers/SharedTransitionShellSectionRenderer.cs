using System;
using System.ComponentModel;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public sealed class SharedTransitionShellSectionRenderer : ShellSectionRenderer, ITransitionRenderer
	{
		public event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;
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

		private readonly InteractiveTransitionRecognizer _interactiveTransitionRecognizer;

		public SharedTransitionShellSectionRenderer(IShellContext context) : base(context)
		{
			Delegate = new SharedTransitionDelegate(Delegate, this);
			TransitionMap = ((ISharedTransitionContainer) context.Shell).TransitionMap;
			_interactiveTransitionRecognizer = new InteractiveTransitionRecognizer(this);
		}

		//During PopToRoot we skip everything and make the default animation
		protected override void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			DisableTransition = true;
			base.OnPopToRootRequested(e);
			DisableTransition = false;
		}

		public override UIViewController PopViewController(bool animated)
		{
			//at this point, currentitem is already set to the new page, wich contains our properties
			if (ShellSection != null)
			{
				PropertiesContainer = ((IShellContentController) ShellSection.CurrentItem)?.Page;
				LastPageInStack     = ShellSection.Stack?.Last();
			}
			return base.PopViewController(animated);
		}

		public override void PushViewController(UIViewController viewController, bool animated)
		{
			PropertiesContainer = ((IShellContentController) ShellSection.CurrentItem).Page;
			LastPageInStack = ShellSection.Stack?.Last();
			base.PushViewController(viewController, animated);
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
		/// Event fired when the EdgeGesture is working.
		/// Useful to commanding additional animations attached to the transition
		/// </summary>
		public void OnEdgeGesturePanned(EdgeGesturePannedArgs e)
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
