using AndroidXApp.ViewModels;
using Xamarin.Forms;

namespace AndroidXApp.Views.Collectionview
{
    public partial class CollectionviewToPage : ContentPage
    {
        public CollectionviewToPage(ListviewToPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
