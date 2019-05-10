using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Banana
{
    /// <summary>
    /// 说明：异步线程－在同步程序中调用异步，解决线程死锁问题
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

        //public static TaskAsync Ready
        //{
        //    get
        //    {
        //        return new TaskAsync();
        //    }
        //}

        //public async void Run(Action action, Action callback)
        //{
        //    Func<Task> taskFunc = new Func<Task>(() =>
        //    {
        //        return Task.Run(action);
        //    });
        //    await taskFunc();
        //    callback?.Invoke();
        //}

        //public async void Run<TResult>(Func<TResult> action, Action<TResult> callback)
        //{
        //    Func<Task<TResult>> taskFunc = (() =>
        //    {
        //        return Task.Run(() =>
        //        {
        //            return action();
        //        });
        //    });
        //    var result = await taskFunc();
        //    callback?.Invoke(result);
        //}




        /// <summary>
        /// 运行无返回类型的异步方法
        /// </summary>
        /// <param name="task"></param>
        public static Result Run(Action action)
        {
            var result = Result.Create();
            var previousContext = SynchronizationContext.Current;//同步上下文 
            try
            {
                var syncContext = new ExclusiveSynchronizationContext();//异步上下文
                SynchronizationContext.SetSynchronizationContext(syncContext);//设置当前同步上下文
                syncContext.Post(async obj =>
                {
                    try
                    {
                        await new Func<Task>(() =>
                        {
                            return Task.Run(action);
                        }).Invoke();
                        result.Succeed();
                    }
                    catch (Exception ex)
                    {
                        syncContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        syncContext.EndMessageLoop();
                    }
                }, null);
                syncContext.BeginMessageLoop();
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
            return result;
        }

        /// <summary>
        /// 运行返回类型为T的异步方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Result<T> Run<T>(Func<T> action)
        {
            var result = Result.Create<T>();
            var previousContext = SynchronizationContext.Current;
            try
            {
                var syncContext = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncContext);
                syncContext.Post(async obj =>
                {
                    try
                    {
                        result.Succeed(await new Func<Task<T>>(() =>
                        {
                            return Task.Run<T>(action);
                        }).Invoke());
                    }
                    catch (Exception ex)
                    {
                        syncContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        syncContext.EndMessageLoop();
                    }
                }, null);
                syncContext.BeginMessageLoop();
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
            return result;
        }

        /// <summary>
        /// 异步上下文对象
        /// </summary>
        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            /// <summary>
            /// 添加到异步队列
            /// </summary>
            /// <param name="d"></param>
            /// <param name="state"></param>
            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            /// <summary>
            /// 异步结束
            /// </summary>
            public void EndMessageLoop()
            {
                Post(obj => done = true, null);
            }

            /// <summary>
            /// 处理异步队列中的消息
            /// </summary>
            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            //throw new AggregateException("AsyncInline.Run method threw an exception.", InnerException);
                            throw InnerException;
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }


    }
}
