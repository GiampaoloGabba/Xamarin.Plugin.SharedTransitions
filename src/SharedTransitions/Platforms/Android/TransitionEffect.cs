using Android.OS;
using System.ComponentModel;
using Plugin.SharedTransitions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidViews = Android.Views;

[assembly: ResolutionGroupName(Transition.ResolutionGroupName)]
[assembly: ExportEffect(typeof(Plugin.SharedTransitions.Platforms.Android.TransitionEffect), Transition.EffectName)]

namespace Plugin.SharedTransitions.Platforms.Android
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
                args.PropertyName == Transition.GroupProperty.PropertyName)
                UpdateTag();

            base.OnElementPropertyChanged(args);
        }

        void UpdateTag()
        {
            if (Element is View element && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var transitionName = Transition.GetName(element);
                var transitionGroup = Transition.GetGroup(element);
                
                //TODO: Rethink this mess... it works but is superduper ugly 
                if (transitionGroup != null)
                    transitionName += "_" + transitionGroup;

                if (Control != null)
                {
                    if (Control.Id == -1)
                        Control.Id = AndroidViews.View.GenerateViewId();

                    //TransitionName needs to be unique for page to enable transitions between more than 2 pages
                    Transition.RegisterTransition(element, Control.Id, _currentPage);
                        Control.TransitionName = _currentPage.Id + "_" + transitionName;
                } 
                else if (Container != null)
                {
                    if (Container.Id == -1)
                        Container.Id = AndroidViews.View.GenerateViewId();

                    Transition.RegisterTransition(element, Container.Id, _currentPage);
                    Container.TransitionName = _currentPage.Id + "_" + transitionName;
                }
            }
        }
    }
}