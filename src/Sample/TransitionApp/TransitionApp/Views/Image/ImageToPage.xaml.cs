using System;
using Plugin.SharedTransitions;
using Xamarin.Forms;

namespace TransitionApp.Views.Image
{
    public partial class ImageToPage : ContentPage, ITransitionAware
    {
        public ImageToPage()
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
            if (args.PageTo == this && args.NavOperation == NavOperation.Push)
                DisplayAlert("Message", "Shared Transition ended","ok");
        }

        public void OnTransitionCancelled(SharedTransitionEventArgs args)
        {

        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync(false);
        }
    }
}
