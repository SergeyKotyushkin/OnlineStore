using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OnlineStore.BuisnessLogic.ImageService.Contracts;

namespace OnlineStore.BuisnessLogic.ImageService
{
    public class ImageServiceAgent : IImageService
    {
        public Image ByteArrayToImage(byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        public byte[] ImageToByteArray(Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public Size GetSize(Size size, int bound)
        {
            var newSize = new Size();

            if (size.Width >= size.Height)
            {
                newSize.Width = bound;
                newSize.Height = bound * size.Height / size.Width;
            }
            else
            {
                newSize.Height = bound;
                newSize.Width = bound * size.Width / size.Height;
            }

            return newSize;
        } 
    }
}