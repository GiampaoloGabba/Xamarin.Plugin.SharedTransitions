using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Foundation;
using ObjCRuntime;
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
		public bool PopToRoot { get; set; }
		public string SelectedGroup { get; set; }
		public ISharedTransitionContainer NavPage { get; set; }
		public UIPercentDrivenInteractiveTransition PercentDrivenInteractiveTransition { get; set; }

		public SharedTransitionShellSectionRenderer(IShellContext context) : base(context)
		{
			NavPage  = (SharedTransitionShell) context.Shell;
			Delegate = new SharedTransitionDelegate(Delegate, this);
		}

		//During PopToRoot we skip everything and make the default animation
		protected override void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			PopToRoot = true;
			base.OnPopToRootRequested(e);
			PopToRoot = false;
		}

		public override UIViewController PopViewController(bool animated)
		{
			//at this point, currentitem is already set to the new page, wich contains our properties
			PropertiesContainer = ((IShellContentController) ShellSection.CurrentItem).Page;
			LastPageInStack = ShellSection.Stack?.Last();
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
				EdgeGestureRecognizer.Enabled          = false;
				InteractivePopGestureRecognizer.Enabled = true;
			}

			InteractivePopGestureRecognizer.Enabled = true;
		}

		/// <summary>
		///  Handle the custom EdgePanGesture to control the Shared Transition state
		/// </summary>
		void InteractiveTransitionRecognizerAction(UIScreenEdgePanGestureRecognizer sender)
		{
			var percent               = sender.TranslationInView(sender.View).X / sender.View.Frame.Width;
			var finishTransitionOnEnd = percent > 0.5 || sender.VelocityInView(sender.View).X > 300;

			OnEdgeGesturePanned(new EdgeGesturePannedArgs
			{
				State                 = sender.State,
				Percent               = percent,
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

						var pageCount = ShellSection.Stack.Count;
						if (pageCount > 2 &&
						    NavPage.TransitionMap.GetMap(ShellSection.Stack[pageCount - 3], null).Count == 0)
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
			TransitionDuration = (double) SharedTransitionNavigationPage.GetTransitionDuration(PropertiesContainer) /
			                     1000;
		}

		void UpdateSelectedGroup()
		{
			SelectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
		}
	}
}
