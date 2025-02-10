namespace Sorter.Console
{
    internal class ConsoleLogger : ILogger
    {
        private int _originalCursorLeft;
        private int _originalCursorTop;

        public void LogLine(string message)
        {
            System.Console.WriteLine(message);
        }

        public void FixPosition()
        {
            _originalCursorLeft = System.Console.CursorLeft;
            _originalCursorTop = System.Console.CursorTop;
        }

        public void LogFromFixedPosition(string message)
        {
            System.Console.SetCursorPosition(_originalCursorLeft, _originalCursorTop);
            System.Console.Write(message);
        }
    }
}
