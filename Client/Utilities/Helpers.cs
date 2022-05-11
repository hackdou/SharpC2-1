using System.Reflection;

namespace Client.Utilities
{
    public static class Helpers
    {
        public static async Task<byte[]> GetEmbeddedResource(string name)
        {
            var self = Assembly.GetExecutingAssembly();
            await using var rs = self.GetManifestResourceStream($"Client.Embedded.{name}");

            if (rs is null)
                return Array.Empty<byte>();

            await using var ms = new MemoryStream();
            await rs.CopyToAsync(ms);

            return ms.ToArray();
        }
    }
}