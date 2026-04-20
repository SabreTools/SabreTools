using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD
using System.Net;
using System.Threading.Tasks;
#endif
using SabreTools.IO.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Text.Compare;
using ItemStatus = SabreTools.Data.Models.Metadata.ItemStatus;
using ItemType = SabreTools.Data.Models.Metadata.ItemType;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for splitting DatFiles
    /// </summary>
    /// <remarks>TODO: Implement Level split</remarks>
    public class Splitter
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        #region Extension

        /// <summary>
        /// Split a DAT by input extensions
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="extA">List of extensions to split on (first DAT)</param>
        /// <param name="extB">List of extensions to split on (second DAT)</param>
        /// <returns>Extension Set A and Extension Set B DatFiles</returns>
        public static (DatFile? extADat, DatFile? extBDat) SplitByExtension(DatFile datFile, List<string> extA, List<string> extB)
        {
            // If roms is empty, return false
            if (datFile.DatStatistics.TotalCount == 0)
                return (null, null);

            InternalStopwatch watch = new($"Splitting DAT by extension");

            // Initialize the outputs
            SplitByExtensionInit(datFile, extA, extB, out DatFile extADat, out DatFile extBDat);

            // Now separate the roms accordingly
            SplitByExtensionImpl(datFile, extA, extB, extADat, extBDat);
            SplitByExtensionDBImpl(datFile, extA, extB, extADat, extBDat);

            // Then return both DatFiles
            watch.Stop();
            return (extADat, extBDat);
        }

        /// <summary>
        /// Initialize splitting by extension
        /// </summary>
        /// <param name="datFile">DatFile representing the data to split</param>
        /// <param name="extA">Set of extensions to go in the first DatFile</param>
        /// <param name="extB">Set of extensions to go in the second DatFile</param>
        /// <param name="extADat">Header-initialized DatFile representing the first set</param>
        /// <param name="extBDat">Header-initialized DatFile representing the second set</param>
        private static void SplitByExtensionInit(DatFile datFile, List<string> extA, List<string> extB, out DatFile extADat, out DatFile extBDat)
        {
            // Make sure all of the extensions don't have a dot at the beginning
            extA = extA.ConvertAll(s => s.TrimStart('.').ToLowerInvariant());
            extB = extB.ConvertAll(s => s.TrimStart('.').ToLowerInvariant());

            // Set all of the appropriate outputs for each of the subsets
            string extAString = string.Join(",", [.. extA]);
            extADat = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
            extADat.Header.FileName = $"{extADat.Header.FileName} ({extAString})";
            extADat.Header.Name = $"{extADat.Header.Name} ({extAString})";
            extADat.Header.Description = $"{extADat.Header.Description} ({extAString})";

            string extBString = string.Join(",", [.. extB]);
            extBDat = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
            extBDat.Header.FileName = $"{extBDat.Header.FileName} ({extBString})";
            extBDat.Header.Name = $"{extBDat.Header.Name} ({extBString})";
            extBDat.Header.Description = $"{extBDat.Header.Description} ({extBString})";
        }

        /// <summary>
        /// Split a DAT by input extensions
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="extA">List of extensions to split on (first DAT)</param>
        /// <param name="extB">List of extensions to split on (second DAT)</param>
        /// <param name="extADat">Header-initialized DatFile representing the first set</param>
        /// <param name="extBDat">Header-initialized DatFile representing the second set</param>
        private static void SplitByExtensionImpl(DatFile datFile, List<string> extA, List<string> extB, DatFile extADat, DatFile extBDat)
        {
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                var items = datFile.GetItemsForBucket(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    if (extA.Contains(item.GetName().GetNormalizedExtension() ?? string.Empty))
                    {
                        extADat.AddItem(item, statsOnly: false);
                    }
                    else if (extB.Contains(item.GetName().GetNormalizedExtension() ?? string.Empty))
                    {
                        extBDat.AddItem(item, statsOnly: false);
                    }
                    else
                    {
                        extADat.AddItem(item, statsOnly: false);
                        extBDat.AddItem(item, statsOnly: false);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Split a DAT by input extensions
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="extA">List of extensions to split on (first DAT)</param>
        /// <param name="extB">List of extensions to split on (second DAT)</param>
        /// <param name="extADat">Header-initialized DatFile representing the first set</param>
        /// <param name="extBDat">Header-initialized DatFile representing the second set</param>
        private static void SplitByExtensionDBImpl(DatFile datFile, List<string> extA, List<string> extB, DatFile extADat, DatFile extBDat)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = extADat.AddSourceDB(source.Value);
                _ = extBDat.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = extADat.AddMachineDB(machine.Value);
                _ = extBDat.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Set the machine and source index for this item
                long machineIndex = item.Value.MachineIndex;
                item.Value.MachineIndex = machineRemapping[machineIndex];

                long sourceIndex = item.Value.SourceIndex;
                item.Value.SourceIndex = sourceRemapping[sourceIndex];

                if (extA.Contains(item.Value.GetName().GetNormalizedExtension() ?? string.Empty))
                {
                    extADat.AddItemDB(item.Value, statsOnly: false);
                }
                else if (extB.Contains(item.Value.GetName().GetNormalizedExtension() ?? string.Empty))
                {
                    extBDat.AddItemDB(item.Value, statsOnly: false);
                }
                else
                {
                    extADat.AddItemDB(item.Value, statsOnly: false);
                    extBDat.AddItemDB(item.Value, statsOnly: false);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Hash

        /// <summary>
        /// Split a DAT by best available hashes
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <returns>Dictionary of Field to DatFile mappings</returns>
        public static Dictionary<string, DatFile> SplitByHash(DatFile datFile)
        {
            // Create each of the respective output DATs
            var watch = new InternalStopwatch($"Splitting DAT by best available hashes");

            // Initialize the outputs
            Dictionary<string, DatFile> fieldDats = SplitByHashInit(datFile);

            // Now populate each of the DAT objects in turn
            SplitByHashImpl(datFile, fieldDats);
            SplitByHashDBImpl(datFile, fieldDats);

            watch.Stop();
            return fieldDats;
        }

        /// <summary>
        /// Initialize splitting by hash
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <returns>Dictionary of hash-specific DatFiles</returns>
        private static Dictionary<string, DatFile> SplitByHashInit(DatFile datFile)
        {
            // Create mapping of keys to suffixes
            var mappings = new Dictionary<string, string>
            {
                ["status"] = " (Nodump)",
                ["sha512"] = " (SHA-512)",
                ["sha384"] = " (SHA-384)",
                ["sha256"] = " (SHA-256)",
                ["sha1"] = " (SHA-1)",
                ["ripemd160"] = " (RIPEMD160)",
                ["ripemd128"] = " (RIPEMD128)",
                ["md5"] = " (MD5)",
                ["md4"] = " (MD4)",
                ["md2"] = " (MD2)",
                ["crc64"] = " (CRC-64)",
                ["crc32"] = " (CRC-32)",
                ["crc16"] = " (CRC-16)",
                ["null"] = " (Other)",
            };

            // Create the set of field-to-dat mappings
            Dictionary<string, DatFile> fieldDats = [];
            foreach (var kvp in mappings)
            {
                fieldDats[kvp.Key] = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
                fieldDats[kvp.Key].Header.FileName = fieldDats[kvp.Key].Header.FileName + kvp.Value;
                fieldDats[kvp.Key].Header.Name = fieldDats[kvp.Key].Header.Name + kvp.Value;
                fieldDats[kvp.Key].Header.Description = fieldDats[kvp.Key].Header.Description + kvp.Value;
            }

            return fieldDats;
        }

        /// <summary>
        /// Split a DAT by best available hashes
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="fieldDats">Dictionary of hash-specific DatFiles</param>
        private static void SplitByHashImpl(DatFile datFile, Dictionary<string, DatFile> fieldDats)
        {
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                var items = datFile.GetItemsForBucket(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif
                foreach (DatItem item in items)
                {
                    // If the file is not a Disk, Media, or Rom, continue
                    switch (item)
                    {
                        case Disk disk:
                            if (disk.Status == ItemStatus.Nodump)
                                fieldDats["status"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(disk.SHA1))
                                fieldDats["sha1"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(disk.MD5))
                                fieldDats["md5"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(disk.MD5))
                                fieldDats["md5"].AddItem(item, statsOnly: false);
                            else
                                fieldDats["null"].AddItem(item, statsOnly: false);
                            break;

                        case Media media:
                            if (!string.IsNullOrEmpty(media.SHA256))
                                fieldDats["sha256"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(media.SHA1))
                                fieldDats["sha1"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(media.MD5))
                                fieldDats["md5"].AddItem(item, statsOnly: false);
                            else
                                fieldDats["null"].AddItem(item, statsOnly: false);
                            break;

                        case Rom rom:
                            if (rom.Status == ItemStatus.Nodump)
                                fieldDats["status"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.SHA512))
                                fieldDats["sha512"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.SHA384))
                                fieldDats["sha384"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.SHA256))
                                fieldDats["sha256"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.SHA1))
                                fieldDats["sha1"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.RIPEMD160))
                                fieldDats["ripemd160"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.RIPEMD128))
                                fieldDats["ripemd128"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.MD5))
                                fieldDats["md5"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.MD4))
                                fieldDats["md4"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.MD2))
                                fieldDats["md2"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.CRC64))
                                fieldDats["crc64"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.CRC32))
                                fieldDats["crc32"].AddItem(item, statsOnly: false);
                            else if (!string.IsNullOrEmpty(rom.CRC16))
                                fieldDats["crc16"].AddItem(item, statsOnly: false);
                            else
                                fieldDats["null"].AddItem(item, statsOnly: false);
                            break;

                        default:
                            continue;
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Split a DAT by best available hashes
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="fieldDats">Dictionary of hash-specific DatFiles</param>
        private static void SplitByHashDBImpl(DatFile datFile, Dictionary<string, DatFile> fieldDats)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = fieldDats["status"].AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
                _ = fieldDats["sha512"].AddSourceDB(source.Value);
                _ = fieldDats["sha384"].AddSourceDB(source.Value);
                _ = fieldDats["sha256"].AddSourceDB(source.Value);
                _ = fieldDats["sha1"].AddSourceDB(source.Value);
                _ = fieldDats["ripemd160"].AddSourceDB(source.Value);
                _ = fieldDats["ripemd128"].AddSourceDB(source.Value);
                _ = fieldDats["md5"].AddSourceDB(source.Value);
                _ = fieldDats["md4"].AddSourceDB(source.Value);
                _ = fieldDats["md2"].AddSourceDB(source.Value);
                _ = fieldDats["crc64"].AddSourceDB(source.Value);
                _ = fieldDats["crc32"].AddSourceDB(source.Value);
                _ = fieldDats["crc16"].AddSourceDB(source.Value);
                _ = fieldDats["null"].AddSourceDB(source.Value);
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = fieldDats["status"].AddMachineDB(machine.Value);
                _ = fieldDats["sha512"].AddMachineDB(machine.Value);
                _ = fieldDats["sha384"].AddMachineDB(machine.Value);
                _ = fieldDats["sha256"].AddMachineDB(machine.Value);
                _ = fieldDats["sha1"].AddMachineDB(machine.Value);
                _ = fieldDats["ripemd128"].AddMachineDB(machine.Value);
                _ = fieldDats["ripemd160"].AddMachineDB(machine.Value);
                _ = fieldDats["md5"].AddMachineDB(machine.Value);
                _ = fieldDats["md4"].AddMachineDB(machine.Value);
                _ = fieldDats["md2"].AddMachineDB(machine.Value);
                _ = fieldDats["crc64"].AddMachineDB(machine.Value);
                _ = fieldDats["crc32"].AddMachineDB(machine.Value);
                _ = fieldDats["crc16"].AddMachineDB(machine.Value);
                _ = fieldDats["null"].AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Set the machine and source index for this item
                long machineIndex = item.Value.MachineIndex;
                item.Value.MachineIndex = machineRemapping[machineIndex];

                long sourceIndex = item.Value.SourceIndex;
                item.Value.SourceIndex = sourceRemapping[sourceIndex];

                // Only process Disk, Media, and Rom
                switch (item.Value)
                {
                    case Disk disk:
                        if (disk.Status == ItemStatus.Nodump)
                            fieldDats["status"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(disk.SHA1))
                            fieldDats["sha1"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(disk.MD5))
                            fieldDats["md5"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(disk.MD5))
                            fieldDats["md5"].AddItemDB(item.Value, statsOnly: false);
                        else
                            fieldDats["null"].AddItemDB(item.Value, statsOnly: false);
                        break;

                    case Media media:
                        if (!string.IsNullOrEmpty(media.SHA256))
                            fieldDats["sha256"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(media.SHA1))
                            fieldDats["sha1"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(media.MD5))
                            fieldDats["md5"].AddItemDB(item.Value, statsOnly: false);
                        else
                            fieldDats["null"].AddItemDB(item.Value, statsOnly: false);
                        break;

                    case Rom rom:
                        if (rom.Status == ItemStatus.Nodump)
                            fieldDats["status"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.SHA512))
                            fieldDats["sha512"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.SHA384))
                            fieldDats["sha384"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.SHA256))
                            fieldDats["sha256"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.SHA1))
                            fieldDats["sha1"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.RIPEMD160))
                            fieldDats["ripemd160"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.RIPEMD128))
                            fieldDats["ripemd128"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.MD5))
                            fieldDats["md5"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.MD4))
                            fieldDats["md4"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.MD2))
                            fieldDats["md2"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.CRC64))
                            fieldDats["crc64"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.CRC32))
                            fieldDats["crc32"].AddItemDB(item.Value, statsOnly: false);
                        else if (!string.IsNullOrEmpty(rom.CRC16))
                            fieldDats["crc16"].AddItemDB(item.Value, statsOnly: false);
                        else
                            fieldDats["null"].AddItemDB(item.Value, statsOnly: false);
                        break;

                    default:
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Level

        /// <summary>
        /// Split a SuperDAT by lowest available directory level
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="shortname">True if short names should be used, false otherwise</param>
        /// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public static bool SplitByLevel(DatFile datFile, string outDir, bool shortname, bool basedat)
        {
            InternalStopwatch watch = new($"Splitting DAT by level");

            // First, bucket by games so that we can do the right thing
            datFile.BucketBy(ItemKey.Machine, lower: false, norename: true);

            // Create a temporary DAT to add things to
            DatFile tempDat = Parser.CreateDatFile(datFile.Header, datFile.Modifiers);
            tempDat.Header.Name = null;

            // Sort the input keys
            List<string> keys = [.. datFile.Items.SortedKeys];
            keys.Sort(SplitByLevelSort);

            // Then, we loop over the games
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                // Here, the key is the name of the game to be used for comparison
                if (tempDat.Header.Name is not null && tempDat.Header.Name != Path.GetDirectoryName(key))
                {
                    // Reset the DAT for the next items
                    tempDat = Parser.CreateDatFile(datFile.Header, datFile.Modifiers);
                    tempDat.Header.Name = null;
                }

                // Clean the input list and set all games to be pathless
                List<DatItem>? items = datFile.GetItemsForBucket(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif
                items.ForEach(item => item.Machine!.Name = Path.GetFileName(item.Machine!.Name));
                items.ForEach(item => item.Machine!.Description = Path.GetFileName(item.Machine!.Description));

                // Now add the game to the output DAT
                items.ForEach(item => tempDat.AddItem(item, statsOnly: false));

                // Then set the DAT name to be the parent directory name
                tempDat.Header.Name = Path.GetDirectoryName(key);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif

            watch.Stop();
            return true;
        }

        /// <summary>
        /// Helper function for SplitByLevel to sort the input game names
        /// </summary>
        /// <param name="a">First string to compare</param>
        /// <param name="b">Second string to compare</param>
        /// <returns>-1 for a coming before b, 0 for a == b, 1 for a coming after b</returns>
        private static int SplitByLevelSort(string a, string b)
        {
            var nc = new NaturalComparer();
            int adeep = a.Count(c => c == '/' || c == '\\');
            int bdeep = b.Count(c => c == '/' || c == '\\');

            if (adeep == bdeep)
                return nc.Compare(a, b);

            return adeep - bdeep;
        }

#pragma warning disable IDE0051
        /// <summary>
        /// Helper function for SplitByLevel to clean and write out a DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="newDatFile">DAT to clean and write out</param>
        /// <param name="outDir">Directory to write out to</param>
        /// <param name="shortname">True if short naming scheme should be used, false otherwise</param>
        /// <param name="restore">True if original filenames should be used as the base for output filename, false otherwise</param>
        /// TODO: Finish implementation of level splitting
        private static void SplitByLevelHelper(DatFile datFile, DatFile newDatFile, string outDir, bool shortname, bool restore)
        {
            // Get the name from the DAT to use separately
            string? name = newDatFile.Header.Name;
            string? expName = name?.Replace("/", " - ")?.Replace("\\", " - ");

            // Now set the new output values
#if NET20 || NET35
            newDatFile.Header.FileName = (string.IsNullOrEmpty(name)
                ? datFile.Header.FileName
                : (shortname
                    ? Path.GetFileName(name)
                    : expName
                ));
#else
            newDatFile.Header.FileName = WebUtility.HtmlDecode(string.IsNullOrEmpty(name)
                ? datFile.Header.FileName
                : (shortname
                    ? Path.GetFileName(name)
                    : expName
                    )
                );
#endif
            newDatFile.Header.FileName = restore
                ? $"{datFile.Header.FileName} ({newDatFile.Header.FileName})"
                : newDatFile.Header.FileName;
            newDatFile.Header.Name = $"{datFile.Header.Name} ({expName})";
            newDatFile.Header.Name = string.IsNullOrEmpty(datFile.Header.Description)
                ? newDatFile.Header.Name
                : $"{datFile.Header.Description} ({expName})";
            newDatFile.Header.Type = null;

            // Write out the temporary DAT to the proper directory
            // Writer.Write(newDatFile, datFormats, outDir);
        }
#pragma warning restore IDE0051

        #endregion

        #region Size

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="radix">Long value representing the split point</param>
        /// <returns>Less Than and Greater Than DatFiles</returns>
        public static (DatFile lessThan, DatFile greaterThan) SplitBySize(DatFile datFile, long radix)
        {
            // Create each of the respective output DATs
            InternalStopwatch watch = new($"Splitting DAT by size");

            // Initialize the outputs
            SplitBySizeInit(datFile, radix, out DatFile lessThan, out DatFile greaterThan);

            // Now populate each of the DAT objects in turn
            SplitBySizeImpl(datFile, radix, lessThan, greaterThan);
            SplitBySizeDBImpl(datFile, radix, lessThan, greaterThan);

            // Then return both DatFiles
            watch.Stop();
            return (lessThan, greaterThan);
        }

        /// <summary>
        /// Initialize splitting by size
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="radix">Size to use as the radix between the outputs</param>
        /// <param name="lessThan">DatFile representing items less than <paramref name="radix"/></param>
        /// <param name="greaterThan">DatFile representing items greater than or equal to <paramref name="radix"/></param>
        private static void SplitBySizeInit(DatFile datFile, long radix, out DatFile lessThan, out DatFile greaterThan)
        {
            lessThan = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
            lessThan.Header.FileName = lessThan.Header.FileName + $" (less than {radix})";
            lessThan.Header.Name = lessThan.Header.Name + $" (less than {radix})";
            lessThan.Header.Description = lessThan.Header.Description + $" (less than {radix})";

            greaterThan = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
            greaterThan.Header.FileName = greaterThan.Header.FileName + $" (equal-greater than {radix})";
            greaterThan.Header.Name = greaterThan.Header.Name + $" (equal-greater than {radix})";
            greaterThan.Header.Description = greaterThan.Header.Description + $" (equal-greater than {radix})";
        }

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="radix">Size to use as the radix between the outputs</param>
        /// <param name="lessThan">DatFile representing items less than <paramref name="radix"/></param>
        /// <param name="greaterThan">DatFile representing items greater than or equal to <paramref name="radix"/></param>
        private static void SplitBySizeImpl(DatFile datFile, long radix, DatFile lessThan, DatFile greaterThan)
        {
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem>? items = datFile.GetItemsForBucket(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif
                foreach (DatItem item in items)
                {
                    // If the file is not a Rom, it automatically goes in the "lesser" dat
                    if (item is not Rom rom)
                        lessThan.AddItem(item, statsOnly: false);

                    // If the file is a Rom and has no size, put it in the "lesser" dat
                    else if (rom.Size is null)
                        lessThan.AddItem(item, statsOnly: false);

                    // If the file is a Rom and less than the radix, put it in the "lesser" dat
                    else if (rom.Size < radix)
                        lessThan.AddItem(item, statsOnly: false);

                    // If the file is a Rom and greater than or equal to the radix, put it in the "greater" dat
                    else if (rom.Size >= radix)
                        greaterThan.AddItem(item, statsOnly: false);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="radix">Size to use as the radix between the outputs</param>
        /// <param name="lessThan">DatFile representing items less than <paramref name="radix"/></param>
        /// <param name="greaterThan">DatFile representing items greater than or equal to <paramref name="radix"/></param>
        private static void SplitBySizeDBImpl(DatFile datFile, long radix, DatFile lessThan, DatFile greaterThan)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = lessThan.AddSourceDB(source.Value);
                _ = greaterThan.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = lessThan.AddMachineDB(machine.Value);
                _ = greaterThan.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Set the machine and source index for this item
                long machineIndex = item.Value.MachineIndex;
                item.Value.MachineIndex = machineRemapping[machineIndex];

                long sourceIndex = item.Value.SourceIndex;
                item.Value.SourceIndex = sourceRemapping[sourceIndex];

                // If the file is not a Rom, it automatically goes in the "lesser" dat
                if (item.Value is not Rom rom)
                    lessThan.AddItemDB(item.Value, statsOnly: false);

                // If the file is a Rom and has no size, put it in the "lesser" dat
                else if (rom.Size is null)
                    lessThan.AddItemDB(item.Value, statsOnly: false);

                // If the file is a Rom and less than the radix, put it in the "lesser" dat
                else if (rom.Size < radix)
                    lessThan.AddItemDB(item.Value, statsOnly: false);

                // If the file is a Rom and greater than or equal to the radix, put it in the "greater" dat
                else if (rom.Size >= radix)
                    greaterThan.AddItemDB(item.Value, statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Total Size

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="chunkSize">Long value representing the total size to split at</param>
        /// <returns>Less Than and Greater Than DatFiles</returns>
        /// TODO: Create DB version of this method
        public static List<DatFile> SplitByTotalSize(DatFile datFile, long chunkSize)
        {
            // If the size is invalid, just return
            if (chunkSize <= 0)
                return [];

            // Create each of the respective output DATs
            InternalStopwatch watch = new($"Splitting DAT by total size");

            // Sort the DatFile by machine name
            datFile.BucketBy(ItemKey.Machine);

            // Get the keys in a known order for easier sorting
            var keys = datFile.Items.SortedKeys;

            // Get the output list
            List<DatFile> datFiles = [];

            // Initialize everything
            long currentSize = 0;
            long currentIndex = 0;
            DatFile currentDat = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
            currentDat.Header.FileName = currentDat.Header.FileName + $"_{currentIndex}";
            currentDat.Header.Name = currentDat.Header.Name + $"_{currentIndex}";
            currentDat.Header.Description = currentDat.Header.Description + $"_{currentIndex}";

            // Loop through each machine
            foreach (string machine in keys)
            {
                // Get the current machine
                var items = datFile.GetItemsForBucket(machine);
                if (items is null || items.Count == 0)
                {
                    _staticLogger.Error($"{machine} contains no items and will be skipped");
                    continue;
                }

                // Get the total size of the current machine
                long machineSize = 0;
                foreach (var item in items)
                {
                    if (item is Rom rom)
                    {
                        // TODO: Should there be more than just a log if a single item is larger than the chunksize?
                        machineSize += rom.Size ?? 0;
                        if ((rom.Size ?? 0) > chunkSize)
                            _staticLogger.Error($"{rom.GetName() ?? string.Empty} in {machine} is larger than {chunkSize}");
                    }
                }

                // If the current machine size is greater than the chunk size by itself, we want to log and skip
                // TODO: Should this eventually try to split the machine here?
                if (machineSize > chunkSize)
                {
                    _staticLogger.Error($"{machine} is larger than {chunkSize} and will be skipped");
                    continue;
                }

                // If the current machine size makes the current DatFile too big, split
                else if (currentSize + machineSize > chunkSize)
                {
                    datFiles.Add(currentDat);
                    currentSize = 0;
                    currentIndex++;
                    currentDat = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
                    currentDat.Header.FileName = currentDat.Header.FileName + $"_{currentIndex}";
                    currentDat.Header.Name = currentDat.Header.Name + $"_{currentIndex}";
                    currentDat.Header.Description = currentDat.Header.Description + $"_{currentIndex}";
                }

                // Add the current machine to the current DatFile
                items.ForEach(item => currentDat.AddItem(item, statsOnly: false));
                currentSize += machineSize;
            }

            // Add the final DatFile to the list
            datFiles.Add(currentDat);

            // Then return the list
            watch.Stop();
            return datFiles;
        }

        #endregion

        #region Type

        /// <summary>
        /// Split a DAT by type of DatItem
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <returns>Dictionary of ItemType to DatFile mappings</returns>
        public static Dictionary<ItemType, DatFile> SplitByType(DatFile datFile)
        {
            // Create each of the respective output DATs
            InternalStopwatch watch = new($"Splitting DAT by item type");

            // Create the set of type-to-dat mappings
            Dictionary<ItemType, DatFile> typeDats = [];

            // We only care about a subset of types
            List<ItemType> outputTypes =
            [
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
                ItemType.Sample,
            ];

            // Setup all of the DatFiles
            foreach (ItemType itemType in outputTypes)
            {
                typeDats[itemType] = Parser.CreateDatFile((DatHeader)datFile.Header.Clone(), datFile.Modifiers);
                typeDats[itemType].Header.FileName = typeDats[itemType].Header.FileName + $" ({itemType})";
                typeDats[itemType].Header.Name = typeDats[itemType].Header.Name + $" ({itemType})";
                typeDats[itemType].Header.Description = typeDats[itemType].Header.Description + $" ({itemType})";
            }

            // Now populate each of the DAT objects in turn
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(outputTypes, itemType =>
#else
            foreach (var itemType in outputTypes)
#endif
            {
                FillWithItemType(datFile, typeDats[itemType], itemType);
                FillWithItemTypeDB(datFile, typeDats[itemType], itemType);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif

            watch.Stop();
            return typeDats;
        }

        /// <summary>
        /// Fill a DatFile with all items with a particular ItemType
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="indexDat">DatFile to add found items to</param>
        /// <param name="itemType">ItemType to retrieve items for</param>
        /// <returns>DatFile containing all items with the ItemType/returns>
        private static void FillWithItemType(DatFile datFile, DatFile indexDat, ItemType itemType)
        {
            // Loop through and add the items for this index to the output
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = ItemDictionary.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items is null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    if (item.ItemType == itemType)
                        indexDat.AddItem(item, statsOnly: false);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Fill a DatFile with all items with a particular ItemType
        /// </summary>
        /// <param name="datFile">Current DatFile object to split</param>
        /// <param name="indexDat">DatFile to add found items to</param>
        /// <param name="itemType">ItemType to retrieve items for</param>
        /// <returns>DatFile containing all items with the ItemType/returns>
        private static void FillWithItemTypeDB(DatFile datFile, DatFile indexDat, ItemType itemType)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = indexDat.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = indexDat.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Set the machine and source index for this item
                long machineIndex = item.Value.MachineIndex;
                item.Value.MachineIndex = machineRemapping[machineIndex];

                long sourceIndex = item.Value.SourceIndex;
                item.Value.SourceIndex = sourceRemapping[sourceIndex];

                if (item.Value.ItemType == itemType)
                    indexDat.AddItemDB(item.Value, statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion
    }
}
