using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace WartornNetworking
{
    namespace Utility
    {
        public static class Helper
        {
            public static string DecodeEncodedNonAsciiCharacters(string value)
            {
                return Regex.Replace(
                    value,
                    @"\\u(?<Value>[a-zA-Z0-9]{4})",
                    m =>
                    {
                        return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                    });
            }
        }

        public static class ExtensionMethod
        {
            public static T ToEnum<T>(this string value)
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
        }
    }
}