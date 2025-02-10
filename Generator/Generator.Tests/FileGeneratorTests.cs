namespace Generator.Tests
{
    class ProgressObserverMock : IProgressObserver
    {
        public void ObserveProgress(double percent) {}
        public void Finish() {}
    }

    [TestFixture]
    public class FileGeneratorTests
    {
        private const string OutputPath = "test_output.txt";
        private readonly IProgressObserver _progressObserver = new ProgressObserverMock();

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(OutputPath))
                File.Delete(OutputPath);
        }

        [TestCase(1024)] // 1 KB
        [TestCase(1024 * 1024)] // 1 MB
        [TestCase(10 * 1024 * 1024)] // 10 MB
        [TestCase(150 * 1024 * 1024)] // 150 MB
        public async Task CreatesFileWithSpecifiedSize_WithinAcceptableTolerance(long targetSize)
        {
            // Arrange
            const double tolerancePercentage = 0.05;
            var allowedDeviation = targetSize * tolerancePercentage;

            await using (var generator = GenerationFacadeFactory.CreateFileGenerationFacade(OutputPath, targetSize, _progressObserver))
            {
                // Act
                await generator.GenerateAsync();
            }

            // Assert
            var fileInfo = new FileInfo(OutputPath);

            Assert.That(fileInfo.Exists, Is.True, "Output file should exist");
            Assert.That(fileInfo.Length, Is.InRange(
                targetSize - allowedDeviation,
                targetSize + allowedDeviation),
                $"File size should be within {tolerancePercentage:P} of target size");
        }

        [Test]
        public async Task HandlesSmallFiles()
        {
            // Arrange
            const long verySmallSize = 100; // 100 bytes
            await using (var generator = GenerationFacadeFactory.CreateFileGenerationFacade(OutputPath, verySmallSize, _progressObserver))
            {
                // Act
                await generator.GenerateAsync();
            }

            // Assert
            var fileInfo = new FileInfo(OutputPath);

            Assert.That(fileInfo.Exists, Is.True, "Output file should exist");
            Assert.That(fileInfo.Length, Is.GreaterThan(0), "File should not be empty");
            Assert.That(fileInfo.Length, Is.LessThanOrEqualTo(verySmallSize * 1.5),
                "File size should not significantly exceed target size");
        }

        [Test]
        public async Task CreatesValidTextFile()
        {
            // Arrange
            await using (var generator = GenerationFacadeFactory.CreateFileGenerationFacade(OutputPath, 1024, _progressObserver))
            {
                // Act
                await generator.GenerateAsync();
            }

            // Assert
            var lines = await File.ReadAllLinesAsync(OutputPath);
            Assert.That(lines, Is.Not.Empty, "File should contain text");

            foreach (var line in lines)
            {
                Assert.That(line, Does.Match(@"^\d+\. .+$"),
                    $"Line '{line}' should match format 'number. string'");
            }
        }
    }
}
