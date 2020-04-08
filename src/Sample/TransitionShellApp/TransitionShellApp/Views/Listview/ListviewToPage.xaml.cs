using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Listview
{
    public partial class ListviewToPage : ContentPage
    {
        public ListviewToPage(ListviewToPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
