using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Foundation;
using ImageSearch.Contract;
using UIKit;

namespace ImageSearch.iOS
{
    public class ImageTools : IImageTools
    {
        //thanks to Chris Honselaar https://forums.xamarin.com/discussion/4170/resize-images-and-save-thumbnails
        public byte[] MaxResizeImage(byte[] source, float maxWidth, float maxHeight)
        {
            var sourceImage = UIImage.LoadFromData(NSData.FromArray(source));

            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage.AsJPEG().ToArray();
            var width = Convert.ToInt64(maxResizeFactor*sourceSize.Width);
            var height = Convert.ToInt64(maxResizeFactor * sourceSize.Height);
            UIGraphics.BeginImageContext(new SizeF(width, height));
            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage.AsJPEG().ToArray();
        }

    }
}
