using System;
using TeamServer.Models;

namespace C2Lint
{
    public class StageChecks
    {
        private readonly C2Profile.StageBlock _block;
        private readonly C2Profile _default;

        public StageChecks(C2Profile.StageBlock block)
        {
            _block = block;
            _default = new C2Profile();
        }

        public void CheckDllExport()
        {
            if (string.IsNullOrEmpty(_block.DllExport))
                Console.WriteLine($"[!!!] DllExport not defined. It will have its default value of \"{_default.Stage.DllExport}\".");
        }
    }
}