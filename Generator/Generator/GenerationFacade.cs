using System.Text;
using System.Threading.Channels;

using Generator.Data;
using Generator.Output;

namespace Generator
{
    public sealed class GenerationFacade : IAsyncDisposable
    {
        private const int ChannelCapacity = 1000;
        private const int BatchSize = 1000;

        private readonly IDataProvider _dataProvider;
        private readonly IOutputWriter _outputWriter;
        private readonly IProgressObserver _progressObserver;
        private readonly long _targetSizeInBytes;
        
        private readonly int _newLineLength = Encoding.UTF8.GetByteCount(Environment.NewLine);

        internal GenerationFacade(IDataProvider dataProvider, IOutputWriter outputWriter,
            long targetSizeInBytes, IProgressObserver progressObserver)
        {
            _dataProvider = dataProvider;
            _outputWriter = outputWriter;
            _targetSizeInBytes = targetSizeInBytes;
            _progressObserver = progressObserver;
        }

        public async Task GenerateAsync()
        {
            var channel = Channel.CreateBounded<List<string>>(new BoundedChannelOptions(ChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });

            // Start consumer
            var writerTask = WriteOutputAsync(channel.Reader);

            // Start producers. Do not parallelise if file size is less than 100 MB
            var producersCount = Math.Min(4, Environment.ProcessorCount);
            if (_targetSizeInBytes < 100 * 1024 * 1024)
                producersCount = 1;

            var producerTasks = Enumerable.Range(0, producersCount)
                .Select(_ => GenerateLinesAsync(channel.Writer, _targetSizeInBytes / producersCount))
                .ToList();

            await Task.WhenAll(producerTasks);
            channel.Writer.Complete();

            // Wait for the writer to finish
            await writerTask;
        }

        private async Task GenerateLinesAsync(ChannelWriter<List<string>> writer, long targetSizeInBytes)
        {
            var currentSize = 0L;

            while (currentSize < targetSizeInBytes)
            {
                var batch = new List<string>(BatchSize);

                for (var i = 0; i < BatchSize && currentSize < targetSizeInBytes; i++)
                {
                    var line = _dataProvider.GetLine();
                    currentSize += Encoding.UTF8.GetByteCount(line) + _newLineLength;

                    batch.Add(line);
                }
                
                await writer.WriteAsync(batch);
            }
        }

        private async Task WriteOutputAsync(ChannelReader<List<string>> reader)
        {
            var bytesWritten = 0L;
            
            await foreach (var batch in reader.ReadAllAsync())
            {
                foreach (var line in batch)
                {
                    await _outputWriter.WriteLineAsync(line);
                    bytesWritten += Encoding.UTF8.GetByteCount(line) + _newLineLength;
                }

                _progressObserver.ObserveProgress(bytesWritten / (double)_targetSizeInBytes);
            }
            
            _progressObserver.Finish();
        }

        public async ValueTask DisposeAsync()
        {
            await _outputWriter.DisposeAsync();
        }
    }
}
