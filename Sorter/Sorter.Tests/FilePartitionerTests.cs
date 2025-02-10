namespace Sorter.Tests
{
    [TestFixture]
    public class FilePartitionerTests
    {
        private string _tempDirectory;
        private FilePartitioner _partitioner;

        private readonly ILogger _logger = new LoggerMock();

        [SetUp]
        public void Setup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            _partitioner = new FilePartitioner(_tempDirectory, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public async Task SmallFile_CreatesOneChunk()
        {
            // Arrange
            var inputFile = Path.Combine(_tempDirectory, "small.txt");
            await File.WriteAllLinesAsync(inputFile, new[]
            {
                "3. bbb",
                "5. aaa",
                "1. aaa",
                "2. ccc"
            });

            // Act
            await _partitioner.SplitIntoSortedChunksAsync(inputFile);

            // Assert
            var chunkFiles = Directory.GetFiles(_tempDirectory, "chunk_*.txt");
            Assert.That(chunkFiles, Has.Length.EqualTo(1), "Small file should create only one chunk");

            // Verify chunk is sorted
            var lines = await File.ReadAllLinesAsync(chunkFiles[0]);
            Assert.That(lines, Is.Ordered.Using(new LinesComparer()));
        }

        [Test]
        public async Task LargeFile_CreatesMultipleChunks()
        {
            // Arrange
            var inputFile = Path.Combine(_tempDirectory, "large.txt");
            await CreateLargeFile(inputFile, 15 * 1024 * 1024); // 15MB file

            // Act
            await _partitioner.SplitIntoSortedChunksAsync(inputFile);

            // Assert
            var chunkFiles = Directory.GetFiles(_tempDirectory, "chunk_*.txt");
            Assert.That(chunkFiles, Has.Length.GreaterThan(1), "Large file should create multiple chunks");

            // Verify each chunk is sorted
            foreach (var chunkFile in chunkFiles)
            {
                var lines = await File.ReadAllLinesAsync(chunkFile);
                Assert.That(lines, Is.Ordered.Using(new LinesComparer()));
            }
        }

        [Test]
        public async Task EmptyFile_CreatesNoChunks()
        {
            // Arrange
            var inputFile = Path.Combine(_tempDirectory, "empty.txt");
            await File.WriteAllTextAsync(inputFile, string.Empty);

            // Act
            await _partitioner.SplitIntoSortedChunksAsync(inputFile);

            // Assert
            var chunkFiles = Directory.GetFiles(_tempDirectory, "chunk_*.txt");
            Assert.That(chunkFiles, Is.Empty, "Empty file should create no chunks");
        }

        private async Task CreateLargeFile(string filePath, int sizeInBytes)
        {
            await using var writer = new StreamWriter(filePath);

            var random = new Random(42);
            var bytesWritten = 0;

            while (bytesWritten < sizeInBytes)
            {
                var number = random.Next(1, 1000000);
                var line = $"{number}. line{number}";

                await writer.WriteLineAsync(line);
                bytesWritten += System.Text.Encoding.UTF8.GetByteCount(line + Environment.NewLine);
            }
        }
    }

}