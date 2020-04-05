using TransitionShellApp.ViewModels;
using Xamarin.Forms;

namespace TransitionShellApp.Views
{
    public partial class DynamicSampleTo : ContentPage
    {
        public DynamicSampleTo(DynamicSampleToViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
