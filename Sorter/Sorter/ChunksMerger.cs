namespace Sorter
{
    public class ChunksMerger(string tmpDirectory)
    {
        private const int MaxOpenChunks = 100;

        // Merge is intentionally synchronous to avoid async overhead (since there is no concurrency)
        public void MergeSortedChunks(string outputFilePath)
        {
            List<string> chunks;

            while ((chunks = Directory.EnumerateFiles(tmpDirectory).ToList()).Count > 1)
            {
                for (var i = 0; i < chunks.Count; i += MaxOpenChunks)
                {
                    var chunksBatch = chunks.Skip(i).Take(MaxOpenChunks).ToList();
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
                    var readStream = new FileStream(chunks[i], FileMode.Open, FileAccess.Read, FileShare.Read,
                        64 * 1024, FileOptions.SequentialScan);
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

                var mergeResultPath = Path.Combine(tmpDirectory, $"chunk_{Guid.NewGuid()}.txt");

                using var fileStream = new FileStream(mergeResultPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, 64 * 1024, FileOptions.SequentialScan);
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