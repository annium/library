using System.Linq;

namespace xdomains.Tools;

public class Parser
{
    private readonly string[] isFree = new []
    {
        "available for registration",
        "not been registered",
        "not found",
        "not registered",
    };

    private readonly string[] isBusy = new []
    {
        "creation date",
        "prohibited",
        "registered",
        "registrant",
        "reserved",
    };

    public bool IsFree(string result)
    {
        result = result.ToLowerInvariant();

        if (isFree.Any(result.Contains))
            return true;

        if (isBusy.Any(result.Contains))
            return false;

        return true;
    }
}