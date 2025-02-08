using System.Text;
using Bogus;

namespace Generator.Data
{
    public class CompaniesDataProvider : IDataProvider
    {
        private readonly long _totalLinesEstimate;

        private readonly Lazy<List<string>> _firstPool;
        private readonly Lazy<List<string>> _secondPool;
        private readonly Lazy<List<string>> _thirdPool;

        public CompaniesDataProvider(long totalSizeInBytes)
        {
            // 3 pools of strings => total combinations number is first * second * third
            // Assuming that all pools have the same size (for simplicity) it equals cube root on total lines
            // 0.9 coefficient is guarantee duplicate lines (because total number of combinations won't be "enough")
            _totalLinesEstimate = EstimateTotalLinesCount(totalSizeInBytes);
            var poolSize = (int) Math.Ceiling(Math.Cbrt(_totalLinesEstimate * 0.9));

            _firstPool = new(() => GenerateCompaniesPool(poolSize));
            _secondPool = new(() => GenerateCompaniesPool(poolSize));
            _thirdPool = new(() => GenerateCompaniesPool(poolSize));
        }

        private static long EstimateTotalLinesCount(long totalSizeInBytes)
        {
            var faker = new Faker(locale: "en");
            var exampleString = $"{10000L}. {faker.Company.CompanyName()}, {faker.Company.CompanyName()}, {faker.Company.CompanyName()}{Environment.NewLine}";

            var estimateStringSize = Encoding.UTF8.GetByteCount(exampleString);
            return (long) Math.Ceiling(totalSizeInBytes / (double) estimateStringSize);
        }

        private static List<string> GenerateCompaniesPool(int size)
        {
            var faker = new Faker(locale: "en");
            return Enumerable.Range(0, size).Select(_ => faker.Company.CompanyName()).ToList();
        }

        public string GetLine()
        {
            var number = Random.Shared.Next(1, (int) (_totalLinesEstimate * 0.9));
            
            var first = _firstPool.Value[Random.Shared.Next(_firstPool.Value.Count)];
            var second = _secondPool.Value[Random.Shared.Next(_secondPool.Value.Count)];
            var third = _thirdPool.Value[Random.Shared.Next(_thirdPool.Value.Count)];

            return $"{number}. {first}, {second}, {third}";
        }
    }
}