#region File info
// *********************************************************************************************************
// Funcular.ExtensionMethods>UnitTests>ExpressionTests.cs
// Created: 2015-07-08 7:20 PM
// Updated: 2015-07-18 10:32 AM
// By: Paul Smith 
// 
// *********************************************************************************************************
// LICENSE: The MIT License (MIT)
// *********************************************************************************************************
// Copyright (c) 2010-2015 Funcular Labs and Paul Smith
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Funcular.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion




namespace UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        private readonly string[] _strings = { null, string.Empty, "0", " ", "a" };




        #region Nested type: Thing
        private class Thing
        {
            public string String1 { get; set; }
            public string String2 { get; set; }
            public int Int1 { get; set; }
            public int Int2 { get; set; }
        }

        private class DerivedThing : Thing
        {
            public string String3 { get; set; }
            public string String4 { get; set; }
        }
        #endregion




        #region Enumerables
        [TestMethod]
        public void Empty_Enumerable_Does_Not_Have_Contents()
        {
            Assert.IsTrue(Enumerable.Empty<string>().HasContents() == false);
            Assert.IsTrue(new List<string>().HasContents() == false);
        }

        [TestMethod]
        public void Enumerable_Has_Contents()
        {
            Assert.IsTrue(Enumerable.Range(0, 1).HasContents());
            Assert.IsTrue(new List<string>(new[] { " " }).HasContents());
        }

        [TestMethod]
        public void Order_By_Sorts_Correctly()
        {
            // Use DerivedThing instead of Thing to ensure member access via
            // reflection is including inherited properties.
            var things = new List<DerivedThing>
            {
                new DerivedThing {String1 = "A1", String2 = "Z9"},
                new DerivedThing {String1 = "00", String2 = "ZZ"},
                new DerivedThing {String1 = "Z9", String2 = "A1", String3 = "Blah", String4 = "Ignore me"}
            }.ToArray();

            Assert.IsTrue(things.OrderBy("String1").First().String1 == "00");
            Assert.IsTrue(things.OrderBy("String1", desc: true).First().String1 == "Z9");
            Assert.IsTrue(things.OrderBy("String2").First().String2 == "A1");
            Assert.IsTrue(things.OrderBy("String2", desc: true).First().String2 == "ZZ");

            things = Enumerable.Empty<DerivedThing>().ToArray();
            Assert.IsFalse(things.OrderBy("String1").HasContents());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = true)]
        public void Order_By_Throws_When_Parameter_Is_Null()
        {
            var things = new List<DerivedThing>
            {
                new DerivedThing {String1 = "A1", String2 = "Z9"},
                new DerivedThing {String1 = "00", String2 = "ZZ"},
                new DerivedThing {String1 = "Z9", String2 = "A1", String3 = "Blah", String4 = "Ignore me"}
            }.ToArray();
            var ordered = things.OrderBy(orderByProperty: "zasdgkha");
            Console.WriteLine(ordered);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = true)]
        public void Order_By_Throws_When_Value_Is_Null()
        {
            // ReSharper disable once RedundantAssignment
            var things = new List<DerivedThing>
            {
                new DerivedThing {String1 = "A1", String2 = "Z9"},
                new DerivedThing {String1 = "00", String2 = "ZZ"},
                new DerivedThing {String1 = "Z9", String2 = "A1", String3 = "Blah", String4 = "Ignore me"}
            }.ToArray();
            things = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            var ordered = things.OrderBy(orderByProperty: "String1");
        }

        [TestMethod]
        public void Null_Enumerable_Or_Empty_Is_Not_Null()
        {
            IEnumerable<int> ints = null;
            Assert.IsNotNull(ints.OrEmpty());
            ints = Enumerable.Range(1, 5).OrEmpty();
            Assert.IsTrue(ints != null && ints.Count() == 5);
        }

        [TestMethod]
        public void Median_Enumerable_Finds_Value()
        {
            IEnumerable<int> ints = Enumerable.Range(1, 10);
            var array = ints.ToArray();
            IEnumerable<Thing> things = array.Select(t => new Thing() { Int1 = t, Int2 = t });
            // ReSharper disable PossibleInvalidOperationException
            var median = things.Median(x => x.Int1).Value;
            Assert.IsTrue(median == 5.5m);
            ints = Enumerable.Range(1, 11);
            array = ints.ToArray();
            things = array.Select(t => new Thing() { Int1 = t, Int2 = t });
            median = things.Median(x => x.Int1).Value;
            // ReSharper restore PossibleInvalidOperationException
            Assert.IsTrue(median == 6);

            IEnumerable<int?> nullableInts;
            nullableInts = array.Select(i => (int?)i).AsEnumerable();
            var mid = nullableInts.Median();
            Assert.IsTrue(mid == 6m);

            nullableInts = Enumerable.Empty<int?>();
            mid = nullableInts.Median();
            Assert.IsNull(mid);

            Assert.IsNull(Enumerable.Empty<int>().Median());

        }

        [TestMethod]
        public void Enumerable_Safe_Cast_Fails_Gracefully()
        {
            IEnumerable<int> ints = Enumerable.Range(0, 10);
            Assert.AreEqual(ints.SafeCast<int,string>().Count(), 0);
        }

        [TestMethod]
        public void Enumerable_Safe_Cast_Does_Not_Throw()
        {
            IEnumerable<int> ints = Enumerable.Range(0, 10);
            IEnumerable<string> strings = ints.Select(i => i.ToString());
            var dateTimes = strings.SafeCast<string, DateTime>().ToList();
            Assert.IsFalse(dateTimes.HasContents());
        }
        #endregion




        #region Queryables
        [TestMethod]
        public void Modified_Expression_Changes_String_Comparison()
        {
            Expression<Func<string, string, bool>> expr = (s1, s2) => s1 == s2;
            Debug.WriteLine(expr);

            var treeModifier = new CaseInsensitiveExpressionVisitor();
            var modifiedExpr = (Expression<Func<string, string, bool>>)treeModifier.Modify(expr);

            Debug.WriteLine(modifiedExpr);
            var modifiedExpr2 = modifiedExpr.Compile();
            var expr2 = expr.Compile();

            Debug.WriteLine("Original expression, “s1” == “S1” is {0}", expr2("s1", "S1"));
            Debug.WriteLine("Modified expression, “s1” == “S1” is {0}", modifiedExpr2("s1", "S1"));

            Assert.IsFalse(expr2("s1", "S1"));
            Assert.IsTrue(modifiedExpr2("s1", "S1"));

        }

        /// <summary>
        /// Portions of this test don't relate to supported functionality, but 
        /// are included for coverage completeness.
        /// </summary>
        [TestMethod]
        public void Case_Insensitive_Does_Not_Suppress_Native_Operator()
        {
            var things = new List<Thing>
            {
                new Thing {String1 = "S1", String2 = "S1", Int1 = 1, Int2 = 2},
                new Thing {String1 = "S1", String2 = "s1", Int1 = 4, Int2 = 5}
            }.AsQueryable();

            var caseInsensitiveQueryable = things.AsCaseInsensitive();

            Assert.IsTrue(caseInsensitiveQueryable.Count(thing => thing.Int2 > thing.Int1) == 2);
            foreach (var thing in caseInsensitiveQueryable)
            {
                Assert.IsTrue(thing.Int2 > thing.Int1);
            }

            Expression<Func<Thing, bool>> expr = (s1) => s1.String1 != s1.String2;

            var execute = caseInsensitiveQueryable.Provider.Execute(expr);
            Assert.IsNotNull(execute);

            Assert.IsNotNull(caseInsensitiveQueryable.ElementType);
            Assert.IsNotNull(caseInsensitiveQueryable.Expression);
            Assert.IsNotNull(((IEnumerable)caseInsensitiveQueryable).GetEnumerator());

        }



        [TestMethod]
        public void Intercepted_Queryable_Is_Case_Insensitive()
        {
            var things = new List<Thing>
            {
                new Thing {String1 = "S1", String2 = "S1"},
                new Thing {String1 = "S1", String2 = "s1"}
            };

            var originalQueryable = things.AsQueryable();
            var caseInsensitiveQueryable = originalQueryable.AsCaseInsensitive();

            Assert.IsTrue(originalQueryable.Count(thing => thing.String1 == thing.String2) == 1);
            Assert.IsTrue(caseInsensitiveQueryable.Count(thing => thing.String1 == thing.String2) == 2);
        }
        #endregion




        #region Primitives
        [TestMethod]
        public void Coalesce_Null_Strings_Is_Null()
        {
            var nullString = ((string)null);
            Assert.IsNull(nullString.Coalesce(nullString, nullString));
        }

        [TestMethod]
        public void Coalesce_Strings_Is_Not_Null()
        {
            var nullString = ((string)null);
            Assert.IsNotNull(nullString.Coalesce(string.Empty));
            // Should prefer the space to the empty string:
            Assert.IsTrue(string.Empty.Coalesce(" ") == " ");
            // Should prefer non-whitespace over the space:
            Assert.IsTrue(" ".Coalesce("@") == "@");

            // ReSharper disable ExpressionIsAlwaysNull
            Assert.IsNotNull(nullString.Coalesce(nullString, string.Empty));
            // Should prefer the space to the empty string:
            Assert.IsTrue(nullString.Coalesce(nullString, string.Empty, " ") == " ");
            // Should prefer non-whitespace over the space:
            Assert.IsTrue(nullString.Coalesce(nullString, string.Empty, " ", "@") == "@");
            // ReSharper restore ExpressionIsAlwaysNull
        }


        [TestMethod]
        public void Strings_Contain_Others()
        {
#pragma warning disable 219
            var nullString = ((string)null);
#pragma warning restore 219
            var space = " ";
            var abc = "abc";
            var abcde = "abcde";

            var strings = new[] { space, abc, abcde };

            Assert.IsTrue(strings.Contains(abc));
            Assert.IsTrue(strings.Contains(abc.ToUpper()));
            Assert.IsTrue(abcde.ContainsAny(strings));
            Assert.IsTrue(abcde.ToUpper().ContainsAny(strings));

            Assert.IsTrue(strings.Contains("abcdefg") == false);
            Assert.IsTrue("abcdefg".ContainsAny(strings));
            Assert.IsFalse(((string)null).ContainsAny(strings));
            Assert.IsFalse(((string)null).ContainsAny(string.Empty));
            Assert.IsFalse(string.Empty.ContainsAny(null));
            Assert.IsFalse(strings.Contains(abcde.ToUpper(), caseSensitive: true));

        }

        [TestMethod]
        public void Empty_or_Null_String_Does_Not_Have_Value()
        {
            Assert.IsTrue(_strings[0].HasValue() == false);
            Assert.IsTrue(_strings[1].HasValue() == false);
        }

        [TestMethod]
        public void Strings_Have_Value()
        {
            Assert.IsTrue(_strings[2].HasValue());
            Assert.IsTrue(_strings[4].HasValue());
        }

        [TestMethod]
        public void Space_Does_Not_have_Word_Value()
        {
            Assert.IsTrue(_strings[3].IsNonWhitespace() == false);
        }

        [TestMethod]
        public void Trim_Null_Or_Empty_Is_Empty()
        {
            var nullString = (string)null;
            Assert.IsNotNull(nullString.TrimOrEmpty());
        }

        [TestMethod]
        public void Get_Integer_Suffix_Returns_Integer()
        {
            Assert.IsTrue("Number 1".GetIntegerSuffix() == 1);
        }

        [TestMethod]
        public void Is_Numeric_Distinguishes_Numbers()
        {
            Assert.IsTrue("1".IsNumeric());
            Assert.IsTrue("0".IsNumeric());
            Assert.IsTrue("-1".IsNumeric());
            Assert.IsTrue("$123.45".IsNumeric());
            Assert.IsFalse("4 score".IsNumeric());
            Assert.IsFalse("".IsNumeric());
            Assert.IsFalse(((string)null).IsNumeric());
        }

        [TestMethod]
        public void Is_Positive_Integer_Distinguishes_Positive_Integers()
        {
            Assert.IsTrue("1".IsPositiveInteger());
            Assert.IsTrue("0".IsPositiveInteger());
            Assert.IsFalse("-1".IsPositiveInteger());
            Assert.IsFalse("".IsPositiveInteger());
            Assert.IsFalse(" ".IsPositiveInteger());
            Assert.IsFalse("a".IsPositiveInteger());
            Assert.IsFalse(((string)null).IsPositiveInteger());

        }

        [TestMethod]
        public void Is_In_Includes_Excludes()
        {
            Assert.IsTrue("a".IsIn("c", "b", "a"));
            Assert.IsTrue("A".IsIn("c", "b", "a"));
            Assert.IsFalse("z".IsIn("c", "b", "a"));

            Assert.IsFalse(string.Empty.IsIn("a", "b"));
            Assert.IsTrue(string.Empty.IsIn("a", "b", string.Empty));
            Assert.IsFalse(((string)null).IsIn("a", "b", (string)null));
            Assert.IsFalse(string.Empty.IsIn(new string[0]));


        }

        [TestMethod]
        public void Only_Zip_Codes_Are_Zip_Codes()
        {
            Assert.IsTrue("12345-0000".IsUsZipCode());
            Assert.IsTrue("60611".IsUsZipCode());

            Assert.IsFalse("6061".IsUsZipCode());
            Assert.IsFalse("6061a".IsUsZipCode());
            Assert.IsFalse(((string)null).IsUsZipCode());
            Assert.IsFalse("".IsUsZipCode());
            Assert.IsFalse(" ".IsUsZipCode());
        }

        [TestMethod]
        public void LeftOf_Strings_Locate_Correct_Values()
        {
            Assert.IsTrue("<html><body>".LeftOfFirst("<body>") == "<html>");
            Assert.AreSame(((string)null).LeftOfFirst("<body>"), string.Empty);
            Assert.AreSame("".LeftOfFirst("<body>"), string.Empty);
            Assert.AreSame(" ".LeftOfFirst("<body>"), string.Empty);


            Assert.IsTrue("<html><body><div><div>".LeftOfLast("<div>") == "<html><body><div>");
            Assert.AreSame(((string)null).LeftOfLast("<div>"), string.Empty);
            Assert.AreSame("".LeftOfLast("<div>"), string.Empty);
            Assert.AreSame(" ".LeftOfLast("<div>"), string.Empty);

        }

        [TestMethod]
        public void RightOf_Strings_Locate_Correct_Values()
        {
            Assert.IsTrue("<html><body>".RightOfFirst("<html>") == "<body>");
            Assert.AreSame(((string)null).RightOfFirst("<body>"), string.Empty);
            Assert.AreSame("".RightOfFirst("<body>"), string.Empty);
            Assert.AreSame(" ".RightOfFirst("<body>"), string.Empty);

            Assert.IsTrue("<html><head><body><div><div></div></div>".RightOfLast("<div>") == "</div></div>");
            Assert.AreSame(((string)null).RightOfLast("<div>"), string.Empty);
            Assert.AreSame("".RightOfLast("<div>"), string.Empty);
            Assert.AreSame(" ".RightOfLast("<div>"), string.Empty);
        }

        [TestMethod]
        public void To_Numeric_String_Includes_Only_Digits()
        {
            var str = "abc1def2ghi3jkl4mno5pqr6stu7vwx8yz90 right?";
            var numeric = str.ToNumericString();
            Assert.IsTrue(numeric.All(char.IsDigit));
            Assert.AreEqual(string.Empty.ToNumericString(), string.Empty);
            Assert.AreEqual(((string)null).ToNumericString(), ((string)null));
        }

        [TestMethod]
        public void Replace_All_Replaces_Recursively()
        {
            var str = "     spaces     spaces   spaces          spaces  spaces";
            var replaced = str.ReplaceAll("  ", "");
            Assert.IsFalse(replaced.Contains("  "));
            replaced = str.ReplaceAll("Z", "Y");
            Assert.AreEqual(str, replaced);
        }



        [TestMethod]
        public void Ensure_Methods_Append_Correctly()
        {
            Assert.IsTrue("<html><body></body>".EnsureEndsWith("</html>").EndsWith("</html>"));
            Assert.IsFalse("<html><body></body></html>".EnsureEndsWith("</html>").Contains("</html></html>"));

            Assert.IsTrue("<body></body></html>".EnsureStartsWith("<html>").StartsWith("<html>"));
            Assert.IsFalse("<html><body></body></html>".EnsureStartsWith("<html>").Contains("<html><html>"));
        }

        [TestMethod]
        public void Truncate_String_Produces_Expected_Length()
        {
            Assert.IsTrue("12345".Truncate(4).Length == 4);
            Assert.IsTrue("".Truncate(4).Length == 0);
        }

        [TestMethod]
        public void String_Contains_Respects_Comparison()
        {
            Assert.IsTrue("ABCDE".Contains("bcd", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse("ABCDE".Contains("bcd", StringComparison.CurrentCulture));
            Assert.IsFalse(string.Empty.Contains("bcd", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(((string)null).Contains("bcd", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void Coalesce_Decimals_Is_Not_Null()
        {
            decimal? nullDecimal = null;
            decimal? nonNullDecimal = 0.0m;
            Assert.IsNotNull(nullDecimal.Coalesce(nonNullDecimal));
            Assert.AreEqual(nonNullDecimal.Coalesce(5.0m), nonNullDecimal);
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual(nullDecimal.Coalesce(nullDecimal, nullDecimal), default(decimal));

            Assert.AreEqual(((decimal?)null).Coalesce(new decimal?[] { }), default(decimal));
            Assert.AreEqual(((decimal?)null).Coalesce(new decimal?[] { null, 5.0m }), 5.0m);

        }

        [TestMethod]
        public void Round_Up_Returns_Expected_Value()
        {
            decimal x = 0.0001m;
            Assert.AreEqual(x.RoundUp(), 1);
            x = 2.000m;
            Assert.AreEqual(x.RoundUp(), 2);
        }

        [TestMethod]
        public void Round_Down_Returns_Expected_Value()
        {
            decimal x = 1.9999m;
            Assert.AreEqual(x.RoundDown(), 1);
        }

        [TestMethod]
        public void Random_Longs_Are_Within_Range()
        {
            Random rnd = new Random();
            Assert.IsTrue(rnd.NextLong(100) <= 100);
            Assert.IsTrue(rnd.NextLong(50, 100) <= 100 && rnd.NextLong(50, 100) >= 50);
        }

        [TestMethod]
        public void Byte_Array_To_Hex_Converts_Back_To_Original_Value()
        {
            var originalString = "123456";
            var bytes = Encoding.UTF8.GetBytes(originalString);
            var hex = bytes.ToHex();// BitConverter.ToString(bytes);

            var bytes2 = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
            Assert.AreEqual(Encoding.UTF8.GetString(bytes2), originalString);
            Assert.AreEqual((new byte[0]).ToHex(), string.Empty);
        }
        #endregion

        #region Timespan tests
        [TestMethod]
        public void One_Second_Equals_One_Million_Nanoseconds()
        {
            var timespan = TimeSpan.FromSeconds(1);
            // 1 million
            Assert.AreEqual(timespan.TotalMicroseconds(), 1000000);
        }

        [TestMethod]
        public void One_Second_Equals_One_Billion_Microseconds()
        {
            var timespan = TimeSpan.FromSeconds(1);
            // 10 million
            var totalNanoseconds = timespan.TotalNanoseconds();
            Assert.AreEqual(totalNanoseconds, 1000000000);
        }
        #endregion

    }
}