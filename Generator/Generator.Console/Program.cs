using System.Diagnostics;
using CommandLine;

using Generator;
using Generator.Console;

var options = Parser.Default.ParseArguments<Options>(args)
    .WithParsed(o =>
    {
        try
        {
            var directory = Path.GetDirectoryName(o.FilePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory!);
        }
        catch
        {
            Console.WriteLine("Error during specified output directory creation (either invalid name or access denied)");
            Environment.Exit(1);
        }

        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        if (Path.GetFileName(o.FilePath).Any(s => invalidFileNameChars.Contains(s)))
        {
            Console.WriteLine("Invalid output file name");
            Environment.Exit(1);
        }

        try
        {
            _ = o.FileSizeBytes;
        }
        catch
        {
            Console.WriteLine("Invalid size, only K, M and G suffixes supported");
            Environment.Exit(1);
        }
    })
    .WithNotParsed(_ => Environment.Exit(1));

try
{
    Console.Write("Generation is in progress... ");
    var progressBar = new ProgressBar();
    
    var watch = new Stopwatch();
    watch.Start();

    await using var generator = GenerationFacadeFactory.CreateFileGenerationFacade(
        options.Value.FilePath, options.Value.FileSizeBytes, progressBar)
        ;
    await generator.GenerateAsync();

    Console.WriteLine($"Generation is successfully finished. It took {watch.Elapsed}");
}
catch (Exception e)
{
    Console.WriteLine($"Unexpected error occurred: {e}");
    Environment.Exit(1);
}