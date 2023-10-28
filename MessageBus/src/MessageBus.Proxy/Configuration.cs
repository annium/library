namespace MessageBus.Proxy;

internal record Configuration
{
    public string PubEndpoint { get; set; } = string.Empty;
    public string SubEndpoint { get; set; } = string.Empty;
}
