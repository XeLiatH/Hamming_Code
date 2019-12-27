using System;
using System.Collections;

namespace Hamming
{
    /*

        Reference
        - http://sanity-free.org/12/crc32_implementation_in_csharp.html
        - https://stackoverflow.com/questions/2587766/how-is-a-crc32-checksum-calculated
        - https://rosettacode.org/wiki/CRC-32#C.23

    */

    public class Crc32
    {
        public const int BLOCK_LENGTH = 8192;

        private const uint POLYNOMIAL = 0xEDB88320; // note: g(x) generator polynomial set by IEEE 802.3
        private const uint SEED = 0xFFFFFFFF; // note: default initialize value

        private uint[] table;

        public Crc32()
        {
            this.table = new uint[256];

            uint entry = 0;
            for (uint i = 0; i < table.Length; i++)
            {
                entry = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((entry & 1) == 1)
                    {
                        entry = (uint)((entry >> 1) ^ POLYNOMIAL);
                    }
                    else
                    {
                        entry >>= 1;
                    }
                }

                this.table[i] = entry;
            }
        }

        public byte[] GetHash(byte[] bytes)
        {
            uint crc = SEED;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ this.table[index]);
            }

            byte[] result = BitConverter.GetBytes(~crc); // note: ~ doplnek
            Array.Reverse(result);

            return result;
        }
    }
}