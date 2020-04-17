using System.Linq;
using AndroidXApp.Models;
using AndroidXApp.ViewModels;
using Xamarin.Forms;

namespace AndroidXApp.Views.Collectionview
{
    public partial class CollectionviewFromPage : ContentPage
    {
        readonly ListViewFromPageViewModel _viewModel;

        public CollectionviewFromPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ListViewFromPageViewModel();
        }

        async void OnItemSelected(object sender, SelectionChangedEventArgs args)
        {
            var item = args.CurrentSelection.FirstOrDefault() as DogModel;
            if (item == null)
                return;

            // We can set the SelectedGroup both in binding or using the static method
            // SharedTransitionShell.SetTransitionSelectedGroup(this, item.Id.ToString());
            _viewModel.SelectedDog = item;

            await Navigation.PushAsync(new CollectionviewToPage(new ListviewToPageViewModel(item)));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel.Dogs.Count == 0)
                _viewModel.LoadDogsCommand.Execute(null);
        }
    }
}
