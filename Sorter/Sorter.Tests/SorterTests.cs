namespace Sorter.Tests
{
    [TestFixture]
    public class SorterTests
    {
        private string _inputFilePath;
        private string _outputFilePath;
        private SortingFacade _sortingFacade;

        private readonly ILogger _logger = new LoggerMock();

        [SetUp]
        public void Setup()
        {
            _sortingFacade = new SortingFacade(_logger);

            _inputFilePath = Path.GetTempFileName();
            _outputFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_inputFilePath))
                File.Delete(_inputFilePath);

            if (File.Exists(_outputFilePath))
                File.Delete(_outputFilePath);
        }

        [Test]
        public async Task SortAsync_SmallFile_SortsCorrectly()
        {
            // Arrange
            File.WriteAllLines(_inputFilePath, ["3. third line", "1. first line", "5. second line", "2. second line"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = File.ReadAllLines(_outputFilePath);

            Assert.That(result, Has.Length.EqualTo(4));
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }

        [Test]
        public async Task SortAsync_LargeFile_SortsCorrectly()
        {
            // Arrange
            var random = new Random(42);
            var inputLines = new List<string>();

            for (int i = 1; i <= 10000; i++)
                inputLines.Add($"{i}. Line number {random.Next(1, 10001)}");

            // Shuffle the lines
            inputLines = inputLines.OrderBy(_ => random.Next()).ToList();
            File.WriteAllLines(_inputFilePath, inputLines);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = File.ReadAllLines(_outputFilePath);
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }

        [Test]
        public async Task SortAsync_EmptyFile_CreatesEmptyOutput()
        {
            // Arrange
            File.WriteAllText(_inputFilePath, string.Empty);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = File.ReadAllLines(_outputFilePath);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SortAsync_HandlesSpecialCharacters()
        {
            // Arrange
            File.WriteAllLines(_inputFilePath, ["3. line with @special# chars", "1. another !@#$%^&*"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = File.ReadAllLines(_outputFilePath);

            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }
    }

}
