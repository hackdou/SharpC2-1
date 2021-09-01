using System.Collections.Generic;

namespace SharpC2.Models
{
    public class HostedFile : SharpSploitResult
    {
        public string Filename { get; set; }
        public long Size { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "Filename", Value = Filename },
                new() { Name = "Size", Value = Size }
            };
    }
}