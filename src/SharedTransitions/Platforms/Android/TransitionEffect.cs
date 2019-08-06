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
        protected override void OnAttached()
        {
            UpdateTag();
        }

        protected override void OnDetached()
        {
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
                if (Control != null)
                {
                    if (Control.Id == -1)
                        Control.Id = AndroidViews.View.GenerateViewId();

                    //TransitionName unique for page to enable transitions between more than 2 pages
                    Transition.RegisterTransition(element, Control.Id, out var currentPage);
                    Control.TransitionName = currentPage.Id + "_" + transitionName;
                } 
                else if (Container != null)
                {
                    //layout (boxview, stacklayout, frame, ecc...
                    var view = element.GetRenderer()?.View;
                    if (view != null)
                    {
                        //TransitionName unique for page to enable transitions between more than 2 pages
                        view.Id = AndroidViews.View.GenerateViewId();
                        Transition.RegisterTransition(element, view.Id, out var currentPage);
                        view.TransitionName = currentPage.Id + "_" + transitionName; 
                    }
                }
            }
        }
    }
}