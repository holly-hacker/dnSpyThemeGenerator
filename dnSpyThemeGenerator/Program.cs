using System.IO;
using CommandLine;
using dnSpyThemeGenerator.Converters;
using dnSpyThemeGenerator.Themes;

namespace dnSpyThemeGenerator
{
    internal static class Program
    {
        private static void Main(string[] args) =>
            Parser.Default.ParseArguments<CommandLineArguments>(args).WithParsed(RunOptions);

        private static void RunOptions(CommandLineArguments args)
        {
            var input = RiderTheme.ReadFromStream(File.OpenRead(args.ThemePath));
            var donor = DnSpyTheme.ReadFromStream(File.OpenRead(args.DonorPath));

            new RiderToDnSpyConverter().CopyTo(input, donor);
            donor.WriteToStream(File.Open(args.OutputPath, FileMode.Create));
        }
    }
}