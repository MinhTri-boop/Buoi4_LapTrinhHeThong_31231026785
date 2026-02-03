using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingDemo
{
    class Program
    {
        // Configuration
        private const int NumberOfTasks = 5;
        private const int IterationsPerTask = 100000;
        private static int _sharedCounter = 0;
        
        // Lock object for the "lock" solution
        private static readonly object _lockObject = new object();

        static async Task Main(string[] args)
        {
            Console.WriteLine($"--- Configuration ---");
            Console.WriteLine($"Tasks: {NumberOfTasks}");
            Console.WriteLine($"Iterations per Task: {IterationsPerTask:N0}");
            Console.WriteLine($"Expected Total: {NumberOfTasks * IterationsPerTask:N0}");
            Console.WriteLine("---------------------\n");

            // 1. Demonstrate the Race Condition (The Problem)
            _sharedCounter = 0;
            await RunTest("Unsafe (Race Condition)", UnsafeIncrement);

            // 2. Fix using 'lock'
            _sharedCounter = 0;
            await RunTest("Fixed using 'lock'", LockIncrement);

            // 3. Fix using 'Interlocked'
            _sharedCounter = 0;
            await RunTest("Fixed using 'Interlocked'", InterlockedIncrement);
        }

        static async Task RunTest(string testName, Action incrementAction)
        {
            Console.Write($"{testName,-30}: Running... ");
            
            var tasks = new List<Task>();

            // Create 5 parallel tasks
            for (int i = 0; i < NumberOfTasks; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < IterationsPerTask; j++)
                    {
                        incrementAction();
                    }
                }));
            }

            // Wait for all tasks to finish
            await Task.WhenAll(tasks);

            Console.WriteLine($"Final Value: {_sharedCounter:N0}");
        }

        // --- THE INCREMENT METHODS ---

        // Method A: Unsafe - Causes Race Condition
        static void UnsafeIncrement()
        {
            _sharedCounter++; 
        }

        // Method B: Safe - Uses Monitor/Lock
        static void LockIncrement()
        {
            lock (_lockObject)
            {
                _sharedCounter++;
            }
        }

        // Method C: Safe - Uses Atomic CPU Instructions
        static void InterlockedIncrement()
        {
            Interlocked.Increment(ref _sharedCounter);
        }
    }
}