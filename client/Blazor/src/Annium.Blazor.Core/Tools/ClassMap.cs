using System.Collections.Generic;

namespace Annium.Blazor.Core.Tools
{
    public class ClassMap
    {
        public static ClassMap New(string? value, bool apply = true)
        {
            return new ClassMap().Add(value, apply);
        }

        private readonly IList<string> _classes = new List<string>();

        private ClassMap()
        {
        }

        public ClassMap Add(string? value, bool apply = true)
        {
            if (apply && !string.IsNullOrWhiteSpace(value))
                _classes.Add(value);
            return this;
        }

        public override string ToString() => string.Join(' ', _classes);
    }
}