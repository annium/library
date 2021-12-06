using System;
using System.Text;
using Xmg.Core.Models;

namespace Xmg.Migration.FluentMigrator.Views.Syntax;

internal class ColumnSyntax
{
    private readonly TableColumn _column;

    public ColumnSyntax(TableColumn column)
    {
        _column = column;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($".As{GetTypeString()}");
        sb.Append(_column.CanBeNull ? ".Nullable()" : ".NotNullable()");

        return sb.ToString();
    }

    private string GetTypeString() => _column.Type switch
    {
        DataType.Blob           => "Binary()",
        DataType.Boolean        => "Boolean()",
        DataType.Byte           => "Byte()",
        DataType.Char           => "String(1)",
        DataType.Date           => "Date()",
        DataType.Decimal        => "Decimal()",
        DataType.Double         => "Double()",
        DataType.Guid           => "Guid()",
        DataType.Int16          => "Int16()",
        DataType.Int32          => "Int32()",
        DataType.Int64          => "Int64()",
        DataType.Money          => "Currency()",
        DataType.Single         => "Float()",
        DataType.Text           => _column.Length.HasValue ? $"String({_column.Length.Value})" : "String()",
        DataType.Time           => "Time()",
        DataType.Timestamp      => "Binary()",
        DataType.DateTime       => "DateTime()",
        DataType.DateTime2      => "DateTime2()",
        DataType.DateTimeOffset => "DateTimeOffset()",
        DataType.NChar          => "String(1)",
        DataType.NText          => _column.Length.HasValue ? $"String({_column.Length.Value})" : "String()",
        DataType.NVarChar       => _column.Length.HasValue ? $"String({_column.Length.Value})" : "String()",
        DataType.SByte          => "Int16()",
        DataType.SmallMoney     => "Currency()",
        DataType.VarChar        => _column.Length.HasValue ? $"String({_column.Length.Value})" : "String()",
        DataType.UInt16         => throw new NotSupportedException($"Column '{_column.Name}' type '{_column.Type}' is not supported"),
        DataType.UInt32         => throw new NotSupportedException($"Column '{_column.Name}' type '{_column.Type}' is not supported"),
        DataType.UInt64         => throw new NotSupportedException($"Column '{_column.Name}' type '{_column.Type}' is not supported"),
        _                       => throw new NotSupportedException($"Column '{_column.Name}' type '{_column.Type}' is not supported")
    };
}