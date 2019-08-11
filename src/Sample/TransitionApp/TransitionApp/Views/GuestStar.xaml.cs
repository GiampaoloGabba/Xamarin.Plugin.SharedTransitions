using Xamarin.Forms;

namespace TransitionApp.Views
{
    public partial class GuestStar : ContentPage
    {
        public GuestStar()
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
