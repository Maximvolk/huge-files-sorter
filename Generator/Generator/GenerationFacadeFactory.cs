using Generator.Data;
using Generator.Output;

namespace Generator
{
    public static class GenerationFacadeFactory
    {
        public static GenerationFacade CreateFileGenerationFacade(string filePath, long targetSizeInBytes, IProgressObserver progressObserver)
        {
            // Products provider is good for small files
            // Companies provider supports bigger number of combinations and leads to smaller amount of duplicates on a big files
            // Target size passed to constructor to create suitable inner pool (for reasonable amount of duplicates)
            IDataProvider dataProvider = targetSizeInBytes switch
            {
                <1024*1024 => new ProductsDataProvider(targetSizeInBytes),
                _ => new CompaniesDataProvider(targetSizeInBytes)
            };
            
            var outputWriter = new FileWriter(filePath);

            return new GenerationFacade(dataProvider, outputWriter, targetSizeInBytes, progressObserver);
        }
    }
}
