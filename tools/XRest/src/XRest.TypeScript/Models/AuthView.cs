namespace XRest.TypeScript.Models
{
    internal class AuthView
    {
        public bool IsEnabled { get; }

        public AuthView(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}