using CommandLine;

namespace ParseMaster
{
    public class Options
    {
        [Option('c', "config", Required = true, HelpText = "Path to the configuration folder.")]
        public string ConfigFolder { get; set; }

        [Option('i', "input", Required = true, HelpText = "Path to the input folder.")]
        public string InputFolder { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to the output folder.")]
        public string OutputFolder { get; set; }

        [Option('d', "dumpDim", Required = false, HelpText = "Disables the parsing of DynamicFloats.")]
        public bool DisableDynamicFloatParsing { get; set; }
    }
}