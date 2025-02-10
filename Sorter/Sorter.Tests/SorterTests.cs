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
        public async Task SmallFile_SortsCorrectly()
        {
            // Arrange
            File.WriteAllLines(_inputFilePath, ["3. third line", "1. first line", "5. second line", "2. second line"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);

            Assert.That(result, Has.Length.EqualTo(4));
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }

        [Test]
        public async Task LargeFile_SortsCorrectly()
        {
            // Arrange
            var random = new Random(42);
            var inputLines = new List<string>();

            for (var i = 1; i <= 2000000; i++)
                inputLines.Add($"{i}. Line number {random.Next(1, 10001)}");

            // Shuffle the lines
            inputLines = inputLines.OrderBy(_ => random.Next()).ToList();
            await File.WriteAllLinesAsync(_inputFilePath, inputLines);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }

        [Test]
        public async Task EmptyFile_CreatesEmptyOutput()
        {
            // Arrange
            await File.WriteAllTextAsync(_inputFilePath, string.Empty);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task HandlesSpecialCharacters()
        {
            // Arrange
            File.WriteAllLines(_inputFilePath, ["3. line with @special# chars", "1. another !@#$%^&*"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);

            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }
        
        [Test]
        public async Task AllLinesAreInvalid_CreatesEmptyOutput()
        {
            // Arrange
            await File.WriteAllLinesAsync(_inputFilePath, ["dsjhk. 12321", "dklskjlkj", "1232,dsaklsj"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);
            Assert.That(result, Is.Empty);
        }
        
        [Test]
        public async Task SomeLinesAreInvalid_IgnoresInvalidLines()
        {
            // Arrange
            await File.WriteAllLinesAsync(_inputFilePath, ["dsjhk. 12321", "123. bbb", "1. bbb", "\n"]);

            // Act
            await _sortingFacade.SortAsync(_inputFilePath, _outputFilePath);

            // Assert
            var result = await File.ReadAllLinesAsync(_outputFilePath);
            
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Is.Ordered.Using(new LinesComparer()));
        }
    }
}
