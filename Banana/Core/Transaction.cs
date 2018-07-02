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
        [TransactionMethod(error = false)]
        public bool Create(Func<bool> action, Action<Exception> onException)
        {
            try
            {
                if (!action.Invoke())
                {
                    throw new Exception("transaction exception");
                }
                return true;
            }
            catch (Exception ex)
            {
                onException.Invoke(ex);
                throw ex;
            }
        }

        [TransactionMethod(error = false)]
        public bool Create(Action action, Action<Exception> onException)
        {
            try
            {

#if NETSTANDARD2_0

                //System.Text.EncodingProvider ppp = System.Text.CodePagesEncodingProvider.Instance;
                //Encoding.RegisterProvider(ppp);
                //using (var db = DBInitializer.Instance.Init())
                //{
                //    using (var transaction = db.GetTransaction())
                //    {
                //        CallContext<Database>.SetData("banana_core_transaction", db);
                //        try
                //        {
                //            action.Invoke();
                //            transaction.Complete();
                //        }
                //        catch (Exception ex)
                //        {
                //            transaction.Dispose();
                //            throw ex;
                //        }
                //        finally
                //        {
                //            CallContext<Database>.SetData("banana_core_transaction", null);
                //        }
                //        return true;
                //    }
                //}

                InvokeService.Proxy<ITransactionProxy>().ProxyAction(action);
                return true;
#else
                action.Invoke();
                return true;
#endif
            }
            catch (Exception ex)
            {
                onException.Invoke(ex);
                throw ex;
            }
        }
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
            using (var db = DatabaseInitializer.Instance.Init())
            {
                using (var transaction = db.GetTransaction())
                {
                    CallContext<Database>.SetData(DatabaseInitializer.DATABASE_CONTEXT_KEY, db);
                    try
                    {
                        targetMethod.Invoke(Activator.CreateInstance(typeof(TransactionProxy)), args);
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        transaction.Dispose();
                        throw ex;
                    }
                    finally
                    {
                        CallContext<Database>.SetData(DatabaseInitializer.DATABASE_CONTEXT_KEY, null);
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
