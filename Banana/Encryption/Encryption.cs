using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Banana.Encryption
{
    /// <summary>
    /// 加密
    /// </summary>
    public class Encryption
    {
        //https://www.cnblogs.com/jfzhu/p/4020928.html

        #region 对称加密（Symmetric Cryptography）

        //基于“对称密钥”的加密算法主要有DES、3DES（TripleDES）、AES、RC2、RC4、RC5和Blowfish等

        private static byte[] salt
            = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };
        private const string _PASSWORD = "CN!3812#*&,hz@wz+";

        /// <summary>
        /// 对称加密
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <param name="password">密钥</param>
        /// <returns></returns>
        [Obsolete("请使用AES",true)]
        public static string Encrypt(string input, string password)
        {
            byte[] cipherText;
            var key = new Rfc2898DeriveBytes(password, salt);
            var algorithm = new RijndaelManaged();
            algorithm.Key = key.GetBytes(16);
            algorithm.IV = key.GetBytes(16);
            var sourceBytes = new UnicodeEncoding().GetBytes(input);
            using (var sourceStream = new MemoryStream(sourceBytes))
            using (var destinationStream = new MemoryStream())
            using (var crypto = new CryptoStream(sourceStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read))
            {
                MoveBytes(crypto, destinationStream);
                cipherText = destinationStream.ToArray();
            }
            return Convert.ToBase64String(cipherText);
        }

        [Obsolete("请使用AES", true)]
        public static string Encrypt(string input)
        {
            return Encrypt(input, _PASSWORD);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <param name="password">密钥</param>
        /// <returns></returns>
        [Obsolete("请使用AES", true)]
        public static string Decrypt(string input, string password)
        {
            if (input.IsNullOrWhiteSpace())
                return string.Empty;

            string decryptedValue;
            byte[] sourceBytes = Convert.FromBase64String(input);
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
        [Obsolete("请使用AES", true)]
        public static string Decrypt(string input)
        {
            return Decrypt(input, _PASSWORD);
        }

        static void MoveBytes(Stream source, Stream dest)
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

        #region 非对称加密（Asymmetric Cryptography）

        /// <summary>
        /// 非对称加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncryptAsymmetric(string input)
        {
            byte[] rsaCipherText;
            var rsa = 1;
            var cspParms = new CspParameters(rsa)
            {
                Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = "Banana"
            };
            var algorithm = new RSACryptoServiceProvider(cspParms);
            //algorithm.FromXmlString()
            var sourceBytes = new UnicodeEncoding().GetBytes(input);
            rsaCipherText = algorithm.Encrypt(sourceBytes, true);
            return Convert.ToBase64String(rsaCipherText);
        }

        /// <summary>
        /// 非对称加密=>解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DecryptAsymmetric(string input)
        {
            if (input.IsNullOrWhiteSpace())
                return string.Empty;

            byte[] rsaCipherText = Convert.FromBase64String(input); //new UnicodeEncoding().GetBytes(input);

            var rsa = 1;
            // decrypt the data.
            var cspParms = new CspParameters(rsa)
            {
                Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = "Banana"
            };
            var algorithm = new RSACryptoServiceProvider(cspParms);
            var unencrypted = algorithm.Decrypt(rsaCipherText, true);
            return new UnicodeEncoding().GetString(unencrypted);
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的内容</param>
        /// <returns></returns>
        [Obsolete("请使用MD5", true)]
        public static string MD5Encrypt(string input)
        {
            return MD5Encrypt(input, Encoding.UTF8);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的内容</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        [Obsolete("请使用MD5", true)]
        public static string MD5Encrypt(string input, Encoding encoding)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("值不能为空");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            byte[] bytes = encoding.GetBytes("$#CN@!##z%@W杭州32DFS4中国@#$%HT幸福RAW@!#DO?￥3812" + input);
            return MD5Encrypt(bytes);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="bytes">需要加密的内容</param>
        /// <returns></returns>
        [Obsolete("请使用MD5", true)]
        static string MD5Encrypt(byte[] bytes)
        {
            if (bytes == null || !bytes.GetEnumerator().MoveNext())
            {
                throw new ArgumentNullException("参数不能为空");
            }

            StringBuilder builder = new StringBuilder();
            System.Security.Cryptography.MD5 hash = new MD5CryptoServiceProvider(); // MD5.Create();
            bytes = hash.ComputeHash(bytes);
            foreach (byte item in bytes)
            {
                builder.AppendFormat("{0:x2}", item);
            }
            return builder.ToString().ToLower();
        }

        /// <summary>
        /// MD5验证传入的密文是否经由传入的明文加密所得
        /// </summary>
        /// <param name="input">明文</param>
        /// <param name="hash">密文</param>
        /// <returns>密文是否由明文加密所得</returns>
        [Obsolete("请使用MD5", true)]
        public static bool MD5Verify(string input, string hash, Encoding encoding = null)
        {
            string hashOfInput = MD5Encrypt(input, encoding);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }

        /// <summary>
        ///  计算指定文件的MD5值
        /// </summary>
        /// <param name="fileName">指定文件的完全限定名称</param>
        /// <returns>返回值的字符串形式</returns>
        [Obsolete("请使用MD5", true)]
        public static string ComputeMD5(string fileName)
        {
            string hashMD5 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    //计算文件的MD5值
                    System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                    Byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();
                    //将字节数组转换成十六进制的字符串形式
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    hashMD5 = stringBuilder.ToString();
                }//关闭文件流
            }//结束计算
            return hashMD5;
        }

        #endregion

        /// <summary>
        ///  计算指定文件的SHA1值
        /// </summary>
        /// <param name="fileName">指定文件的完全限定名称</param>
        /// <returns>返回值的字符串形式</returns>
        public static string ComputeSHA1(string fileName)
        {
            string hashSHA1 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    //计算文件的SHA1值
                    System.Security.Cryptography.SHA1 calculator = System.Security.Cryptography.SHA1.Create();
                    Byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();
                    //将字节数组转换成十六进制的字符串形式
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    hashSHA1 = stringBuilder.ToString();
                }//关闭文件流
            }
            return hashSHA1;
        }
        
        /// <summary>
        /// SHA1 加密，返回大写字符串
        /// </summary>
        /// <param name="content">需要加密字符串</param>
        /// <returns>返回40位UTF8 大写</returns>
        public static string SHA1(string content)
        {
            return SHA1(content, Encoding.UTF8);
        }

        /// <summary>
        /// SHA1 加密，返回大写字符串
        /// </summary>
        /// <param name="content">需要加密字符串</param>
        /// <param name="encode">指定加密编码</param>
        /// <returns>返回40位大写字符串</returns>
        public static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }

    }
}
