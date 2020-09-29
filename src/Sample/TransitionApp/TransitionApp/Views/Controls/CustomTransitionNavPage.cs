using System.Diagnostics;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp.Views.Controls
{
    public class CustomTransitionNavPage : SharedTransitionNavigationPage
    {
        public CustomTransitionNavPage()
        {
            CurrentTransition.Changed += data =>
            {
                Debug.WriteLine($"CurrentTransition Changed: {data?.NavOperation} {data?.PageFrom} {data?.PageTo}");
            };
        }

        public override void OnTransitionStarted(SharedTransitionEventArgs args)
        {
            Debug.WriteLine($"From override: Transition started - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
        }

        public override void OnTransitionEnded(SharedTransitionEventArgs args)
        {
            Debug.WriteLine($"From override: Transition ended - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
        }

        public override void OnTransitionCancelled(SharedTransitionEventArgs args)
        {
            Debug.WriteLine($"From override: Transition cancelled - {args.PageFrom}|{args.PageTo}|{args.NavOperation}");
        }

    }
}
