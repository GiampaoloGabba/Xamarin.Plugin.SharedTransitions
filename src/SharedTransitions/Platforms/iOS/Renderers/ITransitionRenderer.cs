using System;
using UIKit;
using Xamarin.Forms;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public interface ITransitionRenderer
	{
		double TransitionDuration { get; set; }
		bool   PopToRoot     { get; set; }
		string SelectedGroup { get; set; }
		BackgroundAnimation BackgroundAnimation { get; set; }
		Page PropertiesContainer { get; set; }
		Page LastPageInStack { get; set; }
		ISharedTransitionContainer NavPage { get; set; }
		UIPercentDrivenInteractiveTransition PercentDrivenInteractiveTransition { get; set; }
		UIScreenEdgePanGestureRecognizer EdgeGestureRecognizer { get; set; }

		/// <summary>
		/// Add our custom EdgePanGesture
		/// </summary>
		void AddInteractiveTransitionRecognizer();

		/// <summary>
		/// Remove our custom EdgePanGesture
		/// </summary>
		void RemoveInteractiveTransitionRecognizer();

		event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;
	}
}
