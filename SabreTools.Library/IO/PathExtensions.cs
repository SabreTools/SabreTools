using System.IO;

using SabreTools.Library.Data;

namespace SabreTools.Library.IO
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
        /// Get a proper romba sub path
        /// </summary>
        /// <param name="hash">SHA-1 hash to get the path for</param>
        /// <param name="depth">Positive value representing the depth of the depot</param>
        /// <returns>Subfolder path for the given hash</returns>
        public static string GetDepotPath(string hash, int depth)
        {
            // If the hash isn't the right size, then we return null
            if (hash.Length != Constants.SHA1Length) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
                return null;

            // Cap the depth between 0 and 20, for now
            if (depth < 0)
                depth = 0;
            else if (depth > Constants.SHA1ZeroBytes.Length)
                depth = Constants.SHA1ZeroBytes.Length;

            // Loop through and generate the subdirectory
            string path = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                path += hash.Substring(i * 2, 2) + Path.DirectorySeparatorChar;
            }

            // Now append the filename
            path += $"{hash}.gz";
            return path;
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        public static bool HasValidArchiveExtension(string path)
        {
            // Get the extension from the path, if possible
            string ext = GetNormalizedExtension(path);

            // Check against the list of known archive extensions
            switch (ext)
            {
                // Aaruformat
                case "aaru":
                case "aaruf":
                case "aaruformat":
                case "aif":
                case "dicf":

                // Archives
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

                // CHD
                case "chd":
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
