using System;

namespace Annium.Social.Telegram.Obsolete.Models;

[AttributeUsage(AttributeTargets.Field)]
public class LabelAttribute : Attribute
{
    public string Label { get; }

    public LabelAttribute(string label)
    {
        Label = label;
    }
}
