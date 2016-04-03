using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Banana.Common
{
    public class CreateImg
    {

        public static byte[] GetImg(int width = 100, int height = 100, string suffix = ".png")
        {
            suffix = suffix.ToLower();
            Dictionary<string, ImageFormat> imageFormats = new Dictionary<string, ImageFormat> 
            {
                { ".png",ImageFormat.Png },
                { ".jpeg",ImageFormat.Jpeg },
                { ".jpg",ImageFormat.Jpeg },
                { ".bmp",ImageFormat.Bmp },
                { ".gif",ImageFormat.Gif }
            };
            if (!imageFormats.ContainsKey(suffix))
            {
                suffix = ".png";
            }

            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            //SolidBrush solidBrush = new SolidBrush(Color.Green);
            //graphics.FillRegion(solidBrush, new Region());
            graphics.Clear(Color.Green);
            Font font = new Font(FontFamily.GenericMonospace, 10f, FontStyle.Regular);
            LinearGradientBrush linearBrush = new LinearGradientBrush(new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Red, Color.Red, 0f, true);
            graphics.DrawString(width + "*" + height, font, linearBrush, 2, 2);
            bitmap.Save(Environment.CurrentDirectory + "/" + width + "-" + height + suffix, imageFormats[suffix]);
            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, imageFormats[suffix]);
            byte[] data = ms.GetBuffer();
            return data;
        }

        void a()
        {
            GetImg();
        }
    }
}
