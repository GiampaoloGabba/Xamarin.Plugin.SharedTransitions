using System;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    public class SharedTransitionEventArgs : EventArgs
    {
        public Page PageFrom { get; set; }
        public Page PageTo { get; set; }
        public NavOperation NavOperation { get; set; }

    }

    public enum NavOperation
    {
        Push,
        Pop
    }
}
