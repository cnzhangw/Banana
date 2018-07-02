namespace Banana.Core.Aop
{
    #if !NETSTANDARD2_0

    using System;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;

    [AttributeUsage(AttributeTargets.Class)]
    public class AopAttribute : ContextAttribute
    {
        //Aspect Oriented Programming
        public AopAttribute() : base("banana_core_aop") { }
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            ctorMsg.ContextProperties.Add(new AopProperty());
        }
    }
    
    #endif
}