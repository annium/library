namespace XRest.Core.Models
{
    public class AuthModel
    {
        public bool IsEnabled { get; }

        public AuthModel(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}