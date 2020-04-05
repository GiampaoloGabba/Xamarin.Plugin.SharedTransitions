using TransitionShellApp.Models;

namespace TransitionShellApp.ViewModels
{
    public class DynamicSampleToViewModel : ViewModelBase
    {
        private DogModel _selectedDog;
        public DogModel SelectedDog
        {
            get => _selectedDog;
            set => SetProperty(ref _selectedDog, value);
        }

        public DynamicSampleToViewModel(DogModel item = null)
        {
            SelectedDog = item;
        }
    }
}
