using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Social.Telegram.Obsolete.Models;

public abstract class BaseTelegramModel<T> : ITelegramModel
    where T : BaseTelegramModel<T>
{
    private static readonly Dictionary<string, Func<T, string>> _resolvers = new();

    static BaseTelegramModel()
    {
        var pairs = typeof(T)
            .GetTypeInfo()
            .GetProperties()
            .Select(e => (key: e.GetCustomAttributes<FormatAttribute>(), property: e))
            .Where(e => e.key.Any())
            .SelectMany(e =>
                e.key.Select(key => (key, resolver: GetResolver(key.TargetFormat, e.property.GetGetMethod().NotNull())))
            )
            .ToArray();

        foreach (var (key, resolver) in pairs)
            _resolvers.Add(key.Format, resolver);
    }

    private static Func<object?, string> GetResolver(string format, MethodInfo getter)
    {
        var type = getter.ReturnType;

        // if Nullable
        if (type.IsGenericType && typeof(Nullable<>).MakeGenericType(type.GetGenericArguments()).IsAssignableFrom(type))
        {
            var resolver = GetResolver(format, type.GetProperty("Value").NotNull().GetGetMethod().NotNull());
            return obj =>
            {
                obj = getter.Invoke(obj, []);

                return obj == null ? string.Empty : resolver(obj);
            };
        }

        // if enum
        if (typeof(Enum).IsAssignableFrom(type))
            return obj =>
            {
                var value = getter.Invoke(obj, [])?.ToString();
                var label = type.GetField(value.NotNull())?.GetCustomAttribute<LabelAttribute>()?.Label;

                return label ?? value ?? string.Empty;
            };

        // if DateTime
        if ((type == typeof(DateTime) || type == typeof(DateTimeOffset)))
            return GetFormattedStringResolver();

        // if nested formatting option given
        return typeof(BaseTelegramModel<>).MakeGenericType(type).IsAssignableFrom(type)
            ? GetFormattedStringResolver()
            : obj => getter.Invoke(obj, [])?.ToString() ?? string.Empty;

        Func<object?, string> GetFormattedStringResolver()
        {
            var toString = type.GetMethod(nameof(ToString), [typeof(string)]).NotNull();

            return obj =>
            {
                var value = getter.Invoke(obj, []);
                return value == null ? string.Empty : toString.Invoke(value, [format])?.ToString() ?? string.Empty;
            };
        }
    }

    public string ToString(string format)
    {
        var result = format;
        foreach (var (key, resolver) in _resolvers.Where(e => format.Contains(e.Key)))
            result = result.Replace($"{{{key}}}", resolver((T)this));

        return result;
    }

    protected string ToString(string format, params string[] formatParts) =>
        ToString(string.Join(Environment.NewLine, new[] { format }.Concat(formatParts).OfType<string>()));

    public abstract string ToFullString();
}
