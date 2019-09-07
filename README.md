# Flettu Library #

It contains many small pieces of codes for reducing preliminary coding overheads, such as Async/Await locks on object, ids, string.. or store password safely or reduce boiler plate code such as checking for nulls.

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


### Async/Await Lock Example ###
``` csharp
private static async Task CheckAsyncLockReentranceAsnyc(AsyncLock asyncLock, int maxReentrances = 2)
{
    // Locks are IDisposable to support fimilar construct lock(){...}
    using (await asyncLock.AcquireAsync())
    {
        Console.WriteLine($"Lock taken as reentrance: {maxReentrances}.. TaskId: {asyncLock.TaskId}");

        if (maxReentrances > 0)
            await CheckAsyncLockReentranceAsnyc(asyncLock, maxReentrances - 1);

        Console.WriteLine($"Press any key to release reentrance: {maxReentrances} lock... TaskId: {asyncLock.TaskId}");
        Console.ReadKey();
    }
}
```
