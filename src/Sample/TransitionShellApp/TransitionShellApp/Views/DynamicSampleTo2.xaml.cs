using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class DynamicSampleTo2 : ContentPage
    {
        public DynamicSampleTo2(DynamicSampleToViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
