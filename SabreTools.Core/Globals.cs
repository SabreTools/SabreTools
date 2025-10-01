using System;
using System.Reflection;
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace SabreTools.Core
{
    /// <summary>
    /// Globally-accessible functionality
    /// </summary>
    public class Globals
    {
        /// <summary>
        /// The current toolset version to be used by all child applications
        /// </summary>
        public static string? Version
        {
            get
            {
                try
                {
                    var assembly = Assembly.GetEntryAssembly();
                    if (assembly == null)
                        return null;

                    var assemblyVersion = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                    return assemblyVersion?.InformationalVersion;
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// Maximum threads to use during parallel operations
        /// </summary>
        public static int MaxThreads { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// ParallelOptions object for use in parallel operations
        /// </summary>
        public static ParallelOptions ParallelOptions => new()
        {
            MaxDegreeOfParallelism = MaxThreads
        };
#endif

        /// <summary>
        /// Readies the console and outputs the header
        /// </summary>
        /// <param name="program">The name to be displayed as the program</param>
        public static void SetConsoleHeader(string program)
        {
            // Dynamically create the header string, adapted from http://stackoverflow.com/questions/8200661/how-to-align-string-in-fixed-length-string
            int width = (Console.WindowWidth == 0 ? 80 : Console.WindowWidth) - 3;
            string border = $"+{new string('-', width)}+";
            string mid = $"{program} {Globals.Version}";
            mid = $"|{mid.PadLeft(((width - mid.Length) / 2) + mid.Length).PadRight(width)}|";

            // If we're outputting to console, do fancy things
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            if (!Console.IsOutputRedirected)
            {
                // Set the console to ready state
                ConsoleColor formertext = Console.ForegroundColor;
                ConsoleColor formerback = Console.BackgroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Blue;

                Console.Title = $"{program} {Globals.Version}";

                // Output the header
                Console.WriteLine(border);
                Console.WriteLine(mid);
                Console.WriteLine(border);
                Console.WriteLine();

                // Return the console to the original text and background colors
                Console.ForegroundColor = formertext;
                Console.BackgroundColor = formerback;
            }
#endif
        }
    }
}
