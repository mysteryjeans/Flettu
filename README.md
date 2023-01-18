# Flettu Library #

It contains many small pieces of codes for reducing preliminary coding overheads, such as Async/Await locks on object, ids, string.. or store password safely or reduce boiler plate code such as checking for nulls.

### Features ###

* Async/Await Task base locking/synchronizations on built-in value types which supports lock *reentrance* (i.e. Int32, Int64, string and etc)
* Extension functions for value types and reference types
* Password encryption and authentication.
* Concurrent Pipe Stream for multiple reading and writing simultanously for multiplexing/duplexing on any given underline stream like MemoryStream or         FileStream (Underline stream must support both read and write operations)

### Namespaces ###

* **Flettu.IO:** helps in threadsafe reading and writing, also Pipe Stream for multiple readers and writers support
  recursive function calls.
* **Flettu.Lock:** helps acquiring locks with await support on object, integer, long and etc ids and supports lock reentrace in
  recursive function calls.
* **Flettu.Security:** Store password safely as random hash value to prevent against brute force attacks, or encrypt sensitive data quickly. 
* **Flettu.Extensions:** Difference extension methods for primitive types like string, integer, IEnumerable
* **Flettu.Util:** Helper methods to check for empty values 
* **Flettu.Collections:** Contains ConcurrentList<T> and ConcurrentHashSet<T> for maximum performance, since reads can be
  simultaneous

### Getting Started ###
Install it from [NuGet](https://www.nuget.org/packages/Flettu/) packages

### Async/Await Lock Example ###
``` csharp
private AsyncLock asynMutex = new AsyncLock;

// Do something async..
public async Task DoSomethingAsync(CancellationToken cancellationToken = default(CancellationToken))
{
    using(await asyncMutex.AcquireAsync())
    {
       await Task.Delay(10000, cancellationToken);
    }
}

// Take locks recursively and allow reenterance
public async Task DoRecursionAsync(int maxRecursion = 2, CancellationToken cancellationToken = default(CancellationToken))
{
    using(await asyncMutex.AcquireAsync())
    {
       if(maxRecursion > 0)
          await DoRecursionAsync(maxRecursion - 1, cancellationToken);
          
       await Task.Delay(10000, cancellationToken);
    }
}

// ... Dispose pattern implemenation
protected virtual void Dispose(bool disposing)
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

### Pipe Stream Example ###
``` csharp
static void Main(string[] args)
{
    var buffer = new byte[768];
    using (var underlineStream = new MemoryStream())
    {
        int count = 0;
        using (var writer = new ConcurrentPipeWriter(underlineStream))
        {
            var readTask1 = Task.Run(async () => await ReadAllAsync(writer, 512)); 
            var readTask2 = Task.Run(async () => await CopyToAsync(writer));

            while (count++ < 10)
            {
                writer.WriteAsync(buffer, 0, buffer.Length).Wait();
                Console.WriteLine($"Write index: {count}, size: {buffer.Length}, press key to proceed");

                //Console.ReadKey();
                Thread.Sleep(1000);
            }

            // writer.EndOfStream()
        }
    }

    Console.ReadKey();
}

private static async Task ReadAllAsync(ConcurrentPipeWriter writer, int bufferSize)
{
    var buffer = new byte[bufferSize];
    var totalRead = 0;

    Console.WriteLine($"ReadAllAsync => Started");
    using (var memoryStream = new MemoryStream())
    using (var reader = writer.OpenStreamReader())
    {
        int readSize = 0;
        while ((readSize = await reader.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            totalRead += readSize;
            Console.WriteLine($"ReadAllAsync => Read size: {readSize}, total read size: {totalRead}, buffer length: {buffer.Length}");
        }
    }

    Console.WriteLine($"ReadAllAsync => Ended");
}

private static async Task CopyToAsync(ConcurrentPipeWriter writer)
{
    Console.WriteLine($"CopyToAsync => Started");
    using (var memoryStream = new MemoryStream())
    using (var reader = writer.OpenStreamReader())
    {
        var copyTask = reader.CopyToAsync(memoryStream);
        while (!copyTask.IsCompleted)
        {
            Console.WriteLine($"CopyToAsync => Copying external stream position: {memoryStream.Position}, length: {memoryStream.Length}");
            await Task.Delay(500);
        }

        Console.WriteLine($"CopyToAsync => Copied size to external stream: {memoryStream.Length}");
    }

    Console.WriteLine($"CopyToAsync => Ended");
}
```
