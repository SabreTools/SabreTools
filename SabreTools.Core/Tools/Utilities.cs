using System;
using System.IO;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;

namespace SabreTools.Core.Tools
{
    /// <summary>
    /// Static utility functions used throughout the library
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Determine if two hashes are equal for the purposes of merging
        /// </summary>
        public static bool ConditionalHashEquals(byte[]? firstHash, byte[]? secondHash)
        {
            // If either hash is empty, we say they're equal for merging
            if (firstHash.IsNullOrEmpty() || secondHash.IsNullOrEmpty())
                return true;

            // If they're different sizes, they can't match
            if (firstHash!.Length != secondHash!.Length)
                return false;

            // Otherwise, they need to match exactly
            return Matching.Extensions.EqualsExactly(firstHash, secondHash);
        }

        /// <summary>
        /// Determine if two hashes are equal for the purposes of merging
        /// </summary>
        public static bool ConditionalHashEquals(string? firstHash, string? secondHash)
        {
            // If either hash is empty, we say they're equal for merging
            if (string.IsNullOrEmpty(firstHash) || string.IsNullOrEmpty(secondHash))
                return true;

            // If they're different sizes, they can't match
            if (firstHash!.Length != secondHash!.Length)
                return false;

            // Otherwise, they need to match exactly
            return string.Equals(firstHash, secondHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get a proper romba sub path
        /// </summary>
        /// <param name="hash">SHA-1 hash to get the path for</param>
        /// <param name="depth">Positive value representing the depth of the depot</param>
        /// <returns>Subfolder path for the given hash</returns>
        public static string? GetDepotPath(byte[]? hash, int depth)
        {
            string? sha1 = hash.ToHexString();
            return GetDepotPath(sha1, depth);
        }

        /// <summary>
        /// Get a proper romba sub path
        /// </summary>
        /// <param name="hash">SHA-1 hash to get the path for</param>
        /// <param name="depth">Positive value representing the depth of the depot</param>
        /// <returns>Subfolder path for the given hash</returns>
        public static string? GetDepotPath(string? hash, int depth)
        {
            // If the hash is null or empty, then we return null
            if (string.IsNullOrEmpty(hash))
                return null;

            // If the hash isn't the right size, then we return null
            if (hash!.Length != Constants.SHA1Length)
                return null;

            // Cap the depth between 0 and 20, for now
            if (depth < 0)
                depth = 0;
            else if (depth > ZeroHash.SHA1Arr.Length)
                depth = ZeroHash.SHA1Arr.Length;

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
        public static bool HasValidDatExtension(string? path)
        {
            // If the path is null or empty, then we return false
            if (string.IsNullOrEmpty(path))
                return false;

            // Get the extension from the path, if possible
            string ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            // Check against the list of known DAT extensions
            return ext switch
            {
                "csv" => true,
                "dat" => true,
                "json" => true,
                "md2" => true,
                "md4" => true,
                "md5" => true,
                "ripemd128" => true,
                "ripemd160" => true,
                "sfv" => true,
                "sha1" => true,
                "sha256" => true,
                "sha384" => true,
                "sha512" => true,
                "spamsum" => true,
                "ssv" => true,
                "tsv" => true,
                "txt" => true,
                "xml" => true,
                _ => false,
            };
        }
    }
}
