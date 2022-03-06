using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public static class Utils
    {
        public static BigInteger ParseHexUnsigned(string toParse)
        {
            toParse = toParse.Replace("0x", "");
            var parsResult = BigInteger.Parse(toParse, System.Globalization.NumberStyles.HexNumber);
            if (parsResult < 0)
                parsResult = BigInteger.Parse("0" + toParse, System.Globalization.NumberStyles.HexNumber);
            return parsResult;
        }
    }
}
