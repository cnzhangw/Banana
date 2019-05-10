using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Cache
{
    public class CacheFactory
    {
        // https://www.cnblogs.com/CoderLinkf/p/8798533.html

        private static readonly object _object = new object();
        private static volatile CacheHub _cache = null;

        /// <summary>
        /// Cache
        /// </summary>
        public static CacheHub Cache
        {
            get
            {
                if (_cache == null)
                {
                    lock (_object)
                    {
                        if (_cache == null)
                        {
                            _cache = new CacheHub();
                        }
                    }
                }
                return _cache;
            }
        }
    }
}
