using System;

namespace Xmg.Core.Models
{
    public class TableColumn
    {
        public string Name { get; }
        public Type Type { get; }
        public int? Length { get; }
        public bool CanBeNull { get; }
        public bool IsIdentity { get; }

        public TableColumn(
            string name,
            Type type,
            int? length,
            bool canBeNull,
            bool isIdentity
        )
        {
            Name = name;
            Type = type;
            Length = length;
            CanBeNull = canBeNull;
            IsIdentity = isIdentity;
        }
    }
}