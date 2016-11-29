using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace HtmlTools
{
    public static class ImageHelper
    {
        private static IVsImageService2 _imageService;

        static ImageHelper()
        {
            _imageService = Package.GetGlobalService(typeof(SVsImageService)) as IVsImageService2;
        }

        public static BitmapSource GetImage(ImageMoniker moniker)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags;
            imageAttributes.ImageType = (uint)_UIImageType.IT_Bitmap;
            imageAttributes.Format = (uint)_UIDataFormat.DF_WPF;
            imageAttributes.LogicalHeight = 16;
            imageAttributes.LogicalWidth = 16;
            imageAttributes.StructSize = Marshal.SizeOf(typeof(ImageAttributes));

            IVsUIObject result = _imageService.GetImage(moniker, imageAttributes);

            Object data;
            result.get_Data(out data);

            if (data == null)
                return null;

            return data as BitmapSource;
        }
    }
}