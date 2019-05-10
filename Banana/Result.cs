using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Banana
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

        // [DisplayName("")]
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }

        /// <summary>
        /// 转json字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJSON();
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
        public static Result<U> Create<U>()
        {
            return new Result<U>();
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="U"></typeparam>
        ///// <param name="message">失败时的提示语</param>
        ///// <returns></returns>
        //public static Result<U> Create<U>(string message)
        //{
        //    return new Result<U>()
        //    {
        //        Message = message
        //    };
        //}

        /// <summary>
        /// 操作成功
        /// </summary>
        /// <returns></returns>
        public virtual Result<T> Succeed()
        {
            this.Success = true;
            return this;
        }
        /// <summary>
        /// 操作成功
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Result<T> Succeed(T data)
        {
            this.Success = true;
            this.Data = data;
            return this;
        }

        public virtual Result<T> SetData(T data)
        {
            this.Data = data;
            return this;
        }
        public virtual Result<T> SetMessage(string message)
        {
            this.Message = message;
            return this;
        }
        public virtual Result<T> SetCode(string code)
        {
            this.Code = code;
            return this;
        }

        public virtual Result<T> Fail()
        {
            this.Success = false;
            return this;
        }
        public virtual Result<T> Fail(string message)
        {
            this.Success = false;
            this.Message = message;
            return this;
        }
        public virtual Result<T> Fail(Exception exception)
        {
            this.Success = false;
            this.Message = exception.Message;
            if (this.Message.IsNullOrWhiteSpace() && exception.InnerException != null)
            {
                this.Message = exception.InnerException.Message ?? "操作失败，未知异常";
            }
            return this;
        }
        public virtual void Clear()
        {
            this.Success = false;
            this.Data = default(T);
            this.Code = string.Empty;
            this.Message = string.Empty;
        }
    }

    [Serializable]
    public sealed class Result : Result<object>
    {
        private Result() { }
        public static Result Create()
        {
            return new Result();
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="message">失败时的提示语</param>
        ///// <returns></returns>
        //public static Result Create(string message)
        //{
        //    return new Result()
        //    {
        //        Message = message
        //    };
        //}
    }
}
