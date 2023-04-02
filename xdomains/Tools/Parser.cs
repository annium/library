using System.Linq;

namespace xdomains.Tools;

public class Parser
{
    private readonly string[] _isFree = {
        "available for registration",
        "not been registered",
        "not found",
        "not registered",
    };

    private readonly string[] _isBusy = {
        "creation date",
        "prohibited",
        "registered",
        "registrant",
        "reserved",
    };

    public bool IsFree(string result)
    {
        result = result.ToLowerInvariant();

        if (_isFree.Any(result.Contains))
            return true;

        if (_isBusy.Any(result.Contains))
            return false;

        return true;
    }
}