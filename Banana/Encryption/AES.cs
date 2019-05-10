using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Banana.Encryption
{
    /// <summary>
    /// Advanced Encrytion Standard（高级加密标准）
    /// 
    /// 特点：1. 对称加密 2. 一个SK扩展成多个子SK，轮加密
    /// </summary>
    public class AES
    {
        //https://blog.csdn.net/m0_37543627/article/details/71244473


        ////default iv 
        //private readonly static byte[] IV
        //    = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };
        //default key
        private readonly static string _key = "CN@!007#$.89glzgc)IL0x00vV,|3812";

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="value">待加密的字符串</param>
        /// <param name="key">密钥的长度，16位密钥=128位，24位密钥=192位，32位密钥=256位</param>
        /// <param name="iv">[选填]向量，长度必须为16位，默认取密钥前16位</param>
        /// <returns>加密成功返回加密后的字符串,失败返回空字符串</returns> 
        public static string Encrypt(string value, string key, string iv)
        {
            if (value.IsNullOrWhiteSpace()) { return string.Empty; }
            if (key.Length < 16) { throw new Exception("指定的密钥长度不能少于16位。"); }
            if (key.Length > 32) { throw new Exception("指定的密钥长度不能多于32位。"); }
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) { throw new Exception("指定的密钥长度不明确。"); }
            if (iv.HasValue())
            {
                if (iv.Length != 16) { throw new Exception("指定的向量长度必须是16位。"); }
            }
            else
            {
                iv = key.Substring(0, 16);
            }

            // key = key.PadRight(32, ' ').Substring(0, 32);
            RijndaelManaged provider = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes(iv) // IV;
            };
            ICryptoTransform transform = provider.CreateEncryptor();
            byte[] inputBuffer = Encoding.UTF8.GetBytes(value);
            byte[] inArray = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="value">待加密的字符串</param>
        /// <param name="key">密钥的长度，16位密钥=128位，24位密钥=192位，32位密钥=256位</param>
        /// <returns>加密成功返回加密后的字符串,失败返回空字符串</returns> 
        public static string Encrypt(string value, string key)
        {
            return Encrypt(value, key, string.Empty);
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="value">待加密的字符串</param>
        /// <param name="key">密钥的长度，16位密钥=128位，24位密钥=192位，32位密钥=256位</param>
        /// <returns>加密成功返回加密后的字符串,失败返回空字符串</returns> 
        public static string Encrypt(string value)
        {
            return Encrypt(value, _key);
        }


        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="value">待解密的字符串</param>
        /// <param name="key">密钥的长度，16位密钥=128位，24位密钥=192位，32位密钥=256位</param>
        /// <param name="iv">[选填]向量，长度必须为16位，默认取密钥前16位</param>
        /// <returns>解密成功返回解密后的字符串,失败返回空字符串</returns>
        public static string Decrypt(string value, string key, string iv)
        {
            if (value.IsNullOrWhiteSpace()) { return string.Empty; }
            if (key.Length < 16) { throw new Exception("指定的密钥长度不能少于16位。"); }
            if (key.Length > 32) { throw new Exception("指定的密钥长度不能多于32位。"); }
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) { throw new Exception("指定的密钥长度不明确。"); }
            if (iv.HasValue())
            {
                if (iv.Length != 16) { throw new Exception("指定的向量长度必须是16位。"); }
            }
            else
            {
                iv = key.Substring(0, 16);
            }

            RijndaelManaged provider = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes(key) // IV;
            };
            ICryptoTransform transform = provider.CreateDecryptor();
            byte[] inputBuffer = Convert.FromBase64String(value);
            byte[] bytes = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="value">待解密的字符串</param>
        /// <param name="key">密钥的长度，16位密钥=128位，24位密钥=192位，32位密钥=256位</param>
        /// <returns>解密成功返回解密后的字符串,失败返回空字符串</returns>
        public static string Decrypt(string input, string key)
        {
            return Decrypt(input, key, string.Empty);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="value">待解密的字符串</param>
        /// <returns>解密成功返回解密后的字符串,失败返回空字符串</returns>
        public static string Decrypt(string input)
        {
            return Decrypt(input, _key);
        }
    }
}
