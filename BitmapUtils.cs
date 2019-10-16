using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RawViewer
{
    static class BitmapUtils
    {
        public static ushort[] BitmapToArray(BitmapSource aBitmap)
        {
            int stride = aBitmap.PixelWidth * (aBitmap.Format.BitsPerPixel / 8);
            ushort[] data = new ushort[stride * aBitmap.PixelHeight / 2];
            aBitmap.CopyPixels(data, stride, 0);
            return data;
        }

        public static BitmapSource AdjustBitmap(BitmapSource aBitmap, double aBrigtness, double aContrast, double aGamma)
        {
            ushort colorMaxValue = ushort.MaxValue & 0xffff;
            ushort[] pixels = BitmapToArray(aBitmap);

            double brShift = aBrigtness * colorMaxValue / 100;
            double conMult = 1;
            if (aContrast > 0)
            {
                conMult = 1.0 + aContrast * 4.0 / 100.0;
            }
            if (aContrast < 0)
            {
                conMult = (100 + aContrast * 3 / 4) / 100.0;
            }

            for (int i = 0; i< pixels.Length; i++)
            {
                double val = pixels[i];
                double y = brShift + colorMaxValue * Math.Pow((val / colorMaxValue), 1.0 / aGamma) * conMult;

                if (y < 0) y = 0;
                if (y > colorMaxValue) y = colorMaxValue;

                pixels[i] = (ushort)y;

                // pixels[i] += (ushort)aBrigtness + (ushort)(colorMaxValue * Math.Pow((pixels[i] / colorMaxValue), aGamma) * conMult);
            }

            int stride = ((int)aBitmap.Width * aBitmap.Format.BitsPerPixel + 7) / 8;
            return BitmapSource.Create(aBitmap.PixelWidth, aBitmap.PixelHeight, aBitmap.DpiX, aBitmap.DpiY, PixelFormats.Gray16, null, pixels, stride);
        }


    }
}
