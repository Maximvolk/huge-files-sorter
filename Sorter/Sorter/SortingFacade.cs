namespace Sorter
{
    public class SortingFacade
    {
        private const string TmpDirectory = "sorterTmp";

        private readonly ChunksMerger _chunksMerger = new(TmpDirectory);
        private readonly FilePartitioner _partitioner = new(TmpDirectory);
        
        public async Task SortAsync(string inputFilePath, string outputFilePath)
        {
            if (Directory.Exists(TmpDirectory))
                Directory.Delete(TmpDirectory, true);

            Directory.CreateDirectory(TmpDirectory);

            try
            {
                await _partitioner.SplitIntoSortedChunksAsync(inputFilePath);
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
