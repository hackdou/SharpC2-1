using System.Collections.Generic;

namespace SharpC2.Models
{
    public class Payload : SharpSploitResult
    {
        public string Handler { get; set; } = "";
        public PayloadFormat Format { get; set; } = PayloadFormat.Exe;
        public string DllExport { get; set; } = "Execute";

        public enum PayloadFormat : int
        {
            Exe = 0,
            Dll = 1,
            PowerShell = 2,
            Raw = 3
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "Handler", Value = Handler },
                new() { Name = "Format", Value = Format },
                new() { Name = "DllExport", Value = DllExport }
            };
    }
}