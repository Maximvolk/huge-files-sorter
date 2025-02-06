using Generator.Data;
using Generator.Output;

namespace Generator
{
    public static class GenerationFacadeFactory
    {
        public static GenerationFacade CreateFileGenerationFacade(string filePath, long targetSizeInBytes)
        {
            var dataProvider = new ProductsDataProvider(targetSizeInBytes);
            var outputWriter = new FileWriter(filePath);

            return new GenerationFacade(dataProvider, outputWriter, targetSizeInBytes);
        }
    }
}
