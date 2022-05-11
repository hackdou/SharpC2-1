using YamlDotNet.Serialization;

namespace Client.Models
{
    public class C2Profile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public HttpOptions Http { get; set; }

        public class HttpOptions
        {
            public string Endpoint { get; set; }
            public int Sleep { get; set; }
            public int Jitter { get; set; }
        }

        public override string ToString()
        {
            var builder = new SerializerBuilder().EmitDefaults();
            var serialiser = builder.Build();
            return serialiser.Serialize(this).TrimEnd();
        }
    }
}