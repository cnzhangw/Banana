using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Utility
{
    public class Common
    {
        private static char[] values =
        {
          '0','1','2','3','4','5','6','7','8','9',
          'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
          'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        };

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">长度，默认6</param>
        /// <param name="onlyNumber">是否只含有数字</param>
        /// <returns></returns>
        public static string Generate(int length = 6, bool onlyNumber = false)
        {
            StringBuilder builder = new StringBuilder(length);
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                if (onlyNumber)
                {
                    builder.Append(values[rd.Next(10)]);
                }
                else
                {
                    builder.Append(values[rd.Next(62)]);
                }
            }
            return builder.ToString();
        }

    }
}
