using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public static class TokenAmountConverter
    {
        //maybe some day use clients Culture? for now invariant
        public static CultureInfo Culture = CultureInfo.InvariantCulture;

        public static decimal ToDecimal(double? balance, int? decimals, decimal conversionRate = 1)
        {
            if (balance == null) return 0;
            return (decimal)(((decimals ?? 0) > 0) ? balance / Math.Pow(10, (double)decimals!) : balance) * conversionRate;
        }

        public static string ToString(double? balance, int? decimals, decimal conversionRate = 1)
        {
            if (balance == null) return "";

            decimal floatBalance = ToDecimal(balance, decimals, conversionRate);
            string format = "";
            if (decimals != null)
            {
                //reduce digits, the larger balance is, i.e. exponent (never reduce digits, if sub zero)
                int formatDecimals = Math.Max(decimals.Value - Math.Max((int)Math.Log10(Math.Abs((double)floatBalance)), 0), 0);
                format = $"#,###0.{new string('#', formatDecimals)}";
            }
            return floatBalance.ToString(format, Culture);
        }

        public static decimal ToDecimalWithExponent(decimal amount, out string exponentPrefix)
        {
            exponentPrefix = "";
            if (amount == 0) return amount;

            //get the exponent - sign doesn't matter, i.e. 6 for 1,000,000 aka 1E6
            var exponent = Math.Log10((double)Math.Abs(amount));

            //we since we're only interested in k, M and B, keep it simple
            if (exponent >= 9)
            {
                exponentPrefix = "B";
                return amount / (decimal)1E9;
            }
            else if (exponent >= 6)
            {
                exponentPrefix = "M";
                return amount / (decimal)1E6;
            }
            else if (exponent >= 3)
            {
                exponentPrefix = "k";
                return amount / (decimal)1E3;
            }
            else
                return amount;
        }

        public static string ToStringWithExponent(double num, int decimals, decimal conversionRate, string format = "N3")
        {
            string expPrefix = "";
            decimal amount = ToDecimalWithExponent(ToDecimal(num, decimals, conversionRate), out expPrefix);
            return amount.ToString(format, Culture) + expPrefix;
        }
    }
}
