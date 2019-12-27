using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace Hamming
{
    public static class Hamming
    {
        public static int INPUT_LENGTH = 11;
        public static int OUTPUT_LENGTH = 16;

        private static Dictionary<int, int> PARITY_POSITIONS = new Dictionary<int, int>(
            // note: pozice jsou mocniny 2 ponizeny o 1, protoze pole indexuje od 0
            new KeyValuePair<int, int>[] {
                new KeyValuePair<int, int>(0, 0),
                new KeyValuePair<int, int>(1, 1),
                new KeyValuePair<int, int>(3, 2),
                new KeyValuePair<int, int>(7, 3),
            }
        );

        private static int[][] DATA_BITS_POSITIONS_TO_CALCULATE_PARITY = new int[][]
        {
            new int[] { 0, 1, 3, 4, 6, 8, 10 },
            new int[] { 0, 2, 3, 5, 6, 9, 10 },
            new int[] { 1, 2, 3, 7, 8, 9, 10 },
            new int[] { 4, 5, 6, 7, 8, 9, 10 }
        };

        private static int[][] DATA_BITS_POSITIONS_TO_CHECK_PARITY = new int[][]
        {
            new int[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            new int[] { 1, 2, 5, 6, 9, 10, 13, 14 },
            new int[] { 3, 4, 5, 6, 11, 12, 13, 14 },
            new int[] { 7, 8, 9, 10, 11, 12, 13, 14 }
        };

        public static BitArray Encode(BitArray input)
        {
            if (input.Length != INPUT_LENGTH)
            {
                throw new ArgumentException(string.Format("Invalid input length. Must match the length of {0}.", INPUT_LENGTH.ToString()));
            }

            BitArray parity = new BitArray(PARITY_POSITIONS.Count);

            for (int i = 0; i < PARITY_POSITIONS.Count; i++)
            {
                int cnt = 0;
                foreach (int bitPosition in DATA_BITS_POSITIONS_TO_CALCULATE_PARITY[i])
                {
                    cnt += input[bitPosition] ? 1 : 0;
                }

                parity[i] = cnt % 2 == 0 ? false : true;
            }

            return InsertParityBits(input, parity);
        }

        public static BitArray Decode(BitArray encodedData)
        {
            if (encodedData.Length != OUTPUT_LENGTH)
            {
                throw new Exception(string.Format("Invalid encoded data length. Must match the length of {0}.", OUTPUT_LENGTH.ToString()));
            }

            int[] parities = new int[4];
            for (int i = 0; i < PARITY_POSITIONS.Count; i++)
            {
                int cnt = 0; // counting bits on the given positions
                foreach (int bitPosition in DATA_BITS_POSITIONS_TO_CHECK_PARITY[i])
                {
                    cnt += encodedData[bitPosition] ? 1 : 0;
                }

                parities[i] = cnt % 2;
            }

            int syndrome = parities.Sum();
            bool syndromeOK = syndrome == 0;

            int bitCnt = 0;
            foreach (bool bit in encodedData)
            {
                bitCnt += bit ? 1 : 0;
            }

            bool extraParity = bitCnt % 2 == 0 ? false : true;
            bool extraParityOK = extraParity == encodedData[encodedData.Count - 1];

            if (!syndromeOK && extraParityOK)
            {
                // throw new Exception("Too many errors in encoded data.");
            }

            if (!syndromeOK && !extraParityOK)
            {
                int positionToCorrect = 0;
                for (int i = 0; i < parities.Length; i++)
                {
                    if (parities[i] == 1)
                    {
                        positionToCorrect += (int)Math.Pow(2, i);
                    }
                }

                encodedData[positionToCorrect - 1] = !encodedData[positionToCorrect - 1];
            }

            return RemoveParityBits(encodedData);
        }

        private static BitArray InsertParityBits(BitArray input, BitArray parity)
        {
            List<bool> inputList = new List<bool>();

            int j = 0;
            for (int i = 0; i < input.Length + parity.Length; i++)
            {
                bool bit = PARITY_POSITIONS.ContainsKey(i)
                    ? parity[PARITY_POSITIONS[i]]
                    : input[j++];

                inputList.Add(bit);
            }

            inputList.Add(!ParityHelper.EvenParity(new BitArray(inputList.ToArray())));

            if (inputList.Count != OUTPUT_LENGTH)
            {
                throw new Exception("Error encounter during calculation of parity bits. Output length does not match.");
            }

            return new BitArray(inputList.ToArray());
        }

        private static BitArray RemoveParityBits(BitArray encodedData)
        {
            List<bool> originalData = new List<bool>();

            for (int i = 0; i < encodedData.Count - 1; i++)
            {
                if (!PARITY_POSITIONS.ContainsKey(i))
                {
                    originalData.Add(encodedData[i]);
                }
            }

            if (originalData.Count != INPUT_LENGTH)
            {
                throw new Exception("Error encounter during removal of parity bits. Input length does not match.");
            }

            return new BitArray(originalData.ToArray());
        }
    }
}
