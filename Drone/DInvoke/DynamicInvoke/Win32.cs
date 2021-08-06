// Author: Ryan Cobb (@cobbr_io), The Wover (@TheRealWover)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.Runtime.InteropServices;

namespace Drone.DInvoke.DynamicInvoke
{
    public static class Win32
    {
        public static bool CloseHandle(IntPtr handle)
        {

            // Build the set of parameters to pass in to CloseHandle
            object[] funcargs =
            {
                handle
            };

            bool retVal = (bool)Generic.DynamicAPIInvoke(@"kernel32.dll", @"CloseHandle", typeof(Delegates.CloseHandle), ref funcargs);

            // Dynamically load and invoke the API call with out parameters
            return retVal;
        }

        public static class Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool CloseHandle(
                IntPtr handle
            );
        }
    }
}