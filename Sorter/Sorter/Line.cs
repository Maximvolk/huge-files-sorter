namespace Sorter
{
    internal readonly struct Line(long number, string str) : IComparable<Line>
    {
        public long Number { get; } = number;
        public string String { get; } = str;

        public static Line? FromString(string line)
        {
            var dotIndex = line.IndexOf('.');
            if (dotIndex == -1)
                return null;

            if (!long.TryParse(line.AsSpan()[..dotIndex], out var number))
                return null;

            if (line.Length <= dotIndex + 2)
                return null;

            return new Line(number, line[(dotIndex + 2)..]);
        }

        public override string ToString()
        => $"{Number}. {String}";

        public int CompareTo(Line other)
        {
            var cmp = string.CompareOrdinal(String, other.String);
            if (cmp != 0)
                return cmp;

            return Number.CompareTo(other.Number);
        }
    }
}
