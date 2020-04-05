using System;
using System.Collections.Generic;

namespace XRest.TypeScript.Views.Types
{
    internal class EnumView : DefinedTypeView
    {
        public override TypeViewEnum Type { get; } = TypeViewEnum.Enum;

        public IReadOnlyDictionary<string, int> Values { get; }

        public EnumView(
            string name,
            IReadOnlyDictionary<string, int> values
        ) : base(name)
        {
            if (values.Count == 0)
                throw new ArgumentException($"Enum '{this}' values count must be greater than 0");

            Values = values;
        }

        public override string ToString() => Name;
    }
}