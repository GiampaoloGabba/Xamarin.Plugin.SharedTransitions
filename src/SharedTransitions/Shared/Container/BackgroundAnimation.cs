namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Background Animation Type
    /// </summary>
    public enum BackgroundAnimation
    {
        /// <summary>
        /// Do not animate.
        /// </summary>
        None = 0,

        /// <summary>
        /// Show a fade animation.
        /// </summary>
        Fade = 1,

        /// <summary>
        /// Show a flip animation.
        /// </summary>
        Flip = 2,

        /// <summary>
        /// Show a slide from left animation.
        /// </summary>
        SlideFromLeft = 3,

        /// <summary>
        /// Show a slide from right animation.
        /// </summary>
        SlideFromRight = 4,

        /// <summary>
        /// Show a slide from top animation.
        /// </summary>
        SlideFromTop = 5,

        /// <summary>
        /// Show a slide from bottom animation.
        /// </summary>
        SlideFromBottom = 6
    }
}
