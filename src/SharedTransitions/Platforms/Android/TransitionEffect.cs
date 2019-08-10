using Android.OS;
using System.ComponentModel;
using System.Linq;
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
            var navStack = Application.Current.MainPage.Navigation?.NavigationStack;
            if (navStack == null || navStack.Count == 0)
                throw new System.InvalidOperationException("Shared transitions effect can be attached only to element in a SharedNavigationPage");

            _currentPage  = navStack.Last();
            UpdateTag();
        }

        protected override void OnDetached()
        {
            //WHAT? Nothing on detach?
            //Well no, i clear the MapStack while popping a page :P
            //There are a number of reasons for doing this, with performance in primis
            //We dont risk NRE or reference to detached object anyway, only the ids
            //When we need a view we get them with FindViewById and check for null just after
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == Transition.NameProperty.PropertyName)
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