using System;
using System.Linq;
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

                BitArray encodedData = DoHammingEncoding(buffer);
                byte[] encodedDataBytes = IOHelper.BitsToBytes(encodedData);

                byte[] crc32Bytes = (new Crc32()).GetHash(encodedDataBytes);
                BitArray encodedCrc32 = DoHammingEncoding(crc32Bytes);
                byte[] encodedCrc32Bytes = IOHelper.BitsToBytes(encodedCrc32);

                writer.Write(encodedCrc32Bytes);
                writer.Write(encodedDataBytes);

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

                byte[] originalEncodedCrc = new byte[6];
                for (int i = 0; i < originalEncodedCrc.Length; i++)
                {
                    originalEncodedCrc[i] = buffer[i];
                }

                byte[] encodedData = buffer.Skip(6).ToArray();

                BitArray decodedData = DoHammingDecoding(encodedData);
                byte[] decodedDataBytes = IOHelper.BitsToBytes(decodedData);

                // TODO, note: odebirani posledniho byte 8193 -> 8192, na konci je divnoznak
                Array.Resize(ref decodedDataBytes, decodedDataBytes.Length - 1);

                byte[] crc32Bytes = (new Crc32()).GetHash(encodedData);
                BitArray encodedCrc32 = DoHammingEncoding(crc32Bytes);
                byte[] encodedCrc32Bytes = IOHelper.BitsToBytes(encodedCrc32);

                if (!CrcMatches(originalEncodedCrc, encodedCrc32Bytes))
                {
                    decodedDataBytes = new byte[decodedDataBytes.Length];
                }

                writer.Write(Encoding.UTF8.GetString(decodedDataBytes));

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

        private BitArray DoHammingEncoding(byte[] data)
        {
            List<bool> result = new List<bool>();

            List<BitArray> inputChunks = SplitTo11BitChunks(data);
            foreach (BitArray chunk in inputChunks)
            {
                BitArray hamming = Hamming.Encode(chunk);
                foreach (bool bit in hamming)
                {
                    result.Add(bit);
                }
            }

            return new BitArray(result.ToArray());
        }

        private BitArray DoHammingDecoding(byte[] data)
        {
            List<BitArray> encodedDataChunks = new List<BitArray>();
            for (int i = 0; i < data.Length; i = i + 2)
            {
                encodedDataChunks.Add(
                    new BitArray(new byte[] { data[i], data[i + 1] })
                );
            }

            List<bool> result = new List<bool>();
            foreach (BitArray chunk in encodedDataChunks)
            {
                BitArray decodedPair = Hamming.Decode(chunk);
                foreach (bool bit in decodedPair)
                {
                    result.Add(bit);
                }
            }

            return new BitArray(result.ToArray());
        }
    }
}
