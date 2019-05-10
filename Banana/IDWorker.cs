using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Banana
{
    /// <summary>
    /// 主键生成器
    /// From: https://github.com/twitter/snowflake
    /// An object that generates IDs.
    /// This is broken into a separate class in case
    /// we ever want to support multiple worker threads
    /// per process
    /// </summary>
    public class IDWorker
    {
        /*
         snowflake是Twitter开源的分布式ID生成算法，结果是一个long型的ID。其核心思想是：使用41bit作为毫秒数，10bit作为机器的ID（5个bit是数据中心，5个bit的机器ID），12bit作为毫秒内的流水号（意味着每个节点在每毫秒可以产生 4096 个 ID），最后还有一个符号位，永远是0。
         
         snowflake算法可以根据自身项目的需要进行一定的修改。比如估算未来的数据中心个数，每个数据中心的机器数以及统一毫秒可以能的并发数来调整在算法中所需要的bit数。
            优点：
                1）不依赖于数据库，灵活方便，且性能优于数据库。
                2）ID按照时间在单机上是递增的。
            缺点：
                1）在单机上是递增的，但是由于涉及到分布式环境，每台机器上的时钟不可能完全同步，也许有时候也会出现不是全局递增的情况。 
         */

        private readonly long workerId;
        private readonly long dataCenterId;
        private long sequence = 0L;

        private static readonly long twepoch = 1288834974657L;

        private static readonly long workerIdBits = 5L;
        private static readonly long datacenterIdBits = 5L;
        private static long maxWorkerId = -1L ^ (-1L << (int)workerIdBits);
        private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);
        private static readonly long sequenceBits = 12L;

        private readonly long workerIdShift = sequenceBits;
        private readonly long datacenterIdShift = sequenceBits + workerIdBits;
        private readonly long timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
        private readonly long sequenceMask = -1L ^ (-1L << (int)sequenceBits);

        private long lastTimestamp = -1L;
        private static readonly object syncRoot = new object();

        /// <summary>
        /// 场景：一次调用
        /// 循环内建议自己创建实例调用Next方法
        /// </summary>
        public static IDWorker Builder
        {
            get
            {
                return new IDWorker();
            }
        }

        public IDWorker()
        {
            new IDWorker(0, 0);
        }

        public IDWorker(long workerId, long dataCenterId)
        {
            // sanity check for workerId
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format("worker Id can't be greater than %d or less than 0", maxWorkerId));
            }

            if (dataCenterId > maxDatacenterId || dataCenterId < 0)
            {
                throw new ArgumentException(string.Format("datacenter Id can't be greater than %d or less than 0", maxDatacenterId));
            }

            this.workerId = workerId;
            this.dataCenterId = dataCenterId;
        }

        /// <summary>
        /// [推荐使用] twitter's snowflake算法生成，长度19位。
        /// </summary>
        /// <returns></returns>
        public long Next()
        {
            lock (syncRoot)
            {
                long timestamp = GetTimestamp();
                if (timestamp < lastTimestamp)
                {
                    throw new ApplicationException(string.Format("Clock moved backwards.  Refusing to generate id for %d milliseconds", lastTimestamp - timestamp));
                }

                if (lastTimestamp == timestamp)
                {
                    sequence = (sequence + 1) & sequenceMask;
                    if (sequence == 0)
                    {
                        timestamp = TilNextMillis(lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0L;
                }

                lastTimestamp = timestamp;
                return ((timestamp - twepoch) << (int)timestampLeftShift) | (dataCenterId << (int)datacenterIdShift) | (workerId << (int)workerIdShift) | sequence;
            }
        }

        /// <summary>
        /// 时间戳拼接
        /// </summary>
        /// <param name="length">default 19, range: 13-19</param>
        /// <returns></returns>
        public static long Next(int length = 19)
        {
            if (length > 19 || length < 13)
            {
                throw new ArgumentException(string.Format("length can't greater than 19 or less than 13"));
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(GetTimestamp());
            Random random = new Random();
            string randomValue = string.Empty;
            int diff = length - 13;
            if (diff > 0)
            {
                int max = 10;
                for (int i = 0; i < diff; i++)
                {
                    randomValue = random.Next(0, max).ToString();
                    builder.Append(randomValue);
                }
            }
            return Convert.ToInt64(builder.ToString());
        }

        /// <summary>
        /// uuid变种主键
        /// 缺点：无序，不可读
        /// </summary>
        /// <returns></returns>
        public static long NextUUID()
        {
            return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
        }

        /// <summary>
        /// guid，格式：06bd38d0c75a4bcbb24e25e0a8166ebf
        /// </summary>
        /// <returns></returns>
        public static string NextGUID()
        {
            return Guid.NewGuid().ToString("N").ToLower();
        }

        protected long TilNextMillis(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 转换指定字符串为短字符
        /// </summary>
        /// <param name="input">为空时返回空字符串，最大长度60</param>
        /// <param name="keep">当前一个参数不变时，默认保持转换结果一致</param>
        /// <returns></returns>
        public static string Shorten(string input = "", bool keep = true)
        {
            byte[] bytes = null;
            if (input.IsNullOrWhiteSpace())
            {
                // bytes = Guid.NewGuid().ToByteArray();
                return string.Empty;
            }
            else
            {
                if (input.Length > 60)
                {
                    throw new ArgumentOutOfRangeException("input", "参数长度不能超过60个字符");
                }

                bytes = Encoding.UTF8.GetBytes(input);
            }

            long i = 1L;
            foreach (byte item in bytes)
            {
                i *= ((int)item + 1);
            }

            long t = 0L;
            if (keep)
            {
                t = i - (new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            }
            else
            {
                t = i - DateTime.Now.Ticks;
            }
            return string.Format("{0:x}", t);
        }

        private Random random = new Random();
        private int seed = 0;
        /// <summary>
        /// 创建一个随机token
        /// </summary>
        /// <returns>返回一个长度11位的字符串</returns>
        public string CreateToken()
        {
            var randomData = new byte[4];
            random.NextBytes(randomData);

            var seedValue = Interlocked.Add(ref seed, 1);
            var seedData = BitConverter.GetBytes(seedValue);

            var tokenData = randomData.Concat(seedData).OrderBy(e => random.Next());
            return Convert.ToBase64String(tokenData.ToArray()).TrimEnd('=');

            #region 写法二
            //var rnd = new Random();
            //var tokenData = new byte[8];
            //rnd.NextBytes(tokenData);
            //var token = Convert.ToBase64String(tokenData).TrimEnd('=');
            //return token;
            #endregion

        }

        private static readonly char[] values =
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
        public static string Random(int length = 6, bool onlyNumber = false)
        {
            StringBuilder builder = new StringBuilder(length);
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                if (onlyNumber)
                {
                    builder.Append(values[random.Next(10)]);
                }
                else
                {
                    builder.Append(values[random.Next(62)]);
                }
            }
            return builder.ToString();
        }

    }
}
