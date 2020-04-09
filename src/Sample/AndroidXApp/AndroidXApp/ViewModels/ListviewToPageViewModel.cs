using AndroidXApp.Models;

namespace AndroidXApp.ViewModels
{
    public class ListviewToPageViewModel : ViewModelBase
    {
        private DogModel _selectedDog;
        public DogModel SelectedDog
        {
            get => _selectedDog;
            set => SetProperty(ref _selectedDog, value);
        }

        public ListviewToPageViewModel(DogModel item = null)
        {
            SelectedDog = item;
        }
    }
}
