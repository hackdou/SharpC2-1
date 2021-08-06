using System.Collections.Generic;

namespace SharpC2.Screens
{
    public class ScreenCommand : SharpSploitResult
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string Usage { get; init; }
        public Screen.Callback Callback { get; init; }

        public ScreenCommand(string name, string description, Screen.Callback callback, string usage = null)
        {
            Name = name;
            Description = description;
            Usage = usage;
            Callback = callback;
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Description", Value = Description}
            };
    }
}