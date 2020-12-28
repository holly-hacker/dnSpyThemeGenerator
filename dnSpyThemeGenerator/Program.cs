using CommandLine;

namespace dnSpyThemeGenerator
{
    internal static class Program
    {
        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<CommandLineArguments>(args).WithParsed(RunOptions);

        private static void RunOptions(CommandLineArguments obj)
        {
            
        }
    }
}