using Plugin.SharedTransitions;
using TransitionShellApp.Models;
using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class DynamicSampleFrom : ContentPage
    {
        readonly DynamicSampleFromViewModel _viewModel;

        public DynamicSampleFrom()
        {
            InitializeComponent();
            BindingContext = _viewModel = new DynamicSampleFromViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as DogModel;
            if (item == null)
                return;

            // We can set the SelectedGroup both in binding or using the static method
            // SharedTransitionShell.SetTransitionSelectedGroup(this, item.Id.ToString());

            _viewModel.SelectedDog = item;
            await Navigation.PushAsync(new DynamicSampleTo(new DynamicSampleToViewModel(item)));

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
