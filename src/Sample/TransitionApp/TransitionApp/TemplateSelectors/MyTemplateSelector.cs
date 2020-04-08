using TransitionApp.Models;
using Xamarin.Forms;

namespace TransitionApp.TemplateSelectors
{
    public class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template1   { get; set; }
        public DataTemplate Template2 { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((DogModel) item).Id % 2 == 0 ? Template1 : Template2;
        }
    }
}
