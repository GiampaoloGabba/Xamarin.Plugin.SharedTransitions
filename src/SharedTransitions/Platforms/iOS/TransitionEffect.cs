using System.ComponentModel;
using Plugin.SharedTransitions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName(Transition.ResolutionGroupName)]
[assembly: ExportEffect(typeof(Plugin.SharedTransitions.Platforms.iOS.TransitionEffect), Transition.EffectName)]

namespace Plugin.SharedTransitions.Platforms.iOS
{
    public class TransitionEffect : PlatformEffect
    {
        private Page _currentPage;
        protected override void OnAttached()
        {
            _currentPage = Application.Current.MainPage.GetCurrentPageInNavigationStack();
            if (_currentPage == null)
                throw new System.InvalidOperationException("Shared transitions effect can be attached only to element in a SharedNavigationPage");

            UpdateTag();
        }

        protected override void OnDetached()
        {
            //WHAT? Nothing on detach?
            //Well no, i clear the MapStack while popping a page :P
            //There are a number of reasons for doing this, with performance in primis
            //We dont risk NRE or reference to detached object anyway, only the ids
            //When we need a view we get them with ViewWithTag and check for null just after
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == Transition.NameProperty.PropertyName)
                UpdateTag();

            base.OnElementPropertyChanged(args);
        }

        void UpdateTag()
        {
            if (Element is View element)
            {
                if (Control != null)
                {
                    Control.Tag = Transition.RegisterTransition(element, _currentPage);
                } 
                else if (Container != null)
                {
                    Container.Tag = Transition.RegisterTransition(element, _currentPage);
                }
            }
        }
    }
}
