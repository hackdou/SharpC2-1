using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpC2.Helpers
{
    public static class Arguments
    {
        //Get the argument value (if any) from a array based on arg pre-fix
        public static string GetValue(this string[] args, string substring)
        {
            if (args.Contains(substring))
                return args[args.GetIndex(substring) + 1];

            return "";

        }

        //Get the index of a string in an array
        public static int GetIndex(this string[] args, string substring)
        {
            return args.ToList().FindIndex(a => a == substring);

        }

        //Bool check if an array contains a given set of substrings
        public static bool Contains(this string[] args, string[] substrings)
        {
            foreach (var subString in substrings)
            {
                if (args.ToList().FindIndex(a => a == subString) == -1)
                    return false;
            }
            return true;

        }

        //Bool check if an array contains an string
        public static bool Contains(this string[] args, string substring)
        {

            if (args.ToList().FindIndex(a => a == substring) != -1)
                return true;

            return false;

        }
    }
}
