namespace Sorter
{
    public class SortingFacade
    {
        private const int ChunkSize = 10 * 1024 * 1024; // 10 MB
        private const string TmpDirectory = "sorterTmp";

        private readonly Merger _merger = new(TmpDirectory);
        private readonly SortingPartitioner _partitioner = new(ChunkSize, TmpDirectory);
        
        public async Task SortAsync(string inputFilePath, string outputFilePath)
        {
            if (Directory.Exists(TmpDirectory))
                Directory.Delete(TmpDirectory, true);

            Directory.CreateDirectory(TmpDirectory);

            var fileSizeBytes = new FileInfo(inputFilePath).Length;
            var chunksCount = (int)Math.Ceiling(fileSizeBytes / (double)ChunkSize);

            var maxDegreeOfParallelism = Environment.ProcessorCount;

            foreach (var chunkIndices in Enumerable.Range(0, chunksCount).GroupBy(i => i / maxDegreeOfParallelism))
            {
                var tasks = chunkIndices
                    .Select(i => _partitioner.SplitIntoSortedChunksAsync(inputFilePath, i))
                    .ToList();
                
                await Task.WhenAll(tasks);
            }

            _merger.MergeSortedChunks(outputFilePath);
        }
    }
}
