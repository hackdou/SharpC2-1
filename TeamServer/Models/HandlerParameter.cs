namespace TeamServer.Models
{
    public class HandlerParameter
    {
        public string Name { get; }
        public string Value { get; private set; }
        public bool Optional { get; }

        public HandlerParameter(string name, string value, bool optional)
        {
            Name = name;
            Value = value;
            Optional = optional;
        }

        public void SetValue(string value)
        {
            Value = value;
        }
    }
}