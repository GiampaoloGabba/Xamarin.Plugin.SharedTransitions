using System;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public interface ITransitionRenderer
	{
		event EventHandler<EdgeGesturePannedArgs> EdgeGesturePanned;
		double TransitionDuration { get; set; }
		BackgroundAnimation BackgroundAnimation { get; set; }
	}
}
