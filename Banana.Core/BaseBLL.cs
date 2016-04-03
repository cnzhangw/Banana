using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    public abstract class BaseBLL<T, TId>
        where T : DataContextModel, new()
    {
        protected IDAL<T, TId> Service = null;

        protected BaseBLL()
        {
            this.Service = DALManager<T, TId>.GetInstance();
        }

        /// <summary>
        /// 获取其他实体服务接口
        /// </summary>
        /// <typeparam name="T1">实体类模型</typeparam>
        /// <typeparam name="T2">主键类型</typeparam>
        /// <returns></returns>
        public IDAL<T1, T2> GetService<T1, T2>() where T1 : DataContextModel, new()
        {
            return DALManager<T1, T2>.GetInstance();
        }

        /// <summary>
        /// 获取其他实体服务接口
        /// </summary>
        /// <typeparam name="T1">实体类模型</typeparam>
        /// <returns></returns>
        public IDAL<T1, int> GetService<T1>() where T1 : DataContextModel, new()
        {
            return this.GetService<T1, int>();
        }

    }

    public abstract class BaseBLL<T> : BaseBLL<T, int> where T : DataContextModel, new() { }    

}
