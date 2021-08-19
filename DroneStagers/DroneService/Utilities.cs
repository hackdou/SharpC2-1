using System.IO;
using System.Reflection;

namespace DroneService
{
    public static class Utilities
    {
        public static byte[] GetEmbeddedResource(string name)
        {
            var self = Assembly.GetExecutingAssembly();

            using var rs = self.GetManifestResourceStream(name);

            if (rs is null) return null;
            
            using var ms = new MemoryStream();
            rs.CopyTo(ms);
            return ms.ToArray();
        }
    }
}