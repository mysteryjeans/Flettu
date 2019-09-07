using System;
using System.Threading.Tasks;
using Flettu.Lock;

namespace Flettu.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating AsyncLock object..");
            using (var asyncLock = new AsyncLock())
            {
                var checkTask1 = Task.Run(async () => await CheckAsyncLockReentranceAsnyc(asyncLock));
                var checkTask2 = Task.Run(async () => await CheckAsyncLockReentranceAsnyc(asyncLock));
       
                Console.WriteLine("Creating AsyncLock<string> object..");
                var key1 = "Faraz";
                var key2 = "Masood";

                var asyncValueLock = new AsyncValueLock<string>();

                var checkValueTask1 = Task.Run(async () => await CheckAsyncValueLockReentranceAsnyc(asyncValueLock, key1));
                var checkValueTask2 = Task.Run(async () => await CheckAsyncValueLockReentranceAsnyc(asyncValueLock, key2));
                Task.WaitAll(checkTask1, checkTask2, checkValueTask1, checkValueTask2);

                Console.WriteLine("Press any key to stop..");
                Console.ReadKey();

            }
        }

        private static async Task CheckAsyncLockReentranceAsnyc(AsyncLock asyncLock, int maxReentrances = 2)
        {
            using (await asyncLock.AcquireAsync())
            {
                Console.WriteLine($"Lock taken as reentrance: {maxReentrances}.. TaskId: {asyncLock.TaskId}");

                if (maxReentrances > 0)
                    await CheckAsyncLockReentranceAsnyc(asyncLock, maxReentrances - 1);

                Console.WriteLine($"Press any key to release reentrance: {maxReentrances} lock... TaskId: {asyncLock.TaskId}");
                Console.ReadKey();
            }
        }

        private static async Task CheckAsyncValueLockReentranceAsnyc<T>(AsyncValueLock<T> asyncLock, T value, int maxReentrances = 2)
        {
            using (await asyncLock.AcquireAsync(value))
            {
                Console.WriteLine($"Lock taken for value: {value} as reentrance: {maxReentrances}.. TaskId: {asyncLock.GetTaskId(value)}");

                if (maxReentrances > 0)
                    await CheckAsyncValueLockReentranceAsnyc<T>(asyncLock, value, maxReentrances - 1);

                Console.WriteLine($"Press any key to release lock for value: {value} reentrance: {maxReentrances}.. TaskId: {asyncLock.GetTaskId(value)}");
                Console.ReadKey();
            }
        }
    }
}
