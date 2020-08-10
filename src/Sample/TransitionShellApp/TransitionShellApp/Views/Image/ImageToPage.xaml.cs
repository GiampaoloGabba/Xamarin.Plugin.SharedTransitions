using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Image
{
    public partial class ImageToPage : ContentPage, ITransitionAware
    {
        public ImageToPage()
        {
            InitializeComponent();
        }

        public void OnTransitionStarted(SharedTransitionEventArgs args)
        {

        }

        public void OnTransitionEnded(SharedTransitionEventArgs args)
        {
            if (args.PageTo == this && args.NavOperation == NavOperation.Push)
                DisplayAlert("Message", "Shared Transition ended","ok");
        }

        public void OnTransitionCancelled(SharedTransitionEventArgs args)
        {

        }
    }
}
