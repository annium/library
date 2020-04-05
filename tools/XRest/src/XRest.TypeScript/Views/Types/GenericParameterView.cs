namespace XRest.TypeScript.Views.Types
{
    internal class GenericParameterView : TypeView
    {
        public GenericParameterView(
            string name
        ) : base(name)
        {
        }

        public override string ToString() => Name;
    }
}