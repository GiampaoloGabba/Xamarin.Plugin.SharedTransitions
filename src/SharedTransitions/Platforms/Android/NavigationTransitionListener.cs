using System;
using System.Diagnostics;

#if __ANDROID_29__
using SupportTransitions = AndroidX.Transitions;
#else
using SupportTransitions = Android.Support.Transitions;
#endif

namespace Plugin.SharedTransitions.Platforms.Android
{
    public class NavigationTransitionListener : SupportTransitions.TransitionListenerAdapter
    {
        private readonly ITransitionRenderer _transitionRenderer;

        public NavigationTransitionListener(ITransitionRenderer transitionRenderer)
        {
            _transitionRenderer = transitionRenderer;
        }

        public override void OnTransitionStart(SupportTransitions.Transition transition)
        {
            if (transition != null)
            {
                Debug.WriteLine($"{DateTime.Now} - SHARED: Transition started");
                _transitionRenderer.SharedTransitionStarted();
            }
            base.OnTransitionStart(transition);
        }

        public override void OnTransitionEnd(SupportTransitions.Transition transition)
        {
            if (transition != null)
            {
                Debug.WriteLine($"{DateTime.Now} - SHARED: Transition ended");
                _transitionRenderer.SharedTransitionEnded();
            }
            base.OnTransitionEnd(transition);
        }

        public override void OnTransitionCancel(SupportTransitions.Transition transition)
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
