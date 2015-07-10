#region File info
// *********************************************************************************************************
// Funcular.ExtensionMethods>UnitTests>ExpressionTests.cs
// Created: 2015-07-08 5:06 PM
// Updated: 2015-07-10 11:42 AM
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

        [TestMethod]
        public void Intercepted_Queryable_Is_Case_Insensitive()
        {
            var things = new List<Thing>
            {
                new Thing {String1 = "S1", String2 = "S1"},
                new Thing {String1 = "S1", String2 = "s1"},
            };

            var originalQueryable = things.AsQueryable();
            var caseInsensitiveQueryable = originalQueryable.AsCaseInsensitive();

            Assert.IsTrue(originalQueryable.Count(thing => thing.String1 == thing.String2) == 1);
            Assert.IsTrue(caseInsensitiveQueryable.Count(thing => thing.String1 == thing.String2) == 2);
        }


        #region Nested type: Thing
        private class Thing
        {
            public string String1 { get; set; }
            public string String2 { get; set; }
        }
        #endregion
    }
}