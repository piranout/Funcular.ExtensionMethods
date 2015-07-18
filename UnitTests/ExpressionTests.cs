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
using Funcular.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion




namespace UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        private readonly string[] _strings = {null, string.Empty, "0", " ", "a"};




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
            Assert.IsTrue(new List<string>(new[] {" "}).HasContents());
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
        }
        #endregion




        #region Queryables
        [TestMethod]
        public void Modified_Expression_Changes_String_Comparison()
        {
            Expression<Func<string, string, bool>> expr = (s1, s2) => s1 == s2;
            Debug.WriteLine(expr);

            var treeModifier = new CaseInsensitiveExpressionVisitor();
            var modifiedExpr = (Expression<Func<string, string, bool>>) treeModifier.Modify(expr);

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
            var nullString = ((string) null);
            Assert.IsNull(nullString.Coalesce(nullString, nullString));
        }

        [TestMethod]
        public void Coalesce_Strings_Is_Not_Null()
        {
            var nullString = ((string) null);
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
            var nullString = ((string) null);
#pragma warning restore 219
            var space = " ";
            var abc = "abc";
            var abcde = "abcde";

            var strings = new[] {space, abc, abcde};

            Assert.IsTrue(strings.Contains(abc));
            Assert.IsTrue(strings.Contains(abc.ToUpper()));
            Assert.IsTrue(abcde.ContainsAny(strings));
            Assert.IsTrue(abcde.ToUpper().ContainsAny(strings));

            Assert.IsTrue(strings.Contains("abcdefg") == false);
            Assert.IsTrue("abcdefg".ContainsAny(strings));
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
            var nullString = (string) null;
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
            Assert.IsTrue("a".IsIn("c","b","a"));
            Assert.IsTrue("A".IsIn("c", "b", "a"));
            Assert.IsFalse("z".IsIn("c", "b", "a"));
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
        public void Positional_Strings_Locate_Values()
        {
            Assert.IsTrue("<html><body>".LeftOfFirst("<body>") == "<html>");
            Assert.AreSame(((string)null).LeftOfFirst("<body>"), string.Empty);
            Assert.AreSame("".LeftOfFirst("<body>"), string.Empty);
            Assert.AreSame(" ".LeftOfFirst("<body>"), string.Empty);


            Assert.IsTrue("<html><body>".RightOfFirst("<html>") == "<body>");
            Assert.AreSame(((string)null).RightOfFirst("<body>"), string.Empty);
            Assert.AreSame("".RightOfFirst("<body>"), string.Empty);
            Assert.AreSame(" ".RightOfFirst("<body>"), string.Empty);


            Assert.IsTrue("<html><body><div><div>".LeftOfLast("<div>") == "<html><body><div>");
            Assert.AreSame(((string)null).LeftOfLast("<div>"), string.Empty);
            Assert.AreSame("".LeftOfLast("<div>"), string.Empty);
            Assert.AreSame(" ".LeftOfLast("<div>"), string.Empty);


            Assert.IsTrue("<html><head><body><div><div></div></div>".RightOfLast("<div>") == "</div></div>");
            Assert.AreSame(((string)null).RightOfLast("<div>"), string.Empty);
            Assert.AreSame("".RightOfLast("<div>"), string.Empty);
            Assert.AreSame(" ".RightOfLast("<div>"), string.Empty);
        }

        [TestMethod]
        public void Coalesce_Decimals_Is_Not_Null()
        {
            decimal? nullDecimal = null;
            decimal? nonNullDecimal = 0.0m;
            Assert.IsNotNull(nullDecimal.Coalesce(nonNullDecimal));
        }

        [TestMethod]
        public void Random_Longs_Are_Within_Range()
        {
            Random rnd = new Random();
            Assert.IsTrue(rnd.NextLong(100) <= 100);
            Assert.IsTrue(rnd.NextLong(50, 100) <= 100 && rnd.NextLong(50, 100) >= 50);
        }
        #endregion
    }
}