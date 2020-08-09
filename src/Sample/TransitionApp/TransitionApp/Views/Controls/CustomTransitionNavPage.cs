using System.Diagnostics;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp.Views.Controls
{
    public class CustomTransitionNavPage : SharedTransitionNavigationPage
    {
        public override void OnTransitionStarted(Page pageFrom, Page pageTo, NavOperation navOperation)
        {
            Debug.WriteLine($"From override: Transition started - {pageFrom}|{pageTo}|{navOperation}");
        }

        public override void OnTransitionEnded(Page pageFrom, Page pageTo, NavOperation navOperation)
        {
            Debug.WriteLine($"From override: Transition ended - {pageFrom}|{pageTo}|{navOperation}");
        }

        public override void OnTransitionCancelled(Page pageFrom, Page pageTo, NavOperation navOperation)
        {
            Debug.WriteLine($"From override: Transition cancelled - {pageFrom}|{pageTo}|{navOperation}");
        }
    }
}
