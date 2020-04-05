using Annium.Data.Models;

namespace XRest.TypeScript.Views.Types
{
    internal abstract class TypeView : Equatable<TypeView>
    {
        public string Name { get; }

        protected TypeView(
            string name
        )
        {
            Name = name;
        }

        public override int GetHashCode() => ToString()!.GetHashCode();
    }
}