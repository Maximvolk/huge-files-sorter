namespace Sorter.Tests
{
    class LinesComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            var xParts = x!.Split(". ");
            var yParts = y!.Split(". ");

            var cmp = string.CompareOrdinal(xParts[1], yParts[1]);
            if (cmp != 0)
                return cmp;

            return long.Parse(xParts[0]).CompareTo(long.Parse(yParts[0]));
        }
    }
}
