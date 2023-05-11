using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace lib.services
{
    public static class DebuggerSetup
    {
        public static void WaitForDebugger()
        {
            string? debug = System.Environment.GetEnvironmentVariable("debug");
            if (debug == null) return;
            if (debug.ToLower() != "true") return;
            int pid = Process.GetCurrentProcess().Id;

            var HandleCancelKeyPress = new ConsoleCancelEventHandler((sender, args) =>
            {
                Console.WriteLine("");
                Console.WriteLine("Cancellation Requested. Exiting...");
                Environment.Exit(0);
            });

            Console.CancelKeyPress += HandleCancelKeyPress;

            while (!Debugger.IsAttached)
            {
                Console.WriteLine($"Waiting for debugger to attach to PID {pid}...");
                Task.Delay(10000).GetAwaiter().GetResult();
            }
            Console.WriteLine("Debugger attached.");
            
            Console.CancelKeyPress -= HandleCancelKeyPress;
        }

        
    }
}