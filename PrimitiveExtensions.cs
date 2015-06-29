#region File info

// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>PrimitiveExtensions.cs
// Created: 2015-06-26 3:02 PM
// Updated: 2015-06-29 10:46 AM
// By: Paul Smith 
// 
// *********************************************************************************************************
// LICENSE: The MIT License (MIT)
// *********************************************************************************************************
// Copyright (c) 2010-2015 <copyright holders>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// *********************************************************************************************************

#endregion



#region Usings

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion



namespace Funcular.ExtensionMethods
{
    public static class PrimitiveExtensions
    {
        private static readonly Regex _notNullOrWhitespace = new Regex(@"/^[\s]*$/", RegexOptions.Compiled);

        /// <summary>
        ///     Returns the first non null value found.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static string Coalesce(this string value, string other)
        {
            return value.HasValue() ? value : other;
        }

        /// <summary>
        ///     Returns the first non null value found.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static string Coalesce(this string value, params string[] others)
        {
            return value.HasValue() ? value : others.FirstOrDefault(s => s.HasValue());
        }

        /// <summary>
        ///     Returns the first non null value found.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static decimal Coalesce(this decimal? value, params decimal?[] others)
        {
            if (value.HasValue)
                return value.Value;
            decimal? tmp;
            return (others == null || others.Length == 0) ? default(decimal)
                : (tmp = (others.FirstOrDefault(arg => arg.HasValue))).HasValue ? tmp.Value : default(decimal);
        }

        /// <summary>
        ///     The negation of string.IsNullOrEmpty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasValue(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }

        /// <summary>
        ///     The negation of string.IsNullOrWhitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasWordValue(this string value)
        {
#if GT_NET_35
            return !string.IsNullOrWhiteSpace(value);
#else
            return !(_notNullOrWhitespace.IsMatch(value ?? ""));
#endif
        }

        public static string TrimOrEmpty(this string value)
        {
            return (value ?? String.Empty).Trim();
        }

        /// <summary>
        ///     Returns true if string <paramref name="value" /> contains any of <paramref name="others" /> (case-insensitive).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static bool ContainsAny(this string value, params string[] others)
        {
            if (others.HasContents() && value.HasValue())
                return others.Any(s => value.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            return false;
        }

        /// <summary>
        ///     Returns true if <paramref name="values" /> contain <paramref name="sought" />, used to
        ///     perform case-insensitive searches in arrays.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="sought"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static bool Contains(this string[] values, string sought, bool caseSensitive = false)
        {
            if (caseSensitive)
                return values.AsQueryable().Any(s => s == sought);
            return values.AsQueryable().Any(s => s.Equals(sought, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Returns true if <paramref name="value" /> is any of <paramref name="others" /> (case-insensitive).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static bool IsIn(this string value, params string[] others)
        {
            if (others.HasContents() && value.HasValue())
                return others.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));
            return false;
        }

        /// <summary>
        ///     Recursively remove all occurrences of <paramref name="needle" /> from <paramref name="haystack" />.
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceAll(this string haystack, string needle, string replacement)
        {
            const StringComparison COMPARISON = StringComparison.Ordinal;
            while (haystack.IndexOf(needle, COMPARISON) > -1)
                haystack = haystack.Replace(needle, replacement);
            return haystack;
        }

        /// <summary>
        ///     Returns a string comprised of only the digits in <paramref name="incomingNumber" />
        /// </summary>
        /// <param name="incomingNumber"></param>
        /// <returns></returns>
        public static string ToNumericString(this string incomingNumber)
        {
            if (!incomingNumber.HasValue())
                return incomingNumber;
            return String.Join("", incomingNumber.Split(incomingNumber
                .Where(c => !Char.IsNumber(c))
                .Distinct()
                .ToArray()));
        }

        /// <summary>
        ///     If <paramref name="value" /> does not end with <paramref name="endWith" />, appends it.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="endWith"></param>
        /// <returns></returns>
        public static string EnsureEndsWith(this string value, string endWith)
        {
            return value.EndsWith(endWith) ? value : value + endWith;
        }

        /// <summary>
        ///     If <paramref name="value" /> does not start with <paramref name="startWith" />, prepends it.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startWith"></param>
        /// <returns></returns>
        public static string EnsureStartsWith(this string value, string startWith)
        {
            return value.StartsWith(startWith) ? value : startWith + value;
        }

        /// <summary>
        ///     If <paramref name="originalString" /> contains <paramref name="ofString" />, returns everything before
        ///     <paramref name="ofString" />.
        ///     Returns an empty string if <paramref name="originalString" /> starts with <paramref name="ofString" />.
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="ofString"></param>
        /// <returns></returns>
        public static string LeftOfFirst(this string originalString, string ofString)
        {
            if (String.IsNullOrEmpty(originalString))
                return originalString;
            int idx = originalString.IndexOf(ofString, StringComparison.OrdinalIgnoreCase);
            return (idx >= 0 ? originalString.Substring(0, idx) : "");
        }

        /// <summary>
        ///     If <paramref name="originalString" /> contains <paramref name="ofString" />, returns the text to the right of it.
        ///     Otherwise returns the full string.
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="ofString"></param>
        /// <returns></returns>
        public static string LeftOfLast(this string originalString, string ofString)
        {
            if (String.IsNullOrEmpty(originalString))
                return originalString;
            int idx = originalString.LastIndexOf(ofString, StringComparison.OrdinalIgnoreCase);
            return (idx >= 0 ? originalString.Substring(0, idx) : originalString);
        }

        /// <summary>
        ///     If <paramref name="originalString" /> contains <paramref name="ofString" />, returns the portion after the last
        ///     occurrence.
        ///     If not, returns an empty string.
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="ofString"></param>
        /// <returns></returns>
        public static string RightOfLast(this string originalString, string ofString)
        {
            if (String.IsNullOrEmpty(originalString))
                return originalString;
            int idx = originalString.LastIndexOf(ofString, StringComparison.OrdinalIgnoreCase);
            return (idx < 0 ?
                "" :
                originalString.Substring(idx + ofString.Length, originalString.Length - (idx + ofString.Length)));
        }

        /// <summary>
        ///     If <paramref name="originalString" /> contains <paramref name="ofString" />, returns the portion after the first
        ///     occurrence.
        ///     If not, returns original string.
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="ofString"></param>
        /// <returns></returns>
        public static string RightOfFirst(this string originalString, string ofString)
        {
            if (String.IsNullOrEmpty(originalString))
                return originalString;
            int idx = originalString.IndexOf(ofString, StringComparison.OrdinalIgnoreCase);
            return (idx < 0 ?
                originalString :
                originalString.Substring(idx + ofString.Length));
        }

        /// <summary>
        ///     Truncates the string if it exceeds the max length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        ///     If value ends with digits, returns as many ending characters as are digits,
        ///     otherwise, returns -1.
        /// </summary>
        public static int GetIntegerSuffix(this string value)
        {
            int ret = -1;
            if (!String.IsNullOrWhiteSpace(value))
            {
                var sb = new StringBuilder();
                char[] chars = value.ToCharArray();
                for (int i = chars.Length - 1; i >= 0; i--)
                {
                    if (Char.IsDigit(chars[i]))
                        sb.Insert(0, chars[i]);
                    else
                        break;
                }
                int parsed;
                if (Int32.TryParse(sb.ToString(), out parsed))
                    ret = parsed;
            }
            return ret;
        }

        /// <summary>
        ///     If value is all digits, returns true, otherwise false.
        /// </summary>
        public static bool IsPositiveInteger(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return false;
            char[] chars = value.ToCharArray();
            return !chars.Any(c => !Char.IsDigit(c));
        }

        /// <summary>
        ///     True if value without spaces is all digits, or a decimal place, comma, or dollar sign, otherwise false.
        /// </summary>
        public static bool IsNumeric(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            value = value.Replace(" ", "");
            char[] chars = value.ToCharArray();
            return chars.All(c => Char.IsDigit(c) || c.ToString().IsIn(".", ",", "$"));
        }

        /// <summary>
        ///     If value is 5 or 9 digits (plus dash), returns true, otherwise false.
        /// </summary>
        public static bool IsUsZipCode(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return false;
            value = value.Replace(" ", "").Replace("-", "");
            return (new[] {5, 9}).Contains(value.Length) && value.ToCharArray().All(Char.IsDigit);
        }

        /// <summary>
        ///     If fractional component is greater than 0, rounds value up to the next integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int RoundUp(this decimal value)
        {
            decimal mod = value%1;
            return (int) (mod == 0 ? value : Math.Truncate(value) + 1);
        }

        /// <summary>
        ///     Rounds value down to its integer component
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int RoundDown(this decimal value)
        {
            return (int) Math.Truncate(value);
        }

        /// <summary>
        ///     Convert this array of bytes to a hexadecimal string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHex(this byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }

        /// <summary>
        ///     How many billionths (1/1,000,000,000) of a second <paramref name="interval" /> represents.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static long TotalNanoseconds(this TimeSpan interval)
        {
            return (long) (interval.TotalMilliseconds*1000000.0000);
        }

        /// <summary>
        ///     How many millionths (1/1,000,000) of a second <paramref name="interval" /> represents.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static long TotalMicroseconds(this TimeSpan interval)
        {
            return (long) (interval.TotalMilliseconds*1000.0000);
        }

        public static long NextLong(this Random rand, long max)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand%max));
        }

        public static long NextLong(this Random rand, long min, long max)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand%(max - min)) + min);
        }
    }
}