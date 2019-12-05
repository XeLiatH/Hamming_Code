using System;

namespace Hamming
{
    class Options
    {
        [Option('m', "mode", Required = true, HelpText = "k - encode | d - decode")]
        public char Mode { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "If specified, output is saved to a file.")]
        public string OutputFile { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            return;
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
           
        }
    }
}
