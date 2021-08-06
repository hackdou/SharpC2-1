using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TeamServer
{
    public static class Utilities
    {
        public static async Task<byte[]> GetEmbeddedResource(string name)
        {
            var self = Assembly.GetExecutingAssembly();

            await using var rs = self.GetManifestResourceStream($"TeamServer.Resources.{name}");
            if (rs is null) return null;

            await using var ms = new MemoryStream();
            int read;
            do
            {
                var buf = new byte[1024];
                read = await rs.ReadAsync(buf, 0, buf.Length);
                await ms.WriteAsync(buf, 0, read);
            }
            while (read > 0);

            return ms.ToArray();
        }
    }
}