using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpC2.Helpers
{
    public static class Arguments
    {
        // Get the argument value (if any) from a array based on arg pre-fix
        public static string GetValue(this string[] args, string substring)
        {
            return args.Contains(substring)
                ? args[args.GetIndex(substring) + 1]
                : "";
        }

        // Get the index of a string in an array
        private static int GetIndex(this IEnumerable<string> args, string substring)
        {
            return args.ToList().FindIndex(a =>
                a.Equals(substring, StringComparison.OrdinalIgnoreCase));
        }

        // Bool check if an array contains a given set of substrings
        public static bool Contains(this IEnumerable<string> args, IEnumerable<string> subStrings)
        {
            return subStrings.All(subString =>
                args.ToList().FindIndex(a =>
                    a.Equals(subString, StringComparison.OrdinalIgnoreCase)) != -1);
        }

        // Bool check if an array contains an string
        private static bool Contains(this IEnumerable<string> args, string substring)
        {
            return args.ToList().FindIndex(a =>
                a.Equals(substring, StringComparison.OrdinalIgnoreCase)) != -1;
        }
    }
}
