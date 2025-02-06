namespace Sorter
{
    public class Sorter
    {
        private const int _chunkSize = 10 * 1024 * 1024; // 10 MB
        private const int _maxOpenChunks = 100;
        private const string _tmpDirectory = "sorterTmp";

        public async Task SortAsync(string inputFilePath, string outputFilePath)
        {
            if (Directory.Exists(_tmpDirectory))
                Directory.Delete(_tmpDirectory, true);

            Directory.CreateDirectory(_tmpDirectory);

            var fileSizeBytes = new FileInfo(inputFilePath).Length;
            var chunksCount = (int)Math.Ceiling(fileSizeBytes / (double)_chunkSize);

            var maxDegreeOfParallelism = Environment.ProcessorCount;

            foreach (var chunkIndices in Enumerable.Range(0, chunksCount).GroupBy(i => i / maxDegreeOfParallelism))
            {
                var tasks = chunkIndices.Select(i => SplitIntoChunksAndSortAsync(inputFilePath, i)).ToList();
                await Task.WhenAll(tasks);
            }

            MergeSortedChunks(outputFilePath);
        }

        private async Task SplitIntoChunksAndSortAsync(string inputFilePath, int chunkIndex)
        {
            var chunk = await ReadChunkAsync(inputFilePath, chunkIndex);
            chunk.Sort();

            await WriteChunkToTempFileAsync(chunk);
        }

        private async Task<List<Line>> ReadChunkAsync(string filePath, int chunkIndex)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024);

            var startPosition = chunkIndex * _chunkSize;
            fileStream.Position = startPosition;

            using var streamReader = new StreamReader(fileStream);

            if (chunkIndex > 0 && await streamReader.ReadLineAsync() == null)
                return [];

            var bytesRead = 0L;

            var lines = new List<Line>();
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var lineParsed = Line.FromString(line);
                if (lineParsed != null)
                    lines.Add(lineParsed.Value);

                bytesRead = fileStream.Position - startPosition;
                if (bytesRead > _chunkSize)
                    break;
            }

            return lines;
        }

        private async Task WriteChunkToTempFileAsync(List<Line> chunk)
        {
            var filePath = Path.Combine(_tmpDirectory, $"chunk_{Guid.NewGuid()}.txt");

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024);
            await using var streamWriter = new StreamWriter(fileStream);

            foreach (var line in chunk)
                await streamWriter.WriteLineAsync(line.ToString());
        }

        // Merge is intentionally synchronous to avoid async overhead (since there is no concurrency)
        private void MergeSortedChunks(string outputFilePath)
        {
            List<string> chunks;

            while ((chunks = Directory.EnumerateFiles(_tmpDirectory).ToList()).Count > 1)
            {
                for (var i = 0; i < chunks.Count; i += _maxOpenChunks)
                {
                    var chunksBatch = chunks.Skip(i).Take(_maxOpenChunks).ToList();
                    MergeSortedChunksBatch(chunksBatch);
                }
            }

            File.Move(chunks[0], outputFilePath, true);
        }

        private void MergeSortedChunksBatch(List<string> chunks)
        {
            var readStreams = new List<FileStream>(chunks.Count);
            var readers = new List<StreamReader>(chunks.Count);

            try
            {
                var priorityQueue = new PriorityQueue<MergeItem, Line>(chunks.Count);

                // Enqueue first line of all chunks
                for (var i = 0; i < chunks.Count; i++)
                {
                    var readStream = new FileStream(chunks[i], FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
                    var reader = new StreamReader(readStream);

                    readStreams.Add(readStream);
                    readers.Add(reader);

                    var line = reader.ReadLine();
                    if (line == null)
                        continue;

                    // Temporary files contain only correct lines, so it won't be null
                    var parsedLine = Line.FromString(line)!.Value;
                    priorityQueue.Enqueue(new MergeItem(i, parsedLine), parsedLine);
                }

                var mergeResultPath = Path.Combine(_tmpDirectory, $"chunk_{Guid.NewGuid()}.txt");

                using var fileStream = new FileStream(mergeResultPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, FileOptions.SequentialScan);
                using var streamWriter = new StreamWriter(fileStream);

                while (priorityQueue.Count > 0)
                {
                    var mergeItem = priorityQueue.Dequeue();
                    streamWriter.WriteLine(mergeItem.Line.ToString());

                    // Read next line from dequeued item source file
                    var line = readers[mergeItem.ReaderIndex].ReadLine();
                    if (line == null)
                        continue;

                    var parsedLine = Line.FromString(line)!.Value;
                    priorityQueue.Enqueue(new MergeItem(mergeItem.ReaderIndex, parsedLine), parsedLine);
                }
            }
            finally
            {
                for (var i = 0; i < readers.Count; i++)
                {
                    readers[i].Dispose();
                    readStreams[i].Dispose();
                }

                foreach (var chunk in chunks)
                    File.Delete(chunk);
            }
        }
    }
}
