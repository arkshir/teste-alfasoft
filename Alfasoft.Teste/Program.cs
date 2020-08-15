using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Alfasoft.Teste
{
    internal static class Program
    {
        private static readonly DateTime RunTime = DateTime.Now;
        private static readonly string BasePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        private static readonly string LastRunPath = Path.Combine(BasePath, ".lastrun");

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
                await GetUsersInfoFromBitbucket(usernames).ConfigureAwait(false);
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

        private static async Task CheckLastRunTime()
        {
            if (File.Exists(LastRunPath))
            {
                var lastRunText = await File.ReadAllTextAsync(LastRunPath).ConfigureAwait(false);
                var lastRun = DateTime.Parse(lastRunText);
                var timeSpan = (int) RunTime.Subtract(lastRun).TotalSeconds;
                if (timeSpan < 60)
                {
                    Console.WriteLine($"You can run this Program again in {60 - timeSpan} seconds");
                    Environment.Exit(0);
                }
            }
        }
        
        private static async Task UpdateLastRunTime()
        {
            try
            {
                await File.WriteAllTextAsync(LastRunPath, $"{DateTime.Now}").ConfigureAwait(false);
            }
            catch (IOException)
            {
                Console.WriteLine("Couldn't update last run");
            }
        }
        
        private static async Task<IEnumerable<string>> ReadUsernamesFromFile(string path)
        {
            Console.Write($"Reading usernames from {path}");
            var usernames = await File.ReadAllLinesAsync(path).ConfigureAwait(false);
                
            if (usernames.Length == 0)
            {
                Console.WriteLine("\nNo usernames provided on file");
                Environment.Exit(0);
            }
            
            Console.WriteLine(" - OK!");
            
            return usernames;
        }

        private static async Task GetUsersInfoFromBitbucket(IEnumerable<string> usernames)
        {
            Console.WriteLine("Getting users from bitbucket api");
            const string baseAddress = "https://api.bitbucket.org/2.0";
            foreach (var username in usernames)
            {
                using var httpClient = new HttpClient();
                var uri = $"{baseAddress}/users/{username}";
                await PrintAndLog($"{username} - {uri}", false).ConfigureAwait(false);
                var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
                await PrintAndLog($" - Status: {response.StatusCode}").ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var formattedContent = FormatJsonString(content);
                await PrintAndLog($"{formattedContent}\n").ConfigureAwait(false);
                await UpdateLastRunTime().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }

        private static string FormatJsonString(string json)
        {
            var jObject = JObject.Parse(json);
            var sb = new StringBuilder();
            foreach (var prop in jObject.Properties())
            {
                sb.AppendLine($"{prop.Name}: {prop.Value}");
            }

            return sb.ToString();
        }

        private static async Task PrintAndLog(string text, bool appendNewLine = true)
        {
            if (appendNewLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }

            var filename = $"{RunTime:yyyyMMdd-hhmmss}.log";
            
            try
            {
                await File.AppendAllTextAsync(Path.Combine(BasePath, filename), text);
            }
            catch (IOException)
            {
                Console.WriteLine($"Couldn't Write {filename}");
            }
        }
    }
}
