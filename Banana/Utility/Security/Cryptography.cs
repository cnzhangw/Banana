using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Utility.Security
{
    /// <summary>
    /// 加密
    /// </summary>
    public class Cryptography
    {

        //对称加密（Symmetric Cryptography）
        //非对称加密（Asymmetric Cryptography）
        //https://www.cnblogs.com/jfzhu/p/4020928.html


        #region 对称加密

        private byte[] salt = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };

        /// <summary>
        /// 对称加密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncryptionSymmetric(string value, string password)
        {
            byte[] cipherText;
            var key = new Rfc2898DeriveBytes(password, salt);
            var algorithm = new RijndaelManaged();
            algorithm.Key = key.GetBytes(16);
            algorithm.IV = key.GetBytes(16);
            var sourceBytes = new UnicodeEncoding().GetBytes(value);
            using (var sourceStream = new MemoryStream(sourceBytes))
            using (var destinationStream = new MemoryStream())
            using (var crypto = new CryptoStream(sourceStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read))
            {
                MoveBytes(crypto, destinationStream);
                cipherText = destinationStream.ToArray();
            }
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// 对称加密=>解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string DecryptionSymmetric(string value, string password)
        {
            if (value.IsNullOrWhiteSpace()) return string.Empty;

            string decryptedValue;
            byte[] sourceBytes = new UnicodeEncoding().GetBytes(value);
            var key = new Rfc2898DeriveBytes(password, salt);
            // Try to decrypt, thus showing it can be round-tripped.
            var algorithm = new RijndaelManaged();
            algorithm.Key = key.GetBytes(16);
            algorithm.IV = key.GetBytes(16);
            using (var sourceStream = new MemoryStream(sourceBytes))
            using (var destinationStream = new MemoryStream())
            using (var crypto = new CryptoStream(sourceStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
            {
                MoveBytes(crypto, destinationStream);
                var decryptedBytes = destinationStream.ToArray();
                decryptedValue = new UnicodeEncoding().GetString(decryptedBytes);
            }
            return decryptedValue;
        }

        private void MoveBytes(Stream source, Stream dest)
        {
            byte[] bytes = new byte[2048];
            var count = source.Read(bytes, 0, bytes.Length);
            while (0 != count)
            {
                dest.Write(bytes, 0, count);
                count = source.Read(bytes, 0, bytes.Length);
            }
        }

        #endregion

        #region 非对称加密

        /// <summary>
        /// 非对称加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string EncryptionAsymmetric(string value)
        {
            byte[] rsaCipherText;
            var rsa = 1;
            var cspParms = new CspParameters(rsa);
            cspParms.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParms.KeyContainerName = "Banana";
            var algorithm = new RSACryptoServiceProvider(cspParms);
            //algorithm.FromXmlString()
            var sourceBytes = new UnicodeEncoding().GetBytes(value);
            rsaCipherText = algorithm.Encrypt(sourceBytes, true);
            return Convert.ToBase64String(rsaCipherText);
        }

        /// <summary>
        /// 非对称加密=>解密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string DecryptionAsymmetric(string value)
        {
            if (value.IsNullOrWhiteSpace()) return string.Empty;

            byte[] rsaCipherText = new UnicodeEncoding().GetBytes(value);

            var rsa = 1;
            // decrypt the data.
            var cspParms = new CspParameters(rsa);
            cspParms.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParms.KeyContainerName = "Banana";
            var algorithm = new RSACryptoServiceProvider(cspParms);
            var unencrypted = algorithm.Decrypt(rsaCipherText, true);
            return new UnicodeEncoding().GetString(unencrypted);
        }

        #endregion

        #region md5

        public static string GetMD5(string value)
        {
            return GetMD5(value, Encoding.ASCII);
        }

        public static string GetMD5(string value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("值不能为空");
            }

            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            byte[] bytes = encoding.GetBytes(value);
            return GetMD5(bytes);
        }

        public static string GetMD5(byte[] bytes)
        {
            if (bytes == null || !bytes.GetEnumerator().MoveNext())
            {
                throw new ArgumentNullException("参数不能为空");
            }

            StringBuilder builder = new StringBuilder();
            MD5 hash = new MD5CryptoServiceProvider();
            bytes = hash.ComputeHash(bytes);
            foreach (byte item in bytes)
            {
                builder.AppendFormat("{0:x2}", item);
            }
            return builder.ToString().ToLower();
        }

        #endregion



        public class AES
        {
            //默认密钥向量 
            private readonly static byte[] keys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };
            //默认盐
            private readonly static string key_default = "484C5050744D4887BEFA841A740B976C";

            /// <summary>
            /// AES加密字符串
            /// </summary>
            /// <param name="value">待加密的字符串</param>
            /// <param name="key">加密密钥,要求为32位</param>
            /// <returns>加密成功返回加密后的字符串,失败返回源串</returns> 
            public static string Encrypt(string value, string key)
            {
                key = key.PadRight(32, ' ').Substring(0, 32);

                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = Encoding.UTF8.GetBytes(key.Substring(0, 32));
                rijndaelProvider.IV = keys;
                ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();

                byte[] inputData = Encoding.UTF8.GetBytes(value);
                byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return Convert.ToBase64String(encryptedData);
            }
            /// <summary>
            /// AES加密字符串
            /// </summary>
            /// <param name="value">待加密的字符串</param>
            /// <returns>加密成功返回加密后的字符串,失败返回源串</returns> 
            public static string Encrypt(string value)
            {
                return Encrypt(value, key_default);
            }
            /// <summary>
            /// AES解密字符串
            /// </summary>
            /// <param name="value">待解密的字符串</param>
            /// <param name="key">解密密钥,要求为32位,和加密密钥相同</param>
            /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
            public static string Decrypt(string value, string key)
            {
                try
                {
                    key = key.PadRight(32, ' ').Substring(0, 32);

                    RijndaelManaged rijndaelProvider = new RijndaelManaged();
                    rijndaelProvider.Key = Encoding.UTF8.GetBytes(key);
                    rijndaelProvider.IV = keys;
                    ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

                    byte[] inputData = Convert.FromBase64String(value);
                    byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                    return Encoding.UTF8.GetString(decryptedData);
                }
                catch
                {
                    return "";
                }
            }
            /// <summary>
            /// AES解密字符串
            /// </summary>
            /// <param name="value">待解密的字符串</param>
            /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
            public static string Decrypt(string value)
            {
                return Decrypt(value, key_default);
            }
        }


        public class DES
        {
            //默认密钥向量
            private static byte[] keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            private static string key_default = "31415926";


            /// <summary>
            /// DES加密字符串
            /// </summary>
            /// <param name="value">待加密的字符串</param>
            /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
            public static string Encrypt(string value)
            {
                return Encrypt(value, key_default);
            }

            /// <summary>
            /// DES加密字符串
            /// </summary>
            /// <param name="value">待加密的字符串</param>
            /// <param name="key">加密密钥,要求为8位</param>
            /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
            public static string Encrypt(string value, string key)
            {
                key = key.PadRight(8, ' ').Substring(0, 8);
                byte[] rgbKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] rgbIV = keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(value);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }

            /// <summary>
            /// DES解密字符串
            /// </summary>
            /// <param name="value">待解密的字符串</param>
            /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
            public static string Decrypt(string value)
            {
                return Decrypt(value, key_default);
            }

            /// <summary>
            /// DES解密字符串
            /// </summary>
            /// <param name="value">待解密的字符串</param>
            /// <param name="key">解密密钥,要求为8位,和加密密钥相同</param>
            /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
            public static string Decrypt(string value, string key)
            {
                try
                {
                    key = key.PadRight(8, ' ').Substring(0, 8);
                    byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                    byte[] rgbIV = keys;
                    byte[] inputByteArray = Convert.FromBase64String(value);
                    DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();

                    MemoryStream mStream = new MemoryStream();
                    CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    return Encoding.UTF8.GetString(mStream.ToArray());
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}
