using System;
using Foundation;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class ViewExtensions
    {
        /*
         * IMPORTANT!
         *
         * Boxview snapshot:
         * cant get the UIBezierPath created in the Draw override, so i rebuild one
         *
         * GetImageFrame:
         * Get the snapshot based on the real image size, not his containing frame!
         * This is needed to avoid deformations with image aspectfit
         * where the container frame can a have different size from the contained image
         */
        internal static CGRect GetImageFrame(this UIImageView imageView)
        {
            //We dont need these calculations from *Fill methods
            if (imageView.ContentMode == UIViewContentMode.ScaleAspectFit)
            {
                nfloat imageAspect   = imageView.Image.Size.Width / imageView.Image.Size.Height;
                nfloat boundslAspect = imageView.Frame.Size.Width / imageView.Frame.Size.Height;

                if (imageAspect != boundslAspect)
                {
                    //calculate new dimensions based on aspect ratio
                    nfloat newWidth   = imageView.Frame.Width * imageAspect;
                    nfloat newHeight  = newWidth / imageAspect;
                    nfloat marginTop  = 0;
                    nfloat marginLeft = 0;

                    if (newWidth > imageView.Frame.Width || newHeight > imageView.Frame.Height)
                    {
                        //depending on which of the two exceeds the box dimensions set it as the box dimension
                        //and calculate the other one based on the aspect ratio
                        if (newWidth > newHeight)
                        {
                            newWidth  = imageView.Frame.Width;
                            newHeight = newWidth / imageAspect;
                            marginTop = (imageView.Frame.Height - newHeight) / 2;
                        }
                        else
                        {
                            newHeight  = imageView.Frame.Height;
                            newWidth   = (int) (newHeight * imageAspect);
                            marginLeft = (imageView.Frame.Width - newWidth) / 2;
                        }
                    }
                    return new CGRect(imageView.Frame.X + marginLeft, imageView.Frame.Y + marginTop, newWidth, newHeight);
                }
            }
            return new CGRect(imageView.Frame.X, imageView.Frame.Y, imageView.Frame.Width, imageView.Frame.Height);
        }

        //Copy a view
        //For softcopy for now i'm fine to get only background & mask and put them in the main layer that will be animated
        //TODO: Moar work on mask, border, shape, ecc... BUT is a bit out of scope, we dont reallly want to
        //create a franework for transition-shaping... its superdifficult because every layout/plugin is different from another
        internal static UIView CopyView(this UIView fromView, bool softCopy = false)
        {
            if (!softCopy)
                return (UIView) NSKeyedUnarchiver.UnarchiveObject(NSKeyedArchiver.ArchivedDataWithRootObject(fromView));

            var fromViewSnapshot = new UIView
            {
                AutoresizingMask = fromView.AutoresizingMask,
                ContentMode      = fromView.ContentMode,
                Alpha            = fromView.Alpha,
                BackgroundColor  = fromView.BackgroundColor,
                LayoutMargins    = fromView.LayoutMargins
            };

            //properties from standard xforms controls
            fromViewSnapshot.Layer.CornerRadius  = fromView.Layer.CornerRadius;
            fromViewSnapshot.Layer.MasksToBounds = fromView.Layer.MasksToBounds;
            fromViewSnapshot.Layer.BorderWidth   = fromView.Layer.BorderWidth ;
            fromViewSnapshot.Layer.BorderColor   = fromView.Layer.BorderColor;
            fromViewSnapshot.Layer.Mask          = fromView.Layer.Mask;

            if (fromView is VisualElementRenderer<BoxView> fromBoxRenderer)
            {
                /*
                 * IMPORTANT:
                 *
                 * OK, lets admin i'm a noob and i cant figure how to take bounds and background for a boxview
                 * Probably because everything is set in the ondraw method of the renderer (UIBezierPath with fill)
                 * I dont like to take a simple raster snapshot cause here i dont know if the destination view
                 * has different corner radius. So for the sake of good transitions lets rebuild that bezier path!
                 */
                var bezierPath = fromBoxRenderer.Element.GetCornersPath(fromBoxRenderer.Bounds);
                fromViewSnapshot.Layer.BackgroundColor = fromBoxRenderer.Element.BackgroundColor.ToCGColor() ?? Color.Default.ToCGColor();

                fromViewSnapshot.Layer.Mask = new CAShapeLayer
                {
                    Frame = bezierPath.Bounds,
                    Path  = bezierPath.CGPath
                };
            }
            else
            {
                fromViewSnapshot.Layer.BackgroundColor = fromView.Layer.BackgroundColor ?? fromView.BackgroundColor?.CGColor ?? Color.Default.ToCGColor();
                //lets deep more on layers!
                fromViewSnapshot.SetPropertiesFromLayer(fromView.Layer);
            }

            return fromViewSnapshot;
        }

        //Get the properties we like!
        internal static void SetPropertiesFromLayer(this UIView fromViewSnapshot, CALayer fromLayer)
        {
            if (fromLayer.Sublayers != null)
            {
                foreach (CALayer sublayer in fromLayer.Sublayers)
                {
                    if (sublayer.BackgroundColor != null && sublayer.BackgroundColor.Alpha > 0)
                    {
                        fromViewSnapshot.Layer.Frame           = sublayer.Bounds;
                        fromViewSnapshot.Layer.MasksToBounds   = true;
                        fromViewSnapshot.Layer.BackgroundColor = sublayer.BackgroundColor;
                    }
                    if (sublayer is CAShapeLayer subShapeLayer)
                    {
                        if (subShapeLayer.Path != null)
                        {
                            fromViewSnapshot.Layer.Mask = subShapeLayer.CopyToMask();
                        }

                        if (subShapeLayer.FillColor != null)
                        {
                            fromViewSnapshot.Layer.BackgroundColor = subShapeLayer.FillColor;
                        }
                    }
                    else if (sublayer is CAGradientLayer subGradientLayer && subGradientLayer.Colors != null)
                    {
                        //just the first color for now...
                        fromViewSnapshot.Layer.BackgroundColor = subGradientLayer.Colors[0];
                    }
                    else
                    {
                        fromViewSnapshot.SetPropertiesFromLayer(sublayer);
                    }
                }
            }
        }
    }
}