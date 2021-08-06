using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SharpC2
{
    public static class Extensions
    {

        public static string Serialize<T>(this T data)
        {
            var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
            return JsonSerializer.Serialize(data, options);
        }
        
        public static T Deserialize<T>(this string json)
        {
            var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static IEnumerable<string> GetPartialPath(string path)
        {
            // could be something like C:\Users or /Users
            var index = path.LastIndexOf(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/');
            index++;

            var partial = path[..index];

            if (!Directory.Exists(partial)) return Array.Empty<string>();

            return Directory.EnumerateFileSystemEntries(partial)
                .Where(p => p.StartsWith(path)).ToArray();
        }
    }
}