using System;

namespace Annium.Integrations.Social.Telegram.Obsolete.Models;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class FormatAttribute : Attribute
{
    public string Format { get; }

    public string TargetFormat { get; } = string.Empty;

    public FormatAttribute(string format)
    {
        Format = format;
    }

    public FormatAttribute(string format, string targetFormat)
    {
        Format = format;
        TargetFormat = targetFormat;
    }
}
