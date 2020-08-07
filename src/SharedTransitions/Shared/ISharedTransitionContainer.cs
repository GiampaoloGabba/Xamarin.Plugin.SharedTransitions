using System;
using System.ComponentModel;

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
		/// Fired when the Shared Transition starts
		/// </summary>
		event EventHandler TransitionStarted;

		/// <summary>
		/// Fired when the Shared Transition ends
		/// </summary>
		event EventHandler TransitionEnded;

		/// <summary>
		/// Fired when the Shared Transition is cancelled
		/// </summary>
		event EventHandler TransitionCancelled;

		void SendTransitionStarted();
		void SendTransitionEnded();
		void SendTransitionCancelled();
	}
}
