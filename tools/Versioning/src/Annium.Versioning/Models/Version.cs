using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Data.Models;

namespace Annium.Versioning.Models;

public sealed record Version : Comparable<Version>
{
    public static Version Empty(VersionChain chain) => new(chain.Major, chain.Minor, 0, string.Empty);

    public static bool TryParse(string raw, out Version version)
    {
        version = new Version(0, 0, 0, string.Empty);

        var parts = raw.Split('.', 3);
        if (parts.Length < 3)
            return false;

        if (!uint.TryParse(parts[0], out var major) || !uint.TryParse(parts[1], out var minor))
            return false;

        // drop scm hash (build metadata), keeping the pre-release part intact
        var rest = parts[2];
        var buildIndex = rest.IndexOf('+');
        if (buildIndex >= 0)
            rest = rest[..buildIndex];

        var patchLength = 0;
        while (patchLength < rest.Length && char.IsAsciiDigit(rest[patchLength]))
            patchLength++;

        if (!uint.TryParse(rest[..patchLength], out var patch))
            return false;

        version = new Version(major, minor, patch, rest[patchLength..]);

        return true;
    }

    public uint Major { get; init; }
    public uint Minor { get; init; }
    public uint Patch { get; init; }
    public string Suffix { get; init; }

    public Version(uint major, uint minor, uint patch, string suffix)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Suffix = suffix;
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}{Suffix}";

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Suffix);

    protected override IEnumerable<Func<Version, IComparable>> GetComparables()
    {
        yield return x => x.Major;
        yield return x => x.Minor;
        yield return x => x.Patch;
        yield return x => new PreRelease(x.Suffix);
    }

    /// <summary>
    /// SemVer 2.0.0 pre-release precedence (§11) over a raw suffix such as <c>-rc.1</c>.
    /// Plain string ordering is wrong here: it sorts <c>1.2.3</c> below <c>1.2.3-rc1</c>,
    /// while a release always outranks its own pre-releases.
    /// </summary>
    private readonly record struct PreRelease(string Suffix) : IComparable<PreRelease>, IComparable
    {
        private string Identifiers => Suffix.StartsWith('-') ? Suffix[1..] : Suffix;

        public int CompareTo(PreRelease other)
        {
            var left = Identifiers;
            var right = other.Identifiers;

            if (left == right)
                return 0;

            // absent pre-release outranks any present one
            if (left.Length == 0)
                return 1;
            if (right.Length == 0)
                return -1;

            var leftParts = left.Split('.');
            var rightParts = right.Split('.');

            for (var i = 0; i < Math.Min(leftParts.Length, rightParts.Length); i++)
            {
                var leftIsNumeric = IsNumeric(leftParts[i]);
                var rightIsNumeric = IsNumeric(rightParts[i]);

                int result;
                if (leftIsNumeric && rightIsNumeric)
                    result = CompareNumeric(leftParts[i], rightParts[i]);
                // numeric identifiers always rank lower than alphanumeric ones
                else if (leftIsNumeric)
                    result = -1;
                else if (rightIsNumeric)
                    result = 1;
                else
                    result = string.CompareOrdinal(leftParts[i], rightParts[i]);

                if (result != 0)
                    return result;
            }

            // all shared identifiers equal — the longer pre-release ranks higher
            return leftParts.Length.CompareTo(rightParts.Length);
        }

        /// <summary>
        /// §11 calls an identifier numeric when it is digits only — with no width limit, so parsing it
        /// into a number would misclassify anything past <c>uint.MaxValue</c> (a millisecond timestamp,
        /// say) as alphanumeric and rank it by ordinal instead.
        /// </summary>
        /// <param name="identifier">The dot-separated pre-release identifier.</param>
        /// <returns>True when the identifier consists only of digits.</returns>
        private static bool IsNumeric(string identifier) => identifier.Length > 0 && identifier.All(char.IsAsciiDigit);

        /// <summary>
        /// Compares two digit-only identifiers by magnitude without parsing them: leading zeros carry
        /// no value, so the longer remainder is the larger number, and equal lengths compare ordinally.
        /// </summary>
        /// <param name="left">The left identifier.</param>
        /// <param name="right">The right identifier.</param>
        /// <returns>A negative value, zero or a positive value, as <see cref="IComparable{T}"/> requires.</returns>
        private static int CompareNumeric(string left, string right)
        {
            var leftDigits = left.TrimStart('0');
            var rightDigits = right.TrimStart('0');

            return leftDigits.Length == rightDigits.Length
                ? string.CompareOrdinal(leftDigits, rightDigits)
                : leftDigits.Length.CompareTo(rightDigits.Length);
        }

        public int CompareTo(object? obj) =>
            obj switch
            {
                null => 1,
                PreRelease other => CompareTo(other),
                _ => throw new ArgumentException($"Cannot compare {nameof(PreRelease)} with {obj.GetType().FullName}"),
            };
    }
}
