using System.ServiceProcess;

namespace DroneService
{
    static class Program
    {
        static void Main()
        {
            var services = new ServiceBase[]
            {
                new Service()
            };

            ServiceBase.Run(services);
        }
    }
}
