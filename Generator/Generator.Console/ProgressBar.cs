namespace Generator.Console
{
    public class ProgressBar : IProgressObserver
    {
        private const int BarWidth = 24;
        private const char ProgressChar = '#';
        private const char EmptyChar = '-';

        private readonly int _cursorTop = System.Console.CursorTop;
        private readonly int _cursorLeft = System.Console.CursorLeft;

        private int _currentProgressCharsCount = 0;

        public ProgressBar()
        {
            // Draw empty bar
            System.Console.Write("[");

            for (var i = 0; i < BarWidth; i++)
                System.Console.Write(EmptyChar);

            System.Console.Write("]");
        }

        public void ObserveProgress(double percent)
        {
            var progress = (int)(percent * 100);
            progress = Math.Max(0, Math.Min(100, progress));

            // Calculate how many progress chars must be shown
            var progressChars = (int)(BarWidth * (progress / 100.0));
            if (progressChars == _currentProgressCharsCount)
                return;

            System.Console.SetCursorPosition(_cursorLeft + 1 + _currentProgressCharsCount, _cursorTop);

            for (var i = _currentProgressCharsCount; i < progressChars; i++)
                System.Console.Write(ProgressChar);

            System.Console.SetCursorPosition(_cursorLeft + BarWidth + 2, _cursorTop);
            System.Console.Write($" {progress}%");

            _currentProgressCharsCount = progressChars;
        }

        public void Finish()
        {
            ObserveProgress(1);
            System.Console.Write("\n");
        }
    }
}