using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Memory;

namespace AnyHook
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Mem m = new Mem();
            Process[] proc;

            string scanAOB = "0F 86 85 00 00 00 48";
            string hookAddr = "";
            byte[] patchedBytes = { 0xE9, 0x86, 0x00, 0x00, 0x00, 0x90 };

            //Start
            try
            {
                Process.Start(@"AnyDesk.exe");
                Thread.Sleep(2000);
            }
            catch
            {
                Console.WriteLine("Couldn't find AnyDesk in the same folder");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            Process selectedProcess = null;

            while (selectedProcess == null)
            {
                proc = Process.GetProcessesByName("AnyDesk");

                if (proc.Length > 0)
                {
                    // Find the AnyDesk process with the highest private bytes
                    selectedProcess = proc.OrderByDescending(p => p.PrivateMemorySize64).FirstOrDefault();

                    if (selectedProcess != null)
                    {
                        Console.WriteLine($"Connected to PID: {selectedProcess.Id}");
                        m.OpenProcess(selectedProcess.Id);
                        Thread.Sleep(800);
                    }
                }
                else
                {
                    Console.WriteLine("AnyDesk process not found. Trying again...");
                    Thread.Sleep(2000);
                }
            }

            Console.Clear();
            Console.WriteLine("<---Nightfall's AnyDesk hook--->");
            Console.WriteLine("\nHooking...");
            Thread.Sleep(800);

            hookAddr = m.AoBScan(scanAOB).Result.Sum().ToString("X2");

            if (hookAddr != "00")
            {
                Console.WriteLine("Patching at: " + hookAddr);

                m.WriteBytes(hookAddr, patchedBytes);

                Console.Clear();
                Console.WriteLine("<---Nightfall's AnyDesk hook--->");
                Console.WriteLine("\nHooked!\nThe timers should be shorter now ;)");
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine("\nHook failed!\nTrying again...");
                selectedProcess.Kill();
                Thread.Sleep(2000);
            }

            Environment.Exit(1);
        }
    }
}