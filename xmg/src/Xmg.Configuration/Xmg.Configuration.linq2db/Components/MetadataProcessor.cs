using System;
using System.Collections.Generic;
using System.Linq;
using Annium.linq2db.Extensions;
using Annium.Reflection;
using LinqToDB.Internal.Extensions;
using Xmg.Core.Models;
using LDataType = LinqToDB.DataType;

namespace Xmg.Configuration.linq2db.Components;

internal class MetadataProcessor : IMetadataProcessor
{
    public Database Process(DatabaseMetadata database)
    {
        var schemas = new Dictionary<string, List<Table>>();
        foreach (var (schema, table) in database.Tables.Values.Select(x => ProcessTable(database, x)))
        {
            if (schemas.TryGetValue(schema, out var tables))
                tables.Add(table);
            else
                schemas[schema] = [table];
        }

        return new Database(schemas.Select(x => new Schema(x.Key, x.Value)).ToArray());
    }

    private (string schema, Table table) ProcessTable(DatabaseMetadata database, TableMetadata table)
    {
        var columns = table.Columns.Values.Select(x => ProcessColumn(table, x)!).Where(x => x != null!).ToArray();

        var primaryKey = ResolvePrimaryKey(table.Schema, table.Name, table.Columns.Values);

        var foreignKeys = table
            .Columns.Values.Select(x => ProcessForeignKey(database, table, x)!)
            .Where(x => x != null!)
            .ToArray();

        return (table.Schema ?? string.Empty, new Table(table.Name, columns, primaryKey, [], foreignKeys));
    }

    private TableColumn? ProcessColumn(TableMetadata table, ColumnMetadata column)
    {
        if (column.Association != null)
            return null;

        var dataType = column.DataType;
        var nullable = column.Nullable;

        var valueType = dataType?.DataType is null
            ? MapType(table.Name, column.Name, column.Member.GetMemberType(), column.Attribute.Length > 0)
            : MapDataType(table.Name, column.Name, dataType.DataType.Value);

        return new TableColumn(
            column.Name,
            valueType,
            column.Attribute.Length > 0 ? column.Attribute.Length : null,
            nullable?.CanBeNull ?? false
        );
    }

    private TablePrimaryKeyConstraint? ResolvePrimaryKey(
        string? schema,
        string table,
        IEnumerable<ColumnMetadata> columns
    )
    {
        var primaryKeyColumns = columns
            .Where(x => x.Attribute.IsColumn && x.PrimaryKey != null)
            .OrderBy(x => x.PrimaryKey!.Order)
            .Select(x => x.Name)
            .ToArray();

        return primaryKeyColumns.Length > 0 ? new TablePrimaryKeyConstraint(schema, table, primaryKeyColumns) : null;
    }

    private TableForeignKeyConstraint? ProcessForeignKey(
        DatabaseMetadata db,
        TableMetadata table,
        ColumnMetadata column
    )
    {
        if (column.Association is null)
            return null;

        // foreign key will be created only if there's Relationship.OneToOne (thus, ThisKey, OtherKey will be defined)
        // if (column.Association.Relationship != Relationship.OneToOne)
        //     return null;

        var foreignColumn =
            table.Columns.Values.SingleOrDefault(x => x.Member.Name == column.Association.ThisKey)
            ?? throw new InvalidOperationException(
                $"Foreign table '{table}' has no key column '{column.Association.ThisKey}'. Ensure table '{table}' configuration is valid."
            );

        var targetType = column.Member.GetMemberType();
        var primaryTable =
            db.Tables.Values.SingleOrDefault(x => x.Type == targetType) ?? throw new InvalidOperationException(
                $"Foreign table refers to value of type '{targetType}', that was not discovered during discovery process. Ensure type '{targetType}' configuration is declared."
            );
        var primaryColumn =
            primaryTable.Columns.Values.SingleOrDefault(x => x.Member.Name == column.Association.OtherKey)
            ?? throw new InvalidOperationException(
                $"Primary table '{primaryTable}' has no column, mapped to '{table}'.'{foreignColumn.Name}'. Ensure table '{primaryTable}' configuration is valid."
            );

        return new TableForeignKeyConstraint(
            table.Schema,
            table.Name,
            foreignColumn.Name,
            primaryTable.Schema,
            primaryTable.Name,
            primaryColumn.Name
        );
    }

    private DataType MapType(string table, string column, Type type, bool hasLength)
    {
        if (type == typeof(char))
            return DataType.NChar;

        if (type == typeof(string))
            return hasLength ? DataType.NChar : DataType.NVarChar;

        if (type.GetTargetImplementation(typeof(IEnumerable<byte>)) != null)
            return DataType.Blob;

        if (type == typeof(bool))
            return DataType.Boolean;

        if (type == typeof(Guid))
            return DataType.Guid;

        if (type == typeof(sbyte))
            return DataType.SByte;

        if (type == typeof(short))
            return DataType.Int16;

        if (type == typeof(int))
            return DataType.Int32;

        if (type == typeof(long))
            return DataType.Int64;

        if (type == typeof(byte))
            return DataType.Byte;

        if (type == typeof(ushort))
            return DataType.UInt16;

        if (type == typeof(uint))
            return DataType.UInt32;

        if (type == typeof(ulong))
            return DataType.UInt64;

        if (type == typeof(float))
            return DataType.Single;

        if (type == typeof(double))
            return DataType.Double;

        if (type == typeof(decimal))
            return DataType.Decimal;

        if (type == typeof(DateTime))
            return DataType.DateTime;

        if (type == typeof(DateTimeOffset))
            return DataType.DateTimeOffset;

        if (type.IsEnum)
            return MapType(table, column, Enum.GetUnderlyingType(type), false);

        throw new NotSupportedException(
            $"Column '{table}'.'{column}' has type '{type}', that is not implicitly mappable. Explicitly specify the column DataType."
        );
    }

    private DataType MapDataType(string table, string column, LDataType type) =>
        type switch
        {
            LDataType.Char => DataType.Char,
            LDataType.VarChar => DataType.VarChar,
            LDataType.Text => DataType.Text,
            LDataType.NChar => DataType.NChar,
            LDataType.NVarChar => DataType.NVarChar,
            LDataType.NText => DataType.NText,
            LDataType.Binary => DataType.Blob,
            LDataType.VarBinary => DataType.Blob,
            LDataType.Blob => DataType.Blob,
            LDataType.Image => DataType.Blob,
            LDataType.Boolean => DataType.Boolean,
            LDataType.Guid => DataType.Guid,
            LDataType.SByte => DataType.SByte,
            LDataType.Int16 => DataType.Int16,
            LDataType.Int32 => DataType.Int32,
            LDataType.Int64 => DataType.Int64,
            LDataType.Byte => DataType.Byte,
            LDataType.UInt16 => DataType.UInt16,
            LDataType.UInt32 => DataType.UInt32,
            LDataType.UInt64 => DataType.UInt64,
            LDataType.Single => DataType.Single,
            LDataType.Double => DataType.Double,
            LDataType.Decimal => DataType.Decimal,
            LDataType.Money => DataType.Money,
            LDataType.SmallMoney => DataType.Money,
            LDataType.Date => DataType.Date,
            LDataType.Time => DataType.Time,
            LDataType.DateTime => DataType.DateTime,
            LDataType.DateTime2 => DataType.DateTime2,
            LDataType.SmallDateTime => DataType.DateTime,
            LDataType.DateTimeOffset => DataType.DateTimeOffset,
            LDataType.Timestamp => DataType.Timestamp,
            LDataType.BitArray => DataType.Blob,
            _ => throw new NotSupportedException($"Column '{table}'.'{column}' has type '{type}', that"),
        };
}
