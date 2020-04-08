using System;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Layout
{
    public partial class LayoutFromPage : ContentPage
    {
        public LayoutFromPage()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LayoutToPage());
        }
    }
}
