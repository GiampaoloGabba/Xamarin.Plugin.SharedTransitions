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
            if (args.PropertyName == Transition.TransitionNameProperty.PropertyName)
                UpdateTag();


            base.OnElementPropertyChanged(args);
        }

        void UpdateTag()
        {
            if (Element is View element && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (Control != null)
                {
                    if (Control.Id == -1)
                        Control.Id = AndroidViews.View.GenerateViewId();

                    var tag = Transition.RegisterTransition(element, Control.Id);
                    Control.TransitionName = $"transition_{tag}";
                } 
                else if (Container != null)
                {
                    //layout (boxview, stacklayout, frame, ecc...
                    var view = element.GetRenderer()?.View;
                    if (view != null)
                    {
                        view.Id = AndroidViews.View.GenerateViewId();
                        var tag = Transition.RegisterTransition(element, view.Id);
                        view.TransitionName = $"transition_{tag}";
                    }
                }
            }
        }
    }
}