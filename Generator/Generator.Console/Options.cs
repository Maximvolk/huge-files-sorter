using CommandLine;
using CommandLine.Text;

namespace Generator.Console
{
    public record Options
    {
        [Option('o', "output", Required = true, HelpText = "Generated file path")]
        public string FilePath { get; set; } = "./generated.txt";

        [Option('s', "size", Required = true, HelpText = "Generated file size")]
        public long FileSizeBytes { get; set; }

        [Usage(ApplicationAlias = "generator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return [new Example("Generate 10MB file test.txt", new Options { FilePath = "test.txt", FileSizeBytes = 10 * 1024 * 1024 })];
            }
        }
    }
}
