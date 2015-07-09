#region File info

// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>CaseInsensitiveExpressionVisitor.cs
// Created: 2015-07-08 7:20 PM
// Updated: 2015-07-08 8:35 PM
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
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Funcular.ExtensionMethods
{
    public class CaseInsensitiveExpressionVisitor : ExpressionVisitor
    {
        protected static readonly MethodInfo _method = typeof (string).GetMethod("Equals", new[] {typeof (string), typeof (StringComparison)});
        
        protected static readonly ConstantExpression _comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof (StringComparison));

        protected static readonly ConstantExpression _comparisonValue = Expression.Constant((int)StringComparison.OrdinalIgnoreCase, typeof (int));

        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitBinary(BinaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.Equal
                && !exp.Left.CanReduce && exp.Left.NodeType == ExpressionType.Parameter && exp.Left.Type == typeof(string)
                && !exp.Right.CanReduce && exp.Right.NodeType == ExpressionType.Parameter)
            {
                var method = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) });
                var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));
                // This converts {(s1, s2) => (s1 == s2)} into {(s1, s2) => s1.Equals(s2, OrdinalIgnoreCase)}:
                // ...when it should be {(s1, s2) => s1.Equals(s2, StringComparison.OrdinalIgnoreCase)}
                var caseInsensitiveMethodExp = Expression.Call(exp.Left, method, exp.Right, comparison);
                return caseInsensitiveMethodExp;
            }
            return base.VisitBinary(exp);
        }
    }
}