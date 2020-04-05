using System;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class LayoutSampleFrom : ContentPage
    {
        public LayoutSampleFrom()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LayoutSampleTo());
        }
    }
}
