using System.Text;
using Generator.Data;

namespace Generator.Tests
{
    [TestFixture]
    public class DataProviderTests
    {
        private static IEnumerable<IDataProvider> Providers(long targetSizeInBytes)
        {
            yield return new ProductsDataProvider(targetSizeInBytes);
            yield return new CompaniesDataProvider(targetSizeInBytes);
        }

        [Test]
        public void NegativeOrZeroSize_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProductsDataProvider(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompaniesDataProvider(-1));

            Assert.Throws<ArgumentOutOfRangeException>(() => new ProductsDataProvider(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompaniesDataProvider(0));
        }

        [Test]
        [TestCaseSource(nameof(Providers), new object[] { 1024 })]
        public void GetLine_ReturnsCorrectFormat(IDataProvider dataProvider)
        {
            // Act
            var line = dataProvider.GetLine();

            // Assert
            Assert.That(line, Does.Contain(". "));
            Assert.That(int.TryParse(line.Split(". ")[0], out _), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(Providers), new object[] { 10 * 1024 })]
        public void GetLine_GeneratesDuplicateStrings(IDataProvider dataProvider)
        {
            // Arrange
            var strings = new HashSet<string>();
            var totalSize = 0;
            var duplicateCount = 0;

            // Act
            // Duplicate are guaranteed when size of data generated is close to the one specified in constructor (10K)
            while (totalSize < 10 * 1024)
            {
                var line = dataProvider.GetLine();
                var stringPart = line[(line.IndexOf(". ") + 2)..];

                if (!strings.Add(stringPart))
                    duplicateCount++;

                totalSize += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
            }

            // Assert
            Assert.That(duplicateCount, Is.GreaterThan(0), "Should generate some duplicate strings");
        }
    }
}