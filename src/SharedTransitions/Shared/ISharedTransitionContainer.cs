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
	}
}
