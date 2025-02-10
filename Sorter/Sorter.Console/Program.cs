using System.Diagnostics;
using CommandLine;

using Sorter.Console;

var options = Parser.Default.ParseArguments<Options>(args)
    .WithParsed(o =>
    {
        try
        {
            var directory = Path.GetDirectoryName(o.OutputFilePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory!);
        }
        catch
        {
            Console.WriteLine("Error during specified output directory creation (either invalid name or access denied)");
            Environment.Exit(1);
        }

        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        if (Path.GetFileName(o.OutputFilePath).Any(s => invalidFileNameChars.Contains(s)))
        {
            Console.WriteLine("Invalid output file name");
            Environment.Exit(1);
        }

        if (!File.Exists(o.InputFilePath))
        {
            Console.WriteLine("Input file is not found");
            Environment.Exit(1);
        }
    })
    .WithNotParsed(_ => Environment.Exit(1));

try
{
    var watch = new Stopwatch();
    watch.Start();

    var logger = new ConsoleLogger();
    var sorter = new Sorter.SortingFacade(logger);

    await sorter.SortAsync(options.Value.InputFilePath, options.Value.OutputFilePath);
    Console.WriteLine($"Sorting is successfully finished. It took {watch.Elapsed}");
}
catch (Exception e)
{
    Console.WriteLine($"Unexpected error occurred: {e}");
    Environment.Exit(1);
}