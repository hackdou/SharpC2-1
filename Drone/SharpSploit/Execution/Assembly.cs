using Reflect = System.Reflection;

namespace Drone.SharpSploit.Execution
{
    public static class Assembly
    {
        public static void Execute(byte[] assemblyBytes, string[] args = null)
        {
            if (args == null) args = new string[] { };
            
            var asm = Reflect.Assembly.Load(assemblyBytes);
            asm.EntryPoint.Invoke(null, new object[] {args});
        }
    }
}