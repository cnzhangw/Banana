using Banana.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Weixin
{
    public class Weixin
    {
        public static string AppID
        {
            get
            {
                return Config.GetString("weixin.appid");
            }
        }
        private static string AppSecret
        {
            get
            {
                return Config.GetString("weixin.appsecret");
            }
        }
        private static string RedirectUri { get; set; } //获取用户信息的回调页面

        private string access_token { get; set; } //通用的access_token
        private string jsapi_ticket { get; set; }

        private const string access_token_redis = "_at"; //redis缓存中添加进key值标识access_token
        private const string access_token_authorize_redis = "_ata"; //redis缓存中添加进key值标识access_token
        private const string js_api_ticket_redis = "_jat"; //redis缓存中添加进key值标识access_token

        public Weixin() { }

        public static Weixin Builder
        {
            get
            {
                return new Weixin();
            }
        }

        public Weixin SetRediretUrl(string url)
        {
            RedirectUri = url;
            return this;
        }

        /// <summary>
        /// 静默授权-获取用户登录授权页
        /// </summary>
        /// <returns></returns>
        public string GetBaseRedirectUrl(string state = "")
        {
            return GetAuthorizationUrl("base", state);
        }

        /// <summary>
        /// 静默授权-获取用户OpenId
        /// </summary>
        /// <returns></returns>
        public string GetUserOpenId(string code)
        {
            return GetAuthorizationAccessTokenRequest(code).Data.OpenId;
        }


        /// <summary>
        /// 非静默授权-获取用户登录授权页
        /// </summary>
        /// <returns></returns>
        public string GetSeniorRedirectUrl(string state = "")
        {
            return GetAuthorizationUrl("senior", state);
        }

        /// <summary>
        /// 非静默授权-获取用户信息
        /// </summary>
        /// <returns></returns>
        public WeixinUser GetUserInfo(string code)
        {
            var result = GetAuthorizationAccessTokenRequest(code);
            return GetWeixinUser(result.Data.Token, result.Data.OpenId).Data;
        }
        
        /// <summary>
        /// 获取二维码图片
        /// </summary>
        /// <param name="scene"></param>
        public string GetQRCode(string scene)
        {
            GetAccessToken();
            var QRCodeTicket = GetQRCodeTicket(scene);
            return $"https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={QRCodeTicket.Ticket}";
        }

        /// <summary>
        /// 获取调用jsApi需要的参数
        /// </summary>
        public JsApiParam GetJsApiPara(string url)
        {
            GetAccessToken();
            GetJsApiTicket();

            var timeStamp = GetTimestamp().ToString();
            var nonceStr = IDWorker.NextGUID();
            var signature = GetSignature(nonceStr, jsapi_ticket, timeStamp, url);

            return new JsApiParam
            {
                AppId = AppID,
                Timestamp = timeStamp,
                NonceStr = nonceStr,
                Signature = signature
            };
        }

        #region 内部方法

        /// <summary>
        /// 获取通用access_token
        /// </summary>
        public string GetAccessToken()
        {
            access_token = CacheFactory.Cache.Get($"{AppID}{access_token_redis}");
            if (access_token.IsNullOrWhiteSpace())
            {
                GetAccessTokenRequest();
            }

            return access_token;
        }

        private Result<string> GetAccessTokenRequest()
        {
            var result = Result.Create<string>();
            string url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={AppID}&secret={AppSecret}";

            int count = 3;
            do
            {
                count--;
                var httpResult = HttpHelper.Builder.Request(url);
                var reqResult = httpResult.Html.Deserialize<AccessToken>();
                if (reqResult.Token.HasValue())
                {
                    access_token = reqResult.Token;
                    CacheFactory.Cache.Set($"{AppID}{access_token_redis}", reqResult.Token, DateTimeOffset.Now.AddSeconds(7200));
                    result.Succeed(reqResult.Token);
                    break;
                }

                result.Fail($"获取微信用户 access_token 失败，错误码：{reqResult.ErrCode}，错误信息：{reqResult.ErrMsg}");
            } while (count > 0);
            return result;
        }

        /// <summary>
        /// 跳转Url并获取code
        /// </summary>
        private string GetAuthorizationUrl(string type, string state)
        {
            string url = string.Empty;
            switch (type)
            {
                case "senior":
                    url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={AppID}&redirect_uri={RedirectUri}&response_type=code&scope=snsapi_userinfo&state={state}#wechat_redirect";
                    break;
                case "base":
                default:
                    url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={AppID}&redirect_uri={RedirectUri}&response_type=code&scope=snsapi_base&state={state}#wechat_redirect";
                    break;
            }
            return url;
        }

        /// <summary>
        /// 获取专用于网页授权access_token及open_id
        /// </summary>
        /// <param name="code">code</param>
        private string GetAuthorizationAccessToken(string code)
        {
            return GetAuthorizationAccessTokenRequest(code).Data.Token;
        }

        private Result<AuthorizationAccessToken> GetAuthorizationAccessTokenRequest(string code)
        {
            var result = Result.Create<AuthorizationAccessToken>();
            try
            {
                string url = $" https://api.weixin.qq.com/sns/oauth2/access_token?appid={AppID}&secret={AppSecret}&code={code}&grant_type=authorization_code";
                var httpResult = HttpHelper.Builder.Request(url);
                var tokenResult = httpResult.Html.Deserialize<AuthorizationAccessToken>();
                if (tokenResult.ErrCode.HasValue())
                {
                    result.Fail($"获取微信用户 access_token/open_id 失败，错误码：{tokenResult.ErrCode}，错误信息：{tokenResult.ErrMsg}");
                }

                //access_token_authorize = tokenResult.Token;
                //open_id = tokenResult.OpenId;
                result.Succeed(tokenResult);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// 根据access_token以及open_id获取微信用户基本信息
        /// </summary>
        /// <returns></returns>
        private Result<WeixinUser> GetWeixinUser(string authrizeToken, string openId)
        {
            var result = Result.Create<WeixinUser>();
            string url = $"https://api.weixin.qq.com/sns/userinfo?access_token={authrizeToken}&openid={openId}&lang=zh_CN";
            var httpHelper = new HttpHelper();
            var httpResult = HttpHelper.Builder.Request(url);
            var reqResult = httpResult.Html.Deserialize<WeixinUser>();
            if (reqResult.ErrCode.HasValue())
            {
                result.Fail($"获取用户微信基本信息失败!错误码:{reqResult.ErrCode},错误信息:{reqResult.ErrMsg}");
            }
            else
            {
                result.Succeed(reqResult);
            }

            return result;
        }

        /// <summary>
        /// 获取Ticket
        /// </summary>
        /// <param name="scene">自定义场景</param>
        /// <returns></returns>
        private QRCodeResponse GetQRCodeTicket(string scene)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={access_token}";
            QRCodeRequest data = new QRCodeRequest
            {
                ExpireSeconds = "2592000",
                ActionName = "QR_STR_SCENE",
                ActionInfo = new QRCodeInfo
                {
                    Scene = new QRScene
                    {
                        SceneStr = scene
                    }
                }
            };

            var item = new HttpItem()
            {
                Url = url,
                Postdata = data.ToJSON()
            };
            var httpResult = HttpHelper.Builder.Request(item);
            var result = httpResult.Html.Deserialize<QRCodeResponse>();
            if (result.ErrCode.HasValue())
            {
                throw new Exception($"获取微信二维码失败!错误码:{result.ErrCode},错误信息:{result.ErrMsg}");
            }

            return result;
        }

        /// <summary>
        /// 获取JsApiTicket
        /// </summary>
        /// <returns></returns>
        private void GetJsApiTicket()
        {
            jsapi_ticket = CacheFactory.Cache.Get($"{AppID}{js_api_ticket_redis}");
            if (jsapi_ticket.IsNullOrWhiteSpace())
            {
                GetJsApiTicketRequest();
            }
        }

        private void GetJsApiTicketRequest()
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={access_token}&type=jsapi";
            var httpResult = HttpHelper.Builder.Request(url);
            var result = httpResult.Html.Deserialize<JsApiTicket>();
            if (result.ErrCode != "0")
            {
                throw new Exception($"获取JsApiTicket失败!错误码:{result.ErrCode},错误信息:{result.ErrMsg}");
            }

            jsapi_ticket = result.Ticket;
            CacheFactory.Cache.Set($"{AppID}{js_api_ticket_redis}", jsapi_ticket, DateTimeOffset.Now.AddSeconds(7200));
        }

        /// <summary>
        /// 获取jsApi签名
        /// </summary>
        /// <returns></returns>
        private string GetSignature(string noncestr, string jsApiTicket, string timestamp, string url)
        {
            var signatureString = $"jsapi_ticket={jsApiTicket}&noncestr={noncestr}&timestamp={timestamp}&url={url}";
            return Encryption.Encryption.SHA1(signatureString);
        }

        #endregion

        private long GetTimestamp()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

    }
}
