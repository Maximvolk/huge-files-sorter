namespace Sorter
{
    internal readonly struct MergeItem(int readerIndex, Line line)
    {
        public int ReaderIndex { get; } = readerIndex;
        public Line Line { get; } = line;
    }
}
