namespace Xmg.Core.Models
{
    public class TableColumn
    {
        public string Name { get; }
        public DataType Type { get; }
        public int? Length { get; }
        public bool CanBeNull { get; }

        public TableColumn(
            string name,
            DataType type,
            int? length,
            bool canBeNull
        )
        {
            Name = name;
            Type = type;
            Length = length;
            CanBeNull = canBeNull;
        }

        public override string ToString() => $"{Type} {Name}";
    }
}