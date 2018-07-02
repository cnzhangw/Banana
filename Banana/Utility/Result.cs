using System;

namespace Banana.Utility
{
    /// <summary> 
    /// 作者：zhangw 
    /// 时间：2017/4/8 17:34:56 
    /// CLR版本：4.0.30319.42000 
    /// 唯一标识：2c59861f-3141-477c-9481-8e4555ee7d32 
    /// </summary> 
    [Serializable]
    public class Result<T>
    {
        protected Result()
        {
            //this.Message = "操作失败";
        }

        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        /// <summary>
        /// 转json字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToObject().ToJson();
        }

        /// <summary>
        /// 将键转小写
        /// </summary>
        /// <returns></returns>
        public virtual object ToObject()
        {
            return new
            {
                success = this.Success,
                code = this.Code,
                message = this.Message,
                data = this.Data
            };
        }

        public static Result<U> Create<U>(string message = "")
        {
            return new Result<U>()
            {
                Message = message
            };
        }
    }

    [Serializable]
    public sealed class Result : Result<object>
    {
        private Result() { }

        public static Result Create(string message = "")
        {
            return new Result()
            {
                Message = message
            };
        }
    }
}
