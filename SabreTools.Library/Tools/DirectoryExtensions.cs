using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using NaturalSort;

namespace SabreTools.Library.Tools
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
        /// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise (default)</param>
        /// <returns>List of strings representing just directories from the inputs</returns>
        public static List<string> GetDirectoriesOnly(List<string> inputs, bool appendparent = false)
        {
            List<string> outputs = new List<string>();
            foreach (string input in inputs)
            {
                if (Directory.Exists(input))
                {
                    List<string> directories = GetDirectoriesOrdered(input);
                    foreach (string dir in directories)
                    {
                        try
                        {
                            outputs.Add(Path.GetFullPath(dir) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{dir}' was too long");
                        }
                        catch (Exception ex)
                        {
                            Globals.Logger.Error(ex.ToString());
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
        /// <returns>List with all new files</returns>
        private static List<string> GetDirectoriesOrdered(string dir)
        {
            return GetDirectoriesOrderedHelper(dir, new List<string>());
        }

        /// <summary>
        /// Retrieve a list of directories from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <returns>List with all new files</returns>
        private static List<string> GetDirectoriesOrderedHelper(string dir, List<string> infiles)
        {
            // Take care of the files in the top directory
            List<string> toadd = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            toadd.Sort(new NaturalComparer());
            infiles.AddRange(toadd);

            // Then recurse through and add from the directories
            foreach (string subDir in toadd)
            {
                infiles = GetDirectoriesOrderedHelper(subDir, infiles);
            }

            // Return the new list
            return infiles;
        }

        /// <summary>
        /// Retrieve a list of just files from inputs
        /// </summary>
        /// <param name="inputs">List of strings representing directories and files</param>
        /// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise (default)</param>
        /// <returns>List of strings representing just files from the inputs</returns>
        public static List<string> GetFilesOnly(List<string> inputs, bool appendparent = false)
        {
            List<string> outputs = new List<string>();
            foreach (string input in inputs)
            {
                if (Directory.Exists(input))
                {
                    List<string> files = GetFilesOrdered(input);
                    foreach (string file in files)
                    {
                        try
                        {
                            outputs.Add(Path.GetFullPath(file) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{file}' was too long");
                        }
                        catch (Exception ex)
                        {
                            Globals.Logger.Error(ex.ToString());
                        }
                    }
                }
                else if (File.Exists(input))
                {
                    try
                    {
                        outputs.Add(Path.GetFullPath(input) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                    }
                    catch (PathTooLongException)
                    {
                        Globals.Logger.Warning($"The path for '{input}' was too long");
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error(ex.ToString());
                    }
                }
            }

            return outputs;
        }

        /// <summary>
        /// Retrieve a list of files from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <returns>List with all new files</returns>
        public static List<string> GetFilesOrdered(string dir)
        {
            return GetFilesOrderedHelper(dir, new List<string>());
        }

        /// <summary>
        /// Retrieve a list of files from a directory recursively in proper order
        /// </summary>
        /// <param name="dir">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <returns>List with all new files</returns>
        private static List<string> GetFilesOrderedHelper(string dir, List<string> infiles)
        {
            // Take care of the files in the top directory
            List<string> toadd = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            toadd.Sort(new NaturalComparer());
            infiles.AddRange(toadd);

            // Then recurse through and add from the directories
            List<string> subDirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            subDirs.Sort(new NaturalComparer());
            foreach (string subdir in subDirs)
            {
                infiles = GetFilesOrderedHelper(subdir, infiles);
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
