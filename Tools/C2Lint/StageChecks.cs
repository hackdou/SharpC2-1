using System;

using TeamServer.Models;

namespace C2Lint
{
    public class StageChecks
    {
        private readonly C2Profile.StageBlock _block;

        public StageChecks(C2Profile.StageBlock block)
        {
            _block = block;
        }

        public void CheckDllExport()
        {
            if (string.IsNullOrEmpty(_block.DllExport))
                Console.WriteLine("DllExport is not defined. It will have its default value.");
        }
    }
}