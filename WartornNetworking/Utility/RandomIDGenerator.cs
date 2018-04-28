using System;
using System.Text;
namespace WartornNetworking
{
    namespace Utility
    {
        public static class RandomIdGenerator
        {
            private static char[] Digits =
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
                .ToCharArray();

            private static Random _random = new Random();

            public static void Init(int seed)
            {
                _random = new Random(seed);
            }

            public static string GetBase62(int length)
            {
                var sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                    sb.Append(Digits[_random.Next(62)]);

                return sb.ToString();
            }

            public static string GetBase36(int length)
            {
                var sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                    sb.Append(Digits[_random.Next(36)]);

                return sb.ToString();
            }

            public static long GetBase62inLong(int length)
            {
                var sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                    sb.Append(Digits[_random.Next(62)]);

                return Base62ToDecimalSystem(sb.ToString());
            }

            public static long GetBase36inLong(int length)
            {
                var sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                    sb.Append(Digits[_random.Next(36)]);

                return Base36ToDecimalSystem(sb.ToString());
            }

            public static long Base36ToDecimalSystem(string number)
            {
                int radix = 36;

                if (String.IsNullOrEmpty(number))
                    return 0;

                long result = 0;
                long multiplier = 1;
                for (int i = number.Length - 1; i >= 0; i--)
                {
                    char c = number[i];
                    if (i == 0 && c == '-')
                    {
                        // This is the negative sign symbol
                        result = -result;
                        break;
                    }

                    int digit = Array.IndexOf(Digits, c);
                    if (digit == -1)
                        throw new ArgumentException(
                            "Invalid character in the arbitrary numeral system number",
                            "number");

                    result += digit * multiplier;
                    multiplier *= radix;
                }

                return result;
            }

            public static long Base62ToDecimalSystem(string number)
            {
                int radix = 62;

                if (String.IsNullOrEmpty(number))
                    return 0;

                long result = 0;
                long multiplier = 1;
                for (int i = number.Length - 1; i >= 0; i--)
                {
                    char c = number[i];
                    if (i == 0 && c == '-')
                    {
                        // This is the negative sign symbol
                        result = -result;
                        break;
                    }

                    int digit = Array.IndexOf(Digits, c);
                    if (digit == -1)
                        throw new ArgumentException(
                            "Invalid character in the arbitrary numeral system number",
                            "number");

                    result += digit * multiplier;
                    multiplier *= radix;
                }

                return result;
            }

            public static string DecimalToBase62(long decimalNumber)
            {
                int radix = 62;
                const int BitsInLong = 64;

                if (decimalNumber == 0)
                    return "0";

                int index = BitsInLong - 1;
                long currentNumber = Math.Abs(decimalNumber);
                char[] charArray = new char[BitsInLong];

                while (currentNumber != 0)
                {
                    int remainder = (int)(currentNumber % radix);
                    charArray[index--] = Digits[remainder];
                    currentNumber = currentNumber / radix;
                }

                string result = new String(charArray, index + 1, BitsInLong - index - 1);
                if (decimalNumber < 0)
                {
                    result = "-" + result;
                }

                return result;
            }

            public static string DecimalToBase36(long decimalNumber)
            {
                int radix = 36;
                const int BitsInLong = 64;

                if (decimalNumber == 0)
                    return "0";

                int index = BitsInLong - 1;
                long currentNumber = Math.Abs(decimalNumber);
                char[] charArray = new char[BitsInLong];

                while (currentNumber != 0)
                {
                    int remainder = (int)(currentNumber % radix);
                    charArray[index--] = Digits[remainder];
                    currentNumber = currentNumber / radix;
                }

                string result = new String(charArray, index + 1, BitsInLong - index - 1);
                if (decimalNumber < 0)
                {
                    result = "-" + result;
                }

                return result;
            }
        }
    }
}