using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hamming
{
    public static class ParityHelper
    {
        public static bool EvenParity(BitArray ba)
        {
            int count = 0;
            foreach (bool bit in ba)
            {
                count += bit ? 1 : 0;
            }

            return count % 2 == 0;
        }
    }
}
