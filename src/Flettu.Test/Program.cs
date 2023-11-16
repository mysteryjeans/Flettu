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
            // await Task.WhenAll(
            //     ExecuteTask("faraz", 1),
            //     ExecuteTask("faraz", 2),
            //     ExecuteTask("masood", 3),
            //     ExecuteTask("faraz", 4),
            //     ExecuteTask("faraz", 5),
            //     ExecuteTask("faraz", 6)
            //     ) ;

            await TestPipeAsync();
            //await ShiftMemoryStreamTest();

            Console.ReadKey();
        }

        private static async Task ShiftMemoryStreamTest()
        {
            var buffer = new byte[100];
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(i % 255);

            using (var stream = new MemoryStream())
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // now lets shift 10 bytes;

                var shift = 10;
                var count = (int)stream.Length - shift;
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(stream.GetBuffer(), shift, count);
                stream.SetLength(count);
                buffer = stream.ToArray();

            }
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

        private static async Task TestPipeAsync()
        {
            using (var writer = new ConcurrentPipeWriter())
            {
                await Task.WhenAll(
                    WriteAsync(writer, 1000, 24, 100),
                    ReadAsync(writer, 10)
                );
            }
        }

        private static async Task WriteAsync(ConcurrentPipeWriter writer, int totalBytes, int size, int delay = 100)
        {
            var ran = new Random();
            var writeSize = 0;
            while (writeSize < totalBytes)
            {
                await writer.WriteAsync(new byte[size]);
                await Task.Delay(ran.Next(delay));
                writeSize += size;
                await writer.AdvanceToAsync();
            }

            writer.EndOfStream();
            writer.Dispose();
        }

        private static async Task ReadAsync(ConcurrentPipeWriter writer, int bufferSize)
        {
            var rand = new Random();
            var buffer = new byte[bufferSize];
            var totalRead = 0;

            Console.WriteLine($"ReadAllAsync => Started");
            using (var reader = await writer.OpenStreamReaderAsync())
            {
                int readSize = 0;
                while ((readSize = await reader.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await Task.Delay(rand.Next(100));
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
            using (var reader = await writer.OpenStreamReaderAsync())
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
