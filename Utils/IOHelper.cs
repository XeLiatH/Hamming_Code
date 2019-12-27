using System.Collections;

namespace Hamming
{
    public class IOHelper
    {
        public static int ByteLengthFromBitLength(int bitLength)
        {
            return bitLength / 8 + (bitLength % 8 == 0 ? 0 : 1);
        }

        public static byte[] BitsToBytes(BitArray bits)
        {
            int byteLength = ByteLengthFromBitLength(bits.Length);

            byte[] bytes = new byte[byteLength];
            bits.CopyTo(bytes, 0);

            return bytes;
        }
    }
}