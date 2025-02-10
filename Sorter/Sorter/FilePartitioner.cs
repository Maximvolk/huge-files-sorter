namespace Sorter
{
    public class FilePartitioner(string tmpDirectory, ILogger logger)
    {
        private const long ChunkSize = 10 * 1024 * 1024; // 10 MB

        public async Task SplitIntoSortedChunksAsync(string inputFilePath)
        {
            var fileSizeBytes = new FileInfo(inputFilePath).Length;
            if (fileSizeBytes == 0)
            {
                await WriteChunkToTempFileAsync([]);
                return;
            }

            var chunksCount = (int)Math.Ceiling(fileSizeBytes / (double)ChunkSize);

            logger.LogLine(chunksCount == 1
                ? "Sorting in memory (only one chunk)..."
                : "Splitting into chunks...");

            var maxDegreeOfParallelism = Environment.ProcessorCount;
            var chunksCreatedCount = 0;

            logger.FixPosition();

            // Create groups of chunks in parallel
            foreach (var chunkIndices in Enumerable.Range(0, chunksCount).GroupBy(i => i / maxDegreeOfParallelism))
            {
                var tasks = chunkIndices.Select(async i =>
                {
                    var chunk = await ReadChunkAsync(inputFilePath, i);
                    chunk.Sort();

                    await WriteChunkToTempFileAsync(chunk);
                }).ToList();

                await Task.WhenAll(tasks);

                chunksCreatedCount += chunkIndices.Count();
                if (chunksCreatedCount > 1)
                    logger.LogFromFixedPosition($"Created {chunksCreatedCount}/{chunksCount} chunks");
            }

            logger.LogLine("\n");
        }

        private async Task<List<Line>> ReadChunkAsync(string filePath, int chunkIndex)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024);

            long startPosition = chunkIndex * ChunkSize;
            fileStream.Position = startPosition;

            using var streamReader = new StreamReader(fileStream);

            if (chunkIndex > 0 && await streamReader.ReadLineAsync() == null)
                return [];

            var lines = new List<Line>();
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var lineParsed = Line.FromString(line);
                if (lineParsed != null)
                    lines.Add(lineParsed.Value);

                var bytesRead = fileStream.Position - startPosition;
                if (bytesRead > ChunkSize)
                    break;
            }

            return lines;
        }

        private async Task WriteChunkToTempFileAsync(List<Line> chunk)
        {
            var filePath = Path.Combine(tmpDirectory, $"chunk_{Guid.NewGuid()}.txt");

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024);
            await using var streamWriter = new StreamWriter(fileStream);

            foreach (var line in chunk)
                await streamWriter.WriteLineAsync(line.ToString());
        }
    }
}