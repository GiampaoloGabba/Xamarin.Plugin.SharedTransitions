using System.Diagnostics;
using Plugin.SharedTransitions;

namespace TransitionApp.Views.Controls
{
    public class CustomTransitionNavPage : SharedTransitionNavigationPage
    {
        public override void OnTransitionStarted()
        {
            Debug.WriteLine("From override: Transition started");
        }

        public override void OnTransitionEnded()
        {
            Debug.WriteLine("From override: Transition ended");
        }

        public override void OnTransitionCancelled()
        {
            Debug.WriteLine("From override: Transition cancelled");
        }
    }
}
