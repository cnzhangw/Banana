using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Banana.Utility
{

    /// <summary>
    /// 异步线程管理－在同步程序中调用异步，解决了线程死锁问题
    /// </summary>
    public class AsyncTaskManager
    {
        /// <summary>
        /// 运行无返回类型的异步方法
        /// </summary>
        /// <param name="task"></param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;//同步上下文 
            var synch = new ExclusiveSynchronizationContext();//异步上下文
            SynchronizationContext.SetSynchronizationContext(synch);//设置当前同步上下文
            synch.Post(async obj =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }
        /// <summary>
        /// 运行返回类型为T的异步方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);//动作的默认值
            synch.Post(async obj =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        /// <summary>
        /// 异步上下文对象
        /// </summary>
        class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
             new Queue<Tuple<SendOrPostCallback, object>>();

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
                            throw new AggregateException("AsyncInline.Run method threw an exception.",
                             InnerException);
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
