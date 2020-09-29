using System;
using Plugin.SharedTransitions.Shared.Utils;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
	/// <summary>
	/// Container Page with support for shared transitions
	/// </summary>
	public interface ISharedTransitionContainer
	{
		/// <summary>
		/// Gets the transition map
		/// </summary>
		/// <value>
		/// The transition map
		/// </value>
		ITransitionMapper TransitionMap { get; set; }

		/// <summary>
		/// The current transition behing execute
		/// </summary>
		ObservableProperty<SharedTransitionEventArgs> CurrentTransition { get; }

		/// <summary>
		/// Fired when the Shared Transition starts
		/// </summary>
		event EventHandler<SharedTransitionEventArgs> TransitionStarted;

		/// <summary>
		/// Fired when the Shared Transition ends
		/// </summary>
		event EventHandler<SharedTransitionEventArgs> TransitionEnded;

		/// <summary>
		/// Fired when the Shared Transition is cancelled
		/// </summary>
		event EventHandler<SharedTransitionEventArgs> TransitionCancelled;

		void SendTransitionStarted(SharedTransitionEventArgs eventArgs);
		void SendTransitionEnded(SharedTransitionEventArgs eventArgs);
		void SendTransitionCancelled(SharedTransitionEventArgs eventArgs);
	}
}
