# Funcular.ExtensionMethods

Simple extension methods providing syntactic sugar for strings and collections. 

`string.IsNullOrEmpty(myValue)` becomes `myValue.HasValue()`

`myList = (myList ?? (myList = new List<foo>()))` becomes `myList = myList.OrEmpty()`

`if(myList != null && myList.Any())` becomes `if(myList.HasContents())`

##### Case-insensitive IQueryable

This method exists to help correct the mismatch between string comparisons in Entity Framework versus those in ‘regular’ LINQ expressions. 

Entity Framework delegates query evaluation to SQL Server, where string comparisons are according to the database’s collation, which is most often case-insensitive. Outside of EF, however, LINQ queries (regular lambdas on objects) use case-sensitive string comparisons.

This project includes an extension method on IQueryable<T>, queryable.AsCaseInsensitive(). This returns an IQueryable with an interceptor for string equality comparisons, converting them from case-sensitive to StringCompare.OrdinalIgnoreCase. 

To use it, you literally just call `.AsCaseInsensitive()` on any IQueryable<T>. If you have an IList or IEnumerable, just use `.AsQueryable()` first.

Here is a working test-case:

```csharp
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
```