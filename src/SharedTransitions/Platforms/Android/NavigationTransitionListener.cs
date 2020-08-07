#nullable enable
using System;
using System.Diagnostics;
using TransitionListenerAdapter = Android.Transitions.TransitionListenerAdapter;

namespace Plugin.SharedTransitions.Platforms.Android
{
    public class NavigationTransitionListener : TransitionListenerAdapter
    {
        private readonly ITransitionRenderer _transitionRenderer;

        public NavigationTransitionListener(ITransitionRenderer transitionRenderer)
        {
            _transitionRenderer = transitionRenderer;
        }

        public override void OnTransitionStart(global::Android.Transitions.Transition? transition)
        {
            if (transition != null)
            {
                Debug.WriteLine($"{DateTime.Now} - SHARED: Transition started");
                _transitionRenderer.SharedTransitionStarted();
            }
            base.OnTransitionStart(transition);
        }

        public override void OnTransitionEnd(global::Android.Transitions.Transition? transition)
        {
            if (transition != null)
            {
                Debug.WriteLine($"{DateTime.Now} - SHARED: Transition ended");
                _transitionRenderer.SharedTransitionEnded();
            }
            base.OnTransitionEnd(transition);
        }

        public override void OnTransitionCancel(global::Android.Transitions.Transition? transition)
        {
            if (transition != null)
            {
                Debug.WriteLine($"{DateTime.Now} - SHARED: Transition cancelled");
                _transitionRenderer.SharedTransitionCancelled();
            }
            base.OnTransitionCancel(transition);
        }
    }
}
