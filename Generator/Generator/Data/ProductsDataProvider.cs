using System.Text;
using Bogus;

namespace Generator.Data
{
    /// <summary>
    /// Creates lines with product name and short info.
    /// Suitable for small target size results because combinations amount is pretty small,
    /// produces many duplicates on bigger sizes
    /// </summary>
    public class ProductsDataProvider : IDataProvider
    {
        private readonly long _totalLinesEstimate;

        private readonly Lazy<List<string>> _products;
        private readonly Lazy<List<string>> _productAdjectives;
        private readonly Lazy<List<string>> _productMaterials;

        public ProductsDataProvider(long totalSizeInBytes)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalSizeInBytes);

            // 3 pools of strings => total combinations number is first * second * third
            // Assuming that all pools have the same size (for simplicity) it equals cube root on total lines
            // 0.9 coefficient is to guarantee duplicate lines (because total number of combinations won't be "enough")
            _totalLinesEstimate = EstimateTotalLinesCount(totalSizeInBytes);
            var poolSize = (int)Math.Ceiling(Math.Cbrt(_totalLinesEstimate * 0.9));

            // Pool are lazy to avoid potentially long-running provider initialisation and to generate pools only on demand
            _products = new(() => GenerateProductsPool(poolSize));
            _productAdjectives = new(() => GenerateProductAdjectivesPool(poolSize));
            _productMaterials = new(() => GenerateProductMaterialsPool(poolSize));
        }

        private static long EstimateTotalLinesCount(long totalSizeInBytes)
        {
            var faker = new Faker(locale: "en");
            var exampleString = $"{100L}. {faker.Commerce.Product()} {faker.Commerce.ProductAdjective()} {faker.Commerce.ProductMaterial()}{Environment.NewLine}";

            var estimateStringSize = Encoding.UTF8.GetByteCount(exampleString);
            return (long)Math.Ceiling(totalSizeInBytes / (double)estimateStringSize);
        }

        private static List<string> GenerateProductsPool(int size)
        {
            var faker = new Faker(locale: "en");
            return Enumerable.Range(0, size).Select(_ => faker.Commerce.Product()).ToList();
        }

        private static List<string> GenerateProductAdjectivesPool(int size)
        {
            var faker = new Faker(locale: "en");
            return Enumerable.Range(0, size).Select(_ => faker.Commerce.ProductAdjective()).ToList();
        }

        private static List<string> GenerateProductMaterialsPool(int size)
        {
            var faker = new Faker(locale: "en");
            return Enumerable.Range(0, size).Select(_ => faker.Commerce.ProductMaterial()).ToList();
        }

        public string GetLine()
        {
            var number = Random.Shared.Next(1, (int)(_totalLinesEstimate * 0.9));

            if (number % 10 == 0)
                return $"{number}. {_products.Value[Random.Shared.Next(_products.Value.Count)]}";

            var product = _products.Value[Random.Shared.Next(_products.Value.Count)];
            var adjective = _productAdjectives.Value[Random.Shared.Next(_productAdjectives.Value.Count)];
            var material = _productMaterials.Value[Random.Shared.Next(_productMaterials.Value.Count)];

            return $"{number}. {adjective} {material} {product}";
        }
    }
}
