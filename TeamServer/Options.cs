using CommandLine;

namespace TeamServer
{
    public class Options
    {
        [Option('p', "password", Required = true, HelpText = "Shared password to connect to TeamServer.")]
        public string SharedPassword { get; set; }
        
        [Option('h', "handler", Required = false, HelpText = "Path to custom Handler DLL.")]
        public string HandlerPath { get; set; }
        
        [Option('y', "profile", Required = false, HelpText = "Path to custom C2 Profile YAML file.")]
        public string ProfilePath { get; set; }
    }
}