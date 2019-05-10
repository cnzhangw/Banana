using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Banana
{

    [Obsolete("推荐使用 HttpHelper ", true)]
    public class HttpClient
    {
        private static LogFactory logger = LogFactory.GetLogger(typeof(HttpClient));

        const string Accept = "text/html, application/xhtml+xml, */*";
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
        const int Timeout = 150000;//超时时间，但前提是请求允许超时才行。
        const string AcceptLanguage = "Accept-Language:zh-CN";
        const string AcceptEncoding = "Accept-Encoding:gzip, deflate";
        const string Contenttype = "application/x-www-form-urlencoded";
        /// <summary>
        /// 静态构造方法
        /// </summary>
        static HttpClient()
        {
            //伪造证书,验证服务器证书回调自动验证
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
            //客户端系统 win7或者winxp上可能会出现 could not create ssl/tls secure channel的问题导致加载验证码报错
            //这里必须设置
            //ServicePointManager.SecurityProtocol=SecurityProtocolType.Ssl3;
            ServicePointManager.DefaultConnectionLimit = 1000;
        }

        public static string Get(string url, CookieContainer cookie, int retryCount = 3)
        {
            return Request(url, cookie, "GET", null, retryCount);
        }

        public static string Post(string url, CookieContainer cookie, string postData, int retryCount = 3, int timeout = 150000)
        {
            return Request(url, cookie, "POST", postData, retryCount, timeout: timeout);
        }
        public static string Post(string url, CookieContainer cookie, string postData, Action<HttpWebRequest> beginRequest, int reTry = 0, int timeout = 150000)
        {
            return Request(url, cookie, "POST", postData, reTry, beginRequest, timeout);
        }

        public static Task<string> PostAnsyc(string url, CookieContainer cookie, string postData, int retryCount = 3, int timeout = 150000)
        {
            return Task<string>.Factory.StartNew(() =>
            {
                try
                {
                    return Request(url, cookie, "POST", postData, timeout: timeout);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw ex;
                }
            });
        }


        private static string Request(string url, CookieContainer cookie, string method, string postData, int retryCount, Action<HttpWebRequest> beginRequest = null, int timeout = 150000)
        {
            string html = string.Empty;
            html = Request(url, cookie, method, postData, beginRequest, timeout);
            return html;
        }
        private static string Request(string url, CookieContainer cookie, string method, string postData, Action<HttpWebRequest> beginRequestHandle = null, int timeout = 150000)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.CookieContainer = cookie;
            request.AllowAutoRedirect = true;
            request.ContentType = Contenttype;
            request.Accept = Accept;
            request.Timeout = timeout;
            request.UserAgent = UserAgent;
            request.Headers.Add(AcceptLanguage);
            request.Headers.Add(AcceptEncoding);

            request.AutomaticDecompression = DecompressionMethods.GZip;

            if (beginRequestHandle != null)
                beginRequestHandle(request);

            if (!string.IsNullOrEmpty(postData))
            {
                byte[] byteRequest = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteRequest.Length;
                var stream = request.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();
            }
            var httpWebResponse = (HttpWebResponse)request.GetResponse();

            var responseStream = httpWebResponse.GetResponseStream();
            if (responseStream == null) return string.Empty;
            if (responseStream.CanTimeout)
            {
                responseStream.ReadTimeout = 15000;
            }
            var encoding = Encoding.GetEncoding(httpWebResponse.CharacterSet ?? "UTF-8");

            var streamReader = new StreamReader(responseStream, encoding);

            string html = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            request.Abort();
            httpWebResponse.Close();
            return html;
        }

        internal static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 伪造证书,总是接受  
            return true;
        }


        public static byte[] GetFile(string url, CookieContainer cookie)
        {
            var webClient = new WebClient();
            byte[] data = webClient.DownloadData(url);
            return data;
        }


#if !NETSTANDARD2_0
        public static System.Drawing.Image GetImage(string url, CookieContainer cookie)
        {
            byte[] data = GetFile(url, cookie);
            //if (url.EndsWith("webp"))
            //{
            //    var s=new Imazen.WebP.SimpleDecoder();
            //    return s.DecodeFromBytes(data, data.Length);
            //}
            System.Drawing.Image image = null;
            using (var ms = new MemoryStream(data))
            {
                image = System.Drawing.Image.FromStream(ms);
            }
            //var ms = new MemoryStream(data);
            //var image = Image.FromStream(ms);
            //ms.Close();
            return image;
        }
#endif

    }
}
