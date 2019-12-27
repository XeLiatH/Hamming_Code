using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Hamming
{
    public class Crc32HammingHandler
    {
        private Stream _input;
        private Stream _output;
        private bool _verbose;

        public Crc32HammingHandler(Stream input, Stream output = null)
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
            BinaryReader reader = new BinaryReader(this._input);
            BinaryWriter writer = new BinaryWriter(this._output);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                char[] buffer = new char[Crc32.BLOCK_LENGTH];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = reader.ReadChar();
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                    {
                        break;
                    }
                }

                byte[] bufferBytes = Encoding.ASCII.GetBytes(buffer);
                List<bool> result = new List<bool>();

                List<BitArray> inputChunks = SplitTo11BitChunks(bufferBytes);
                foreach (BitArray chunk in inputChunks)
                {
                    BitArray hamming = Hamming.AddParityBits(chunk);
                    foreach (bool bit in hamming)
                    {
                        result.Add(bit);
                    }
                }

                byte[] hammingedInputBytes = IOHelper.BitsToBytes(new BitArray(result.ToArray()));
                byte[] crc32Bytes = (new Crc32()).GetHash(hammingedInputBytes);

                List<BitArray> crc32Chunks = SplitTo11BitChunks(crc32Bytes);
                foreach (BitArray chunk in crc32Chunks)
                {
                    BitArray hamming = Hamming.AddParityBits(chunk);
                    foreach (bool bit in hamming)
                    {
                        result.Add(bit);
                    }
                }

                byte[] byteResult = IOHelper.BitsToBytes(new BitArray(result.ToArray()));
                result.Clear();

                writer.Write(byteResult);

                writer.Flush();
            }

            reader.Close();
            writer.Close();
        }

        public void Decode()
        {
            Console.WriteLine("Decoding ....");
        }

        private List<BitArray> SplitTo11BitChunks(byte[] bytes)
        {
            List<BitArray> result = new List<BitArray>();
            List<bool> currentChunk = new List<bool>();

            BitArray bytesInBits = new BitArray(bytes);
            foreach (bool bit in bytesInBits)
            {
                currentChunk.Add(bit);
                if (currentChunk.Count == Hamming.INPUT_LENGTH)
                {
                    result.Add(new BitArray(currentChunk.ToArray()));
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Count > 0)
            {
                while (currentChunk.Count < Hamming.INPUT_LENGTH)
                {
                    currentChunk.Add(false);
                }

                result.Add(new BitArray(currentChunk.ToArray()));
                currentChunk.Clear();
            }

            return result;
        }
    }
}
