using System;
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
        /// Tag used to associate views between pages
        /// </summary>
        public static readonly BindableProperty TagProperty = BindableProperty.CreateAttached("Tag", typeof(int), typeof(Transition), 0, propertyChanged: OnPropertyChanged);

        /// <summary>
        /// Group used to identify repeated views 
        /// </summary>
        public static readonly BindableProperty TagGroupProperty = BindableProperty.CreateAttached("TagGroup", typeof(int), typeof(Transition), 0);


        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static int GetTag(BindableObject bindable)
        {
            return (int)bindable.GetValue(TagProperty);
        }

        /// <summary>
        /// Gets the tag group.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static int GetTagGroup(BindableObject bindable)
        {
            return (int)bindable.GetValue(TagGroupProperty);
        }

        /// <summary>
        /// Sets the tag.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="value">The Tag.</param>
        public static void SetTag(BindableObject bindable, int value)
        {
            bindable.SetValue(TagProperty, value);
        }

        /// <summary>
        /// Sets the tag.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="value">The Tag.</param>
        public static void SetTagGroup(BindableObject bindable, int value)
        {
            bindable.SetValue(TagGroupProperty, value);
        }

        /// <summary>
        /// Unique ID used for transitions.
        /// For dynamic transition (with tag group defined) start from 100.
        /// In case of hybrid transition (static + dynamic) we have the first 100 reserved to static transitions
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <returns></returns>
        public static int GetUniqueTag(BindableObject bindable)
        {
            return GetUniqueTag(GetTag(bindable), GetTagGroup(bindable));
        }

        /// <summary>
        /// Unique ID used for transitions.
        /// For dynamic transition (with tag group defined) start from 100.
        /// In case of hybrid transition (static + dynamic) we have the first 100 reserved to static transitions
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="group">The group.</param>
        /// <param name="reverse">if set to <c>true</c> prepare tag from details to list</param>
        /// <returns></returns>
        public static int GetUniqueTag(int tag, int group, bool reverse = false)
        {
            if (group > 0)
                return reverse
                    ? tag - group - 99
                    : 99 + group + tag;

            return tag;
        }

        /// <summary>
        /// Registers the tag in the MapStack calculating the current page.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static int RegisterTagInStack(BindableObject bindable)
        {
            return RegisterTagInStack(bindable, 0, out _);
        }

        /// <summary>
        /// Registers the tag in the MapStack calculating the current page.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="viewId">The platform View identifier</param>
        /// <param name="pageId">The Xamarin Forms Page identifier</param>
        /// <returns></returns>
        public static int RegisterTagInStack(BindableObject bindable, int viewId, out Guid pageId)
        {
            pageId = default;
            if (bindable is View element)
            {
                var tag = GetUniqueTag(element);
                var group = GetTagGroup(element);
                if (!(element.Navigation?.NavigationStack.Count > 0) || tag < 0) return 0;

                var currentPage = element.Navigation.NavigationStack.Last();
                if (currentPage.Parent is SharedTransitionNavigationPage navPage)
                {
                    navPage.TagMap.Add(currentPage, tag, group, viewId);
                    pageId = currentPage.Id;
                    return tag;
                }
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

            if (existing == null && newValue != null && (int)newValue > 0)
            {
                element.Effects.Add(new TransitionEffect());
            }
            else if (existing != null)
            {
                element.Effects.Remove(existing);
            }
        }
    }
}
