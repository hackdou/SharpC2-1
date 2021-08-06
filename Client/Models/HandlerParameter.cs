using System.Collections.Generic;

namespace SharpC2.Models
{
    public class HandlerParameter : SharpSploitResult
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Optional { get; set; }

        public void SetValue(string value)
            => Value = value;

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Value", Value = Value},
                new() {Name = "Optional", Value = Optional}
            };
    }
}