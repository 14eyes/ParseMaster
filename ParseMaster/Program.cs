using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParseMaster;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("TODO: usage");
            return;
        }

        var start = Stopwatch.GetTimestamp();
        var parser = new Parser(@$"{args[0]}\configs.txt", @$"{args[0]}\enums.txt", @$"{args[0]}\type-index.json");
        int suc = 0, fail = 0, skip = 0;
        var di = new DirectoryInfo(args[1]);
        //foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
        Parallel.ForEach(di.GetFiles("*", SearchOption.AllDirectories), file =>
        {
            try
            {
                var (className, mode, derivation) = ConfigResolver.Resolve(file.FullName);
                if (className is null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Failed to find config for {file}");
                    skip++;
                    return;
                }

                var output = parser.ParseFile(file.FullName, className, mode, derivation);
                var fileName = file.FullName.Replace(@"\Data\", @"\json\") + ".json";
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