#if !NETSTANDARD2_0

namespace Banana.Core.Aop
{
    using System;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;

    public class LogHandler : IContextProperty, IContributeObjectSink
    {
        public void Freeze(Context newContext)
        {
            //throw new NotImplementedException();
        }

        public bool IsNewContextOK(Context newCtx)
        {
            //throw new NotImplementedException();
            return true;
        }

        public string Name
        {
            get
            {
                return "banana_core_log";
            }
        }

        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            return new LogTrack(obj,nextSink);
        }
    }
}

#endif