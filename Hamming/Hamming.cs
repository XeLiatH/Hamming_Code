using System;
using System.Collections;
using System.Collections.Generic;


namespace Hamming
{
    public static class Hamming
    {
        private static int InputLength = 11;
        private static int OutputLength = 16;

        private static int WordParityPosition = 15;
        private static Dictionary<int, int> ParityPositions = new Dictionary<int, int>(
            // note: pozice jsou mocniny 2 ponizeny o 1, protoze pole indexuje od 0
            new KeyValuePair<int, int>[] {
                new KeyValuePair<int, int>(0, 0),
                new KeyValuePair<int, int>(1, 1),
                new KeyValuePair<int, int>(3, 2),
                new KeyValuePair<int, int>(7, 3),
            }
        );

        private static int[][] ParityBitsPositions = new int[][]
        {
            new int[] { 0, 1, 3, 4, 6, 8, 10 },
            new int[] { 0, 2, 3, 5, 6, 9, 10 },
            new int[] { 1, 2, 3, 7, 8, 9, 10 },
            new int[] { 4, 5, 6, 7, 8, 9, 10 }
        };

        public static BitArray AddParityBits(BitArray input)
        {
            if (input.Length != InputLength)
            {
                throw new ArgumentException(string.Format("Invalid input length. Must match the length of {0}.", InputLength.ToString()));
            }

            BitArray parity = new BitArray(ParityPositions.Count);

            for (int i = 0; i < ParityPositions.Count; i++)
            {
                int cnt = 0;
                foreach (int bitPosition in ParityBitsPositions[i])
                {
                    cnt += input[bitPosition] ? 1 : 0;
                }

                parity[i] = cnt % 2 == 0 ? false : true;
            }

            return InsertParityBits(input, parity);
        }

        public static BitArray InsertParityBits(BitArray input, BitArray parity)
        {
            List<bool> inputList = new List<bool>();

            int j = 0;
            for (int i = 0; i < input.Length + parity.Length; i++)
            {
                bool bit = ParityPositions.ContainsKey(i)
                    ? parity[ParityPositions[i]]
                    : input[j++];

                inputList.Add(bit);
            }

            inputList.Add(!ParityHelper.EvenParity(new BitArray(inputList.ToArray())));

            return new BitArray(inputList.ToArray());
        }
    }
}
