using CoreGraphics;
using CoreAnimation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class LayerExtensions
    {
        /// <summary>
        /// Traverse sublayers to get the mask created by a CAShaperLayer or UIBezierPath
        /// </summary>
        /// <param name="fromLayer">Starting layer</param>
        /// <param name="newBounds">New bounds to apply to the mask</param>
        /// <remarks>Not ideal but better than nothing for animate corners :)</remarks>
        internal static CAShapeLayer GetMask(this CALayer fromLayer, CGRect newBounds)
        {
            //Fix for boxRenderer
            if (fromLayer.Delegate is VisualElementRenderer<BoxView> fromBoxRenderer)
            {
                var bezierPath = fromBoxRenderer.Element.GetCornersPath(fromBoxRenderer.Bounds);
                return new CAShapeLayer
                {
                    Frame = bezierPath.Bounds,
                    Path  = bezierPath.CGPath
                };
            }
            if (fromLayer.Mask != null && fromLayer.Mask is CAShapeLayer shapeLayer)
            {
                return shapeLayer.CopyToMask();
            }
            if (fromLayer.Sublayers != null)
            {
                foreach (CALayer sublayer in fromLayer.Sublayers)
                {
                    if (sublayer.Mask != null && fromLayer.Mask is CAShapeLayer shapeSubLayer)
                        return shapeSubLayer.CopyToMask();
                    
                    return sublayer.GetMask(newBounds);
                }
            }
            return null;
        }

        /// <summary>
        /// Copy the shape to a new layer to create a brand new mask
        /// </summary>
        /// <param name="shapeLayer">Layer to copy</param>
        /// <returns></returns>
        internal static CAShapeLayer CopyToMask(this CAShapeLayer shapeLayer)
        {
            return new CAShapeLayer
            {
                Frame = shapeLayer.Bounds,
                Path  = shapeLayer.Path
            };
        }

        /// <summary>
        /// Check if this layer has a valid, visible background
        /// </summary>
        /// <param name="layer">Layer to check</param>
        /// <returns></returns>
        internal static bool HasBackground(this CALayer layer)
        {
            return layer.BackgroundColor != null && layer.BackgroundColor.Alpha > 0;
        }
    }
}