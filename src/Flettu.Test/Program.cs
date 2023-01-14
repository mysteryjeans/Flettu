using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Flettu.IO;
using Flettu.Lock;

namespace Flettu.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // using (Rijndael rijAlg = Rijndael.Create())
            // {
            //     rijAlg.Key = null;
            //     rijAlg.IV = null;

            //     // Create an encryptor to perform the stream transform.
            //     ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

            //     using(var cryptoStream = new CryptoStream(new MemoryStream(), encryptor,CryptoStreamMode.
            // }

            // Console.WriteLine("Creating AsyncLock object..");
            // using (var asyncLock = new AsyncLock())
            // {
            //     var checkTask1 = Task.Run(async () => await CheckAsyncLockReentranceAsync(asyncLock));
            //     var checkTask2 = Task.Run(async () => await CheckAsyncLockReentranceAsync(asyncLock));

            //     Console.WriteLine("Creating AsyncLock<string> object..");
            //     var key1 = "Faraz";
            //     var key2 = "Masood";

            //     var asyncValueLock = new AsyncValueLock<string>();

            //     var checkValueTask1 = Task.Run(async () => await CheckAsyncValueLockReentranceAsync(asyncValueLock, key1));
            //     var checkValueTask2 = Task.Run(async () => await CheckAsyncValueLockReentranceAsync(asyncValueLock, key2));
            //     Task.WaitAll(checkTask1, checkTask2, checkValueTask1, checkValueTask2);

            //     Console.WriteLine("Press any key to stop..");
            //     Console.ReadKey();

            // }

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

        private static async Task CheckAsyncLockReentranceAsync(AsyncLock asyncLock, int maxReentrances = 2)
        {
            using (await asyncLock.AcquireAsync())
            {
                Console.WriteLine($"Lock taken as reentrance: {maxReentrances}.. TaskId: {asyncLock.TaskId}");

                if (maxReentrances > 0)
                    await CheckAsyncLockReentranceAsync(asyncLock, maxReentrances - 1);

                Console.WriteLine($"Press any key to release reentrance: {maxReentrances} lock... TaskId: {asyncLock.TaskId}");
                Console.ReadKey();
            }
        }

        private static async Task CheckAsyncValueLockReentranceAsync<T>(AsyncValueLock<T> asyncLock, T value, int maxReentrances = 2)
        {
            using (await asyncLock.AcquireAsync(value))
            {
                Console.WriteLine($"Lock taken for value: {value} as reentrance: {maxReentrances}.. TaskId: {asyncLock.GetTaskId(value)}");

                if (maxReentrances > 0)
                    await CheckAsyncValueLockReentranceAsync<T>(asyncLock, value, maxReentrances - 1);

                Console.WriteLine($"Press any key to release lock for value: {value} reentrance: {maxReentrances}.. TaskId: {asyncLock.GetTaskId(value)}");
                Console.ReadKey();
            }
        }
    }
}
