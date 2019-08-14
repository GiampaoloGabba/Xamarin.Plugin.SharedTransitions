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
            if (Element is View element)
                Transition.RemoveTransition(element,_currentPage);
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == Transition.NameProperty.PropertyName ||
                args.PropertyName == Transition.GroupProperty.PropertyName && Transition.GetGroup(Element)!=null)
                UpdateTag();

            base.OnElementPropertyChanged(args);
        }

        /// <summary>
        /// Update the shared transition name and/or group
        /// </summary>
        void UpdateTag()
        {
            if (Element is View element)
            {
                if (Control != null)
                {
                    Control.Tag = Transition.RegisterTransition(element, (int)Control.Tag, _currentPage);
                } 
                else if (Container != null)
                {
                    Container.Tag = Transition.RegisterTransition(element, (int)Container.Tag, _currentPage);
                }
            }
        }
    }
}
