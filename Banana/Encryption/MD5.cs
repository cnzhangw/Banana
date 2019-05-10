using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Banana.Encryption
{
    /// <summary>
    /// Message Algorithm（消息摘要算法第五版），散列函数（哈希算法）_不可逆，压缩性
    /// 
    /// </summary>
    public class MD5
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="value">需要加密的内容</param>
        /// <returns></returns>
        public static string Encrypt(string value)
        {
            return Encrypt(value, Encoding.UTF8);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="value">需要加密的内容</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string Encrypt(string value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("值不能为空");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            //"$#CN@!##z%@W杭州32DFS4中国@#$%HT幸福RAW@!#DO?￥3812" +
            byte[] bytes = encoding.GetBytes(value);
            return Encrypt(bytes);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="bytes">需要加密的内容</param>
        /// <returns></returns>
        static string Encrypt(byte[] bytes)
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
        /// <param name="value">明文</param>
        /// <param name="hash">密文</param>
        /// <returns>密文是否由明文加密所得</returns>
        public static bool Verify(string value, string hash, Encoding encoding = null)
        {
            string hashOfInput = Encrypt(value, encoding);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }
        
        /// <summary>
        ///  计算指定文件的MD5值
        /// </summary>
        /// <param name="path">指定文件的完全限定名称</param>
        /// <returns>返回值的字符串形式</returns>
        public static string ComputeMD5(string path)
        {
            string hashMD5 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (System.IO.File.Exists(path))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    //计算文件的MD5值
                    System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                    Byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();
                    //将字节数组转换成十六进制的字符串形式
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        builder.Append(buffer[i].ToString("x2"));
                    }
                    hashMD5 = builder.ToString();
                }//关闭文件流
            }//结束计算
            return hashMD5;
        }


    }
}
