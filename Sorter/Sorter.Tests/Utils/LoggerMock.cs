namespace Sorter.Tests
{
    class LoggerMock : ILogger
    {
        public void LogLine(string message) { }
        public void FixPosition() { }
        public void LogFromFixedPosition(string message) { }
    }
}
