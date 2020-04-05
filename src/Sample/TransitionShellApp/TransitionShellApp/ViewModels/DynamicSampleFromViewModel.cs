using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TransitionShellApp.Models;
using Xamarin.Forms;

namespace TransitionShellApp.ViewModels
{
    public class DynamicSampleFromViewModel : ViewModelBase
    {

        private DogModel _selectedDog;
        public DogModel SelectedDog
        {
            get => _selectedDog;
            set => SetProperty(ref _selectedDog, value);
        }

        private ObservableCollection<DogModel> _dogs;

        public ObservableCollection<DogModel> Dogs
        {
            get => _dogs;
            set => SetProperty(ref _dogs, value);
        }

        //public ObservableCollection<DogModel> Dogs { get; set; }
        public Command  LoadDogsCommand { get; set; }


        public DynamicSampleFromViewModel()
        {
            Dogs            = new ObservableCollection<DogModel>();
            LoadDogsCommand = new Command(ExecuteLoadItemsCommand);
        }

        void ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var description =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                    "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                    "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                    "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

                Dogs = new ObservableCollection<DogModel>(new List<DogModel>
                {
                    new DogModel {Id = 1, Title = "Christmas Dog", Image    = "christmas_dog.jpg", Description = description},
                    new DogModel {Id = 2, Title = "Cute Dog", Image         = "cute_dog.jpg", Description      = description},
                    new DogModel {Id = 3, Title = "Lazy Dog", Image         = "lazy_dog.jpg", Description      = description},
                    new DogModel {Id = 4, Title = "What the Dog??!?", Image = "what_the_dog.jpg", Description  = description},
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
