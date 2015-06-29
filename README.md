# Funcular.ExtensionMethods

Simple extension methods providing syntactic sugar for strings and collections. 

`string.IsNullOrEmpty(myValue)` becomes `myValue.HasValue()`

`myList = (myList ?? (myList = new List<foo>()))` becomes `myList = myList.OrEmpty()`

`if(myList != null && myList.Any())` becomes `if(myList.HasContents())`

...etc...
