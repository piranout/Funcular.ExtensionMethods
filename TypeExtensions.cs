#region File info

// *********************************************************************************************************
// Funcular.ExtensionMethods>Funcular.ExtensionMethods>TypeExtensions.cs
// Created: 2015-07-18 10:19 AM
// Updated: 2015-07-18 10:27 AM
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
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Funcular.ExtensionMethods
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<string, Dictionary<string, PropertyInfo>> _propertyCache =
            new Dictionary<string, Dictionary<string, PropertyInfo>>();

        public static Dictionary<string, PropertyInfo> GetCachedProperties(this Type type)
        {
            if (_propertyCache.ContainsKey(type.FullName))
                return _propertyCache[type.FullName];
            _propertyCache.Add(type.FullName, new Dictionary<string, PropertyInfo>());
            foreach (var property in type.GetProperties())
            {
                _propertyCache[type.FullName].Add(property.Name.ToUpper(), property);
            }
            return _propertyCache[type.FullName];
        }

        public static PropertyInfo GetCachedProperty(this Type type, string propertyName)
        {
            return type.GetCachedProperties()[propertyName.ToUpper()];
        }
    }
}