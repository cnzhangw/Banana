using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Banana.Mail
{
    public class MailItem
    {
        public string Subject { get; set; }
        public string Body { get; set; }

        public bool IsBodyHtml { get; set; } = true;

        public Encoding BodyEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 收件人
        /// </summary>
        public List<string> To { get; } = new List<string>();

        /// <summary>
        /// 抄送
        /// </summary>
        public List<string> CC { get; } = new List<string>();

        public List<string> Attachments { get; } = new List<string>();

        public List<string> ReplyTo { get; set; } = new List<string>();

        /// <summary>
        /// 密送
        /// </summary>
        public List<string> BCC { get; set; } = new List<string>();

        /// <summary>
        /// 0-normal 1-low 2-high
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    public class MailConfig
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; } = "smtp.exmail.qq.com";
        public int Port { get; set; } = 0x19;
        public bool Ssl { get; set; } = true;
    }


    /// <summary>
    /// 邮件操作类
    /// </summary>
    public class MailHelper
    {
        Regex regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");

        /// <summary>
        /// 获取Email登陆地址
        /// </summary>
        /// <param name="email">email地址</param>
        /// <returns></returns>
        public string GetEMailLoginUrl(string email)
        {
            if ((email == string.Empty) || (email.IndexOf("@") <= 0))
            {
                return string.Empty;
            }

            int index = email.IndexOf("@");
            email = "http://mail." + email.Substring(index + 1);
            return email;
        }

        readonly MailConfig config = null;
        private MailHelper(MailConfig config)
        {
            if (!regex.IsMatch(config.Address))
            {
                throw new Exception("无效的发件人");
            }

            if (config.Password.IsNullOrWhiteSpace())
            {
                throw new Exception("密码不能为空");
            }

            this.config = config;
        }

        public static MailHelper Build(MailConfig config)
        {
            return new MailHelper(config);
        }

        public Result<object> Send(MailItem mail)
        {
            return Send(mail, mail.To);
        }

        public Result<object> Send(MailItem mail, string to)
        {
            if (to.IsNullOrWhiteSpace())
            {
                return Result.Create().Fail("无收件人");
            }

            return Send(mail, new List<string> { to });
        }

        public Result<object> Send(MailItem mail, List<string> to)
        {
            var result = Result.Create();

            MailMessage message = new MailMessage
            {
                IsBodyHtml = mail.IsBodyHtml,
                Subject = mail.Subject,
                Body = mail.Body,
                From = new MailAddress(config.Address, (config.DisplayName ?? config.Address)),
                Priority = (MailPriority)mail.Priority
            };

            mail.To.AddRange(to);
            mail.To.Distinct(); // 去重

            if (mail.To == null || mail.To.Count == 0)
            {
                return result.Fail("无收件人");
            }

            foreach (var item in mail.To)
            {
                if (regex.IsMatch(item))
                {
                    message.To.Add(item);
                }
            }

            if (mail.ReplyTo.Count > 0)
            {
                foreach (var item in mail.ReplyTo)
                {
                    if (regex.IsMatch(item))
                    {
                        message.ReplyToList.Add(new MailAddress(item));
                    }
                }
            }

            if (mail.CC.Count > 0)
            {
                foreach (var item in mail.CC)
                {
                    if (regex.IsMatch(item))
                    {
                        message.CC.Add(new MailAddress(item));
                    }
                }
            }

            if (mail.BCC.Count > 0)
            {
                foreach (var item in mail.BCC)
                {
                    if (regex.IsMatch(item))
                    {
                        message.Bcc.Add(new MailAddress(item));
                    }
                }
            }

            SmtpClient smtpClient = new SmtpClient
            {
                EnableSsl = config.Ssl,
                UseDefaultCredentials = false
            };
            NetworkCredential credential = new NetworkCredential(config.Address, config.Password);
            smtpClient.Credentials = credential;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Host = config.Host;
            smtpClient.Port = config.Port;
            // 服务器证书验证回调　
            ServicePointManager.ServerCertificateValidationCallback = delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                return true;
            };

            try
            {
                smtpClient.Send(message);
                result.Succeed();
            }
            catch (SmtpFailedRecipientException ex)
            {
                smtpClient.Dispose();
                result.Fail(ex);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        #region 废弃的

        ///// <summary>
        ///// 发送邮件
        ///// </summary>
        ///// <param name="mailSubjct">邮件主题</param>
        ///// <param name="mailBody">邮件正文</param>
        ///// <param name="mailFrom">发送者</param>
        ///// <param name="mailAddress">邮件地址列表</param>
        ///// <param name="HostIP">主机IP</param>
        ///// <returns></returns>
        //public static string SendMail(string mailSubjct, string mailBody, string mailFrom, List<string> mailAddress, string HostIP)
        //{
        //    string str = "";
        //    try
        //    {
        //        MailMessage message = new MailMessage
        //        {
        //            IsBodyHtml = false,
        //            Subject = mailSubjct,
        //            Body = mailBody,
        //            From = new MailAddress(mailFrom)
        //        };
        //        for (int i = 0; i < mailAddress.Count; i++)
        //        {
        //            message.To.Add(mailAddress[i]);
        //        }
        //        new SmtpClient { UseDefaultCredentials = false, DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis, Host = HostIP, Port = (char)0x19 }.Send(message);
        //    }
        //    catch (Exception exception)
        //    {
        //        str = exception.Message;
        //    }
        //    return str;
        //}

        ///// <summary>
        ///// 发送邮件
        ///// </summary>
        ///// <param name="mailSubjct">邮件主题</param>
        ///// <param name="mailBody">邮件正文</param>
        ///// <param name="mailFrom">发送者</param>
        ///// <param name="mailAddress">接收地址列表</param>
        ///// <param name="HostIP">主机IP</param>
        ///// <param name="filename">附件名</param>
        ///// <param name="username">用户名</param>
        ///// <param name="password">密码</param>
        ///// <param name="ssl">加密类型</param>
        ///// <returns></returns>
        //public static string SendMail(string mailSubjct, string mailBody, string mailFrom, List<string> mailAddress, string HostIP, string filename, string username, string password, bool ssl)
        //{
        //    string str = "";
        //    try
        //    {
        //        MailMessage message = new MailMessage
        //        {
        //            IsBodyHtml = false,
        //            Subject = mailSubjct,
        //            Body = mailBody,

        //            From = new MailAddress(mailFrom)
        //        };
        //        for (int i = 0; i < mailAddress.Count; i++)
        //        {
        //            message.To.Add(mailAddress[i]);
        //        }
        //        if (System.IO.File.Exists(filename))
        //        {
        //            message.Attachments.Add(new Attachment(filename));
        //        }
        //        SmtpClient client = new SmtpClient
        //        {
        //            EnableSsl = ssl,
        //            UseDefaultCredentials = false
        //        };
        //        NetworkCredential credential = new NetworkCredential(username, password);
        //        client.Credentials = credential;
        //        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        client.Host = HostIP;
        //        client.Port = 0x19;
        //        client.Send(message);
        //    }
        //    catch (Exception exception)
        //    {
        //        str = exception.Message;
        //    }
        //    return str;
        //}

        ///// <summary>
        ///// 发送邮件（要求登陆）
        ///// </summary>
        ///// <param name="mailSubjct">邮件主题</param>
        ///// <param name="mailBody">邮件正文</param>
        ///// <param name="mailFrom">发送者</param>
        ///// <param name="mailAddress">接收地址列表</param>
        ///// <param name="HostIP">主机IP</param>
        ///// <param name="username">用户名</param>
        ///// <param name="password">密码</param>
        ///// <returns></returns>
        //public static bool SendMail(string mailSubjct, string mailBody, string mailFrom, List<string> mailAddress, string HostIP, string username, string password)
        //{
        //    string str = SendMail(mailSubjct, mailBody, mailFrom, mailAddress, HostIP, 0x19, username, password, false, string.Empty, out bool flag);
        //    return flag;
        //}

        ///// <summary>
        ///// 发送邮件
        ///// </summary>
        ///// <param name="mailSubjct"></param>
        ///// <param name="mailBody"></param>
        ///// <param name="mailFrom"></param>
        ///// <param name="mailAddress"></param>
        ///// <param name="HostIP"></param>
        ///// <param name="port"></param>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        ///// <param name="ssl"></param>
        ///// <param name="replyTo"></param>
        ///// <param name="sendOK"></param>
        ///// <returns></returns>
        //public static string SendMail(string mailSubjct, string mailBody, string mailFrom, List<string> mailAddress, string HostIP, int port, string username, string password, bool ssl, string replyTo, out bool sendOK)
        //{
        //    sendOK = true;
        //    string str = "";
        //    try
        //    {
        //        MailMessage message = new MailMessage
        //        {
        //            IsBodyHtml = false,
        //            Subject = mailSubjct,
        //            Body = mailBody,
        //            From = new MailAddress(mailFrom)
        //        };
        //        if (replyTo != string.Empty)
        //        {
        //            MailAddress address = new MailAddress(replyTo);
        //            //message.ReplyTo = address;
        //            message.ReplyToList.Add(address);
        //        }
        //        Regex regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        //        for (int i = 0; i < mailAddress.Count; i++)
        //        {
        //            if (regex.IsMatch(mailAddress[i]))
        //            {
        //                message.To.Add(mailAddress[i]);
        //            }
        //        }
        //        if (message.To.Count == 0)
        //        {
        //            return string.Empty;
        //        }
        //        SmtpClient client = new SmtpClient
        //        {
        //            EnableSsl = ssl,
        //            UseDefaultCredentials = false
        //        };
        //        NetworkCredential credential = new NetworkCredential(username, password);
        //        client.Credentials = credential;
        //        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        client.Host = HostIP;
        //        client.Port = port;
        //        client.Send(message);
        //    }
        //    catch (Exception exception)
        //    {
        //        str = exception.Message;
        //        sendOK = false;
        //    }
        //    return str;
        //}

        #endregion

    }
}
