using System;
using System.IO;

using SabreTools.Library.Data;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Extensions to Path functionality
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// Get the extension from the path, if possible
        /// </summary>
        /// <param name="path">Path to get extension from</param>
        /// <returns>Extension, if possible</returns>
        public static string GetNormalizedExtension(string path)
        {
            // Check null or empty first
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Get the extension from the path, if possible
            string ext = Path.GetExtension(path)?.ToLowerInvariant();

            // Check if the extension is null or empty
            if (string.IsNullOrWhiteSpace(ext))
                return null;

            // Make sure that extensions are valid
            ext = ext.TrimStart('.');

            return ext;
        }

        /// <summary>
        /// Get the proper filename (with subpath) from the file and parent combination
        /// </summary>
        /// <param name="path">Input combined path to use</param>
        /// <param name="sanitize">True if path separators should be converted to '-', false otherwise</param>
        /// <returns>Subpath for the file</returns>
        public static string GetNormalizedFileName(string path, bool sanitize)
        {
            // Check that we have a combined path first
            if (!path.Contains("¬"))
            {
                string filename = Path.GetFileName(path);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }

            // First separate out the parts
            string child = path.Split('¬')[0];
            string parent = path.Split('¬')[1];

            // If the parts are the same, return the filename from the first part
            if (string.Equals(child, parent, StringComparison.Ordinal))
            {
                string filename = Path.GetFileName(child);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }

            // Otherwise, remove the parent from the child and return the remainder
            else
            {
                string filename = child.Remove(0, parent.Length + 1);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }
        }

        /// <summary>
        /// Get the proper output path for a given input file and output directory
        /// </summary>
        /// <param name="outDir">Output directory to use</param>
        /// <param name="inputpath">Input path to create output for</param>
        /// <param name="inplace">True if the output file should go to the same input folder, false otherwise</param>
        /// <returns>Complete output path</returns>
        public static string GetOutputPath(string outDir, string inputpath, bool inplace)
        {
            // First, we need to ensure the output directory
            outDir = DirectoryExtensions.Ensure(outDir);

            // Check if we have a split path or not
            bool splitpath = inputpath.Contains("¬");

            // If we have a split path, we need to treat the input separately
            if (splitpath)
            {
                string[] split = inputpath.Split('¬');

                // If we have an inplace output, use the directory name from the input path
                if (inplace)
                {
                    outDir = Path.GetDirectoryName(split[0]);
                }

                // TODO: Should this be the default? Always create a subfolder if a folder is found?
                // If we are processing a path that is coming from a directory and we are outputting to the current directory, we want to get the subfolder to write to
                else if (split[0].Length != split[1].Length && outDir == Environment.CurrentDirectory)
                {
                    outDir = Path.GetDirectoryName(Path.Combine(outDir, split[0].Remove(0, Path.GetDirectoryName(split[1]).Length + 1)));
                }

                // If we are processing a path that is coming from a directory, we want to get the subfolder to write to
                else if (split[0].Length != split[1].Length)
                {
                    outDir = Path.GetDirectoryName(Path.Combine(outDir, split[0].Remove(0, split[1].Length + 1)));
                }

                // If we are processing a single file from the root of a directory, we just use the output directory
                else
                {
                    // No-op
                }
            }
            // Otherwise, assume the input path is just a filename
            else
            {
                // If we have an inplace output, use the directory name from the input path
                if (inplace)
                {
                    outDir = Path.GetDirectoryName(inputpath);
                }

                // Otherwise, just use the supplied output directory
                else
                {
                    // No-op
                }
            }

            // Finally, return the output directory
            return outDir;
        }

        /// <summary>
        /// Get a proper romba sub path
        /// </summary>
        /// <param name="hash">SHA-1 hash to get the path for</param>
        /// <returns>Subfolder path for the given hash</returns>
        public static string GetRombaPath(string hash)
        {
            // If the hash isn't the right size, then we return null
            if (hash.Length != Constants.SHA1Length) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
                return null;

            return Path.Combine(hash.Substring(0, 2), hash.Substring(2, 2), hash.Substring(4, 2), hash.Substring(6, 2), hash + ".gz");
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        public static bool HasValidArchiveExtension(string path)
        {
            // Get the extension from the path, if possible
            string ext = PathExtensions.GetNormalizedExtension(path);

            // Check against the list of known archive extensions
            switch (ext)
            {
                case "7z":
                case "gz":
                case "lzma":
                case "rar":
                case "rev":
                case "r00":
                case "r01":
                case "tar":
                case "tgz":
                case "tlz":
                case "zip":
                case "zipx":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        public static bool HasValidDatExtension(string path)
        {
            // Get the extension from the path, if possible
            string ext = GetNormalizedExtension(path);

            // Check against the list of known DAT extensions
            switch (ext)
            {
                case "csv":
                case "dat":
                case "json":
                case "md5":
                case "ripemd160":
                case "sfv":
                case "sha1":
                case "sha256":
                case "sha384":
                case "sha512":
                case "ssv":
                case "tsv":
                case "txt":
                case "xml":
                    return true;
                default:
                    return false;
            }
        }
    }
}
