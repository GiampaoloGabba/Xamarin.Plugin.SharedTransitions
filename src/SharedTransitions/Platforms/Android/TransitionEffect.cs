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
        protected override void OnAttached()
        {
            UpdateTag();
        }

        protected override void OnDetached()
        {
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
                    Transition.RegisterTransition(element, Control.Id, out var currentPage);
                    if (currentPage != null) //<!-- can't find the navigation stack!
                        Control.TransitionName = currentPage.Id + "_" + transitionName;
                } 
                else if (Container != null)
                {
                    if (Container.Id == -1)
                        Container.Id = AndroidViews.View.GenerateViewId();

                    Transition.RegisterTransition(element, Container.Id, out var currentPage);
                    if (currentPage != null)
                        Container.TransitionName = currentPage.Id + "_" + transitionName;
                }
            }
        }
    }
}