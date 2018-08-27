using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Navigation Page with support for shared transition
    /// </summary>
    /// <seealso cref="Xamarin.Forms.NavigationPage" />
    public class SharedTransitionNavigationPage : NavigationPage
    {
        /// <summary>
        /// Map for all the tags (and support properties) associated with this SharedTransitionNavigationPage.
        /// </summary>
        internal static readonly BindablePropertyKey TagMapPropertyKey =
            BindableProperty.CreateReadOnly("TagMap", typeof(ITagMapper), typeof(SharedTransitionNavigationPage), default(ITagMapper));

        public static readonly BindableProperty TagMapProperty = TagMapPropertyKey.BindableProperty;

        /// <summary>
        /// The background animation associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty BackgroundAnimationProperty =
            BindableProperty.CreateAttached("BackgroundAnimation", typeof(BackgroundAnimation), typeof(SharedTransitionNavigationPage), BackgroundAnimation.Fade);

        /// <summary>
        /// The shared transition duration in ms associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty SharedTransitionDurationProperty =
            BindableProperty.CreateAttached("SharedTransitionDuration", typeof(long), typeof(SharedTransitionNavigationPage), (long)300);

        /// <summary>
        /// The selected Tag Group passed from the previous page.
        /// This need to be called when navigation from dynamic views with tag to a detail page
        /// </summary>
        public static readonly BindableProperty SelectedTagGroupProperty =
            BindableProperty.CreateAttached("SelectedTagGroup", typeof(int), typeof(SharedTransitionNavigationPage), 0);

        /// <summary>
        /// Gets the tag map.
        /// </summary>
        /// <value>
        /// The tag map.
        /// </value>
        public ITagMapper TagMap
        {
            get => (ITagMapper)GetValue(TagMapProperty);
            internal set => SetValue(TagMapPropertyKey, value);
        }

        public SharedTransitionNavigationPage(Page root) : base(root) => TagMap = new TagMapper();

        /// <summary>
        /// Gets the background animation.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static BackgroundAnimation GetBackgroundAnimation(Page page)
        {
            return (BackgroundAnimation)page.GetValue(BackgroundAnimationProperty);
        }

        /// <summary>
        /// Gets the duration of the shared transition.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static long GetSharedTransitionDuration(Page page)
        {
            return (long)page.GetValue(SharedTransitionDurationProperty);
        }

        /// <summary>
        /// Gets the selected tag group.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static int GetSelectedTagGroup(Page page)
        {
            return (int)page.GetValue(SelectedTagGroupProperty);
        }

        /// <summary>
        /// Sets the background animation.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="value">The value.</param>
        public static void SetBackgroundAnimation(Page page, BackgroundAnimation value)
        {
            page.SetValue(BackgroundAnimationProperty, value);
        }

        /// <summary>
        /// Sets the duration of the shared transition.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="value">The value.</param>
        public static void SetSharedTransitionDuration(Page page, long value)
        {
            page.SetValue(SharedTransitionDurationProperty, value);
        }

        /// <summary>
        /// Sets the selected tag group.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="value">The value.</param>
        public static void SetSelectedTagGroup(Page page, int value)
        {
            page.SetValue(SelectedTagGroupProperty, value);
        }

        protected override void OnChildRemoved(Element child)
        {
            TagMap.Remove((Page)child);
        }
    }
}
