#region File info

// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>EnumerableExtensions.cs
// Created: 2015-06-26 3:03 PM
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion



namespace Funcular.ExtensionMethods
{
    /// <summary>
    ///     Extension methods for IEnumerable objects.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Safely determine if the sequence is non-null AND contains elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool HasContents<T>(this IEnumerable<T> enumerable)
        {
            return enumerable != null && enumerable.Any();
        }

        /// <summary>
        ///     Prevent null reference exceptions when referencing enumerables
        ///     by using x.OrEmpty().DoSomething().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        /// <summary>
        ///     Method used to order an enumerable by the property's name as a string
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="orderByProperty">Name of property to sort by</param>
        /// <param name="desc">Descending if true</param>
        /// <returns></returns>
        public static IEnumerable<TEntity> OrderBy<TEntity>(this IEnumerable<TEntity> source, string orderByProperty,
            bool desc = false)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            Type type = typeof (TEntity);
            // TODO: Statically cache property
            PropertyInfo property = type.GetProperty(orderByProperty);
            ParameterExpression parameter = Expression.Parameter(type, "p");
            if (parameter == null || property == null)
                throw new ArgumentNullException("Unable to order by property named [" + orderByProperty + "]. Check to ensure this property exists in your IEnumerable.");

            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
            LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);
            TEntity[] array = source as TEntity[] ?? source.ToArray();
            MethodCallExpression resultExpression = Expression.Call(typeof (Queryable), command, new[] {type, property.PropertyType},
                array.AsQueryable().Expression, Expression.Quote(orderByExpression));
            return array.AsQueryable().Provider.CreateQuery<TEntity>(resultExpression);
        }

        /// <summary>
        ///     Extension method to get a paged result set from an IEnumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="results"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="sortDir"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetPagedResults<T>(this IEnumerable<T> results, int pageSize, int page, string sort = "", string sortDir = "")
        {
            //results.OrderByDescending (column name)

            //var sortBy = string.Format("{0} {1}", sort, sortDir);
            if (sort != "")
            {
                bool isAscendingSort = !(sortDir != null && sortDir.ToLower().Contains("desc"));
                results = results.OrderBy(sort, isAscendingSort);
            }

            if (page > 1)
                results = results.Skip((page - 1)*pageSize);

            return results.Take(pageSize);
        }

        /// <summary>
        ///     Returns the enumerable member having the highest value of <paramref name="selector" />
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TEntity MaxBy<TEntity, TKey>(this IEnumerable<TEntity> source, Func<TEntity, TKey> selector) where TEntity : class
        {
            if (!source.HasContents())
                return null;
            return source.OrderByDescending(selector)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Returns the enumerable member having the lowest value of <paramref name="selector" />
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TEntity MinBy<TEntity, TKey>(this IEnumerable<TEntity> source, Func<TEntity, TKey> selector) where TEntity : class
        {
            if (!source.HasContents())
                return null;
            return source.OrderBy(selector)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Returns the first item encountered for each distinct selector value.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TSource : class
        {
            var keys = new HashSet<TKey>();
            foreach (var item in source)
            {
                if (keys.Add(selector(item)))
                    yield return item;
            }
        }

        /// <summary>
        ///     Returns the median value of <paramref name="selector" /> among this Enumerable.
        /// </summary>
        /// <typeparam name="TColl"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static decimal? Median<TColl, TValue>(this IEnumerable<TColl> source, Func<TColl, TValue> selector)
        {
            return source.Select(selector).Median();
        }

        /// <summary>
        ///     Returns the median Element in this Enumerable, using the default sort comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static decimal? Median<T>(this IEnumerable<T> source)
        {
            if (Nullable.GetUnderlyingType(typeof (T)) != null)
                source = source.Where(x => x != null);
            source = source.OrderBy(n => n);
            T[] array = (source.ToArray());
            int count = array.Count();
            if (count == 0)
                return null;
            //source = array.OrderBy(n => n);

            int midpoint = count/2;
            if (count%2 == 0)
                return (Convert.ToDecimal(array.ElementAt(midpoint - 1)) + Convert.ToDecimal(array.ElementAt(midpoint)))/2.0M;
            else
                return Convert.ToDecimal(array.ElementAt(midpoint));
        }

        /// <summary>
        ///     Fluent syntax to enable constructs like
        ///     <example>myList.ClearAnd().Add(something);</example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> ClearAnd<T>(this List<T> list)
        {
            list.Clear();
            return list;
        }

        /// <summary>
        ///     Fluent syntax to enable constructs like
        ///     <example>myList.AddAnd(thing).Add(otherThing);</example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> AddAnd<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        /// <summary>
        ///     Casts an IEnumerable from one type to a another type, with null pointer protection.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TTarget> SafeCast<TTarget>(this IEnumerable source)
        {
            return source == null ? null : source.Cast<TTarget>();
        }

        /// <summary>
        ///     Returns the collection of values in the enumeration type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> EnumToList<T>()
        {
            Type enumType = typeof (T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof (Enum))
                throw new ArgumentException("T must be of type System.Enum");
            return new List<T>(Enum.GetValues(enumType) as IEnumerable<T>);
        }

        /// <summary>
        ///     Yields all of the individual Enum values represented in this Flags instance.
        ///     Only valid for bitmask/Flags enums.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return getFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return getFlags(value, getFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> getFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            var results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }

            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        /// <summary>
        ///     Enumerates all of the values in a Flags (bitmasked) Enum.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        private static IEnumerable<Enum> getFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits)
                    flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        public static string GetCallingMethodName()
        {
            var stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)
            int i = 0;
            // write call stack method names
            if (stackFrames == null)
                return null;
            foreach (var stackFrame in stackFrames)
            {
                i++;
                if (i >= 2)
                    return stackFrame.GetMethod().Name;
            }

            return null;
        }

        public static DateTime ToDateTime(this string val)
        {
            val = val.Trim();
            return val.Length == 8 ? (DateTime.ParseExact(val, "yyyyMMdd", CultureInfo.CurrentCulture)) : DateTime.Parse(val);
        }

        public static T To<T>(this IConvertible obj)
        {
            return (T) Convert.ChangeType(obj, typeof (T));
        }

        public static T ToOrDefault<T>
            (this IConvertible obj)
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return default(T);
            }
        }

        public static bool ToOrDefault<T>
            (this IConvertible obj,
                out T newObj)
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = default(T);
                return false;
            }
        }

        public static T ToOrOther<T>
            (this IConvertible obj,
                T other)
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return other;
            }
        }

        public static bool ToOrOther<T>
            (this IConvertible obj,
                out T newObj,
                T other)
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = other;
                return false;
            }
        }

        public static T ToOrNull<T>
            (this IConvertible obj)
            where T : class
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return null;
            }
        }

        public static bool ToOrNull<T>
            (this IConvertible obj,
                out T newObj)
            where T : class
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = null;
                return false;
            }
        }
    }
}