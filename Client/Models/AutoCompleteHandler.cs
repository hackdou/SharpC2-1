using System;

namespace SharpC2.Models
{
    public abstract class AutoCompleteHandler : IAutoCompleteHandler
    {
        public abstract string[] GetSuggestions(string text, int index);
        public char[] Separators { get; set; } = {' '};
    }
}