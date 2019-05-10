using System;
using Banana.Core.Aop;
using System.Reflection;

namespace Banana.Core
{
#if !NETSTANDARD2_0
    [Aop]
#endif
    internal sealed class Transaction : ContextBoundObject
    {
        [Obsolete("推荐使用：Create(Action action, Action<Exception> onException)")]
        [TransactionMethod(error = false)]
        public bool Create(Func<bool> action, Action<Exception> onException)
        {
            bool result = false;
            try
            {
                if (!action.Invoke())
                {
                    throw new Exception("transaction exception");
                }
                result = true;
            }
            catch (Exception ex)
            {
                onException.Invoke(ex);
                //throw ex; //此处不可抛出，否则引起整个函数异常
            }
            return result;
        }

        [TransactionMethod(error = false)]
        public bool Create(Action action, Action<Exception> onException)
        {
            bool result = false;
            try
            {

#if NETSTANDARD2_0
                InvokeService.Proxy<ITransactionProxy>().ProxyAction(action);
#else
                action.Invoke();
#endif
                result = true;
            }
            catch (Exception ex)
            {
                onException.Invoke(ex);
                //throw ex; //不可抛出，否则transaction会抛出异常
            }
            return result;
        }



#if NETSTANDARD2_0

        public class InvokeProxy<T> : DispatchProxy
        {
            private Type type = null;
            public InvokeProxy()
            {
                type = typeof(T);
            }
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                bool existAllready = DatabaseInitializer.Instance.GetFromContext() != null;

                using (var db = DatabaseInitializer.Instance.Create())
                {
                    db.KeepConnectionAlive = true;
                    using (var transaction = db.GetTransaction())
                    {
                        CallContext<Database>.SetData(DatabaseInitializer.CONTEXT_DB, db);
                        try
                        {
                            targetMethod.Invoke(Activator.CreateInstance(typeof(TransactionProxy)), args);
                            transaction.Complete();
                        }
                        catch (TargetInvocationException ex)
                        {
                            transaction.Dispose();
                            throw ex.InnerException;
                        }
                        catch (Exception ex)
                        {
                            transaction.Dispose();
                            throw ex;
                        }
                        finally
                        {
                            db.KeepConnectionAlive = false;
                            if (existAllready)
                            {
                                // 已存在的对象，修改对应属性后，重新设置回去，交由外层switch处理
                                //CallContext<Database>.SetData(DatabaseInitializer.CONTEXT_DB, db);
                            }
                            else
                            {
                                CallContext<Database>.SetData(DatabaseInitializer.CONTEXT_DB, null);
                            }
                        }
                        return true;
                    }
                }
            }
        }
        internal class InvokeService
        {
            public static T Proxy<T>()
            {
                return DispatchProxy.Create<T, InvokeProxy<T>>();
            }
        }
        internal interface ITransactionProxy
        {
            void ProxyAction(Action action);
        }
        internal class TransactionProxy : ITransactionProxy
        {
            public void ProxyAction(Action action)
            {
                action.Invoke();
            }
        }

#endif


    }
}
