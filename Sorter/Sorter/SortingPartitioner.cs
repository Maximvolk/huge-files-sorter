namespace Sorter
{
    public class SortingPartitioner(int chunkSize, string tmpDirectory)
    {
        public async Task SplitIntoSortedChunksAsync(string inputFilePath, int chunkIndex)
        {
            var chunk = await ReadChunkAsync(inputFilePath, chunkIndex);
            chunk.Sort();

            await WriteChunkToTempFileAsync(chunk);
        }

        private async Task<List<Line>> ReadChunkAsync(string filePath, int chunkIndex)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024);

            var startPosition = chunkIndex * chunkSize;
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
                if (bytesRead > chunkSize)
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