using Banana.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace WebUI
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler, IRequiresSessionState
    {
        private static Random random = new Random();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "image/jpeg";

            Image image = new Bitmap(60, 30);

            //生成随机数
            int code = random.Next(1000, 10000);
            string codeString = code.ToString();

            //使用会话状态
            context.Session["Code"] = codeString;

            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.WhiteSmoke);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(codeString, new Font("Arial", 14), Brushes.Blue, new RectangleF(0, 0, image.Width, image.Height), sf);
            }

            context.Response.ContentType = "image/jpeg";
            image.Save(context.Response.OutputStream, ImageFormat.Jpeg);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}