#if !NETSTANDARD2_0

using System;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace Banana.Core.Aop
{
    public class AopProperty : IContextProperty, IContributeObjectSink
    {
        public AopProperty() { }

        public string Name
        {
            get
            {
                return "banana_core_aop";
            }
        }

        public void Freeze(Context newContext) { }

        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            return new AopMessageSink(nextSink);
        }

        public bool IsNewContextOK(Context newCtx)
        {
            return true;
        }
    }
}

#endif