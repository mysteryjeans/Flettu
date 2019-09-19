# Flettu Library #

It contains many small pieces of codes for reducing preliminary coding overheads, such as Async/Await locks on object, ids, string.. or store password safely or reduce boiler plate code such as checking for nulls.

### Features ###

* Async/Await Task base locking/synchronizations on built-in value types which supports lock *reentrance* (i.e. Int32, Int64, string and etc)
* Extension functions for value types and reference types
* Password encryption and authentication. 

### Namespaces ###

* Flettu.Lock: helps acquiring locks with await support on object, integer, long and etc ids and supports lock reentrace in
  recursive function calls.
* Flettu.Security: Store password safely as random hash value to prevent against brute force attacks, or encrypt sensitive data quickly. 
* Flettu.Extensions: Difference extension methods for primitive types like string, integer, IEnumerable
* Flettu.Util: Helper methods to check for empty values 

### Getting Started ###
Install it from [NuGet](https://www.nuget.org/packages/Flettu/) packages

### Async/Await Lock Example ###
``` csharp
private AsyncLock asynMutex = new AsyncLock;

// Do something async..
public async Task DoSomethingAsync(CancellationToken cancellationToken = null)
{
    using(await asyncMutex.AcquireAsync())
    {
       await Task.Delay(10000, cancellationToken ?? CancellationToken.None);
    }
}

// Take locks recursively and allow reenterance
public async Task DoRecursionAsync(int maxRecursion = 2, CancellationToken cancellationToken = null)
{
    using(await asyncMutex.AcquireAsync())
    {
       if(maxRecursion > 0)
          await DoRecursionAsync(maxRecursion - 1, cancellationToken);
          
       await Task.Delay(10000, cancellationToken ?? CancellationToken.None);
    }
}

// ... Dispose pattern implemenation
public void Dispose(bool disposing)
{
    if(!disposed)
    {
        if(disposing)
        {   
            // .. disposing other resources here
            this.asyncMuxtex.Dispose();
        }
        
        // ..Releasing unmanage resources here if any
        
        disposed = true;
    }
}

```
