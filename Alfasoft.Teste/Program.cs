using System;
using System.IO;
using System.Runtime.InteropServices;
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
        }

        private static async Task<string[]> ReadUsernamesFromFile(string path)
        {
            var usernames = await File.ReadAllLinesAsync(path).ConfigureAwait(false);
                
            if (usernames.Length == 0)
            {
                Console.WriteLine("No usernames provided on file");
                Environment.Exit(0);
            }

            return usernames;
        }
    }
}
