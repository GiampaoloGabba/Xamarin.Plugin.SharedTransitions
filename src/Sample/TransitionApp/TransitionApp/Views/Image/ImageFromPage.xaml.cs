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
            if (args.PageFrom == this && args.NavOperation == NavOperation.Push)
                DisplayAlert("Message", "Shared Transition started","ok");
        }

        public void OnTransitionEnded(SharedTransitionEventArgs args)
        {

        }

        public void OnTransitionCancelled(SharedTransitionEventArgs args)
        {

        }

    }
}
