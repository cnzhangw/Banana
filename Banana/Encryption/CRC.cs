using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Encryption
{
    public class CRC
    {

        /// <summary>
        /// 生成crc校验
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static uint GetCRCKey(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return GetCRCKey(data);
        }

        public static uint GetCRCKey(byte[] data)
        {
            uint i, j;
            uint modbus_crc;
            modbus_crc = 0xffff;
            for (i = 0; i < data.Length; i++)
            {
                modbus_crc = modbus_crc ^ data[i];
                for (j = 1; j <= 8; j++)
                {
                    if ((modbus_crc & 0x01) == 1)
                    {
                        modbus_crc = (modbus_crc >> 1);
                        modbus_crc ^= 0XA001;
                    }
                    else
                    {
                        modbus_crc = (modbus_crc >> 1);
                    }
                }
            }
            return modbus_crc;
        }

    }
}
