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

        protected override Expression VisitBinary(BinaryExpression b)
        {
            // if (b.NodeType == ExpressionType.AndAlso)
            if (b.NodeType == ExpressionType.Equal
                && !b.Left.CanReduce && b.Left.NodeType == ExpressionType.Constant
                && !b.Right.CanReduce && b.Right.Type == typeof (string))
            {
                var left = Visit(b.Left);
                var right = Visit(b.Right);

                // Make this binary expression an OrElse operation instead of an AndAlso operation. 
                // var someValue = Expression.Constant(propertyValue, typeof(string));
                var caseInsensitiveMethodExp = Expression.Call(left, _method, right, _comparison);
                return caseInsensitiveMethodExp;
            }
            return base.VisitBinary(b);
        }
    }
}