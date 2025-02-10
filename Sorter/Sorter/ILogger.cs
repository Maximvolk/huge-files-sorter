namespace Sorter
{
    public interface ILogger
    {
        void LogLine(string message);
        void FixPosition();
        void LogFromFixedPosition(string message);
    }
}
