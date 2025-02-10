namespace Sorter.Tests
{
    [TestFixture]
    public class ChunksMergerTests
    {
        private string _tempDirectory;
        private ChunksMerger _merger;

        private readonly ILogger _logger = new LoggerMock();

        [SetUp]
        public void Setup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            
            _merger = new ChunksMerger(_tempDirectory, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public void SingleChunk_CopiesFileToOutput()
        {
            // Arrange
            var inputLines = new[] { "3. a", "4. a", "1. b" };
            CreateChunkFile(inputLines);
            var outputPath = Path.Combine(_tempDirectory, "output.txt");

            // Act
            _merger.MergeSortedChunks(outputPath);

            // Assert
            var result = File.ReadAllLines(outputPath);
            CollectionAssert.AreEqual(inputLines, result);
        }

        [Test]
        public void MultipleChunks_MergesInSortedOrder()
        {
            // Arrange
            CreateChunkFile(["1. a", "2. d", "4. d"]);
            CreateChunkFile(["2. b", "5. e", "8. h"]);
            CreateChunkFile(["3. c", "6. f", "9. i"]);

            var expectedLines = new[] { "1. a", "2. b", "3. c", "2. d", "4. d", "5. e", "6. f", "8. h", "9. i" };
            var outputPath = Path.Combine(_tempDirectory, "output.txt");

            // Act
            _merger.MergeSortedChunks(outputPath);

            // Assert
            var result = File.ReadAllLines(outputPath);
            CollectionAssert.AreEqual(expectedLines, result);
        }

        [Test]
        public void ChunksWithOverlappingValues_MergesCorrectly()
        {
            // Arrange
            CreateChunkFile(["1. a", "3. c", "5. e"]);
            CreateChunkFile(["2. b", "3. c2", "6. f"]);
            CreateChunkFile(["4. d", "5. e2", "7. g"]);

            var expectedLines = new[] { "1. a", "2. b", "3. c", "3. c2", "4. d", "5. e", "5. e2", "6. f", "7. g" };
            var outputPath = Path.Combine(_tempDirectory, "output.txt");

            // Act
            _merger.MergeSortedChunks(outputPath);

            // Assert
            var result = File.ReadAllLines(outputPath);
            CollectionAssert.AreEqual(expectedLines, result);
        }

        [Test]
        public void EmptyChunks_HandlesCorrectly()
        {
            // Arrange
            CreateChunkFile(["1. a", "2. b"]);
            CreateChunkFile([]);
            CreateChunkFile(["3. c"]);

            var expectedLines = new[] { "1. a", "2. b", "3. c" };
            var outputPath = Path.Combine(_tempDirectory, "output.txt");

            // Act
            _merger.MergeSortedChunks(outputPath);

            // Assert
            var result = File.ReadAllLines(outputPath);
            CollectionAssert.AreEqual(expectedLines, result);
        }

        private void CreateChunkFile(string[] lines)
        {
            var path = Path.Combine(_tempDirectory, $"chunk_{Guid.NewGuid()}.txt");
            File.WriteAllLines(path, lines);
        }
    }

}
