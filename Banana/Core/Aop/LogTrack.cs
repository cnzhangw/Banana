#if !NETSTANDARD2_0

namespace Banana.Core.Aop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;

    public class LogTrack : IMessageSink
    {
        public MarshalByRefObject refObject
        {
            get; private set;
        }
        public IMessageSink NextSink
        {
            get; private set;
        }

        public LogTrack(MarshalByRefObject refobj, IMessageSink messageSink)
        {
            NextSink = messageSink;
            refObject = refobj;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage message, IMessageSink replySink)
        {
            //throw new NotImplementedException();
            return null;
        }

        public IMessage SyncProcessMessage(IMessage message)
        {
            BeforeMethodStart(message);
            IMessage returnMsg = NextSink.SyncProcessMessage(message);
            AfterMethodEnd(message, returnMsg);
            return returnMsg;
        }

        void BeforeMethodStart(IMessage message)
        {
            if (message is IMethodMessage)
            {
                IMethodMessage methodMessage = message as IMethodMessage;
                Debug.WriteLine("banana_core_log:" + refObject.GetType().ToString() + "." + methodMessage.MethodName + " started.");
                //Console.WriteLine("BananaLog:" + _obj.GetType().ToString() + "." + methodMessage.MethodName + " started.");
            }
        }
        void AfterMethodEnd(IMessage message, IMessage returnMessage)
        {
            if (message is IMethodMessage && returnMessage is IMethodReturnMessage)
            {
                IMethodMessage methodMessage = message as IMethodMessage;
                IMethodReturnMessage returnMethodMessage = returnMessage as IMethodReturnMessage;
                if (returnMethodMessage.Exception != null)
                {
                    Debug.WriteLine("banana_core_log:" + refObject.GetType().ToString() + "." + methodMessage.MethodName + " exception " + Environment.NewLine + returnMethodMessage.Exception.Message);
                }
                Debug.WriteLine("banana_core_log:" + refObject.GetType().ToString() + "." + methodMessage.MethodName + " ended.");
            }
        }
    }
}

#endif