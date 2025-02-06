using Bogus;
using System.Text;

namespace Generator.Data
{
    public class ProductsDataProvider : IDataProvider
    {
        private readonly long _totalLinesEstimate;

        private readonly Lazy<List<string>> _products;
        private readonly Lazy<List<string>> _productAdjectives;
        private readonly Lazy<List<string>> _productMaterials;

        public ProductsDataProvider(long totalSizeInBytes)
        {
            _totalLinesEstimate = EstimateTotalLinesCount(totalSizeInBytes);
            var poolSize = (int)Math.Ceiling(Math.Cbrt(_totalLinesEstimate * 0.75));

            _products = new(() => GenerateProductsPool(poolSize));
            _productAdjectives = new(() => GenerateProductAdjectivesPool(poolSize));
            _productMaterials = new(() => GenerateProductMaterialsPool(poolSize));
        }

        private static long EstimateTotalLinesCount(long totalSizeInBytes)
        {
            var faker = new Faker(locale: "en");
            var exampleString = $"{100L}. {faker.Commerce.ProductName()} {faker.Commerce.ProductAdjective()} {faker.Commerce.ProductMaterial()}{Environment.NewLine}";

            var estimateStringSize = Encoding.UTF8.GetByteCount(exampleString);
            return (long)Math.Ceiling(totalSizeInBytes / (double)estimateStringSize);
        }

        private static List<string> GenerateProductsPool(int size)
        {
            var faker = new Faker(locale: "en");
            return Enumerable.Range(0, size).Select(_ => faker.Commerce.ProductName()).ToList();
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
            var number = Random.Shared.Next(1, (int)(_totalLinesEstimate * 0.75));

            if (number % 10 == 0)
                return $"{number}. {_products.Value[Random.Shared.Next(_products.Value.Count)]}";

            var product = _products.Value[Random.Shared.Next(_products.Value.Count)];
            var adjective = _productAdjectives.Value[Random.Shared.Next(_productAdjectives.Value.Count)];
            var material = _productMaterials.Value[Random.Shared.Next(_productMaterials.Value.Count)];

            return $"{number}. {product} {adjective} {material}";
        }
    }
}
