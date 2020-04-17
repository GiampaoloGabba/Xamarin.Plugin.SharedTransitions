using System;
using Xamarin.Forms;

namespace AndroidXApp.Views.Layout
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
