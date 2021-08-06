namespace Drone
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Execute();
        }

        public static void Execute()
        {
            var drone = new Drone();
            drone.Start();
        }
    }
}