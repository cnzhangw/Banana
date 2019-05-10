namespace Banana.Core.Aop
{
    #if !NETSTANDARD2_0

    using System;
    using System.Runtime.Remoting.Messaging;

    public class AopMessageSink : Attribute, IMessageSink
    {
        public AopMessageSink(IMessageSink sink)
        {
            NextSink = sink;
        }

        public IMessageSink NextSink
        {
            get; private set;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            return null;
        }

        public IMessage SyncProcessMessage(IMessage message)
        {
            IMessage resultMessage = null;
            IMethodCallMessage methodCall = message as IMethodCallMessage;
            if (methodCall == null)
            {
                resultMessage = NextSink.SyncProcessMessage(message);
                return resultMessage;
            }

            TransactionMethodAttribute mcAttr =
                (TransactionMethodAttribute)(Attribute.GetCustomAttribute(methodCall.MethodBase, typeof(TransactionMethodAttribute)));
            if (mcAttr == null)
            {
                resultMessage = NextSink.SyncProcessMessage(message);
                return resultMessage;
            }

            using (var db = DatabaseInitializer.Instance.Create())
            {
                using (var transaction = db.GetTransaction())
                {
                    CallContext.SetData(DatabaseInitializer.CONTEXT_DB, db);
                    resultMessage = NextSink.SyncProcessMessage(message);
                    CallContext.SetData(DatabaseInitializer.CONTEXT_DB, null);
                    IMethodReturnMessage methodReturn = resultMessage as IMethodReturnMessage;
                    Exception exception = methodReturn.Exception;
                    if (exception == null)
                    {
                        transaction.Complete();
                    }
                    else
                    {
                        transaction.Dispose();
                        resultMessage = new ReturnMessage(false, methodCall.Args, methodCall.ArgCount, methodCall.LogicalCallContext, methodCall);
                    }
                }
            }
            return resultMessage;
        }
    }

    #endif
}
