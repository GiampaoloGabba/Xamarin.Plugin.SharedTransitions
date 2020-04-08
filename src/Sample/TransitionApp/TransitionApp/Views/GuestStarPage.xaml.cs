using Xamarin.Forms;

namespace TransitionApp.Views
{
    public partial class GuestStarPage : ContentPage
    {
        public GuestStarPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Camilla.FadeTo(1,350);
        }

    }
}
