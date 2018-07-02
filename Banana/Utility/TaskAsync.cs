using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Utility
{
    /// <summary>
    /// 说明：TaskAsync
    /// 作者：张炜 
    /// 时间：2017/12/22 17:05:28
    /// 公司:浙江广锐信息科技有限公司
    /// CLR版本：4.0.30319.42000
    /// 版权说明：本代码版权归广锐信息所有
    /// 唯一标识：9ac5d6ab-e08a-4b22-887a-98a53cfbae01
    /// </summary>
    public class TaskAsync
    {
        private TaskAsync() { }

        public static TaskAsync Ready
        {
            get
            {
                return new TaskAsync();
            }
        }

        public async void Run(Action action, Action callback)
        {
            Func<Task> taskFunc = new Func<Task>(() =>
            {
                return Task.Run(action);
            });
            await taskFunc();
            callback?.Invoke();
        }

        public async void Run<TResult>(Func<TResult> action, Action<TResult> callback)
        {
            Func<Task<TResult>> taskFunc = (() =>
            {
                return Task.Run(() =>
                {
                    return action();
                });
            });
            var result = await taskFunc();
            callback?.Invoke(result);
        }
    }
}
