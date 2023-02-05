using System;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Flettu.IO;
using Flettu.Lock;

namespace Flettu.Test
{
    class Program
    {
        static AsyncValueLock<string> asyncLock = new AsyncValueLock<string>();
        static async Task Main(string[] args)
        {
            await Task.WhenAll(
                ExecuteTask("faraz", 1),
                ExecuteTask("faraz", 2),
                ExecuteTask("masood", 3),
                ExecuteTask("faraz", 4),
                ExecuteTask("faraz", 5),
                ExecuteTask("faraz", 6)
                ) ;

            Console.ReadKey();
        }

        private static async Task ExecuteTask(string value, int taskId)
        {
            await asyncLock.AcquireAsync(value);
            try
            {
                Console.WriteLine($"Entered: Task# {taskId}, value: {value}");
                await Task.Delay(1000);
            }
            finally
            {
                Console.WriteLine($"Exit: Task# {taskId}, value: {value}");
                asyncLock.Release(value);
            }
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
    }
}
