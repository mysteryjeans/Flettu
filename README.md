# Flettu Library #

It contains many small pieces of codes for reducing preliminary coding overheads, whether you want to transparently interact with database or synchronize threads base on user ids or store password safely or reduce boiler plate code such as check for nulls; Flettu Lib has something for you.
Features

* Async/Await Task base locking/synchronizations on built-in value types which supports lock *reentrance* (i.e. Int32, Int64, string and etc)
* Extension functions for value types and reference types
* Password encryption and authentication. 

### Namespaces ###

* Flettu.Lock: helps acquiring locks with await support on object, integer, long and etc ids and supports lock reentrace in
  recursive function calls.
* Flettu.Security: Store password safely as random hash value to prevent against brute force attacks, or encrypt sensitive data quickly. 
* Flettu.Extensions: Difference extension methods for primitive types like string, integer, IEnumerable
* Flettu.Util: Helper methods to check for empty values 

