using System;
using UIKit;
using CoreGraphics;
using Xamarin.Forms;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class FormsElementExtensions
    {
        /// <summary>
        /// Get an UIBezierPath based on BoxView CornerRadius property
        /// </summary>
        /// <param name="boxview">Xamarin Forms Boxview</param>
        /// <param name="bounds">Bounds of the native view</param>
        internal static UIBezierPath GetCornersPath(this BoxView boxview, CGRect bounds)
        {
            var cornerRadius = boxview.CornerRadius;
            var topLeft      = (float)cornerRadius.TopLeft;
            var topRight     = (float)cornerRadius.TopRight;
            var bottomLeft   = (float)cornerRadius.BottomLeft;
            var bottomRight  = (float)cornerRadius.BottomRight;

            var bezierPath = new UIBezierPath();
            bezierPath.AddArc(new CGPoint((float)bounds.X + bounds.Width - topRight, (float)bounds.Y + topRight), topRight, (float)(Math.PI * 1.5), (float)Math.PI * 2, true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + bounds.Width - bottomRight, (float)bounds.Y + bounds.Height - bottomRight), bottomRight, 0, (float)(Math.PI * .5), true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + bottomLeft, (float)bounds.Y + bounds.Height - bottomLeft), bottomLeft, (float)(Math.PI * .5), (float)Math.PI, true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + topLeft, (float)bounds.Y + topLeft), (float)topLeft, (float)Math.PI, (float)(Math.PI * 1.5), true);

            return bezierPath;
        }
    }
}