using CommandLine;

namespace dnSpyThemeGenerator
{
    internal class CommandLineArguments
    {
        [Option('i', "input", Required = true, HelpText = "The input theme to convert")]
        public string ThemePath { get; set; }
        
        [Option('d', "donor", Required = true, HelpText = "The dnSpy theme to use as base")]
        public string DonorPath { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "The output path to write the new dnSpy theme to")]
        public string OutputPath { get; set; }
    }
}