using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using NaturalSort;

namespace SabreTools.Library.IO
{
    /// <summary>
    /// Extensions to Directory functionality
    /// </summary>
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Cleans out the temporary directory
        /// </summary>
        /// <param name="dir">Name of the directory to clean out</param>
        public static void Clean(string dir)
        {
            foreach (string file in Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly))
            {
                FileExtensions.TryDelete(file);
            }

            foreach (string subdir in Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly))
            {
                TryDelete(subdir);
            }
        }

        /// <summary>
        /// Ensure the output directory is a proper format and can be created
        /// </summary>
        /// <param name="dir">Directory to check</param>
        /// <param name="create">True if the directory should be created, false otherwise (default)</param>
        /// <param name="temp">True if this is a temp directory, false otherwise</param>
        /// <returns>Full path to the directory</returns>
        public static string Ensure(string dir, bool create = false, bool temp = false)
        {
            // If the output directory is invalid
            if (string.IsNullOrWhiteSpace(dir))
            {
                if (temp)
                    dir = Path.GetTempPath();
                else
                    dir = Environment.CurrentDirectory;
            }

            // Get the full path for the output directory
            dir = Path.GetFullPath(dir);

            // If we're creating the output folder, do so
            if (create)
                Directory.CreateDirectory(dir);

            return dir;
        }

        /// <summary>
        /// Retrieve a list of just directories from inputs
        /// </summary>
        /// <param name="inputs">List of strings representing directories and files</param>
        /// <param name="appendparent">True if the parent name should be included in the ParentablePath, false otherwise (default)</param>
        /// <returns>List of strings representing just directories from the inputs</returns>
        public static List<ParentablePath> GetDirectoriesOnly(List<string> inputs, bool appendparent = false)
        {
            List<ParentablePath> outputs = new List<ParentablePath>();
            for (int i = 0; i < inputs.Count; i++)
            {
                string input = inputs[i];

                // If we have a null or empty path
                if (string.IsNullOrEmpty(input))
                    continue;

                // If we have a wildcard
                string pattern = "*";
                if (input.Contains("*") || input.Contains("?"))
                {
                    pattern = Path.GetFileName(input);
                    input = input.Substring(0, input.Length - pattern.Length);
                }

                // Get the parent path in case of appending
                string parentPath;
                try
                {
                    parentPath = Path.GetFullPath(input);
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error($"An exception occurred getting the full path for '{input}': {ex}");
                    continue;
                }

                if (Directory.Exists(input))
                {
                    List<string> directories = GetDirectoriesOrdered(input, pattern);
                    foreach (string dir in directories)
                    {
                        try
                        {
                            outputs.Add(new ParentablePath(Path.GetFullPath(dir), appendparent ? parentPath : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{dir}' was too long");
                        }
                        catch (Exception ex)
                        {
                            Globals.Logger.Error($"An exception occurred processing '{dir}': {ex}");
                        }
                    }
                }
            }

            return outputs;
        }

        /// <summary>
        /// Retrieve a list of directories from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="pattern">Optional pattern to search for directory names</param>
        /// <returns>List with all new files</returns>
        private static List<string> GetDirectoriesOrdered(string dir, string pattern = "*")
        {
            return GetDirectoriesOrderedHelper(dir, new List<string>(), pattern);
        }

        /// <summary>
        /// Retrieve a list of directories from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <param name="pattern">Optional pattern to search for directory names</param>
        /// <returns>List with all new files</returns>
        private static List<string> GetDirectoriesOrderedHelper(string dir, List<string> infiles, string pattern)
        {
            // Take care of the files in the top directory
            List<string> toadd = Directory.EnumerateDirectories(dir, pattern, SearchOption.TopDirectoryOnly).ToList();
            toadd.Sort(new NaturalComparer());
            infiles.AddRange(toadd);

            // Then recurse through and add from the directories
            foreach (string subDir in toadd)
            {
                infiles = GetDirectoriesOrderedHelper(subDir, infiles, pattern);
            }

            // Return the new list
            return infiles;
        }

        /// <summary>
        /// Retrieve a list of just files from inputs
        /// </summary>
        /// <param name="inputs">List of strings representing directories and files</param>
        /// <param name="appendparent">True if the parent name should be be included in the ParentablePath, false otherwise (default)</param>
        /// <returns>List of strings representing just files from the inputs</returns>
        public static List<ParentablePath> GetFilesOnly(List<string> inputs, bool appendparent = false)
        {
            List<ParentablePath> outputs = new List<ParentablePath>();
            for (int i = 0; i < inputs.Count; i++)
            {
                string input = inputs[i].Trim('"');

                // If we have a null or empty path
                if (string.IsNullOrEmpty(input))
                    continue;

                // If we have a wildcard
                string pattern = "*";
                if (input.Contains("*") || input.Contains("?"))
                {
                    pattern = Path.GetFileName(input);
                    input = input.Substring(0, input.Length - pattern.Length);
                }

                // Get the parent path in case of appending
                string parentPath;
                try
                {
                    parentPath = Path.GetFullPath(input);
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error($"An exception occurred getting the full path for '{input}': {ex}");
                    continue;
                }

                if (Directory.Exists(input))
                {
                    List<string> files = GetFilesOrdered(input, pattern);
                    foreach (string file in files)
                    {
                        try
                        {
                            outputs.Add(new ParentablePath(Path.GetFullPath(file), appendparent ? parentPath : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{file}' was too long");
                        }
                        catch (Exception ex)
                        {
                            Globals.Logger.Error($"An exception occurred processing '{file}': {ex}");
                        }
                    }
                }
                else if (File.Exists(input))
                {
                    try
                    {
                        outputs.Add(new ParentablePath(Path.GetFullPath(input), appendparent ? parentPath : string.Empty));
                    }
                    catch (PathTooLongException)
                    {
                        Globals.Logger.Warning($"The path for '{input}' was too long");
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error($"An exception occurred processing '{input}': {ex}");
                    }
                }
            }

            return outputs;
        }

        /// <summary>
        /// Retrieve a list of files from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="pattern">Optional pattern to search for directory names</param>
        /// <returns>List with all new files</returns>
        public static List<string> GetFilesOrdered(string dir, string pattern = "*")
        {
            return GetFilesOrderedHelper(dir, new List<string>(), pattern);
        }

        /// <summary>
        /// Retrieve a list of files from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <param name="pattern">Optional pattern to search for directory names</param>
        /// <returns>List with all new files</returns>
        private static List<string> GetFilesOrderedHelper(string dir, List<string> infiles, string pattern)
        {
            // Take care of the files in the top directory
            List<string> toadd = Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly).ToList();
            toadd.Sort(new NaturalComparer());
            infiles.AddRange(toadd);

            // Then recurse through and add from the directories
            List<string> subDirs = Directory.EnumerateDirectories(dir, pattern, SearchOption.TopDirectoryOnly).ToList();
            subDirs.Sort(new NaturalComparer());
            foreach (string subdir in subDirs)
            {
                infiles = GetFilesOrderedHelper(subdir, infiles, pattern);
            }

            // Return the new list
            return infiles;
        }

        /// <summary>
        /// Get all empty folders within a root folder
        /// </summary>
        /// <param name="root">Root directory to parse</param>
        /// <returns>IEumerable containing all directories that are empty, an empty enumerable if the root is empty, null otherwise</returns>
        public static List<string> ListEmpty(string root)
        {
            // Check if the root exists first
            if (!Directory.Exists(root))
                return null;

            // If it does and it is empty, return a blank enumerable
            if (Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories).Count() == 0)
                return new List<string>();

            // Otherwise, get the complete list
            return Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                .Where(dir => Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories).Count() == 0)
                .ToList();
        }

        /// <summary>
        /// Try to safely delete a directory, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the directory to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryCreateDirectory(string file, bool throwOnError = false)
        {
            // Now wrap creating the directory
            try
            {
                Directory.CreateDirectory(file);
                return true;
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return false;
            }
        }

        /// <summary>
        /// Try to safely delete a directory, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the directory to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryDelete(string file, bool throwOnError = false)
        {
            // Check if the directory exists first
            if (!Directory.Exists(file))
                return true;

            // Now wrap deleting the directory
            try
            {
                Directory.Delete(file, true);
                return true;
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return false;
            }
        }
    }
}
