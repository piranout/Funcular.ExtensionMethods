#region File info
// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>InterceptedQuery.cs
// Created: 2015-07-10 11:03 AM
// Updated: 2015-07-10 11:44 AM
// By: Original code from Vitek Karas (MSFT), and Alex D James
// See http://blogs.msdn.com/b/alexj/archive/2010/03/01/tip-55-how-to-extend-an-iqueryable-by-wrapping-it.aspx
// *********************************************************************************************************
#endregion


#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#endregion


namespace Funcular.ExtensionMethods
{
    public class InterceptedQuery<T> : IOrderedQueryable<T>
    {
        private readonly Expression _expression;
        private readonly InterceptingQueryProvider _provider;

        public InterceptedQuery(
            InterceptingQueryProvider provider,
            Expression expression)
        {
            this._provider = provider;
            this._expression = expression;
        }


        #region IOrderedQueryable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this._provider.ExecuteQuery<T>(this._expression);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._provider.ExecuteQuery<T>(this._expression);
        }

        public Type ElementType { get { return typeof (T); } }
        public Expression Expression { get { return this._expression; } }
        public IQueryProvider Provider { get { return this._provider; } }
        #endregion
    }
}