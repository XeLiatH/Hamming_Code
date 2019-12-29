using System;
using System.Collections;

namespace Hamming
{
    /*

        Reference
        - http://sanity-free.org/12/crc32_implementation_in_csharp.html
        - https://stackoverflow.com/questions/2587766/how-is-a-crc32-checksum-calculated
        - https://rosettacode.org/wiki/CRC-32#C.23
        - https://en.wikipedia.org/wiki/Cyclic_redundancy_check

    */

    public class Crc32
    {
        private const uint POLYNOMIAL = 0xEDB88320; // note: g(x) generator polynomial set by IEEE 802.3

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
                        entry = (uint)((entry >> 1) ^ POLYNOMIAL); // note: ^ xor
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
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte index = (byte)(((crc) ^ bytes[i]) & 0xff);
                crc = (uint)((crc >> 8) ^ this.table[index]);
            }

            byte[] result = BitConverter.GetBytes(~crc); // note: ~ doplnek
            Array.Reverse(result);

            return result;
        }
    }
}