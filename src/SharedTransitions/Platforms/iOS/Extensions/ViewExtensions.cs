using System;
using Foundation;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using ObjCRuntime;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Color = Xamarin.Forms.Color;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class ViewExtensions
    {
        /*
         * IMPORTANT!
         *
         * GetImageFrame:
         * Get the Frame based on the real image size, not his containing frame!
         * This is needed to avoid deformations with image aspectfit
         * where the container frame can a have different size from the contained image
         *
         * Boxview snapshot:
         * cant get the UIBezierPath created in the Draw override, so i rebuild one
         *
         * GradientLayer resize animation:
         * GradientLayers doesnt change their bounds during animation
         * We fix this with a custom UIView as subview that change his main layer as CAGradientLayer
         */

        /// <summary>
        /// Get the Frame based on the real image size, not his containing frame. Useful for images with AspectFit
        /// </summary>
        /// <param name="imageView">Input Image</param>
        /// <returns></returns>
        internal static CGRect GetImageFrame(this UIImageView imageView)
        {
            //We dont need these calculations from *Fill methods
            if (imageView.ContentMode == UIViewContentMode.ScaleAspectFit && imageView.Image != null)
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

        /// <summary>
        /// Copy a view
        /// </summary>
        /// <param name="fromView">View to copy</param>
        /// <param name="softCopy">Get only background & mask and put them in the main layer that will be animated</param>
        internal static UIView CopyView(this UIView fromView, bool softCopy = false)
        {
            //TODO: More work on mask, border, shape, ecc... BUT is a bit out of scope, we dont reallly want to create a full framework for transition-shaping...
            //TODO: its superdifficult because every layout/plugin handle layers and subviews in different ways

            if (!softCopy)
            {
                //Fix for collectionview item selection inside shell
	            if (fromView is UIImageView image)
                    image.Highlighted = false;

                return (UIView) NSKeyedUnarchiver.UnarchiveObject(NSKeyedArchiver.ArchivedDataWithRootObject(fromView));
            }

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
                //Try to get background and mask. If we dont get something we try to traverse sublayers hierarchy
                if (fromView.Layer.Mask != null && fromView.Layer.Mask is CAShapeLayer shapedMask)
                    fromViewSnapshot.Layer.Mask = shapedMask.CopyToMask();

                if (fromView.Layer.BackgroundColor!=null && fromView.Layer.BackgroundColor.Alpha > 0)
                    fromViewSnapshot.Layer.BackgroundColor = fromView.Layer.BackgroundColor;
                
                if (fromViewSnapshot.Layer.Mask == null || 
                    (!fromViewSnapshot.Layer.HasBackground() && fromViewSnapshot.BackgroundColor == null))
                    fromViewSnapshot.SetBgAndMaskFromLayerHierarchy(fromView.Layer);
            }

            return fromViewSnapshot;
        }

        /// <summary>
        /// Update the Background and Mask on main Layer traversing the sublayers
        /// </summary>
        /// <param name="fromViewSnapshot">View to update</param>
        /// <param name="fromLayer">Starting layer to traverse</param>
        internal static void SetBgAndMaskFromLayerHierarchy(this UIView fromViewSnapshot, CALayer fromLayer)
        {
            if (fromLayer.Sublayers != null)
            {
                foreach (CALayer sublayer in fromLayer.Sublayers)
                {
                    //try to get the right background specified
                    if (sublayer is CAGradientLayer subGradientLayer && subGradientLayer.Colors != null)
                    {
                        var gradientView = new UIGradientView();

                        var gradientLayer = (CAGradientLayer) gradientView.Layer;
                        gradientLayer.Colors     = subGradientLayer.Colors;
                        gradientLayer.StartPoint = subGradientLayer.StartPoint;
                        gradientLayer.EndPoint   = subGradientLayer.EndPoint;
                        gradientLayer.Locations  = subGradientLayer.Locations;

                        fromViewSnapshot.AddSubview(gradientView);
                    } 
                    else if (sublayer.HasBackground())
                    {
                        fromViewSnapshot.Layer.BackgroundColor = sublayer.BackgroundColor;
                    }

                    if (sublayer.Mask != null && sublayer.Mask is CAShapeLayer shapedMask)
                        fromViewSnapshot.Layer.Mask = shapedMask.CopyToMask();

                    //Get shapes to create the mask
                    if (sublayer is CAShapeLayer subShapeLayer)
                    {
                        //no mask yet? Take the path!
                        if (fromViewSnapshot.Layer.Mask == null && subShapeLayer.Path != null)
                            fromViewSnapshot.Layer.Mask = subShapeLayer.CopyToMask();

                        //no background yet? Take the fill!
                        if (!fromViewSnapshot.Layer.HasBackground() && subShapeLayer.FillColor != null)
                            fromViewSnapshot.Layer.BackgroundColor = subShapeLayer.FillColor;
                    }

                    fromViewSnapshot.SetBgAndMaskFromLayerHierarchy(sublayer);
                }
            }
        }
    }

    /*
     * IMPORTANT:
     * During animations, GradientLayers doesnt change their bounds based on parent UIView
     * To fix this, we create a customview with a CAGradientLayer as his main layer
     * and then put this new view inside our snapshot view
     * This is important because:
     * 1) Main Layer get always resized to match their parent view
     * 2) Subviews can math parent size automatically with AutoresizingMask
     *
     * I tried everything with CAGradientLayers: CABasicAnimation, subclassing mainview
     * and override LayoutIfNeeded, Bounds, Frame, ecc... But the results where superchoppy or plain wrong
     * So far this is the best method with fluid fluid performance and perfecr animation i found
     */

    /// <summary>
    /// Custom UIView with a CAGradientLayer as his main Layer
    /// </summary>
    internal sealed class UIGradientView : UIView
    {
        [Export ("layerClass")]
        public static Class LayerClass ()
        {
            return new Class (typeof(CAGradientLayer));
        }

        public UIGradientView()
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        }
    }
}
