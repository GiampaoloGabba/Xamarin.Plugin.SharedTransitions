using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Navigation Page with support for shared transitions
    /// </summary>
    /// <seealso cref="Xamarin.Forms.NavigationPage" />
    public class SharedTransitionNavigationPage : NavigationPage
    {
        /// <summary>
        /// Map for all transitions (and support properties) associated with this SharedTransitionNavigationPage.
        /// </summary>
        internal static readonly BindablePropertyKey TransitionMapPropertyKey =
            BindableProperty.CreateReadOnly("TransitionMap", typeof(ITransitionMapper), typeof(SharedTransitionNavigationPage), default(ITransitionMapper));

        public static readonly BindableProperty TransitionMapProperty = TransitionMapPropertyKey.BindableProperty;

        /// <summary>
        /// The shared transition selected group for dynamic transitions
        /// </summary>
        public static readonly BindableProperty TransitionSelectedGroupProperty =
            BindableProperty.CreateAttached("TransitionSelectedGroup", typeof(string), typeof(SharedTransitionNavigationPage), null);

        /// <summary>
        /// The background animation associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty BackgroundAnimationProperty =
            BindableProperty.CreateAttached("BackgroundAnimation", typeof(BackgroundAnimation), typeof(SharedTransitionNavigationPage), BackgroundAnimation.Fade);

        /// <summary>
        /// The shared transition duration in ms associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty TransitionDurationProperty =
            BindableProperty.CreateAttached("TransitionDuration", typeof(long), typeof(SharedTransitionNavigationPage), (long)300);

        /// <summary>
        /// Gets the transition map.
        /// </summary>
        /// <value>
        /// The transition map.
        /// </value>
        public ITransitionMapper TransitionMap
        {
            get => (ITransitionMapper)GetValue(TransitionMapProperty);
            internal set => SetValue(TransitionMapPropertyKey, value);
        }

        public SharedTransitionNavigationPage(Page root) : base(root) => TransitionMap = new TransitionMapper();

        /// <summary>
        /// Gets the transition selected group
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static string GetTransitionSelectedGroup(Page page)
        {
            return (string)page.GetValue(TransitionSelectedGroupProperty);
        }

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
        public static long GetTransitionDuration(Page page)
        {
            return (long)page.GetValue(TransitionDurationProperty);
        }

        /// <summary>
        /// Sets the transition selected group
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static void SetTransitionSelectedGroup(Page page, string value)
        {
            page.SetValue(TransitionSelectedGroupProperty, value);
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
        public static void SetTransitionDuration(Page page, long value)
        {
            page.SetValue(TransitionDurationProperty, value);
        }

        protected override void OnChildRemoved(Element child)
        {
            TransitionMap.Remove((Page)child);
        }
    }
}
