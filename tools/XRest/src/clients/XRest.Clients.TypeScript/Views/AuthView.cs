namespace XRest.Clients.TypeScript.Views;

internal class AuthView
{
    public bool IsEnabled { get; }

    public AuthView(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
