using System.Text.RegularExpressions;

namespace Annium.Versioning.Models;

public readonly record struct VersionChain(uint Major, uint Minor)
{
    public static VersionChain Empty { get; } = new(0, 0);
    public static VersionChain Minimal { get; } = new(0, 1);
    private static readonly Regex _regex = new(@"^(\d+)\.(\d+)$");

    public static bool TryParse(string? input, out VersionChain versionChain)
    {
        versionChain = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var match = _regex.Match(input);
        if (!match.Success)
            return false;

        if (!uint.TryParse(match.Groups[1].Value, out var major))
            return false;

        if (!uint.TryParse(match.Groups[2].Value, out var minor))
            return false;

        versionChain = new VersionChain(major, minor);
        return true;
    }

    public override string ToString() => $"{Major}.{Minor}";
}
