using System;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class ImageSampleFrom : ContentPage
    {
        public ImageSampleFrom()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
	        await Navigation.PushAsync(new ImageSampleTo());
        }
    }
}
