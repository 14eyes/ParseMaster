using System.Diagnostics;
using CommandLine;
using Newtonsoft.Json;

namespace ParseMaster;

internal class Program
{
    private static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(RunWithOptions);
    }

    private static void RunWithOptions(Options o)
    {
        var start = Stopwatch.GetTimestamp();
        var parser = new Parser(o.ConfigFolder, !o.DisableDynamicFloatParsing);
        int suc = 0, fail = 0, skip = 0;
        var dirI = new DirectoryInfo(o.InputFolder);
        var dirO = new DirectoryInfo(o.OutputFolder);
        //foreach (var file in dirI.GetFiles("*", SearchOption.AllDirectories))
        Parallel.ForEach(dirI.GetFiles("*", SearchOption.AllDirectories), file =>
        {
            try
            {
                var (className, mode, derivation) = ConfigResolver.Resolve(file.FullName);
                if (className is null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Failed to find config for {file}");
                    skip++;
                    //continue;
                    return;
                }

                var output = parser.ParseFile(file.FullName, className, mode, derivation);
                var fileName = file.FullName.Replace(dirI.FullName, dirO.FullName) + ".json";
                Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
                File.WriteAllText(fileName,
                    JsonConvert.SerializeObject(JsonConvert.DeserializeObject(output), Formatting.Indented));
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine($"Parsed {file}!!");
                suc++;
            }
            catch (Exception ex)
            {
                fail++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to process {file}: {ex.Message}");
            }
        }
        );
        var done = Stopwatch.GetTimestamp();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(
            $"Completed in {Stopwatch.GetElapsedTime(start, done).TotalSeconds:N}s \nFailed: {fail} \nSucceeded: {suc} \nSkipped: {skip} \n{suc}/{suc + fail}");
    }
}