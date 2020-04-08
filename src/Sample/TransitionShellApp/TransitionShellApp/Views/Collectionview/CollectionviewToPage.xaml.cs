using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Collectionview
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
