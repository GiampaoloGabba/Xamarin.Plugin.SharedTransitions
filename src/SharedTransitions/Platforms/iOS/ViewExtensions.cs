using System;
using CoreGraphics;
using UIKit;

namespace Plugin.SharedTransitions.Platforms.iOS
{
    internal static class ViewExtensions
    {
        internal static CGRect GetImageFrameWithAspectRatio(this UIImageView imageView)
        {
            nfloat aspect = imageView.Image.Size.Width / imageView.Image.Size.Height;

            //calculate new dimensions based on aspect ratio
            nfloat newWidth   = imageView.Frame.Width * aspect;
            nfloat newHeight  = newWidth / aspect;
            nfloat marginTop  = 0;
            nfloat marginLeft = 0;

            if (newWidth > imageView.Frame.Width || newHeight > imageView.Frame.Height)
            {
                //depending on which of the two exceeds the box dimensions set it as the box dimension
                //and calculate the other one based on the aspect ratio
                if (newWidth > newHeight)
                {
                    newWidth  = imageView.Frame.Width;
                    newHeight = newWidth / aspect;
                    marginTop = (imageView.Frame.Height - newHeight) / 2;
                }
                else
                {
                    newHeight  = imageView.Frame.Height;
                    newWidth   = (int)(newHeight * aspect);
                    marginLeft = (imageView.Frame.Width - newWidth) / 2;
                }
            }

            return new CGRect(imageView.Frame.X, imageView.Frame.Y + marginTop, newWidth + marginLeft, newHeight);
        }
    }
}