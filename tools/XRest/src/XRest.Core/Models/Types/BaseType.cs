using System;
using System.Collections.Generic;
using NodaTime;
using XRest.Core.Extensions;

namespace XRest.Core.Models.Types;

public static class BaseType
{
    public static StructModel Boolean { get; } = new StructModel(typeof(bool).GetNamespace(), "boolean", System.Array.Empty<StructModel>());
    public static StructModel String { get; } = new StructModel(typeof(string).GetNamespace(), "string");
    public static StructModel Byte { get; } = new StructModel(typeof(byte).GetNamespace(), "byte");
    public static StructModel SByte { get; } = new StructModel(typeof(sbyte).GetNamespace(), "sbyte");
    public static StructModel Int { get; } = new StructModel(typeof(int).GetNamespace(), "int");
    public static StructModel UInt { get; } = new StructModel(typeof(uint).GetNamespace(), "uint");
    public static StructModel Long { get; } = new StructModel(typeof(long).GetNamespace(), "long");
    public static StructModel ULong { get; } = new StructModel(typeof(ulong).GetNamespace(), "ulong");
    public static StructModel DateTime { get; } = new StructModel(typeof(DateTime).GetNamespace(), "DateTime");
    public static StructModel Date { get; } = new StructModel(typeof(DateOnly).GetNamespace(), "Date");
    public static StructModel Time { get; } = new StructModel(typeof(TimeOnly).GetNamespace(), "Time");
    public static StructModel Instant { get; } = new StructModel(typeof(Instant).GetNamespace(), "Instant");
    public static StructModel Duration { get; } = new StructModel(typeof(Duration).GetNamespace(), "Duration");
    public static StructModel Void { get; } = new StructModel(typeof(bool).GetNamespace(), "void");
    public static StructModel Array { get; } = new StructModel(typeof(Array).GetNamespace(), "Array", new ITypeModel[] { new GenericParameterModel("T") });
    public static StructModel Record { get; } = new StructModel(typeof(Dictionary<,>).GetNamespace(), "Record", new ITypeModel[] { new GenericParameterModel("TKey"), new GenericParameterModel("TValue") });
}