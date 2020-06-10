using System;

namespace SabreTools.Library.Data
{
    /// <summary>
    /// Generic console preparation for program output
    /// </summary>
    public static class Build
    {
        /// <summary>
        /// Returns true if running in a Mono environment
        /// </summary>
        public static bool MonoEnvironment
        {
            get { return (Type.GetType("Mono.Runtime") != null); }
        }

        /// <summary>
        /// Readies the console and outputs the header
        /// </summary>
        /// <param name="name">The name to be displayed as the program</param>
        public static void PrepareConsole(string name)
        {
            // Dynamically create the header string, adapted from http://stackoverflow.com/questions/8200661/how-to-align-string-in-fixed-length-string
            int width = Console.WindowWidth - 3;
            string border = $"+{new string('-', width)}+";
            string mid = $"{name} {Constants.Version}";
            mid = $"|{mid.PadLeft(((width - mid.Length) / 2) + mid.Length).PadRight(width)}|";

            // If we're outputting to console, do fancy things
            if (!Console.IsOutputRedirected)
            {
                // Set the console to ready state
                ConsoleColor formertext = ConsoleColor.White;
                ConsoleColor formerback = ConsoleColor.Black;
                if (!MonoEnvironment)
                {
                    Console.SetBufferSize(Console.BufferWidth, 999);
                    formertext = Console.ForegroundColor;
                    formerback = Console.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Blue;
                }

                Console.Title = $"{name} {Constants.Version}";

                // Output the header
                Console.WriteLine(border);
                Console.WriteLine(mid);
                Console.WriteLine(border);
                Console.WriteLine();

                // Return the console to the original text and background colors
                if (!MonoEnvironment)
                {
                    Console.ForegroundColor = formertext;
                    Console.BackgroundColor = formerback;
                }
            }
        }
    }
}
