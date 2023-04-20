using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        public static string UrlEncodeUpperCase(string stringToEncode)
        {
            var reg = new Regex(@"%[a-f0-9]{2}");
            stringToEncode = HttpUtility.UrlEncode(stringToEncode);
            return reg.Replace(stringToEncode, m => m.Value.ToUpperInvariant());
        }

        public static string Base58Encode(string input)
        {
            const string BASE58_CHARS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var bytes = Encoding.UTF8.GetBytes(input);
            var sb = new StringBuilder();
            ulong buffer = 0;
            int bitsLeft = 0;
            foreach (var b in bytes)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    int index = (int)(buffer >> (bitsLeft - 5)) & 0x1f;
                    sb.Append(BASE58_CHARS[index]);
                    bitsLeft -= 5;
                }
            }
            if (bitsLeft > 0)
            {
                int index = (int)(buffer << (5 - bitsLeft)) & 0x1f;
                sb.Append(BASE58_CHARS[index]);
            }
            return sb.ToString();
        }

        public static byte[] Base58ToByteArray(string input)
        {
            const string BASE58_CHARS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var bytes = new byte[input.Length];
            foreach (var c in input)
            {
                int value = BASE58_CHARS.IndexOf(c);
                if (value == -1)
                {
                    throw new ArgumentException("Invalid base58 string");
                }
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    value += 58 * bytes[i];
                    bytes[i] = (byte)(value % 256);
                    value /= 256;
                }
                if (value != 0)
                {
                    throw new ArgumentException("Invalid base58 string");
                }
            }
            return bytes;
        }
    }
}
