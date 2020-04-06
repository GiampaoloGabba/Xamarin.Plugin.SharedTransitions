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
        private Element _currentElement;
        protected override void OnAttached()
        {
            if (Application.Current.MainPage is Shell appShell)
            {
                _currentPage = appShell.GetCurrentShellPage();
                UpdateTag();
            }
            else
            {
                FindContainerPageAndUpdateTag(Element);
            }
        }

        private void FindContainerPageAndUpdateTag(Element element)
        {
            _currentElement = element;
	        var parent = _currentElement.Parent;
            if (parent != null && parent is Page page)
            {
                _currentPage = page;
                UpdateTag();
            }
            else if (parent != null)
            {
                FindContainerPageAndUpdateTag(parent);
            }
            else if (_currentPage == null)
            {
                _currentElement.PropertyChanged += CurrentElementOnPropertyChanged;
            }
        }

        protected void CurrentElementOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Parent")
            {
                _currentElement.PropertyChanged -= CurrentElementOnPropertyChanged;
                FindContainerPageAndUpdateTag(_currentElement);
            }
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

        /// <summary>
        /// Update the shared transition name and/or group
        /// </summary>
        void UpdateTag()
        {
            if (Element is View element && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && _currentPage != null)
            {
                var transitionName  = Transition.GetName(element);
                var transitionGroup = Transition.GetGroup(element);
                
                //TODO: Rethink this mess... it works but is superduper ugly 
                if (transitionGroup != null)
                    transitionName += "_" + transitionGroup;

                var controlId = Control?.Id ?? Container?.Id;

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
