using System;
using UIKit;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    /// <summary>
    /// EventArgs for the EdgeGesturePanned event fired by SharedTransitionNavigationPage
    /// </summary>
    public class EdgeGesturePannedArgs : EventArgs
    {
        /// <summary>
        /// Current percentage
        /// </summary>
        public nfloat Percent { get; set; }
        /// <summary>
        /// Current state
        /// </summary>
        public UIGestureRecognizerState State { get; set; }
        /// <summary>
        /// True when the gesture has been completed based on custom rules (not only the state)
        /// </summary>
        public bool FinishTransitionOnEnd { get; set; }
    }
}
