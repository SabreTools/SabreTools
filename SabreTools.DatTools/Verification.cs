using System.Collections.Generic;
using System.IO;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Archives;
using SabreTools.Hashing;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for verifying data from DatFiles
    /// </summary>
    public class Verification
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        /// <summary>
        /// Verify a DatFile against a set of depots, leaving only missing files
        /// </summary>
        /// <param name="datFile">Current DatFile object to verify against</param>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public static bool VerifyDepot(DatFile datFile, List<string> inputs)
        {
            bool success = true;

            InternalStopwatch watch = new("Verifying all from supplied depots");

            // Now loop through and get only directories from the input paths
            List<string> directories = [];
            foreach (string input in inputs)
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    _staticLogger.Verbose($"Adding depot: {input}");
                    directories.Add(input);
                }
            }

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to bucket the input DAT by SHA-1
            datFile.BucketBy(ItemKey.SHA1);

            // Then we want to loop through each of the hashes and see if we can rebuild
            foreach (string hash in datFile.Items.SortedKeys)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != HashType.SHA1.ZeroString.Length)
                    continue;

                _staticLogger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string? subpath = datFile.Modifiers.InputDepot?.GetDepotPath(hash);
                if (subpath is null)
                    continue;

                // Find the first depot that includes the hash
                string? foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath is null)
                    continue;

                // If we have a path, we want to try to get the rom information
                var tgz = new GZipArchive(foundpath);
                BaseFile? fileinfo = tgz.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo is null)
                    continue;

                // Now we want to remove all duplicates from the DAT
                _ = datFile.GetDuplicates(fileinfo.ConvertToRom(), sorted: true);
                _ = datFile.GetDuplicates(fileinfo.ConvertToDisk(), sorted: true);
            }

            watch.Stop();

            // Set fixdat headers in case of writing out
            datFile.Header.FileName = $"fixDAT_{datFile.Header.FileName}";
            datFile.Header.Name = $"fixDAT_{datFile.Header.Name}";
            datFile.Header.Description = $"fixDAT_{datFile.Header.Description}";
            datFile.ClearMarked();

            return success;
        }

        /// <summary>
        /// Verify a DatFile against a set of depots, leaving only missing files
        /// </summary>
        /// <param name="datFile">Current DatFile object to verify against</param>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public static bool VerifyDepotDB(DatFile datFile, List<string> inputs)
        {
            bool success = true;

            var watch = new InternalStopwatch("Verifying all from supplied depots");

            // Now loop through and get only directories from the input paths
            List<string> directories = [];
            foreach (string input in inputs)
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    _staticLogger.Verbose($"Adding depot: {input}");
                    directories.Add(input);
                }
            }

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to bucket the input DAT by SHA-1
            datFile.BucketBy(ItemKey.SHA1);

            // Then we want to loop through each of the hashes and see if we can rebuild
            List<string> keys = [.. datFile.ItemsDB.SortedKeys];
            foreach (string hash in keys)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != HashType.SHA1.ZeroString.Length)
                    continue;

                _staticLogger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string? subpath = datFile.Modifiers.InputDepot?.GetDepotPath(hash);
                if (subpath is null)
                    continue;

                // Find the first depot that includes the hash
                string? foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath is null)
                    continue;

                // If we have a path, we want to try to get the rom information
                GZipArchive tgz = new(foundpath);
                BaseFile? fileinfo = tgz.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo is null)
                    continue;

                // Now we want to remove all duplicates from the DAT
                _ = datFile.GetDuplicatesDB(new KeyValuePair<long, DatItem>(-1, fileinfo.ConvertToRom()), sorted: true);
                _ = datFile.GetDuplicatesDB(new KeyValuePair<long, DatItem>(-1, fileinfo.ConvertToDisk()), sorted: true);
            }

            watch.Stop();

            // Set fixdat headers in case of writing out
            datFile.Header.FileName = $"fixDAT_{datFile.Header.FileName}";
            datFile.Header.Name = $"fixDAT_{datFile.Header.Name}";
            datFile.Header.Description = $"fixDAT_{datFile.Header.Description}";
            datFile.ClearMarked();

            return success;
        }

        /// <summary>
        /// Verify a DatFile against a set of inputs, leaving only missing files
        /// </summary>
        /// <param name="datFile">Current DatFile object to verify against</param>
        /// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public static bool VerifyGeneric(DatFile datFile, bool hashOnly)
        {
            bool success = true;

            InternalStopwatch watch = new("Verifying all from supplied paths");

            // Force bucketing according to the flags
            datFile.Items.SetBucketedBy(ItemKey.NULL);
            if (hashOnly)
            {
                datFile.BucketBy(ItemKey.CRC32);
                datFile.Deduplicate();
            }
            else
            {
                datFile.BucketBy(ItemKey.Machine);
                datFile.Deduplicate();
            }

            // Then mark items for removal
            foreach (string key in datFile.Items.SortedKeys)
            {
                List<DatItem>? items = datFile.GetItemsForBucket(key);
                if (items is null)
                    continue;

                for (int i = 0; i < items.Count; i++)
                {
                    // Unmatched items will have a source ID of int.MaxValue, remove all others
                    if (items[i].Source?.Index != int.MaxValue)
                        items[i].RemoveFlag = true;
                }
            }

            watch.Stop();

            // Set fixdat headers in case of writing out
            datFile.Header.FileName = $"fixDAT_{datFile.Header.FileName}";
            datFile.Header.Name = $"fixDAT_{datFile.Header.Name}";
            datFile.Header.Description = $"fixDAT_{datFile.Header.Description}";
            datFile.ClearMarked();

            return success;
        }

        /// <summary>
        /// Verify a DatFile against a set of inputs, leaving only missing files
        /// </summary>
        /// <param name="datFile">Current DatFile object to verify against</param>
        /// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public static bool VerifyGenericDB(DatFile datFile, bool hashOnly)
        {
            bool success = true;

            var watch = new InternalStopwatch("Verifying all from supplied paths");

            // Force bucketing according to the flags
            if (hashOnly)
            {
                datFile.BucketBy(ItemKey.CRC32);
                datFile.Deduplicate();
            }
            else
            {
                datFile.BucketBy(ItemKey.Machine);
                datFile.Deduplicate();
            }

            // Then mark items for removal
            List<string> keys = [.. datFile.ItemsDB.SortedKeys];
            foreach (string key in keys)
            {
                var items = datFile.ItemsDB.GetItemsForBucket(key);
                if (items is null)
                    continue;

                foreach (var item in items)
                {
                    // Get the source associated with the item
                    var source = datFile.GetSourceDB(item.Value.SourceIndex);

                    // Unmatched items will have a source ID of int.MaxValue, remove all others
                    if (source.Value?.Index != int.MaxValue)
                        item.Value.RemoveFlag = true;
                }
            }

            watch.Stop();

            // Set fixdat headers in case of writing out
            datFile.Header.FileName = $"fixDAT_{datFile.Header.FileName}";
            datFile.Header.Name = $"fixDAT_{datFile.Header.Name}";
            datFile.Header.Description = $"fixDAT_{datFile.Header.Description}";
            datFile.ClearMarked();

            return success;
        }
    }
}
