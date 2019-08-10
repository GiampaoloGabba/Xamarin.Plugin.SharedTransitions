using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Transition effect used to specify tags to views
    /// </summary>
    /// <seealso cref="Xamarin.Forms.RoutingEffect" />
    public class TransitionEffect : RoutingEffect
    {
        public TransitionEffect() : base(Transition.FullName)
        {
        }
    }

    /// <summary>
    /// Add specific information to View to activate the Shared Transitions
    /// </summary>
    public static class Transition
    {
        public const string ResolutionGroupName = "Plugin.SharedTransitions";
        public const string EffectName = nameof(Transition);
        public const string FullName = ResolutionGroupName + "." + EffectName;

        /// <summary>
        /// Transition name to associate views animation between pages
        /// </summary>
        public static readonly BindableProperty NameProperty = BindableProperty.CreateAttached(
            "Name", 
            typeof(string), 
            typeof(Transition), 
            null, 
            propertyChanged: OnPropertyChanged);

        /// <summary>
        /// Transition group for dynamic transitions
        /// </summary>
        public static readonly BindableProperty GroupProperty = BindableProperty.CreateAttached(
            "Group", 
            typeof(string), 
            typeof(Transition), 
            null,
            propertyChanged: OnPropertyChanged);

        /// <summary>
        /// Gets the shared transition name for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static string GetName(BindableObject bindable)
        {
            return (string)bindable.GetValue(NameProperty);
        }

        /// <summary>
        /// Gets the shared transition group for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static string GetGroup(BindableObject bindable)
        {
            return (string)bindable.GetValue(GroupProperty);
        }

        /// <summary>
        /// Sets the shared transition name for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="value">The shared transition name.</param>
        public static void SetName(BindableObject bindable, string value)
        {
            bindable.SetValue(NameProperty, value);
        }

        /// <summary>
        /// Sets the shared transition group for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="value">The shared transition group</param>
        public static void SetGroup(BindableObject bindable, string value)
        {
            bindable.SetValue(GroupProperty, value);
        }

        /// <summary>
        /// Registers the transition element in the TransitionStack
        /// when the native View does not already have a unique Id
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="currentPage">The current page where the transition has been added</param>
        /// <returns>The unique Id of the native View</returns>
        public static int RegisterTransition(BindableObject bindable, Page currentPage)
        {
            return RegisterTransition(bindable, 0, currentPage);
        }

        /// <summary>
        /// Registers the transition element in the TransitionStack
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="nativeViewId">The platform View identifier</param>
        /// <param name="currentPage">The current page where the transition has been added</param>
        /// <returns>The unique Id of the native View</returns>
        public static int RegisterTransition(BindableObject bindable, int nativeViewId, Page currentPage)
        {
            if (bindable is View element)
            {
                var transitionName  = GetName(element);
                var transitionGroup = GetGroup(element);

                if (currentPage.Parent is SharedTransitionNavigationPage navPage && !string.IsNullOrEmpty(transitionName))
                    return navPage.TransitionMap.Add(currentPage, transitionName, transitionGroup, element.Id, nativeViewId);
            }

            return 0;
        }

        /// <summary>
        /// Called when a property is changed.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The new value</param>
        static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable == null)
                return;

            var element = (View)bindable;
            var existing = element.Effects.FirstOrDefault(x => x is TransitionEffect);

            if (existing == null && newValue != null && newValue.ToString() != "")
            {
                element.Effects.Add(new TransitionEffect());
            }
        }
    }
}
