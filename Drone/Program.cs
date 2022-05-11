namespace Drone;

public static class Program
{
    public static void Main(string[] args)
    {
        Execute();
    }

    public static void Execute()
    {
        var drone = new Drone();
        drone.Start();
    }
}