namespace Drone
{
    public class Program
    {
        private static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(20000);
            Execute();
        }

        public static void Execute()
        {
            var drone = new Drone();
            drone.Start();
        }
    }
}