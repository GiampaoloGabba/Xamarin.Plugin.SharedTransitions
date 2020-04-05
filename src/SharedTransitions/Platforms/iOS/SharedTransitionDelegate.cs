using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjCRuntime;
using UIKit;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public class SharedTransitionDelegate : UINavigationControllerDelegate
	{
		/*
		 * IMPORTANT NOTES:
		 * Read the dedicate comments in code for more info about those fixes.
		 *
		 * Custom edge gesture recognizer:
		 * I need to enable/disable the standard edge swipe when needed
		 * because the custom one works well with transition but not so much without
		 */

		readonly IUINavigationControllerDelegate _oldDelegate;
		readonly ITransitionRenderer _self;

		public SharedTransitionDelegate(IUINavigationControllerDelegate oldDelegate, ITransitionRenderer renderer)
		{
			_oldDelegate = oldDelegate;
			_self        = renderer;
		}

		public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
		{
			_oldDelegate?.DidShowViewController(navigationController, viewController, animated);
		}

		public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
		{
			_oldDelegate?.WillShowViewController(navigationController, viewController, animated);
		}

		public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation(UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
		{
			if (!_self.DisableTransition)
			{
				//At this point the property TargetPage refers to the view we are pushing or popping
				//This view is not yet visible in our app but the variable is already set
				var viewsToAnimate = new List<(UIView ToView, UIView FromView)>();

				IReadOnlyList<TransitionDetail> transitionStackTo;
				IReadOnlyList<TransitionDetail> transitionStackFrom;

				if (operation == UINavigationControllerOperation.Push)
				{
					transitionStackFrom = _self.TransitionMap.GetMap(_self.PropertiesContainer, _self.SelectedGroup);
					transitionStackTo = _self.TransitionMap.GetMap(_self.LastPageInStack, null);
				}
				else
				{
					transitionStackFrom = _self.TransitionMap.GetMap(_self.LastPageInStack, null);
					transitionStackTo = _self.TransitionMap.GetMap(_self.PropertiesContainer, _self.SelectedGroup);
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
							//Using LastOrDefault because the CollectionView created the first element twice
							//and then hide the first without detaching the effect.
							var fromView = transitionStackFrom.FirstOrDefault(x => x.TransitionName == transitionToMap.TransitionName);

							if (fromView == null)
							{
								Debug.WriteLine($"The from view for {transitionToMap.TransitionName} does not exists in stack, ignoring the transition");
								continue;
							}

							var fromNativeView = fromViewController.View.ViewWithTag(fromView.NativeViewId);
							if (fromNativeView != null)
							{
								viewsToAnimate.Add((toView, fromNativeView));
							}
							else
							{
								Debug.WriteLine($"The native view with id {fromView.NativeViewId} for {transitionToMap.TransitionName} does not exists in page and has been removed from the MapStack");

								Transition.RemoveTransition(fromView.View,
									operation == UINavigationControllerOperation.Push
										? _self.PropertiesContainer
										: _self.LastPageInStack);

							}
						}
						else
						{
							Debug.WriteLine($"The destination ViewId {transitionToMap.NativeViewId} has no corrisponding Navive Views in tree and has been removed from the MapStack");

							Transition.RemoveTransition(transitionToMap.View,
								operation == UINavigationControllerOperation.Push
									? _self.LastPageInStack
									: _self.PropertiesContainer);
						}
					}
				}

				//IF we have views to animate, proceed with custom transition and edge gesture
				//No view to animate = standard push & pop
				if (viewsToAnimate.Any())
				{
					//deactivate normal pop gesture and activate the custom one suited for the shared transitions
					if (operation == UINavigationControllerOperation.Push)
						_self.AddInteractiveTransitionRecognizer();

					return new NavigationTransition(viewsToAnimate, operation, _self, _self.EdgeGestureRecognizer);
				}
			}
			
			/*
			 * IMPORTANT!
			 *
			 * standard push & pop
			 * i dont use my custom edgeswipe because it does not play well with standard pop
			 * Doing this work here, is good for push.
			 * When doing the custom, interactive, pop i need to double check the custom gesture later
			 * (see comments in UIGestureRecognizerState.Ended)
			 */

			_self.RemoveInteractiveTransitionRecognizer();
			return null;
		}

		public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForAnimationController(
			UINavigationController navigationController, IUIViewControllerAnimatedTransitioning animationController)
		{
			return _self.PercentDrivenInteractiveTransition;
		}
	}
}
