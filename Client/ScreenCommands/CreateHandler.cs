using System.Collections.Generic;
using SharpC2.Models;

namespace SharpC2.ScreenCommands
{
    public class CreateHandler : ScreenCommand
    {
        public CreateHandler(Screen.Callback callback)
        {
            Execute = callback;
        }
        
        public override string Name => "create";
        public override string Description => "Create a new Handler";

        public override List<Argument> Arguments => new List<Argument>
        {
            new() { Name = "name", Optional = false },
            new() { Name = "type", Optional = false }
        };
        
        public override Screen.Callback Execute { get; }
    }
}