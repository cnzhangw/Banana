#if !NETSTANDARD2_0

namespace Banana.Core.Aop
{
    using System;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;

    [AttributeUsage(AttributeTargets.Class)]
    public class LogAttribute : ContextAttribute
    {
        public LogAttribute() : base("banana_core_log") { }

        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            //base.GetPropertiesForNewContext(ctorMsg);
            if (this.Name == "banana_core_log")
            {
                ctorMsg.ContextProperties.Add(new LogHandler());
            }
        }

    }
}

#endif