using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alfasoft.Teste
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length != 1 || args[0] == "--help")
            {
                Console.WriteLine("Usage: Alfasoft.Teste <file>");
                return;
            }

            await CheckLastRunTime().ConfigureAwait(false);
            
            try
            {
                var usernames = await ReadUsernamesFromFile(args[0]).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(
                    "<file> is a zero-length string, contains only white space, or contains one or more invalid characters");
                Environment.Exit(ex.HResult);
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine(
                    "<file>'s path exceed the system-defined maximum length.");
                Environment.Exit(ex.HResult);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(
                    "The specified path is invalid (for example, it is on an unmapped drive).");
                Environment.Exit(ex.HResult);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("<file> is a directory or the caller does not have the required permission");
                Environment.Exit(ex.HResult);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("<file> was not found");
                Environment.Exit(ex.HResult);
            }
            catch (IOException ex)
            {
                Console.WriteLine("An I/O error occurred while opening the file.");
                Environment.Exit(ex.HResult);
            }
            
            await UpdateLastRunTime().ConfigureAwait(false);
        }
        
        private static async Task<IEnumerable<string>> ReadUsernamesFromFile(string path)
        {
            var usernames = await File.ReadAllLinesAsync(path).ConfigureAwait(false);
                
            if (usernames.Length == 0)
            {
                Console.WriteLine("No usernames provided on file");
                Environment.Exit(0);
            }

            return usernames;
        }


        private static async Task CheckLastRunTime()
        {
            if (File.Exists("lastrun"))
            {
                var lastRunText = await File.ReadAllTextAsync("lastrun").ConfigureAwait(false);
                var lastRun = DateTime.Parse(lastRunText);
                var timeSpan = (int) DateTime.Now.Subtract(lastRun).TotalSeconds;
                if (timeSpan < 60)
                {
                    Console.WriteLine($"You can run this Program again in {60 - timeSpan} seconds");
                    Environment.Exit(0);
                }
            }
        }
        
        private static async Task UpdateLastRunTime()
        {
            await File.WriteAllTextAsync("lastrun", $"{DateTime.Now}").ConfigureAwait(false);
        }
    }
}
