using Android.OS;
using System.ComponentModel;
using Plugin.SharedTransitions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName(Transition.ResolutionGroupName)]
[assembly: ExportEffect(typeof(Plugin.SharedTransitions.Platforms.Android.TransitionEffect), Transition.EffectName)]

namespace Plugin.SharedTransitions.Platforms.Android
{
    public class TransitionEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control == null)
                return;

            UpdateTag();
        }

        protected override void OnDetached()
        {
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == Transition.TagProperty.PropertyName ||
                args.PropertyName == Transition.TagGroupProperty.PropertyName)
                UpdateTag();

            base.OnElementPropertyChanged(args);
        }

        void UpdateTag()
        {
            if (Element is View element && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var tag = Transition.RegisterTagInStack(element, Control.Id, out var pageId);
                
                //transitionName must be unique in every fragment
                //this is needed when we have more than 2 pages to transition,
                //because the navPage hides old fragments without removing it!
                Control.TransitionName = $"{pageId}_transition_{tag}";
            }
        }
    }
}