namespace Sorter
{
    public class SortingFacade(ILogger logger)
    {
        private const string TmpDirectory = "sorterTmp";

        private readonly ChunksMerger _chunksMerger = new(TmpDirectory, logger);
        private readonly FilePartitioner _filePartitioner = new(TmpDirectory, logger);
        
        public async Task SortAsync(string inputFilePath, string outputFilePath)
        {
            if (Directory.Exists(TmpDirectory))
                Directory.Delete(TmpDirectory, true);

            Directory.CreateDirectory(TmpDirectory);

            try
            {
                await _filePartitioner.SplitIntoSortedChunksAsync(inputFilePath);
                _chunksMerger.MergeSortedChunks(outputFilePath);
            }
            finally
            {
                if (Directory.Exists(TmpDirectory))
                    Directory.Delete(TmpDirectory, true);
            }
        }
    }
}
