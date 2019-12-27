using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Hamming
{
    public class Crc32HammingHandler
    {
        public const int BLOCK_LENGTH = 8192;
        public const int ENCODED_BLOCK_LENGTH = 11922;

        private Stream _input;
        private Stream _output;

        public Crc32HammingHandler(Stream input, Stream output = null)
        {
            if (output == null)
            {
                output = Console.OpenStandardOutput();
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
                byte[] buffer = new byte[BLOCK_LENGTH];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = reader.ReadByte();
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                    {
                        break;
                    }
                }

                List<bool> result = new List<bool>();

                List<BitArray> inputChunks = SplitTo11BitChunks(buffer);
                foreach (BitArray chunk in inputChunks)
                {
                    BitArray hamming = Hamming.Encode(chunk);
                    foreach (bool bit in hamming)
                    {
                        result.Add(bit);
                    }
                }

                byte[] hammingedInputBytes = IOHelper.BitsToBytes(new BitArray(result.ToArray()));
                byte[] crc32Bytes = (new Crc32()).GetHash(hammingedInputBytes);

                List<bool> crcResult = new List<bool>();

                List<BitArray> crc32Chunks = SplitTo11BitChunks(crc32Bytes);
                foreach (BitArray chunk in crc32Chunks)
                {
                    BitArray hamming = Hamming.Encode(chunk);
                    foreach (bool bit in hamming)
                    {
                        crcResult.Add(bit);
                    }
                }

                byte[] crcByteResult = IOHelper.BitsToBytes(new BitArray(crcResult.ToArray()));
                crcResult.Clear();
                byte[] byteResult = IOHelper.BitsToBytes(new BitArray(result.ToArray()));
                result.Clear();

                writer.Write(crcByteResult);
                writer.Write(byteResult);

                writer.Flush();
            }

            reader.Close();
            writer.Close();
        }

        public void Decode()
        {
            BinaryReader reader = new BinaryReader(this._input);
            StreamWriter writer = new StreamWriter(this._output);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                byte[] buffer = new byte[ENCODED_BLOCK_LENGTH];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = reader.ReadByte();
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                    {
                        break;
                    }
                }

                byte[] crcBytes = new byte[6];
                for (int i = 0; i < crcBytes.Length; i++)
                {
                    crcBytes[i] = buffer[i];
                }

                List<byte[]> bytePairs = new List<byte[]>();
                for (int i = 6; i < buffer.Length; i = i + 2)
                {
                    bytePairs.Add(new byte[] { buffer[i], buffer[i + 1] });
                }

                List<byte> dataToCalculateCrcFrom = new List<byte>();

                List<bool> reconstructedDataBits = new List<bool>();
                foreach (byte[] pair in bytePairs)
                {
                    BitArray decodedPair = Hamming.Decode(new BitArray(pair));
                    foreach (bool bit in decodedPair)
                    {
                        reconstructedDataBits.Add(bit);
                    }

                    dataToCalculateCrcFrom.AddRange(pair);
                }

                // Console.WriteLine(dataToCalculateCrcFrom.Count);

                byte[] reconstructedData = IOHelper.BitsToBytes(new BitArray(reconstructedDataBits.ToArray()));

                // Array.Resize(ref reconstructedData, reconstructedData.Length - 1);

                byte[] reconstructedCrc32Bytes = (new Crc32()).GetHash(dataToCalculateCrcFrom.ToArray());

                List<bool> crcResult = new List<bool>();
                List<BitArray> crc32Chunks = SplitTo11BitChunks(reconstructedCrc32Bytes);
                foreach (BitArray chunk in crc32Chunks)
                {
                    BitArray hamming = Hamming.Encode(chunk);
                    foreach (bool bit in hamming)
                    {
                        crcResult.Add(bit);
                    }
                }

                byte[] crcByteResult = IOHelper.BitsToBytes(new BitArray(crcResult.ToArray()));

                if (!CrcMatches(crcBytes, crcByteResult))
                {
                    reconstructedData = new byte[reconstructedData.Length];
                }

                writer.Write(Encoding.UTF8.GetString(reconstructedData));

                writer.Flush();
            }

            reader.Close();
            writer.Close();
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

        private bool CrcMatches(byte[] original, byte[] reconstructed)
        {
            if (original.Length != reconstructed.Length)
            {
                return false;
            }

            for (int i = 0; i < original.Length; i++)
            {
                if (original[i] != reconstructed[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
