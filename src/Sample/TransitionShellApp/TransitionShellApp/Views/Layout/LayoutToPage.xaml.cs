using System;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Layout
{
    public partial class LayoutToPage : ContentPage
    {
        public LayoutToPage()
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("../");
        }
    }
}
