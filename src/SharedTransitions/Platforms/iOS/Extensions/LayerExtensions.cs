using CoreGraphics;
using CoreAnimation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class LayerExtensions
    {
        //Traverse sublayers to get the mask! Not ideal but better than nothing for animate corners :)
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
                    else
                        return sublayer.GetMask(newBounds);
                }
            }
            return null;
        }

        internal static CAShapeLayer CopyToMask(this CAShapeLayer shapeLayer)
        {
            return new CAShapeLayer
            {
                Frame = shapeLayer.Bounds,
                Path  = shapeLayer.Path
            };
        }
    }
}