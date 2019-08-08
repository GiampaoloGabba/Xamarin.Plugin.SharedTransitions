using System.Collections.Generic;
using Prism.AppModel;
using Prism.Navigation;
using TransitionApp.Models;

namespace TransitionApp.ViewModels
{
    public class DynamicSampleToViewModel : ViewModelBase, IAutoInitialize
    {
        private List<DogModel> _dogs;
        public List<DogModel> Dogs
        {
            get { return _dogs; }
            set { SetProperty(ref _dogs, value); }
        }

        private DogModel _selectedDog;
        public DogModel SelectedDog
        {
            get { return _selectedDog; }
            set { SetProperty(ref _selectedDog, value); }
        }

        public DynamicSampleToViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}
