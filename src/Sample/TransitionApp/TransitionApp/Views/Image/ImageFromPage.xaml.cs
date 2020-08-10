using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp.Views.Image
{
    public partial class ImageFromPage : ContentPage, ITransitionAware
    {
        public ImageFromPage()
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
