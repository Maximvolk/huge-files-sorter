using CommandLine;

namespace Sorter.Console
{
    internal class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file path")]
        public string InputFilePath { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Sorted output file path (default is sorted.txt)")]
        public string OutputFilePath { get; set; } = "sorted.txt";
    }
}
