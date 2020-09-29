using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using Xamarin.Forms;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public class InteractiveTransitionRecognizer
	{
		/*
		 * IMPORTANT NOTES:
		 * Read the dedicate comments in code for more info about those fixes.
		 *
		 * Custom edge gesture recognizer:
		 * I need to enable/disable the standard edge swipe when needed
		 * because the custom one works well with transition but not so much without
		 */

		readonly ITransitionRenderer _renderer;
		public event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;

		public InteractiveTransitionRecognizer(ITransitionRenderer renderer)
		{
			_renderer = renderer;
		}

		public void AddInteractiveTransitionRecognizer(IReadOnlyList<Page> pageStack)
		{
			var navigationController = (UINavigationController) _renderer;

			navigationController.InteractivePopGestureRecognizer.Enabled = false;
			if (navigationController?.View?.GestureRecognizers?.Contains(_renderer.EdgeGestureRecognizer) == false)
			{
				//Add PanGesture on left edge to POP page
				_renderer.EdgeGestureRecognizer = new UIScreenEdgePanGestureRecognizer {Edges = UIRectEdge.Left};
				_renderer.EdgeGestureRecognizer.AddTarget(() => InteractiveTransitionRecognizerAction(_renderer.EdgeGestureRecognizer, pageStack));
				navigationController.View.AddGestureRecognizer(_renderer.EdgeGestureRecognizer);
			}
			else
			{
				_renderer.EdgeGestureRecognizer.Enabled = true;
			}
		}

		public void RemoveInteractiveTransitionRecognizer()
		{
			var navigationController = (UINavigationController) _renderer;

			if (navigationController.View?.GestureRecognizers != null && _renderer.EdgeGestureRecognizer != null &&
			    navigationController.View.GestureRecognizers.Contains(_renderer.EdgeGestureRecognizer))
			{
				_renderer.EdgeGestureRecognizer.Enabled = false;
				navigationController.InteractivePopGestureRecognizer.Enabled = true;
			}
			navigationController.InteractivePopGestureRecognizer.Enabled = true;
		}

        void InteractiveTransitionRecognizerAction(UIScreenEdgePanGestureRecognizer sender, IReadOnlyList<Page> pageStack )
        {
            var percent = sender.TranslationInView(sender.View).X / sender.View.Frame.Width;
            var finishTransitionOnEnd = percent > 0.5 || sender.VelocityInView(sender.View).X > 300;

            _renderer.EdgeGesturePanned(new EdgeGesturePannedArgs
            {
                State = sender.State,
                Percent = percent,
                FinishTransitionOnEnd = finishTransitionOnEnd
            });

            switch (sender.State)
            {
                case UIGestureRecognizerState.Began:
	                _renderer.PercentDrivenInteractiveTransition = new UIPercentDrivenInteractiveTransition();
	                ((UINavigationController)_renderer).PopViewController(true);
                    break;

                case UIGestureRecognizerState.Changed:
	                _renderer.PercentDrivenInteractiveTransition.UpdateInteractiveTransition(percent);
                    break;

                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
	                //Unfortunately i have to always complete the transition or the pagestack will get corrupted/polluted
	                _renderer.PercentDrivenInteractiveTransition.FinishInteractiveTransition();
	                //_renderer.PercentDrivenInteractiveTransition.CancelInteractiveTransition();
                    _renderer.PercentDrivenInteractiveTransition = null;
                    break;

                case UIGestureRecognizerState.Ended:

	                //Unfortunately i have to always complete the transition or the pagestack will get corrupted/polluted
	                _renderer.PercentDrivenInteractiveTransition.FinishInteractiveTransition();

	                /*
	                 * IMPORTANT!
	                 *
	                 * at the end of this transition, we need to check if we want a normal pop gesture or the custom one for the new page
	                 * as we said before, the custom pop gesture doesnt play well with "normal" pages.
	                 * So, at the end of the transition, we check if a page exists before the one we are opening and then check the mapstack
	                 * If the previous page of the pop destination doesnt have shared transitions, we remove our custom gesture
	                 */
	                var pageCount = pageStack.Count;
	                if (pageCount > 2 && _renderer.TransitionMap.GetMap(pageStack[pageCount - 3],null).Count==0)
		                RemoveInteractiveTransitionRecognizer();
                    _renderer.PercentDrivenInteractiveTransition = null;
                    break;
            }
        }

	}
}
