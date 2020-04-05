using System.Linq;
using TransitionShellApp.Models;
using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class DynamicSampleFrom2 : ContentPage
    {
        readonly DynamicSampleFromViewModel _viewModel;

        public DynamicSampleFrom2()
        {
            InitializeComponent();
            BindingContext = _viewModel = new DynamicSampleFromViewModel();
        }

        async void OnItemSelected(object sender, SelectionChangedEventArgs args)
        {
            var item = args.CurrentSelection.FirstOrDefault() as DogModel;
            if (item == null)
                return;

            // We can set the SelectedGroup both in binding or using the static method
            // SharedTransitionShell.SetTransitionSelectedGroup(this, item.Id.ToString());
            _viewModel.SelectedDog = item;

            await Navigation.PushAsync(new DynamicSampleTo2(new DynamicSampleToViewModel(item)));

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
