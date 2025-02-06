using System.Diagnostics;
using CommandLine;

using Generator;
using Generator.Console;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(async o =>
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
            var watch = new Stopwatch();
            watch.Start();

            await using var generator = GenerationFacadeFactory.CreateFileGenerationFacade(o.FilePath, o.FileSizeBytes);
            await generator.GenerateAsync();

            Console.WriteLine($"Generation is successfully finished. It took {watch.Elapsed}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error occurred: {e}");
            Environment.Exit(1);
        }
    });