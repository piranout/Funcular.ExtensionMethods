#region File info
// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>InterceptingQueryProvider.cs
// Created: 2015-07-10 9:54 AM
// Updated: 2015-07-10 11:45 AM
// By: Original code from Vitek Karas (MSFT), and Alex D James
// See http://blogs.msdn.com/b/alexj/archive/2010/03/01/tip-55-how-to-extend-an-iqueryable-by-wrapping-it.aspx
// *********************************************************************************************************
#endregion


#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IQToolkit;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;
#endregion


namespace Funcular.ExtensionMethods
{
    public class InterceptingQueryProvider : IQueryProvider
    {
        private readonly IQueryProvider _underlyingProvider;
        private readonly Func<Expression, Expression>[] _visitors;

        private InterceptingQueryProvider(
            IQueryProvider underlyingQueryProvider,
            params Func<Expression, Expression>[] visitors)
        {
            this._underlyingProvider = underlyingQueryProvider;
            this._visitors = visitors;
        }


        #region IQueryProvider Members
        public IQueryable<TElement> CreateQuery<TElement>(
            Expression expression)
        {
            return new InterceptedQuery<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var et = TypeHelper.FindIEnumerable(expression.Type);
            var qt = typeof (InterceptedQuery<>).MakeGenericType(et);
            object[] args = {this, expression};

            var ci = qt.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[]
                {
                    typeof (InterceptingQueryProvider),
                    typeof (Expression)
                },
                null);

            return (IQueryable) ci.Invoke(args);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return this._underlyingProvider.Execute<TResult>(
                InterceptExpr(expression)
                );
        }

        public object Execute(Expression expression)
        {
            return this._underlyingProvider.Execute(
                InterceptExpr(expression)
                );
        }
        #endregion


        public static IQueryable<T> Intercept<T>(
            IQueryable<T> underlyingQuery,
            params ExpressionVisitor[] visitors)
        {
            var visitFuncs =
                visitors
                    .Select(v => (Func<Expression, Expression>) v.Visit)
                    .ToArray();
            return Intercept(underlyingQuery, visitFuncs);
        }

        public static IQueryable<T> Intercept<T>(
            IQueryable<T> underlyingQuery,
            params Func<Expression, Expression>[] visitors)
        {
            var queryProvider = new InterceptingQueryProvider(
                underlyingQuery.Provider,
                visitors
                );
            return queryProvider.CreateQuery<T>(
                underlyingQuery.Expression);
        }

        public IEnumerator<TElement> ExecuteQuery<TElement>(
            Expression expression)
        {
            return this._underlyingProvider.CreateQuery<TElement>(
                InterceptExpr(expression)
                ).GetEnumerator();
        }

        private Expression InterceptExpr(Expression expression)
        {
            var exp = expression;
            foreach (var visitor in this._visitors)
            {
                exp = visitor(exp);
            }
            return exp;
        }
    }
}