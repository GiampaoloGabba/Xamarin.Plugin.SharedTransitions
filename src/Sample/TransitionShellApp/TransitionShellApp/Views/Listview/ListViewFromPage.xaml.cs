using TransitionShellApp.Models;
using TransitionShellApp.ViewModels;
using TransitionShellApp.Views.Collectionview;
using Xamarin.Forms;

namespace TransitionShellApp.Views.Listview
{
    public partial class ListViewFromPage : ContentPage
    {
        readonly ListViewFromPageViewModel _viewModel;

        public ListViewFromPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ListViewFromPageViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as DogModel;
            if (item == null)
                return;

            // We can set the SelectedGroup both in binding or using the static method
            // SharedTransitionShell.SetTransitionSelectedGroup(this, item.Id.ToString());

            _viewModel.SelectedDog = item;
            await Navigation.PushAsync(new ListviewToPage(new ListviewToPageViewModel(item)));

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
