using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Banana.Encryption
{
    /// <summary>
    /// Data Encrytion Standard（数据加密标准）
    /// 
    /// 运算速度快
    /// 对应算法是DEA 特点：1. 对称加密 2. 同一个SK
    /// </summary>
    public class DES
    {
        //默认密钥向量
        // private static readonly byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        private static readonly string _encryptKey = "CN3812hz";


        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
        public static string Encrypt(string input)
        {
            return Encrypt(input, _encryptKey);
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <param name="encryptKey">密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
        public static string Encrypt(string input, string encryptKey)
        {
            //encryptKey = encryptKey.PadRight(8, ' ').Substring(0, 8);
            //byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
            //byte[] rgbIV = IV;
            //byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
            //DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            //MemoryStream mStream = new MemoryStream();
            //CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            //cStream.Write(inputByteArray, 0, inputByteArray.Length);
            //cStream.FlushFinalBlock();
            //return Convert.ToBase64String(mStream.ToArray());

            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
            provider.Key = Encoding.UTF8.GetBytes(encryptKey);
            provider.IV = Encoding.UTF8.GetBytes(encryptKey);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, provider.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
            cryptoStream.FlushFinalBlock();
            StringBuilder builder = new StringBuilder();
            foreach (byte item in memoryStream.ToArray())
            {
                builder.AppendFormat("{0:X2}", item);
            }
            builder.ToString();
            return builder.ToString();
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="input">待解密的字符串</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string Decrypt(string input)
        {
            return Decrypt(input, _encryptKey);
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="input">待解密的字符串</param>
        /// <param name="encryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string Decrypt(string input, string encryptKey)
        {
            try
            {
                //encryptKey = encryptKey.PadRight(8, ' ').Substring(0, 8);
                //byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey);
                //byte[] rgbIV = IV;
                //byte[] inputByteArray = Convert.FromBase64String(input);
                //DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                //MemoryStream mStream = new MemoryStream();
                //CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                //cStream.Write(inputByteArray, 0, inputByteArray.Length);
                //cStream.FlushFinalBlock();
                //return Encoding.UTF8.GetString(mStream.ToArray());

                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                byte[] inputByteArray = new byte[input.Length / 2];
                for (int x = 0; x < input.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(input.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }
                provider.Key = UTF8Encoding.UTF8.GetBytes(encryptKey);
                provider.IV = UTF8Encoding.UTF8.GetBytes(encryptKey);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, provider.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                cryptoStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
