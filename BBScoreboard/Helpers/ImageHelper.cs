using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public static class ImageHelper
    {
        private static readonly Dictionary<string, string> ImageTypes = new Dictionary<string, string>
        {
            {".jpg", "JPEG File"},
            {".jpeg", "JPEG File"},
            {".png", "PNG File"},
            {".gif", "GIF File"},
            {".bmp", "BMP File"},
            {".pcx", "PCX File"},
            {".tiff", "TIFF File"}
        };

        public static bool IsValidImage(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();

            return ImageTypes.ContainsKey(ext);
        }

        public static bool GenerateThumbnail(string source, string thumbnail, int thumbWidth, int thumbHeight, ImageFormat format)
        {
            Image.GetThumbnailImageAbort myCallback =
                        new Image.GetThumbnailImageAbort(ThumbnailCallback);

            // Target Image
            Bitmap destImage = new Bitmap(thumbWidth, thumbHeight);
            destImage.SetResolution(96, 96); // 72, 72

            // Target Quality
            Graphics g = Graphics.FromImage(destImage);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Resize the original
            Bitmap imageBitmap = new Bitmap(source);
            g.DrawImage(imageBitmap, 0, 0, thumbWidth, thumbHeight);
            g.Dispose();

            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 100);          // 100% Percent Compression

            using (var memStream = new MemoryStream())
            {
                destImage.Save(memStream, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);   // jpg format
                destImage.Dispose();

                byte[] matriz = memStream.ToArray();
                using (FileStream fileStream = new FileStream(thumbnail, FileMode.Create, FileAccess.ReadWrite))
                {
                    fileStream.Write(matriz, 0, matriz.Length);
                }
            }


            imageBitmap.Dispose();

            return true;
        }

        public static bool GenerateThumbnail(Image imageBitmap, string sThumbnail, int thumbWidth, int thumbHeight, ImageFormat format)
        {
            Image.GetThumbnailImageAbort myCallback =
                        new Image.GetThumbnailImageAbort(ThumbnailCallback);

            using (Image imageThumbnail = imageBitmap.GetThumbnailImage(thumbWidth, thumbHeight, myCallback, IntPtr.Zero))
            {
                imageThumbnail.Save(sThumbnail, format);
                return true;
            }
        }

        public static bool GenerateThumbnailH(string sourceImage, string thumbnailImage, int height)
        {
            using (Image imageSource = GetImage(sourceImage))
            {
                int iWidth = (int)((double)imageSource.Width * height) / imageSource.Height;
                return GenerateThumbnail(sourceImage, thumbnailImage, iWidth, height, ImageFormat.Jpeg);
            }
        }

        public static bool GenerateThumbnail(string sourceImage, string thumbnailImage, int width, int height)
        {
            using (Image imageSource = GetImage(sourceImage))
            {
                int thumbWidth = width;
                int thumbHeight = height;

                double ratioWidth = (double)imageSource.Width / width;
                double ratioHeight = (double)imageSource.Height / height;

                if (ratioWidth > ratioHeight)
                    thumbHeight = (imageSource.Height * width) / imageSource.Width;
                else
                    thumbWidth = (imageSource.Width * height) / imageSource.Height;

                return GenerateThumbnail(sourceImage, thumbnailImage, thumbWidth, thumbHeight, ImageFormat.Jpeg);
            }
        }

        public static bool GenerateThumbnailW(string sourceImage, string thumbnailImage, int width)
        {
            using (Image imageSource = GetImage(sourceImage))
            {
                int height = (int)((double)imageSource.Height * width) / imageSource.Width; ;
                return GenerateThumbnail(sourceImage, thumbnailImage, width, height, ImageFormat.Jpeg);
            }
        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        public static Image GetImage(string sSource)
        {
            return new Bitmap(sSource);
        }
    }
}
