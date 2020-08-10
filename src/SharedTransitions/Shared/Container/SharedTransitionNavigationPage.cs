using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Navigation Page with support for shared transitions
    /// </summary>
    /// <seealso cref="Xamarin.Forms.NavigationPage" />
    public class SharedTransitionNavigationPage : NavigationPage, ISharedTransitionContainer
    {
        /// <summary>
        /// Map for all transitions (and support properties) associated with this SharedTransitionNavigationPage
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
        /// The shared transition duration (in ms) associated with the current page in stack
        /// </summary>
        public static readonly BindableProperty TransitionDurationProperty =
            BindableProperty.CreateAttached("TransitionDuration", typeof(long), typeof(SharedTransitionNavigationPage), (long)300);

        public event EventHandler<SharedTransitionEventArgs> TransitionStarted;
        public event EventHandler<SharedTransitionEventArgs> TransitionEnded;
        public event EventHandler<SharedTransitionEventArgs> TransitionCancelled;

        /// <summary>
        /// Gets the transition map
        /// </summary>
        /// <value>
        /// The transition map
        /// </value>
        internal ITransitionMapper TransitionMap
        {
	        get => (ITransitionMapper)GetValue(TransitionMapProperty);
	        set => SetValue(TransitionMapPropertyKey, value);
        }

        ITransitionMapper ISharedTransitionContainer.TransitionMap
        {
	        get => TransitionMap;
	        set => TransitionMap = value;
        }

        public SharedTransitionNavigationPage() : base() => TransitionMap = new TransitionMapper();

        public SharedTransitionNavigationPage(Page root) : base(root) => TransitionMap = new TransitionMapper();

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

        public virtual void OnTransitionStarted(Page pageFrom, Page pageTo, NavOperation navOperation){ }
        public virtual void OnTransitionEnded(Page pageFrom, Page pageTo, NavOperation navOperation){ }
        public virtual void OnTransitionCancelled(Page pageFrom, Page pageTo, NavOperation navOperation){ }


        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionStarted(SharedTransitionEventArgs eventArgs)
        {
            TransitionStarted?.Invoke(this, eventArgs);
            OnTransitionStarted(eventArgs.PageFrom, eventArgs.PageTo, eventArgs.NavOperation);
            MessagingCenter.Send(this, "SendTransitionStarted", eventArgs);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionEnded(SharedTransitionEventArgs eventArgs)
        {
            TransitionEnded?.Invoke(this, eventArgs);
            OnTransitionEnded(eventArgs.PageFrom, eventArgs.PageTo, eventArgs.NavOperation);
            MessagingCenter.Send(this, "SendTransitionEnded", eventArgs);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendTransitionCancelled(SharedTransitionEventArgs eventArgs)
        {
            TransitionCancelled?.Invoke(this, eventArgs);
            OnTransitionCancelled(eventArgs.PageFrom, eventArgs.PageTo, eventArgs.NavOperation);
            MessagingCenter.Send(this, "SendTransitionCancelled", eventArgs);
        }

        protected override void OnChildAdded(Element child)
        {
            if (child is ITransitionAware aware)
            {
                var page = (Page) child;
                MessagingCenter.Subscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs> (child, "SendTransitionStarted", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionStarted(args.PageFrom, args.PageTo, args.NavOperation);
                });

                MessagingCenter.Subscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs> (child, "SendTransitionEnded", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionEnded(args.PageFrom, args.PageTo, args.NavOperation);
                });

                MessagingCenter.Subscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs> (child, "SendTransitionCancelled", (sender, args) =>
                {
                    if (page == args.PageFrom || page == args.PageTo)
                        aware.OnTransitionCancelled(args.PageFrom, args.PageTo, args.NavOperation);
                });
            }

            base.OnChildAdded(child);
        }

        protected override void OnChildRemoved(Element child)
        {
            TransitionMap.RemoveFromPage((Page)child);

            if (child is ITransitionAware)
            {
                MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(child, "SendTransitionStarted");
                MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(child, "SendTransitionEnded");
                MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(child, "SendTransitionCancelled");
            }
        }
    }
}
