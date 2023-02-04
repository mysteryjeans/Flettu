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
        static async Task Main(string[] args)
        {
            var valueLock = new AsyncValueLock<string>();

            var token1 = await valueLock.AcquireAsync("faraz");
            var token2 = await valueLock.AcquireAsync("faraz");

            valueLock.Release("faraz", token1);
            valueLock.Release("faraz", token2);

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
    }
}
