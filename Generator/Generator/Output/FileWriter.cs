namespace Generator.Output
{
    public sealed class FileWriter : IOutputWriter
    {
        private const int _flushBatchSize = 5000;
        private int _currentBatchSize = 0;

        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;

        public FileWriter(string filePath)
        {
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024);
            _streamWriter = new StreamWriter(_fileStream);
        }

        public async Task WriteLineAsync(string line)
        {
            await _streamWriter.WriteLineAsync(line);
            _currentBatchSize++;

            if (_currentBatchSize < _flushBatchSize)
                return;

            _currentBatchSize = 0;
            await _streamWriter.FlushAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentBatchSize > 0)
                await _streamWriter.FlushAsync();

            await _streamWriter.DisposeAsync();
            await _fileStream.DisposeAsync();
        }
    }
}
