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
         * Get the snapshot based on the real image size, not his containing frame!
         * This is needed to avoid deformations with image aspectfit
         * where the container frame can a have different size from the contained image
         */
        internal static CGRect GetImageFrame(this UIImageView imageView)
        {
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
                    return new CGRect(imageView.Frame.X + marginLeft, imageView.Frame.Y + marginTop, newWidth,
                        newHeight);
                }
            }
            return new CGRect(imageView.Frame.X, imageView.Frame.Y, imageView.Frame.Width, imageView.Frame.Height);
        }

        //Copy a view
        //For softcopy for now i'm fine to get only the background and put it in the main layer that will be animated
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
            fromViewSnapshot.Layer.CornerRadius    = fromView.Layer.CornerRadius;
            fromViewSnapshot.Layer.MasksToBounds   = fromView.Layer.MasksToBounds;
            fromViewSnapshot.Layer.BorderWidth     = fromView.Layer.BorderWidth ;
            fromViewSnapshot.Layer.BorderColor     = fromView.Layer.BorderColor;
            fromViewSnapshot.Layer.BackgroundColor = fromView.Layer.BackgroundColor ?? fromView.BackgroundColor?.CGColor ?? Color.White.ToCGColor();
            fromViewSnapshot.Layer.Mask            = fromView.Layer.Mask;

            //lets deep more on layers!
            fromViewSnapshot.SetPropertiesFromLayer(fromView.Layer);

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
                            fromViewSnapshot.Layer.Mask = CreateMask(subShapeLayer.Path, subShapeLayer.Bounds);
                        }

                        if (subShapeLayer.FillColor != null)
                        {
                            fromViewSnapshot.Layer.BackgroundColor = subShapeLayer.FillColor;
                        }
                        
                    }
                    else if (sublayer is CAGradientLayer subGradientLayer && subGradientLayer.Colors != null)
                    {
                        //just the first color...
                        fromViewSnapshot.Layer.BackgroundColor = subGradientLayer.Colors[0];
                    }
                    else
                    {
                        fromViewSnapshot.SetPropertiesFromLayer(sublayer);
                    }
                }
            }
        }

        //Traverse sublayers to get the mask! Not ideal but better than nothing for animate corners :)
        internal static CAShapeLayer GetMask(this CALayer fromLayer, CGRect newBounds)
        {
            if (fromLayer.Mask != null && fromLayer.Mask is CAShapeLayer shapeLayer)
            {
                return CreateMask(shapeLayer.Path, shapeLayer.Bounds);
            }

            if (fromLayer.Sublayers != null)
            {
                foreach (CALayer sublayer in fromLayer.Sublayers)
                {
                    if (sublayer.Mask != null && fromLayer.Mask is CAShapeLayer shapeSubLayer)
                        return CreateMask(shapeSubLayer.Path, shapeSubLayer.Bounds);
                    else
                        return sublayer.GetMask(newBounds);
                }
            }
            return null;
        }

        internal static CAShapeLayer CreateMask(CGPath path, CGRect bounds)
        {
            return new CAShapeLayer
            {
                Frame = bounds,
                Path  = path
            };
        }
    }
}