using System.Collections.Generic;

namespace SharpC2.Models
{
    public class Handler : SharpSploitResult
    {
        public string Name { get; set; }
        public IEnumerable<HandlerParameter> Parameters { get; set; }
        public bool Running { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Running", Value = Running}
            };
    }
}