using System.Diagnostics;
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
        /// Transition name used to associate views between pages
        /// </summary>
        public static readonly BindableProperty NameProperty = BindableProperty.CreateAttached(
            "Name", 
            typeof(string), 
            typeof(BindableObject), 
            null, 
            propertyChanged: OnPropertyChanged);

        /// <summary>
        /// Transition group for dynamic transitions
        /// </summary>
        public static readonly BindableProperty GroupProperty = BindableProperty.CreateAttached(
            "Group", 
            typeof(string), 
            typeof(BindableObject), 
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
        /// <param name="value">The shared transition name</param>
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
        /// </summary>
        /// <param name="view">Xamarin Forms View</param>
        /// <param name="nativeView">The platform native View</param>
        /// <param name="currentPage">The current page where the transition has been added</param>
        /// <returns>The unique Id of the native View</returns>
        public static void RegisterTransition(View view, object nativeView, Page currentPage)
        {
            var transitionName  = GetName(view);
            var transitionGroup = GetGroup(view);

            if ((!string.IsNullOrEmpty(transitionName) || !string.IsNullOrEmpty(transitionGroup)))
            {
	            if (Application.Current.MainPage is ISharedTransitionContainer shellPage)
	            {
		            shellPage.TransitionMap.AddOrUpdate(currentPage, transitionName, transitionGroup, view, nativeView);
	            }
	            else if (currentPage.Parent is ISharedTransitionContainer navPage)
	            {
		            navPage.TransitionMap.AddOrUpdate(currentPage, transitionName, transitionGroup, view, nativeView);
	            }
	            else
	            {
		            throw new System.InvalidOperationException("Shared transitions effect can be attached only to element in a ISharedTransitionContainer");
	            }
            }

            Debug.WriteLine($"Trying to attach a TransitionEffect without name or group specified. Nothing done");
        }

        /// <summary>
        /// Remove the transition configuration from the Stack
        /// </summary>
        /// <param name="view">View associated to the transition</param>
        /// <param name="currentPage">Container Page</param>
        public static void RemoveTransition(View view, Page currentPage)
        {
	        switch (currentPage.Parent)
	        {
		        case SharedTransitionNavigationPage sharedTransitionNavigationPage:
			        sharedTransitionNavigationPage.TransitionMap.Remove(currentPage,view);
			        break;
		        case SharedTransitionShell sharedTransitionshell:
			        sharedTransitionshell.TransitionMap.Remove(currentPage,view);
			        break;
	        }
        }

        static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable == null)
            {
                Debug.WriteLine($"BindableObject should not be null at this point (Attached Property changed)");
                return;
            }

            Debug.WriteLine($"===== SAHARED: update property for {bindable}: {oldValue} - {newValue}");

            var element = (View)bindable;
            var existing = element.Effects.FirstOrDefault(x => x is TransitionEffect);

            if (existing == null && newValue != null && newValue.ToString() != "")
            {
                element.Effects.Add(new TransitionEffect());
            }
            else if (existing != null && GetName(bindable) == null && GetGroup(bindable) == null)
            {
	            element.Effects.Remove(new TransitionEffect());
            }
        }
    }
}
