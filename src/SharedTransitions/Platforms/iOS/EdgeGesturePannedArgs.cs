using System;
using UIKit;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    public class EdgeGesturePannedArgs : EventArgs
    {
        public nfloat Percent { get; set; }
        public UIGestureRecognizerState State { get; set; }
        public bool FinishTransitionOnEnd { get; set; }
    }
}
