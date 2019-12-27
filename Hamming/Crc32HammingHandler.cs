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

                // apply hamming to every 11 input bits

                byte[] bufferBytes = Encoding.ASCII.GetBytes(buffer);
                BitArray bufferBits = new BitArray(bufferBytes);

                List<bool> result = new List<bool>();
                List<bool> currentChunk = new List<bool>(); // note: current 11 bits

                foreach (bool bit in bufferBits)
                {
                    currentChunk.Add(bit);
                    if (currentChunk.Count == Hamming.INPUT_LENGTH)
                    {
                        BitArray hamResult = Hamming.AddParityBits(new BitArray(currentChunk.ToArray()));
                        foreach (bool b in hamResult)
                        {
                            result.Add(b);
                        }

                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    // append bits to match 11 length
                    while (currentChunk.Count < Hamming.INPUT_LENGTH)
                    {
                        currentChunk.Add(false);
                    }

                    BitArray hamResult = Hamming.AddParityBits(new BitArray(currentChunk.ToArray()));
                    foreach (bool b in hamResult)
                    {
                        result.Add(b);
                    }

                    currentChunk.Clear();
                }

                // apply crc to hamminged input

                foreach (byte hammingByte in IOHelper.BitsToBytes(new BitArray(result.ToArray())))
                {
                    //writer.Write(hammingByte);
                }

                byte[] crc = (new Crc32()).GetHash(IOHelper.BitsToBytes(new BitArray(result.ToArray())));
                BitArray crcBits = new BitArray(crc);

                foreach (bool bit in crcBits)
                {
                    currentChunk.Add(bit);
                    if (currentChunk.Count == Hamming.INPUT_LENGTH)
                    {
                        BitArray hamResult = Hamming.AddParityBits(new BitArray(currentChunk.ToArray()));
                        foreach (bool b in hamResult)
                        {
                            result.Add(b);
                        }

                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    // append bits to match 11 length
                    while (currentChunk.Count < Hamming.INPUT_LENGTH)
                    {
                        currentChunk.Add(false);
                    }

                    BitArray hamResult = Hamming.AddParityBits(new BitArray(currentChunk.ToArray()));
                    foreach (bool b in hamResult)
                    {
                        result.Add(b);
                    }

                    currentChunk.Clear();
                }

                foreach (byte crcHammingByte in IOHelper.BitsToBytes(new BitArray(result.ToArray())))
                {
                    writer.Write(crcHammingByte);
                }

                result.Clear();
                currentChunk.Clear();

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
