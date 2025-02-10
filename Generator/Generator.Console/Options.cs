using CommandLine;
using CommandLine.Text;

namespace Generator.Console
{
    public record Options
    {
        [Option('o', "output", Required = false, HelpText = "Generated file path")]
        public string FilePath { get; set; } = "./generated.txt";

        [Option('s', "size", Required = true,
            HelpText = "Generated file size - integer number of bytes, M, K and G suffixes supported")]
        public string FileSize { get; set; } = null!;

        public long FileSizeBytes
        {
            get
            {
                if (FileSize.EndsWith('K'))
                    return long.Parse(FileSize[..^1]) * 1024;

                if (FileSize.EndsWith('M'))
                    return long.Parse(FileSize[..^1]) * 1024 * 1024;

                if (FileSize.EndsWith('G'))
                    return long.Parse(FileSize[..^1]) * 1024 * 1024 * 1024;

                return long.Parse(FileSize);
            }
        }

        [Usage(ApplicationAlias = "generator")]
        public static IEnumerable<Example> Examples
            => [new("Generate 10MB file test.txt", new Options {FilePath = "test.txt", FileSize = "10M"})];
    }
}
