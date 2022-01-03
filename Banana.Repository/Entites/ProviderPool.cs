using Banana;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Repository.Entites
{
   public class ProviderPool
    {
        /// <summary>
        /// 
        /// </summary>
        public Func<SqlProvider, SqlProvider> FuncProvider { get; set; }

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string DbName { get; set; }
    }
}
