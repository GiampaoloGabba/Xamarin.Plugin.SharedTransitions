using AndroidXApp.ViewModels;
using Xamarin.Forms;

namespace AndroidXApp.Views.Listview
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
