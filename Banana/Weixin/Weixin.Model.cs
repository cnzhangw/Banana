using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Weixin
{
    /// <summary>
    /// 错误信息
    /// </summary>
    public abstract class ErrorModel
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("errcode")]
        public string ErrCode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonProperty("errmsg")]
        public string ErrMsg { get; set; }
    }

    /// <summary>
    /// 通用 access_token
    /// </summary>
    public class AccessToken : ErrorModel
    {
        /// <summary>
        /// 获取到的凭证
        /// </summary>
        [JsonProperty("access_token")]
        public string Token { get; set; }
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    #region 网页授权

    /// <summary>
    /// 网页授权 access_token
    /// </summary>
    public class AuthorizationAccessToken : ErrorModel
    {
        /// <summary>
        /// 网页授权接口调用凭证,注意：此access_token与基础支持的access_token不同
        /// </summary>
        [JsonProperty("access_token")]
        public string Token { get; set; }

        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// 用户唯一标识
        /// </summary>
        [JsonProperty("openid")]
        public string OpenId { get; set; }

        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    /// <summary>
    /// 微信用户信息
    /// </summary>
    public class WeixinUser : ErrorModel
    {
        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        [JsonProperty("openid")]
        public string OpenId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// 用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>
        [JsonProperty("sex")]
        public int Sex { get; set; }

        /// <summary>
        /// 用户个人资料填写的省份
        /// </summary>
        [JsonProperty("province")]
        public string Province { get; set; }

        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// 国家，如中国为CN
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// 用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空。若用户更换头像，原有头像URL将失效。
        /// </summary>
        [JsonProperty("headimgurl")]
        public string HeadimgUrl { get; set; }

        /// <summary>
        /// 用户特权信息，json 数组，如微信沃卡用户为（chinaunicom）
        /// </summary>
        [JsonProperty("privilege")]
        public string[] Privilege { get; set; }

        /// <summary>
        /// 只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。
        /// </summary>
        [JsonProperty("unionid")]
        public string UnionId { get; set; }
    }

    #endregion

    #region 带参二维码

    /// <summary>
    /// 请求二维码
    /// </summary>
    public class QRCodeRequest
    {
        /// <summary>
        /// 该二维码有效时间，以秒为单位。 最大不超过2592000（即30天），此字段如果不填，则默认有效期为30秒。(永久二维码无需此参数)
        /// </summary>
        [JsonProperty("expire_seconds")]
        public string ExpireSeconds { get; set; }

        /// <summary>
        /// 二维码类型，QR_SCENE为临时的整型参数值，QR_STR_SCENE为临时的字符串参数值，QR_LIMIT_SCENE为永久的整型参数值，QR_LIMIT_STR_SCENE为永久的字符串参数值
        /// </summary>
        [JsonProperty("action_name")]
        public string ActionName { get; set; }

        /// <summary>
        /// 二维码详细信息
        /// </summary>
        [JsonProperty("action_info")]
        public QRCodeInfo ActionInfo { get; set; }
    }

    public class QRCodeInfo
    {
        /// <summary>
        /// 二维码场景值
        /// </summary>
        [JsonProperty("scene")]
        public QRScene Scene { get; set; }
    }

    public class QRScene
    {
        /// <summary>
        /// 场景值ID，临时二维码时为32位非0整型，永久二维码时最大值为100000（目前参数只支持1--100000）
        /// </summary>
        [JsonProperty("scene_id")]
        public string SceneId { get; set; }

        /// <summary>
        /// 场景值ID（字符串形式的ID），字符串类型，长度限制为1到64
        /// </summary>
        [JsonProperty("scene_str")]
        public string SceneStr { get; set; }
    }

    /// <summary>
    /// 请求二维码回应
    /// </summary>
    public class QRCodeResponse : ErrorModel
    {
        /// <summary>
        /// 获取的二维码ticket，凭借此ticket可以在有效时间内换取二维码。
        /// </summary>
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        /// <summary>
        /// 该二维码有效时间，以秒为单位。 最大不超过2592000（即30天）。
        /// </summary>
        [JsonProperty("expire_seconds")]
        public string ExpireSeconds { get; set; }

        /// <summary>
        /// 二维码图片解析后的地址，开发者可根据该地址自行生成需要的二维码图片
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    #endregion

    #region JSAPI相关

    /// <summary>
    /// 获取jsapi_ticket
    /// </summary>
    public class JsApiTicket : ErrorModel
    {
        /// <summary>
        /// 公众号用于调用微信JS接口的临时票据
        /// </summary>
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }

    /// <summary>
    /// 调用jsApi需要的参数
    /// </summary>
    public class JsApiParam
    {
        /// <summary>
        /// appid
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// 签名-随机字符串
        /// </summary>
        public string NonceStr { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
    }

    #endregion


}
