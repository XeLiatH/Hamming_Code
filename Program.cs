using System;
using CommandLine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Hamming
{
    class Options
    {
        [Option('m', "mode", Required = false, HelpText = "k - encode | d - decode")] // TODO: set required to true
        public char Mode { get; set; }

        [Option('i', "input", Required = false, HelpText = "Input file to be processed.")] // TODO: set required to true
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
                .WithNotParsed(errs => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            return;
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            FileStream output = null;
            if (opts.OutputFile != null)
            {
                output = new FileStream(opts.OutputFile, FileMode.Create, FileAccess.ReadWrite);
            }

            var ba = Hamming.AddParityBits(new BitArray(new bool[] { true, false, true, false, true, false, true, false, false, false, true }));

            Console.WriteLine();

            foreach (bool bit in ba)
            {
                Console.Write(bit ? "1 " : "0 ");
            }

            Console.WriteLine();

            // TODO: uncomment
            // var hh = new HammingHandler(new FileStream(opts.InputFile, FileMode.Open, FileAccess.Read), output);

            // if (opts.Mode == 'k')
            // {
            //    hh.Encode();
            // }

            // if (opts.Mode == 'd')
            // {
            //    hh.Decode();
            // }
        }
    }
}
