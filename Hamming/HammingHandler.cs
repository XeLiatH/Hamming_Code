using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Hamming
{
    public class HammingHandler
    {
        private Stream _input;
        private Stream _output;
        private bool _verbose;

        public HammingHandler(Stream input, Stream output = null)
        {
            this._verbose = false;

            if (output == null)
            {
                output = Console.OpenStandardOutput();
                this._verbose = true;
            }

            this._input = input;
            this._output = output;
        }

        public void Encode()
        {
            Console.WriteLine("Encoding ....");
        }

        public void Decode()
        {
            Console.WriteLine("Decoding ....");
        }
    }
}
