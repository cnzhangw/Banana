using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Linq;
using System.Net.Cache;
using System.Drawing;
using System.ComponentModel;

namespace Banana
{
    public class HttpHelper
    {
        // 参考、示例见如下地址
        // http://www.sufeinet.com/thread-3-1-1.html

        #region 预定义方变量  

        //默认的编码  
        private Encoding encoding = Encoding.Default;
        //Post数据编码  
        private Encoding postencoding = Encoding.Default;
        //HttpWebRequest对象用来发起请求  
        private HttpWebRequest request = null;
        //获取影响流的数据对象  
        private HttpWebResponse response = null;
        //设置本地的出口ip和端口  
        private IPEndPoint _IPEndPoint = null;

        #endregion

        public static HttpHelper Builder
        {
            get
            {
                return new HttpHelper();
            }
        }

        #region Public  

        /// <summary>
        /// 默认post发送
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data">指定对象，转为json发送</param>
        /// <returns></returns>
        public HttpResult Request<T>(string url, T data)
        {
            HttpItem item = new HttpItem()
            {
                Url = url,
                Postdata = data.ToJSON()
            };
            return Request(item);
        }

        /// <summary>  
        /// 根据相传入的数据，得到相应页面数据  
        /// </summary>  
        /// <param name="item">参数类对象</param>  
        /// <returns>返回HttpResult类型</returns>  
        private HttpResult GetHTML(HttpItem item)
        {
            //返回参数  
            HttpResult result = new HttpResult();
            try
            {
                //准备参数  
                SetRequest(item);
            }
            catch (Exception ex)
            {
                //配置参数时出错  
                return new HttpResult() { Cookie = string.Empty, Header = null, Html = ex.Message, StatusDescription = "配置参数时出错：" + ex.Message };
            }
            try
            {
                //请求数据  
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = (HttpWebResponse)ex.Response)
                    {
                        GetData(item, result);
                    }
                }
                else
                {
                    result.Html = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            if (item.IsToLower) result.Html = result.Html.ToLower();
            return result;
        }

        /// <summary>
        /// 发起一个get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpResult Request(string url)
        {
            return Request(new HttpItem()
            {
                Url = url,
                Method = "GET",
                ContentType = ContentTypes.HTML
            });
        }

        /// <summary>
        /// 发起一个请求
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public HttpResult Request(HttpItem item)
        {
            //返回参数  
            HttpResult result = new HttpResult();
            try
            {
                //准备参数  
                SetRequest(item);
            }
            catch (Exception ex)
            {
                //配置参数时出错  
                return new HttpResult() { Cookie = string.Empty, Header = null, Html = ex.Message, StatusDescription = "配置参数时出错：" + ex.Message };
            }

            try
            {
                //请求数据  
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = (HttpWebResponse)ex.Response)
                    {
                        GetData(item, result);
                    }
                }
                else
                {
                    result.Html = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }

            if (item.IsToLower)
            {
                result.Html = result.Html.ToLower();
            }

            return result;
        }

        /// <summary>
        /// 发起一个post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="data">string参数</param>
        /// <returns></returns>
        private HttpResult Request(string url, string data)
        {
            var item = new HttpItem()
            {
                Url = url,
                Postdata = data,
                PostDataType = PostDataType.String,
                Method = "POST"
            };
            return Request(item);
        }

        #endregion

        #region GetData  

        /// <summary>  
        /// 获取数据的并解析的方法  
        /// </summary>  
        /// <param name="item"></param>  
        /// <param name="result"></param>  
        private void GetData(HttpItem item, HttpResult result)
        {
            if (response == null)
            {
                return;
            }

            #region base  
            //获取StatusCode  
            result.StatusCode = response.StatusCode;
            //获取StatusDescription  
            result.StatusDescription = response.StatusDescription;
            //获取Headers  
            result.Header = response.Headers;
            //获取最后访问的URl  
            result.ResponseUri = response.ResponseUri.ToString();
            //获取CookieCollection  
            if (response.Cookies != null) result.CookieCollection = response.Cookies;
            //获取set-cookie  
            if (response.Headers["set-cookie"] != null) result.Cookie = response.Headers["set-cookie"];
            #endregion

            #region byte  
            //处理网页Byte  
            byte[] ResponseByte = GetByte();
            #endregion

            #region Html  
            if (ResponseByte != null && ResponseByte.Length > 0)
            {
                //设置编码  
                SetEncoding(item, result, ResponseByte);
                //得到返回的HTML  
                result.Html = encoding.GetString(ResponseByte);
            }
            else
            {
                //没有返回任何Html代码  
                result.Html = string.Empty;
            }
            #endregion
        }

        /// <summary>  
        /// 设置编码  
        /// </summary>  
        /// <param name="item">HttpItem</param>  
        /// <param name="result">HttpResult</param>  
        /// <param name="ResponseByte">byte[]</param>  
        private void SetEncoding(HttpItem item, HttpResult result, byte[] ResponseByte)
        {
            //是否返回Byte类型数据  
            if (item.ResultType == ResultType.Byte) result.ResultByte = ResponseByte;
            //从这里开始我们要无视编码了  
            if (encoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(response.CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                }
            }
        }

        /// <summary>  
        /// 提取网页Byte  
        /// </summary>  
        /// <returns></returns>  
        private byte[] GetByte()
        {
            byte[] ResponseByte = null;
            using (MemoryStream _stream = new MemoryStream())
            {
                //GZIIP处理  
                if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式  
                    new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                }
                else
                {
                    //开始读取流并设置编码方式  
                    response.GetResponseStream().CopyTo(_stream, 10240);
                }
                //获取Byte  
                ResponseByte = _stream.ToArray();
            }
            return ResponseByte;
        }

        #endregion

        #region SetRequest  

        /// <summary>  
        /// 为请求准备参数  
        /// </summary>  
        ///<param name="item">参数列表</param>  
        private void SetRequest(HttpItem item)
        {

            // 验证证书  
            SetCer(item);
            if (item.IPEndPoint != null)
            {
                _IPEndPoint = item.IPEndPoint;
                //设置本地的出口ip和端口  
                request.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(BindIPEndPointCallback);
            }
            //设置Header参数  
            if (item.Header != null && item.Header.Count > 0) foreach (string key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            // 设置代理  
            SetProxy(item);
            if (item.ProtocolVersion != null) request.ProtocolVersion = item.ProtocolVersion;
            request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //请求方式Get或者Post  
            request.Method = item.Method;
            request.Timeout = item.Timeout;
            request.KeepAlive = item.KeepAlive;
            request.ReadWriteTimeout = item.ReadWriteTimeout;
            if (!string.IsNullOrWhiteSpace(item.Host))
            {
                request.Host = item.Host;
            }
            if (item.IfModifiedSince != null) request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            //Accept  
            request.Accept = item.Accept;
            //ContentType返回类型  
            request.ContentType = item.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息  
            request.UserAgent = item.UserAgent;
            // 编码  
            encoding = item.Encoding;
            //设置安全凭证  
            request.Credentials = item.ICredentials;
            //设置Cookie  
            SetCookie(item);
            //来源地址  
            request.Referer = item.Referer;
            //是否执行跳转功能  
            request.AllowAutoRedirect = item.Allowautoredirect;
            if (item.MaximumAutomaticRedirections > 0)
            {
                request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }
            //设置Post数据  
            SetPostData(item);
            //设置最大连接  
            if (item.Connectionlimit > 0) request.ServicePoint.ConnectionLimit = item.Connectionlimit;
        }
        /// <summary>  
        /// 设置证书  
        /// </summary>  
        /// <param name="item"></param>  
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。  
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化对像，并设置请求的URL地址  
                request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
                //将证书添加到请求里  
                request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址  
                request = (HttpWebRequest)WebRequest.Create(item.Url);
                SetCerList(item);
            }
        }
        /// <summary>  
        /// 设置多个证书  
        /// </summary>  
        /// <param name="item"></param>  
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }
        /// <summary>  
        /// 设置Cookie  
        /// </summary>  
        /// <param name="item">Http参数</param>  
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //设置CookieCollection  
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                request.CookieContainer = new CookieContainer();
                if (item.CookieCollection != null && item.CookieCollection.Count > 0)
                    request.CookieContainer.Add(item.CookieCollection);
            }
        }
        /// <summary>  
        /// 设置Post数据  
        /// </summary>  
        /// <param name="item">Http参数</param>  
        private void SetPostData(HttpItem item)
        {
            //验证在得到结果时是否有传入数据  
            if (!request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    postencoding = item.PostEncoding;
                }
                byte[] buffer = null;
                //写入Byte类型  
                if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据  
                    buffer = item.PostdataByte;
                }//写入文件  
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrWhiteSpace(item.Postdata))
                {
                    StreamReader r = new StreamReader(item.Postdata, postencoding);
                    buffer = postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //写入字符串  
                else if (!string.IsNullOrWhiteSpace(item.Postdata))
                {
                    buffer = postencoding.GetBytes(item.Postdata);
                }
                if (buffer != null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }
        /// <summary>  
        /// 设置代理  
        /// </summary>  
        /// <param name="item">参数对象</param>  
        private void SetProxy(HttpItem item)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrWhiteSpace(item.ProxyIp))
            {
                isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrWhiteSpace(item.ProxyIp) && !isIeProxy)
            {
                //设置代理服务器  
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //建议连接  
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //给当前请求对象  
                    request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp, false);
                    //建议连接  
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //给当前请求对象  
                    request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //设置为IE代理  
            }
            else
            {
                request.Proxy = item.WebProxy;
            }
        }

        #endregion

        #region private main  

        /// <summary>  
        /// 回调验证证书问题  
        /// </summary>  
        /// <param name="sender">流对象</param>  
        /// <param name="certificate">证书</param>  
        /// <param name="chain">X509Chain</param>  
        /// <param name="errors">SslPolicyErrors</param>  
        /// <returns>bool</returns>  
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        /// <summary>  
        /// 通过设置这个属性，可以在发出连接的时候绑定客户端发出连接所使用的IP地址。   
        /// </summary>  
        /// <param name="servicePoint"></param>  
        /// <param name="remoteEndPoint"></param>  
        /// <param name="retryCount"></param>  
        /// <returns></returns>  
        private IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            return _IPEndPoint;//端口号  
        }

        #endregion
    }

    #region public calss  

    /****
     * https://www.cnblogs.com/1906859953Lucas/p/9027165.html
     * 
     // 常见浏览器User-Agent大全

Opera
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36 OPR/26.0.1656.60
Opera/8.0 (Windows NT 5.1; U; en)
Mozilla/5.0 (Windows NT 5.1; U; en; rv:1.8.1) Gecko/20061208 Firefox/2.0.0 Opera 9.50
Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en) Opera 9.50

Firefox
Mozilla/5.0 (Windows NT 6.1; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0
Mozilla/5.0 (X11; U; Linux x86_64; zh-CN; rv:1.9.2.10) Gecko/20100922 Ubuntu/10.10 (maverick) Firefox/3.6.10

Safari
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.57.2 (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2

chrome
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36
Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.64 Safari/537.11
Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.133 Safari/534.16

360
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36
Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko


淘宝浏览器
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.11 TaoBrowser/2.0 Safari/536.11

猎豹浏览器
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.71 Safari/537.1 LBBROWSER
Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; LBBROWSER) 
Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; QQDownload 732; .NET4.0C; .NET4.0E; LBBROWSER)"

QQ浏览器
Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; QQBrowser/7.0.3698.400)
Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; QQDownload 732; .NET4.0C; .NET4.0E)

sogou浏览器
Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.84 Safari/535.11 SE 2.X MetaSr 1.0
Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; SV1; QQDownload 732; .NET4.0C; .NET4.0E; SE 2.X MetaSr 1.0)

maxthon浏览器
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Maxthon/4.4.3.4000 Chrome/30.0.1599.101 Safari/537.36

UC浏览器
Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 UBrowser/4.0.3214.0 Safari/537.36


==================== 移动浏览器大全 ====================

IPhone
Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_3_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8J2 Safari/6533.18.5

IPod
Mozilla/5.0 (iPod; U; CPU iPhone OS 4_3_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8J2 Safari/6533.18.5

IPAD
Mozilla/5.0 (iPad; U; CPU OS 4_2_1 like Mac OS X; zh-cn) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8C148 Safari/6533.18.5
Mozilla/5.0 (iPad; U; CPU OS 4_3_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8J2 Safari/6533.18.5

Android
Mozilla/5.0 (Linux; U; Android 2.2.1; zh-cn; HTC_Wildfire_A3333 Build/FRG83D) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1
Mozilla/5.0 (Linux; U; Android 2.3.7; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1

QQ浏览器 Android版本
MQQBrowser/26 Mozilla/5.0 (Linux; U; Android 2.3.7; zh-cn; MB200 Build/GRJ22; CyanogenMod-7) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1

Android Opera Mobile
Opera/9.80 (Android 2.3.4; Linux; Opera Mobi/build-1107180945; U; en-GB) Presto/2.8.149 Version/11.10

Android Pad Moto Xoom
Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13

BlackBerry
Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en) AppleWebKit/534.1+ (KHTML, like Gecko) Version/6.0.0.337 Mobile Safari/534.1+

WebOS HP Touchpad
Mozilla/5.0 (hp-tablet; Linux; hpwOS/3.0.0; U; en-US) AppleWebKit/534.6 (KHTML, like Gecko) wOSBrowser/233.70 Safari/534.6 TouchPad/1.0

Nokia N97
Mozilla/5.0 (SymbianOS/9.4; Series60/5.0 NokiaN97-1/20.0.019; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) BrowserNG/7.1.18124

Windows Phone Mango
Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; HTC; Titan)


UC浏览器
UCWEB7.0.2.37/28/999

NOKIA5700/ UCWEB7.0.2.37/28/999

UCOpenwave
Openwave/ UCWEB7.0.2.37/28/999

UC Opera
Mozilla/4.0 (compatible; MSIE 6.0; ) Opera/UCWEB7.0.2.37/28/999
     * 
     * 
     * */


    /// <summary>  
    /// Http请求参考类  
    /// </summary>  
    public class HttpItem
    {
        /// <summary>  
        /// 请求URL必须填写  
        /// </summary>  
        public string Url { get; set; }

        /// <summary>  
        /// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值  
        /// </summary>  
        public string Method { get; set; } = "POST";

        /// <summary>  
        /// 默认请求超时时间  
        /// </summary>  
        public int Timeout { get; set; } = 100000;

        /// <summary>  
        /// 默认写入Post数据超时间  
        /// </summary>  
        public int ReadWriteTimeout { get; set; } = 30000;

        /// <summary>  
        /// 设置Host的标头信息  
        /// </summary>  
        public string Host { get; set; }

        /// <summary>  
        ///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。  
        /// </summary>  
        public Boolean KeepAlive { get; set; } = true;

        /// <summary>  
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*  
        /// </summary>  
        public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";

        /// <summary>  
        /// 请求返回类型默认 text/html  
        /// </summary>  
        public string ContentType { get; set; } = "application/json"; // "text/html";

        /// <summary>  
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)  
        /// </summary>  
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

        /// <summary>  
        /// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312  
        /// </summary>  
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>  
        /// Post的数据类型  
        /// </summary>  
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>  
        /// Post请求时要发送的字符串Post数据  
        /// </summary>  
        public string Postdata { get; set; }

        /// <summary>  
        /// Post请求时要发送的Byte类型的Post数据  
        /// </summary>  
        public byte[] PostdataByte { get; set; }

        /// <summary>  
        /// Cookie对象集合  
        /// </summary>  
        public CookieCollection CookieCollection { get; set; }
        /// <summary>  
        /// 请求时的Cookie  
        /// </summary>  
        public string Cookie { get; set; }

        /// <summary>  
        /// 来源地址，上次访问地址  
        /// </summary>  
        public string Referer { get; set; }

        /// <summary>  
        /// 证书绝对路径  
        /// </summary>  
        public string CerPath { get; set; }

        /// <summary>  
        /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp  
        /// </summary>  
        public WebProxy WebProxy { get; set; }

        /// <summary>  
        /// 是否设置为全文小写，默认为不转化  
        /// </summary>  
        public Boolean IsToLower { get; set; } = false;

        /// <summary>  
        /// 支持跳转页面，查询结果将是跳转后的页面，默认是不跳转  
        /// </summary>  
        public Boolean Allowautoredirect { get; set; } = false;

        /// <summary>  
        /// 最大连接数  
        /// </summary>  
        public int Connectionlimit { get; set; } = 1024;

        /// <summary>  
        /// 代理Proxy 服务器用户名  
        /// </summary>  
        public string ProxyUserName { get; set; }

        /// <summary>  
        /// 代理 服务器密码  
        /// </summary>  
        public string ProxyPwd { get; set; }

        /// <summary>  
        /// 代理 服务IP,如果要使用IE代理就设置为ieproxy  
        /// </summary>  
        public string ProxyIp { get; set; }

        /// <summary>  
        /// 设置返回类型String和Byte  
        /// </summary>  
        public ResultType ResultType { get; set; } = ResultType.String;

        /// <summary>  
        /// header对象  
        /// </summary>  
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        /// <summary>  
        //     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。  
        /// </summary>  
        public Version ProtocolVersion { get; set; }

        /// <summary>  
        ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。  
        /// </summary>  
        public bool Expect100Continue { get; set; } = false;

        /// <summary>  
        /// 设置509证书集合  
        /// </summary>  
        public X509CertificateCollection ClentCertificates { get; set; }

        /// <summary>  
        /// 设置或获取Post参数编码,默认的为Default编码  
        /// </summary>  
        public Encoding PostEncoding { get; set; }

        /// <summary>  
        /// Cookie返回类型,默认的是只返回字符串类型  
        /// </summary>  
        public ResultCookieType ResultCookieType { get; set; } = ResultCookieType.String;

        /// <summary>  
        /// 获取或设置请求的身份验证信息。  
        /// </summary>  
        public ICredentials ICredentials { get; set; } = CredentialCache.DefaultCredentials;

        /// <summary>  
        /// 设置请求将跟随的重定向的最大数目  
        /// </summary>  
        public int MaximumAutomaticRedirections { get; set; }

        /// <summary>  
        /// 获取和设置IfModifiedSince，默认为当前日期和时间  
        /// </summary>  
        public DateTime? IfModifiedSince { get; set; } = null;

        /// <summary>  
        /// 设置本地的出口ip和端口  
        /// </summary>]  
        /// <example>  
        ///item.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);  
        /// </example>  
        public IPEndPoint IPEndPoint { get; set; } = null;

    }

    /// <summary>  
    /// Http返回参数类  
    /// </summary>  
    public class HttpResult
    {
        /// <summary>  
        /// Http请求返回的Cookie  
        /// </summary>  
        public string Cookie { get; set; }

        /// <summary>  
        /// Cookie对象集合  
        /// </summary>  
        public CookieCollection CookieCollection { get; set; }

        /// <summary>  
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空  
        /// </summary>  
        public string Html { get; set; } = string.Empty;

        /// <summary>  
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空  
        /// </summary>  
        public byte[] ResultByte { get; set; }
        /// <summary>  
        /// header对象  
        /// </summary>  
        public WebHeaderCollection Header { get; set; }
        /// <summary>  
        /// 返回状态说明  
        /// </summary>  
        public string StatusDescription { get; set; }
        /// <summary>  
        /// 返回状态码,默认为OK  
        /// </summary>  
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>  
        /// 最后访问的URl  
        /// </summary>  
        public string ResponseUri { get; set; }
        /// <summary>  
        /// 获取重定向的URl  
        /// </summary>  
        public string RedirectUrl
        {
            get
            {
                try
                {
                    if (Header != null && Header.Count > 0)
                    {
                        if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                        {
                            string baseurl = Header["location"].ToString().Trim();
                            string locationurl = baseurl.ToLower();
                            if (!string.IsNullOrWhiteSpace(locationurl))
                            {
                                bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                if (!b)
                                {
                                    baseurl = new Uri(new Uri(ResponseUri), baseurl).AbsoluteUri;
                                }
                            }
                            return baseurl;
                        }
                    }
                }
                catch { }
                return string.Empty;
            }
        }
    }

    /// <summary>  
    /// 返回类型  
    /// </summary>  
    public enum ResultType
    {
        /// <summary>  
        /// 表示只返回字符串 只有Html有数据  
        /// </summary>  
        String,
        /// <summary>  
        /// 表示返回字符串和字节流 ResultByte和Html都有数据返回  
        /// </summary>  
        Byte
    }

    /// <summary>  
    /// Post的数据格式默认为string  
    /// </summary>  
    public enum PostDataType
    {
        /// <summary>  
        /// 字符串类型，这时编码Encoding可不设置  
        /// </summary>  
        String,
        /// <summary>  
        /// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空  
        /// </summary>  
        Byte,
        /// <summary>  
        /// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值  
        /// </summary>  
        FilePath
    }

    /// <summary>  
    /// Cookie返回类型  
    /// </summary>  
    public enum ResultCookieType
    {
        /// <summary>  
        /// 只返回字符串类型的Cookie  
        /// </summary>  
        String,
        /// <summary>  
        /// CookieCollection格式的Cookie集合同时也返回String类型的cookie  
        /// </summary>  
        CookieCollection
    }

    #endregion

    public class ContentTypes
    {
        public const string X_WWW_FORM_URLENCODED = "application/x-www-form-urlencoded";
        public const string FORM_DATA = "multipart/form-data";
        public const string JSON = "application/json";
        public const string XML = "text/xml";
        public const string HTML = "text/html";
        public const string TEXT = "text/plain";
    }

}


//HttpHelper http = new HttpHelper();
//HttpItem item = new HttpItem()
//{
//    URL = "http://www.sufeinet.com",//URL     必需项
//    Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
//                    //Encoding = Encoding.Default,
//    Method = "get",//URL     可选项 默认为Get
//    Timeout = 100000,//连接超时时间     可选项默认为100000
//    ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
//    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
//    Cookie = "",//字符串Cookie     可选项
//    UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",//用户的浏览器类型，版本，操作系统     可选项有默认值
//    Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
//    ContentType = "text/html",//返回类型    可选项有默认值
//    Referer = "http://www.sufeinet.com",//来源URL     可选项
//    Allowautoredirect = true,//是否根据３０１跳转     可选项
//    CerPath = "d:\\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数
//    Connectionlimit = 1024,//最大连接数     可选项 默认为1024
//    Postdata = "C:\\PERKYSU_20121129150608_ScrubLog.txt",//Post数据     可选项GET时不需要写
//    PostDataType = PostDataType.FilePath,//默认为传入String类型，也可以设置PostDataType.Byte传入Byte类型数据
//    ProxyIp = "192.168.1.105：8015",//代理服务器ID 端口可以直接加到后面以：分开就行了    可选项 不需要代理 时可以不设置这三个参数
//    ProxyPwd = "123456",//代理服务器密码     可选项
//    ProxyUserName = "administrator",//代理服务器账户名     可选项
//    ResultType = ResultType.Byte,//返回数据类型，是Byte还是String
//    PostdataByte = System.Text.Encoding.Default.GetBytes("测试一下"),//如果PostDataType为Byte时要设置本属性的值
//    CookieCollection = new System.Net.CookieCollection(),//可以直接传一个Cookie集合进来
//};
//item.Header.Add("测试Key1", "测试Value1");
//            item.Header.Add("测试Key2", "测试Value2");
//            //得到HTML代码
//            HttpResult result = http.GetHtml(item);
////取出返回的Cookie
//string cookie = result.Cookie;
////返回的Html内容
//string html = result.Html;
//            if (result.StatusCode == System.Net.HttpStatusCode.OK)
//            {
//                //表示访问成功，具体的大家就参考HttpStatusCode类
//            }
//            //表示StatusCode的文字说明与描述
//            string statusCodeDescription = result.StatusDescription;
////把得到的Byte转成图片
//Image img = byteArrayToImage(result.ResultByte);
//        }
//        /// <summary>
//        /// 字节数组生成图片
//        /// </summary>
//        /// <param name="Bytes">字节数组</param>
//        /// <returns>图片</returns>
//   private Image byteArrayToImage(byte[] Bytes)
//{
//    MemoryStream ms = new MemoryStream(Bytes);
//    Image outputImg = Image.FromStream(ms);
//    return outputImg;
//}}