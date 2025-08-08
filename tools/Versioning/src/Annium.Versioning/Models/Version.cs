using System;
using System.Collections.Generic;
using Annium.Data.Models;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Annium.Versioning.Models;

public sealed record Version : Comparable<Version>
{
    public static Version Empty { get; } = new(0, 0, 0, string.Empty);

    public static bool TryParse(string raw, out Version version)
    {
        version = Empty;

        var parts = raw.Split('.', 3);
        if (parts.Length < 3)
            return false;

        try
        {
            var major = uint.Parse(parts[0]);
            var minor = uint.Parse(parts[1]);

            var patchParts = parts[2].Split('.', '-', '+');
            var patch = uint.Parse(patchParts[0]);

            // drop scm hash
            var hasHash = parts[2].Contains('+');
            var suffix = patchParts.Length == 1 || hasHash ? string.Empty : parts[2][patchParts[0].Length..];

            version = new Version(major, minor, patch, suffix);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public uint Major { get; private set; }
    public uint Minor { get; private set; }
    public uint Patch { get; private set; }
    public string Suffix { get; private set; }

    public Version(uint major, uint minor, uint patch, string suffix)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Suffix = suffix;
    }

    public void Update(Version version)
    {
        Major = version.Major;
        Minor = version.Minor;
        Patch = version.Patch;
        Suffix = version.Suffix;
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}{Suffix}";

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Suffix);

    protected override IEnumerable<Func<Version, IComparable>> GetComparables()
    {
        yield return x => x.Major;
        yield return x => x.Minor;
        yield return x => x.Patch;
        yield return x => x.Suffix;
    }
}
