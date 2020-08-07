using System;
using UIKit;
using Xamarin.Forms;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public interface ITransitionRenderer
	{
		double TransitionDuration { get; set; }
		bool DisableTransition { get; set; }
		string SelectedGroup { get; set; }
		BackgroundAnimation BackgroundAnimation { get; set; }
		Page PropertiesContainer { get; set; }
		Page LastPageInStack { get; set; }
		ITransitionMapper TransitionMap { get; set; }
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

		event EventHandler<EdgeGesturePannedArgs> OnEdgeGesturePanned;
		event EventHandler OnSharedTransitionStarted;
		event EventHandler OnSharedTransitionEnded;
		event EventHandler OnSharedTransitionCancelled;

		void EdgeGesturePanned(EdgeGesturePannedArgs e);
		void SharedTransitionStarted();
		void SharedTransitionEnded();
		void SharedTransitionCancelled();
	}
}
