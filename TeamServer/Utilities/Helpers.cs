namespace TeamServer.Utilities;

public static class Helpers
{
    public static string GenerateId()
    {
        return Guid.NewGuid().ToString().Replace("-", "")[..10];
    }
}