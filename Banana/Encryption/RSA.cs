using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Banana.Encryption
{
    /// <summary>
    /// 1. 非对称加密，即：PK与SK不是同一个
    /// 2. PK用于加密，SK用于解密
    /// 3. PK决定SK，但是PK很难算出SK（数学原理：两个大质数相乘，积很难因式分解）
    /// 4. 速度慢，只对少量数据加密
    /// 
    /// RSA算法的密钥很长，具有较好的安全性，但加密的计算量很大，加密速度较慢限制了其应用范围
    /// </summary>
    public class RSA
    {
        internal static X509Certificate2 GetMyx509(string subname)
        {
            X509Certificate2 res = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 myx509 in store.Certificates)
            {
                if (myx509.Subject == subname)
                {
                    if (myx509.NotAfter > DateTime.Now)
                    {
                        res = myx509;
                    }
                    break;
                }
            }
            store.Close();
            if (res == null)
            {
                store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 myx509 in store.Certificates)
                {
                    if (myx509.Subject == subname)
                    {
                        if (myx509.NotAfter > DateTime.Now)
                        {
                            res = myx509;
                        }
                        break;
                    }
                }
                store.Close();
            }
            return res;
        }
        internal static Result<byte[]> Encrypt(byte[] plainText, string subname)
        {
            var result = Result.Create<byte[]>();
            var myx509 = GetMyx509(subname);
            if (myx509 != null)
            {
                X509Certificate2 myX509Certificate2 = new X509Certificate2(myx509);
                RSACryptoServiceProvider myRSACryptoServiceProvider;
                try
                {
                    myRSACryptoServiceProvider = (RSACryptoServiceProvider)myX509Certificate2.PublicKey.Key;
                    result.Data = myRSACryptoServiceProvider.Encrypt(plainText, true);
                    result.Success = true;
                }
                catch
                {
                    result.Code = "2";
                }
            }
            return result;
        }
        internal static Result<byte[]> Decrypt(byte[] Cryptograph, string subname)
        {
            var result = Result.Create<byte[]>();
            var myx509 = GetMyx509(subname);
            if (myx509 != null)
            {
                try
                {
                    X509Certificate2 myX509Certificate2 = new X509Certificate2(myx509);
                    RSACryptoServiceProvider myRSACryptoServiceProvider;
                    myRSACryptoServiceProvider = (RSACryptoServiceProvider)myX509Certificate2.PrivateKey;
                    result.Data = myRSACryptoServiceProvider.Decrypt(Cryptograph, true);
                    result.Success = true;
                }
                catch
                {
                    result.Code = "2";
                }
            }
            return result;
        }
        public static Result<string> Encrypt(string plainText)
        {
            var result = Result.Create<string>();
            var myx509 = GetMyx509("CN=Banana");
            if (myx509 != null)
            {
                X509Certificate2 myX509Certificate2 = new X509Certificate2(myx509);
                RSACryptoServiceProvider myRSACryptoServiceProvider;

                myRSACryptoServiceProvider = (RSACryptoServiceProvider)myX509Certificate2.PrivateKey;
                result.Data = Convert.ToBase64String(myRSACryptoServiceProvider.SignData(Encoding.UTF8.GetBytes(plainText), typeof(SHA1)));
                result.Success = true;
            }
            return result;
        }
        public static bool Verify(string plainText, string SignText)
        {
            bool result;
            var myx509 = GetMyx509("CN=Banana");
            if (myx509 != null)
            {
                X509Certificate2 myX509Certificate2 = new X509Certificate2(myx509);
                RSACryptoServiceProvider myRSACryptoServiceProvider;
                myRSACryptoServiceProvider = (RSACryptoServiceProvider)myX509Certificate2.PublicKey.Key;
                try
                {
                    result = myRSACryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(plainText), typeof(SHA1), Convert.FromBase64String(SignText));
                }
                catch (Exception)
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        private static byte[] BitStr_ToBytes(string bit_str)
        {
            string[] arrSplit = bit_str.Split('-');
            byte[] byteTemp = new byte[arrSplit.Length];
            for (int i = 0; i < byteTemp.Length; i++)
            {
                byteTemp[i] = byte.Parse(arrSplit[i], System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return byteTemp;
        }

        /// <summary>
        /// generate private key and public key arr[0] for private key arr[1] for public key
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateKeys()
        {
            string[] sKeys = new string[2];
            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            //https://www.cnblogs.com/dudu/p/dotnet-core-rsa-openssl.html

            System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create();
            sKeys[0] = rsa.ToXmlString(true);
            sKeys[1] = rsa.ToXmlString(false);


            //sKeys[0] = rsa.ToXmlString(true);
            //sKeys[1] = rsa.ToXmlString(false);
            return sKeys;
        }

        /// <summary>
        /// RSA Encrypt
        /// </summary>
        /// <param name="sSource" >Source string</param>
        /// <param name="sPublicKey" >public key</param>
        /// <returns></returns>
        public static string EncryptString(string sSource, string sPublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string plaintext = sSource;
            rsa.FromXmlString(sPublicKey);
            byte[] cipherbytes;
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(plaintext), false);

            StringBuilder sbString = new StringBuilder();
            for (int i = 0; i < cipherbytes.Length; i++)
            {
                sbString.Append(cipherbytes[i] + ",");
            }
            return sbString.ToString();
        }


        /// <summary>
        /// RSA Decrypt
        /// </summary>
        /// <param name="sSource">Source string</param>
        /// <param name="sPrivateKey">Private Key</param>
        /// <returns></returns>
        public static string DecryptString(string sSource, string sPrivateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(sPrivateKey);
            string[] sBytes = sSource.Split(',');

            byte[] byteEn = new byte[sBytes.Length];
            for (int j = 0; j < sBytes.Length; j++)
            {
                if (sBytes[j] != "")
                {
                    byteEn[j] = byte.Parse(sBytes[j]);
                }
            }
            byte[] plaintbytes = rsa.Decrypt(byteEn, false);
            return Encoding.UTF8.GetString(plaintbytes);
        }



    }
}
