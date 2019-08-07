using System;
using CoreGraphics;
using UIKit;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class ViewExtensions
    {
        internal static CGRect GetImageFrame(this UIImageView imageView)
        {
 
            nfloat imageAspect = imageView.Image.Size.Width / imageView.Image.Size.Height;
            nfloat boundslAspect = imageView.Frame.Size.Width / imageView.Frame.Size.Height;

            if (imageAspect != boundslAspect && imageView.ContentMode == UIViewContentMode.ScaleAspectFit)
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
                        newWidth   = (int)(newHeight * imageAspect);
                        marginLeft = (imageView.Frame.Width - newWidth) / 2;
                    }
                }

                return new CGRect(imageView.Frame.X + marginLeft, imageView.Frame.Y + marginTop, newWidth, newHeight);
            }

            return new CGRect(imageView.Frame.X, imageView.Frame.Y, imageView.Frame.Width, imageView.Frame.Height);

        }

    }
}