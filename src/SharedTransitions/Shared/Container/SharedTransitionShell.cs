using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Shell with support for shared transitions
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Shell" />
    public class SharedTransitionShell : Shell, ISharedTransitionContainer
    {
        /// <summary>
        /// Map for all transitions (and support properties) associated with this SharedTransitionShell
        /// </summary>
        internal static readonly BindablePropertyKey TransitionMapPropertyKey =
            BindableProperty.CreateReadOnly("TransitionMap", typeof(ITransitionMapper), typeof(SharedTransitionShell), default(ITransitionMapper));

        public static readonly BindableProperty TransitionMapProperty = TransitionMapPropertyKey.BindableProperty;

        /// <summary>
        /// The shared transition selected group for dynamic transitions
        /// </summary>
        public static readonly BindableProperty TransitionSelectedGroupProperty =
            BindableProperty.CreateAttached("TransitionSelectedGroup", typeof(string), typeof(SharedTransitionShell), null);

        /// <summary>
        /// The background animation associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty BackgroundAnimationProperty =
            BindableProperty.CreateAttached("BackgroundAnimation", typeof(BackgroundAnimation), typeof(SharedTransitionShell), BackgroundAnimation.None);

        /// <summary>
        /// The shared transition duration (in ms) associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty TransitionDurationProperty =
            BindableProperty.CreateAttached("TransitionDuration", typeof(long), typeof(SharedTransitionShell), (long)300);

        public event EventHandler<SharedTransitionEventArgs> TransitionStarted;
        public event EventHandler<SharedTransitionEventArgs> TransitionEnded;
        public event EventHandler<SharedTransitionEventArgs> TransitionCancelled;

        internal ITransitionMapper TransitionMap
        {
	        get => (ITransitionMapper)GetValue(TransitionMapProperty);
	        set => SetValue(TransitionMapPropertyKey, value);
        }

        /// <summary>
        /// Gets the transition map
        /// </summary>
        /// <value>
        /// The transition map
        /// </value>
        ITransitionMapper ISharedTransitionContainer.TransitionMap
        {
	        get => TransitionMap;
	        set => TransitionMap = value;
        }

        public SharedTransitionShell() => TransitionMap = new TransitionMapper();

        public ShellNavigationState LastNavigating { get; set; }
        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            LastNavigating = args.Target;
        }

        /// <summary>
        /// Gets the transition selected group
        /// </summary>
        public static string GetTransitionSelectedGroup(Page page)
        {
            return (string)page.GetValue(TransitionSelectedGroupProperty);
        }

        /// <summary>
        /// Gets the background animation.
        /// </summary>
        public static BackgroundAnimation GetBackgroundAnimation(Page page)
        {
            return (BackgroundAnimation)page.GetValue(BackgroundAnimationProperty);
        }

        /// <summary>
        /// Gets the duration of the shared transition
        /// </summary>
        public static long GetTransitionDuration(Page page)
        {
            return (long)page.GetValue(TransitionDurationProperty);
        }

        /// <summary>
        /// Sets the transition selected group
        /// </summary>
        public static void SetTransitionSelectedGroup(Page page, string value)
        {
            page.SetValue(TransitionSelectedGroupProperty, value);
        }

        /// <summary>
        /// Sets the background animation
        /// </summary>
        public static void SetBackgroundAnimation(Page page, BackgroundAnimation value)
        {
            page.SetValue(BackgroundAnimationProperty, value);
        }

        /// <summary>
        /// Sets the duration of the shared transition
        /// </summary>
        public static void SetTransitionDuration(Page page, long value)
        {
            page.SetValue(TransitionDurationProperty, value);
        }

        public virtual void OnTransitionStarted(SharedTransitionEventArgs args){ }
        public virtual void OnTransitionEnded(SharedTransitionEventArgs args){ }
        public virtual void OnTransitionCancelled(SharedTransitionEventArgs args){ }


        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionStarted(SharedTransitionEventArgs args)
        {
            TransitionStarted?.Invoke(this, args);
            OnTransitionStarted(args);
            MessagingCenter.Send(this, "SendTransitionStarted", args);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionEnded(SharedTransitionEventArgs args)
        {
            TransitionEnded?.Invoke(this, args);
            OnTransitionEnded(args);
            MessagingCenter.Send(this, "SendTransitionEnded", args);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionCancelled(SharedTransitionEventArgs args)
        {
            TransitionCancelled?.Invoke(this, args);
            OnTransitionCancelled(args);
            MessagingCenter.Send(this, "SendTransitionCancelled", args);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == nameof(CurrentItem))
            {
                //When the first element in a shellsection is a contentTemplate,
                //the event "ChildAdded" will not be called on that section!
                //Se we need to wireup the DescendantAdded event wich will be
                //notified when the first page is attached

                foreach (var shellSection in CurrentItem.Items)
                {
                    if (shellSection.CurrentItem.ContentTemplate != null)
                    {
                        shellSection.DescendantAdded   += ShellSectionOnChildAdded;
                        shellSection.DescendantRemoved += ShellSectionOnChildRemoved;
                    }
                    else
                    {
                        shellSection.ChildAdded   += ShellSectionOnChildAdded;
                        shellSection.ChildRemoved += ShellSectionOnChildRemoved;
                    }
                }
            }

            base.OnPropertyChanged(propertyName);
        }

        private void ShellSectionOnChildAdded(object sender, ElementEventArgs e)
        {
            if (e.Element is ITransitionAware aware)
            {
                var page = (Page) e.Element;
                MessagingCenter.Subscribe<SharedTransitionShell, SharedTransitionEventArgs> (e.Element, "SendTransitionStarted", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionStarted(args);
                });

                MessagingCenter.Subscribe<SharedTransitionShell, SharedTransitionEventArgs> (e.Element, "SendTransitionEnded", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionEnded(args);
                });

                MessagingCenter.Subscribe<SharedTransitionShell, SharedTransitionEventArgs> (e.Element, "SendTransitionCancelled", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionCancelled(args);
                });
            }
        }

        private void ShellSectionOnChildRemoved(object sender, ElementEventArgs e)
        {
            if (e.Element is Page page)
                TransitionMap.RemoveFromPage(page);

            if (e.Element is ITransitionAware)
            {
                MessagingCenter.Unsubscribe<SharedTransitionShell, SharedTransitionEventArgs>(e.Element, "SendTransitionStarted");
                MessagingCenter.Unsubscribe<SharedTransitionShell, SharedTransitionEventArgs>(e.Element, "SendTransitionEnded");
                MessagingCenter.Unsubscribe<SharedTransitionShell, SharedTransitionEventArgs>(e.Element, "SendTransitionCancelled");
            }
        }
    }
}
