using System;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Image
{
    public partial class ImageFromPage : ContentPage, ITransitionAware
    {
        public ImageFromPage()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
	        await Navigation.PushAsync(new ImageToPage());
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
