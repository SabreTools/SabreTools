using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Archives;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;
using SabreTools.Skippers;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for rebuilding from DatFiles
    /// </summary>
    public class Rebuilder
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        /// <summary>
        /// Process the DAT and find all matches in input files and folders assuming they're a depot
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public static bool RebuildDepot(
            DatFile datFile,
            List<string> inputs,
            string outDir,
            bool date = false,
            bool delete = false,
            bool inverse = false,
            OutputFormat outputFormat = OutputFormat.Folder)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (datFile.DatStatistics.TotalCount == 0 && !inverse)
            {
                _staticLogger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            outDir = outDir.Ensure(create: true);

            // Now we want to get forcepack flag if it's not overridden
            PackingFlag forcePacking = datFile.Header.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey).AsPackingFlag();
            if (outputFormat == OutputFormat.Folder && forcePacking != PackingFlag.None)
                outputFormat = GetOutputFormat(forcePacking);

            #endregion

            bool success = true;

            #region Rebuild from depots in order

            string format = FromOutputFormat(outputFormat) ?? string.Empty;
            InternalStopwatch watch = new($"Rebuilding all files to {format}");

            // Now loop through and get only directories from the input paths
            List<string> directories = [];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(inputs, Core.Globals.ParallelOptions, input =>
#elif NET40_OR_GREATER
            Parallel.ForEach(inputs, input =>
#else
            foreach (var input in inputs)
#endif
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    _staticLogger.Verbose($"Adding depot: {input}");
                    lock (directories)
                    {
                        directories.Add(input);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to bucket the input DAT by SHA-1
            datFile.BucketBy(ItemKey.SHA1);

            // Then we want to loop through each of the hashes and see if we can rebuild
            foreach (string hash in datFile.Items.SortedKeys)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != Constants.SHA1Length)
                    continue;

                _staticLogger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string? subpath = Utilities.GetDepotPath(hash, datFile.Modifiers.InputDepot?.Depth ?? 0);
                if (subpath == null)
                    continue;

                // Find the first depot that includes the hash
                string? foundpath = null;
                foreach (string directory in directories)
                {
                    if (System.IO.File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath == null)
                    continue;

                // If we have a path, we want to try to get the rom information
                var archive = new GZipArchive(foundpath);
                BaseFile? fileinfo = archive.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo == null)
                    continue;

                // If there are no items in the hash, we continue
                var items = datFile.GetItemsForBucket(hash);
                if (items == null || items.Count == 0)
                    continue;

                // Otherwise, we rebuild that file to all locations that we need to
                bool usedInternally;
                if (items[0].GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType() == ItemType.Disk)
                {
                    usedInternally = RebuildIndividualFile(datFile,
                        fileinfo.ConvertToDisk(),
                        foundpath,
                        outDir,
                        date,
                        inverse,
                        forceSorted: true,
                        outputFormat,
                        isZip: false);
                }
                else if (items[0].GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType() == ItemType.File)
                {
                    usedInternally = RebuildIndividualFile(datFile,
                        fileinfo.ConvertToFile(),
                        foundpath,
                        outDir,
                        date,
                        inverse,
                        forceSorted: true,
                        outputFormat,
                        isZip: false);
                }
                else if (items[0].GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType() == ItemType.Media)
                {
                    usedInternally = RebuildIndividualFile(datFile,
                        fileinfo.ConvertToMedia(),
                        foundpath,
                        outDir,
                        date,
                        inverse,
                        forceSorted: true,
                        outputFormat,
                        isZip: false);
                }
                else
                {
                    usedInternally = RebuildIndividualFile(datFile,
                        fileinfo.ConvertToRom(),
                        foundpath,
                        outDir,
                        date,
                        inverse,
                        forceSorted: true,
                        outputFormat,
                        isZip: false);
                }

                // If we are supposed to delete the depot file, do so
                if (delete && usedInternally)
                    System.IO.File.Delete(foundpath);
            }

            watch.Stop();

            #endregion

            return success;
        }

        /// <summary>
        /// Process the DAT and find all matches in input files and folders
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="asFile">TreatAsFile representing special format scanning</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public static bool RebuildGeneric(
            DatFile datFile,
            List<string> inputs,
            string outDir,
            bool quickScan = false,
            bool date = false,
            bool delete = false,
            bool inverse = false,
            OutputFormat outputFormat = OutputFormat.Folder,
            TreatAsFile asFile = 0x00)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (datFile.DatStatistics.TotalCount == 0 && !inverse)
            {
                _staticLogger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
                outDir = Path.GetFullPath(outDir);
            }

            // Now we want to get forcepack flag if it's not overridden
            PackingFlag forcePacking = datFile.Header.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey).AsPackingFlag();
            if (outputFormat == OutputFormat.Folder && forcePacking != PackingFlag.None)
                outputFormat = GetOutputFormat(forcePacking);


            #endregion

            bool success = true;

            #region Rebuild from sources in order

            string format = FromOutputFormat(outputFormat) ?? string.Empty;
            InternalStopwatch watch = new($"Rebuilding all files to {format}");

            // Now loop through all of the files in all of the inputs
            foreach (string input in inputs)
            {
                // If the input is a file
                if (System.IO.File.Exists(input))
                {
                    _staticLogger.User($"Checking file: {input}");
                    bool rebuilt = RebuildGenericHelper(datFile, input, outDir, quickScan, date, inverse, outputFormat, asFile);

                    // If we are supposed to delete the file, do so
                    if (delete && rebuilt)
                        System.IO.File.Delete(input);
                }

                // If the input is a directory
                else if (Directory.Exists(input))
                {
                    _staticLogger.Verbose($"Checking directory: {input}");
#if NET20 || NET35
                    foreach (string file in Directory.GetFiles(input, "*"))
#else
                    foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
#endif
                    {
                        _staticLogger.User($"Checking file: {file}");
                        bool rebuilt = RebuildGenericHelper(datFile, file, outDir, quickScan, date, inverse, outputFormat, asFile);

                        // If we are supposed to delete the file, do so
                        if (delete && rebuilt)
                            System.IO.File.Delete(file);
                    }
                }
            }

            watch.Stop();

            #endregion

            return success;
        }

        /// <summary>
        /// Attempt to add a file to the output if it matches
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="asFile">TreatAsFile representing special format scanning</param>
        /// <returns>True if the file was used to rebuild, false otherwise</returns>
        private static bool RebuildGenericHelper(
            DatFile datFile,
            string file,
            string outDir,
            bool quickScan,
            bool date,
            bool inverse,
            OutputFormat outputFormat,
            TreatAsFile asFile)
        {
            // If we somehow have a null filename, return
            if (file == null)
                return false;

            // Set the deletion variables
            bool usedExternally = false, usedInternally = false;

            // Create an empty list of BaseFile for archive entries
            List<BaseFile>? entries = null;

            // Get the TGZ and TXZ status for later
            GZipArchive tgz = new(file);
            XZArchive txz = new(file);
            bool isSingleTorrent = tgz.IsStandardized() || txz.IsStandardized();

            // Get the base archive first
            BaseArchive? archive = FileTypeTool.CreateArchiveType(file);

            // Now get all extracted items from the archive
            HashType[] hashTypes = quickScan ? [HashType.CRC32] : [HashType.CRC32, HashType.MD5, HashType.SHA1];
            if (archive != null)
            {
                archive.SetHashTypes(hashTypes);
                entries = archive.GetChildren();
            }

            // If the entries list is null, we encountered an error or have a file and should scan externally
            if (entries == null && System.IO.File.Exists(file))
            {
                BaseFile? internalFileInfo = FileTypeTool.GetInfo(file, hashTypes);

                // Create the correct DatItem
                DatItem? internalDatItem;
                if (internalFileInfo == null)
                    internalDatItem = null;
#if NET20 || NET35
                else if (internalFileInfo is FileTypes.Aaru.AaruFormat && (asFile & TreatAsFile.AaruFormat) == 0)
#else
                else if (internalFileInfo is FileTypes.Aaru.AaruFormat && !asFile.HasFlag(TreatAsFile.AaruFormat))
#endif
                    internalDatItem = internalFileInfo.ConvertToMedia();
#if NET20 || NET35
                else if (internalFileInfo is FileTypes.CHD.CHDFile && (asFile & TreatAsFile.CHD) == 0)
#else
                else if (internalFileInfo is FileTypes.CHD.CHDFile && !asFile.HasFlag(TreatAsFile.CHD))
#endif
                    internalDatItem = internalFileInfo.ConvertToDisk();
                else
                    internalDatItem = internalFileInfo.ConvertToRom();

                if (internalDatItem != null)
                {
                    usedExternally = RebuildIndividualFile(datFile,
                        internalDatItem,
                        file,
                        outDir,
                        date,
                        inverse,
                        forceSorted: false,
                        outputFormat);
                }
            }
            // Otherwise, loop through the entries and try to match
            else if (entries != null)
            {
                foreach (BaseFile entry in entries)
                {
                    DatItem? internalDatItem = DatItemTool.CreateDatItem(entry);
                    if (internalDatItem == null)
                        continue;

                    usedInternally |= RebuildIndividualFile(datFile,
                        internalDatItem,
                        file,
                        outDir,
                        date,
                        inverse,
                        forceSorted: false,
                        outputFormat,
                        isZip: !isSingleTorrent);
                }
            }

            return usedExternally || usedInternally;
        }

        /// <summary>
        /// Find duplicates and rebuild individual files to output
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="forceSorted">True if the input DAT is already sorted, false to let the program decide</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="isZip">True if the input file is an archive, false if the file is TGZ/TXZ, null otherwise</param>
        /// <returns>True if the file was able to be rebuilt, false otherwise</returns>
        private static bool RebuildIndividualFile(
            DatFile datFile,
            DatItem datItem,
            string file,
            string outDir,
            bool date,
            bool inverse,
            bool forceSorted,
            OutputFormat outputFormat,
            bool? isZip = null)
        {
            // Set the initial output value
            bool rebuilt = false;

            // If the DatItem is a Disk or Media, force rebuilding to a folder except if TGZ or TXZ
            if ((datItem is Disk || datItem is Media)
                && !(outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba)
                && !(outputFormat == OutputFormat.TorrentXZ || outputFormat == OutputFormat.TorrentXZRomba))
            {
                outputFormat = OutputFormat.Folder;
            }

            // If we have a Disk, File, or Media, change it into a Rom for later use
            if (datItem is Disk disk)
                datItem = disk.ConvertToRom();
            else if (datItem is DatItems.Formats.File fileItem)
                datItem = fileItem.ConvertToRom();
            else if (datItem is Media media)
                datItem = media.ConvertToRom();

            // Prepopluate a key string
            string crc = (datItem as Rom)!.GetStringFieldValue(Models.Metadata.Rom.CRCKey) ?? string.Empty;

            // Try to get the stream for the file
            if (!GetFileStream(datItem, file, isZip, out Stream? fileStream) || fileStream == null)
                return false;

            // If either we have duplicates or we're filtering
            if (ShouldRebuild(datFile, datItem, fileStream, inverse, forceSorted, out List<DatItem> dupes))
            //if (ShouldRebuildDB(datFile, datItem, fileStream, inverse, forceSorted, out List<DatItem> dupes))
            {
                // If we have a very specific TGZ->TGZ case, just copy it accordingly
                if (RebuildTorrentGzip(datFile, datItem, file, outDir, outputFormat, isZip))
                    return true;

                // If we have a very specific TXZ->TXZ case, just copy it accordingly
                if (RebuildTorrentXz(datFile, datItem, file, outDir, outputFormat, isZip))
                    return true;

                // Create a temp file if we're compressing the data after or if there are multiple dupes
                string? tempFile = null;
                if (outputFormat != OutputFormat.Folder || dupes.Count > 1)
                {
                    tempFile = Path.Combine(outDir, $"tmp{System.Guid.NewGuid()}");
                    Stream tempStream = System.IO.File.Open(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    byte[] tempBuffer = new byte[4096 * 128];
                    int zlen;
                    while ((zlen = fileStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                    {
                        tempStream.Write(tempBuffer, 0, zlen);
                        tempStream.Flush();
                    }

                    fileStream.Dispose();
                    fileStream = tempStream;
                    fileStream.Seek(0, SeekOrigin.Begin);
                }

                _staticLogger.User($"{(inverse ? "No matches" : $"{dupes.Count} Matches")} found for '{Path.GetFileName(datItem.GetName() ?? datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue())}', rebuilding accordingly...");
                rebuilt = true;

                // Special case for partial packing mode
                bool shouldCheck = false;
                if (outputFormat == OutputFormat.Folder && datFile.Header.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey).AsPackingFlag() == PackingFlag.Partial)
                {
                    shouldCheck = true;
                    datFile.BucketBy(ItemKey.Machine, lower: false);
                }

                // Now loop through the list and rebuild accordingly
                foreach (DatItem item in dupes)
                {
                    // If we don't have a proper machine
                    var machine = item.GetMachine();
                    if (machine?.GetName() == null)
                        continue;

                    // If we should check for the items in the machine
                    var items = datFile.GetItemsForBucket(machine.GetName());
                    if (shouldCheck && items!.Count > 1)
                        outputFormat = OutputFormat.Folder;
                    else if (shouldCheck && items!.Count == 1)
                        outputFormat = OutputFormat.ParentFolder;

                    // Get the output archive, if possible
                    IParent? outputArchive = GetPreconfiguredFolder(datFile, date, outputFormat);

                    // Now rebuild to the output file
                    outputArchive!.Write(fileStream, outDir, (item as Rom)!.ConvertToBaseFile());
                }

                // Close the input stream
                fileStream.Dispose();

                // Delete the file if a temp file was created
                if (tempFile != null && System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }

            // Now we want to take care of headers, if applicable
            if (datFile.Header.GetStringFieldValue(Models.Metadata.Header.HeaderKey) != null)
            {
                // Check to see if we have a matching header first
                SkipperMatch.Init();
                Rule rule = SkipperMatch.GetMatchingRule(fileStream, Path.GetFileNameWithoutExtension(datFile.Header.GetStringFieldValue(Models.Metadata.Header.HeaderKey)!));

                // If there's a match, create the new file to write
                if (rule.Tests != null && rule.Tests.Length != 0)
                {
                    // If the file could be transformed correctly
                    MemoryStream transformStream = new();
                    if (rule.TransformStream(fileStream, transformStream, keepReadOpen: true, keepWriteOpen: true))
                    {
                        // Get the file informations that we will be using
                        HashType[] hashes = [HashType.CRC32, HashType.MD5, HashType.SHA1];
                        Rom headerless = FileTypeTool.GetInfo(transformStream, hashes).ConvertToRom();

                        // If we have duplicates and we're not filtering
                        if (ShouldRebuild(datFile, headerless, transformStream, inverse: false, forceSorted, out dupes))
                        //if (ShouldRebuildDB(datFile, headerless, transformStream, inverse: false, forceSorted, out dupes))
                        {
                            _staticLogger.User($"Headerless matches found for '{Path.GetFileName(datItem.GetName() ?? datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue())}', rebuilding accordingly...");
                            rebuilt = true;

                            // Now loop through the list and rebuild accordingly
                            foreach (DatItem item in dupes)
                            {
                                // Create a headered item to use as well
                                datItem.CopyMachineInformation(item);
                                datItem.SetName($"{datItem.GetName()}_{crc}");

                                // Get the output archive, if possible
                                IParent? outputArchive = GetPreconfiguredFolder(datFile, date, outputFormat);

                                // Now rebuild to the output file
                                bool eitherSuccess = false;
                                eitherSuccess |= outputArchive!.Write(transformStream, outDir, (item as Rom)!.ConvertToBaseFile());
                                eitherSuccess |= outputArchive.Write(fileStream, outDir, (datItem as Rom)!.ConvertToBaseFile());

                                // Now add the success of either rebuild
                                rebuilt &= eitherSuccess;
                            }
                        }
                    }

                    // Dispose of the stream
                    transformStream?.Dispose();
                }

                // Dispose of the stream
                fileStream?.Dispose();
            }

            return rebuilt;
        }

        /// <summary>
        /// Get the rebuild state for a given item
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="stream">Stream representing the input file</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="forceSorted">True if the input DAT is already sorted, false to let the program decide</param>
        /// <param name="dupes">Output list of duplicate items to rebuild to</param>
        /// <returns>True if the item should be rebuilt, false otherwise</returns>
        private static bool ShouldRebuild(DatFile datFile, DatItem datItem, Stream? stream, bool inverse, bool forceSorted, out List<DatItem> dupes)
        {
            // Find if the file has duplicates in the DAT
            dupes = datFile.GetDuplicates(datItem, forceSorted);
            bool hasDuplicates = dupes.Count > 0;

            // If we have duplicates but we're filtering
            if (hasDuplicates && inverse)
            {
                return false;
            }

            // If we have duplicates without filtering
            else if (hasDuplicates && !inverse)
            {
                return true;
            }

            // If we have no duplicates and we're filtering
            else if (!hasDuplicates && inverse)
            {
                string? machinename = null;

                // Get the item from the current file
                HashType[] hashes = [HashType.CRC32, HashType.MD5, HashType.SHA1];
                Rom item = FileTypeTool.GetInfo(stream, hashes).ConvertToRom();
                item.GetMachine()!.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, Path.GetFileNameWithoutExtension(item.GetName()));
                item.GetMachine()!.SetName(Path.GetFileNameWithoutExtension(item.GetName()));

                // If we are coming from an archive, set the correct machine name
                if (machinename != null)
                {
                    item.GetMachine()!.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, machinename);
                    item.GetMachine()!.SetName(machinename);
                }

                dupes.Add(item);
                return true;
            }

            // If we have no duplicates and we're not filtering
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the rebuild state for a given item
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="stream">Stream representing the input file</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="forceSorted">True if the input DAT is already sorted, false to let the program decide</param>
        /// <param name="dupes">Output list of duplicate items to rebuild to</param>
        /// <returns>True if the item should be rebuilt, false otherwise</returns>
        private static bool ShouldRebuildDB(DatFile datFile, KeyValuePair<long, DatItem> datItem, Stream? stream, bool inverse, bool forceSorted, out Dictionary<long, DatItem> dupes)
        {
            // Find if the file has duplicates in the DAT
            dupes = datFile.GetDuplicatesDB(datItem, forceSorted);
            bool hasDuplicates = dupes.Count > 0;

            // If we have duplicates but we're filtering
            if (hasDuplicates && inverse)
            {
                return false;
            }

            // If we have duplicates without filtering
            else if (hasDuplicates && !inverse)
            {
                return true;
            }

            // TODO: Figure out how getting a set of duplicates works with IDDB

            // If we have no duplicates and we're filtering
            else if (!hasDuplicates && inverse)
            {
                string? machinename = null;

                // Get the item from the current file
                HashType[] hashes = [HashType.CRC32, HashType.MD5, HashType.SHA1];
                Rom item = FileTypeTool.GetInfo(stream, hashes).ConvertToRom();

                // Create a machine for the current item
                var machine = new Machine();
                machine.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, Path.GetFileNameWithoutExtension(item.GetName()));
                machine.SetName(Path.GetFileNameWithoutExtension(item.GetName()));
                long machineIndex = datFile.AddMachineDB(machine);

                // If we are coming from an archive, set the correct machine name
                if (machinename != null)
                {
                    machine.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, machinename);
                    machine.SetName(machinename);
                }

                long index = datFile.AddItemDB(item, machineIndex, -1, false);
                dupes[index] = item;
                return true;
            }

            // If we have no duplicates and we're not filtering
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rebuild from TorrentGzip to TorrentGzip
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="isZip">True if the input file is an archive, false if the file is TGZ, null otherwise</param>
        /// <returns>True if rebuilt properly, false otherwise</returns>
        private static bool RebuildTorrentGzip(DatFile datFile, DatItem datItem, string file, string outDir, OutputFormat outputFormat, bool? isZip)
        {
            // If we have a very specific TGZ->TGZ case, just copy it accordingly
            GZipArchive tgz = new(file);
            BaseFile? tgzRom = tgz.GetTorrentGZFileInfo();
            if (isZip == false && tgzRom != null && (outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba))
            {
                _staticLogger.User($"Matches found for '{Path.GetFileName(datItem.GetName() ?? string.Empty)}', rebuilding accordingly...");

                // Get the proper output path
                string sha1 = (datItem as Rom)!.GetStringFieldValue(Models.Metadata.Rom.SHA1Key) ?? string.Empty;
                if (outputFormat == OutputFormat.TorrentGzipRomba)
                    outDir = Path.Combine(outDir, Utilities.GetDepotPath(sha1, datFile.Modifiers.OutputDepot?.Depth ?? 0) ?? string.Empty);
                else
                    outDir = Path.Combine(outDir, sha1 + ".gz");

                // Make sure the output folder is created
                string? dir = Path.GetDirectoryName(outDir);
                if (dir != null)
                    Directory.CreateDirectory(dir);

                // Now copy the file over
                try
                {
                    System.IO.File.Copy(file, outDir);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Rebuild from TorrentXz to TorrentXz
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="isZip">True if the input file is an archive, false if the file is TXZ, null otherwise</param>
        /// <returns>True if rebuilt properly, false otherwise</returns>
        private static bool RebuildTorrentXz(DatFile datFile, DatItem datItem, string file, string outDir, OutputFormat outputFormat, bool? isZip)
        {
            // If we have a very specific TGZ->TGZ case, just copy it accordingly
            XZArchive txz = new(file);
            BaseFile? txzRom = txz.GetTorrentXZFileInfo();
            if (isZip == false && txzRom != null && (outputFormat == OutputFormat.TorrentXZ || outputFormat == OutputFormat.TorrentXZRomba))
            {
                _staticLogger.User($"Matches found for '{Path.GetFileName(datItem.GetName() ?? string.Empty)}', rebuilding accordingly...");

                // Get the proper output path
                string sha1 = (datItem as Rom)!.GetStringFieldValue(Models.Metadata.Rom.SHA1Key) ?? string.Empty;
                if (outputFormat == OutputFormat.TorrentXZRomba)
                    outDir = Path.Combine(outDir, Utilities.GetDepotPath(sha1, datFile.Modifiers.OutputDepot?.Depth ?? 0) ?? string.Empty).Replace(".gz", ".xz");
                else
                    outDir = Path.Combine(outDir, sha1 + ".xz");

                // Make sure the output folder is created
                string? dir = Path.GetDirectoryName(outDir);
                if (dir != null)
                    Directory.CreateDirectory(dir);

                // Now copy the file over
                try
                {
                    System.IO.File.Copy(file, outDir);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the Stream related to a file
        /// </summary>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="isZip">Non-null if the input file is an archive</param>
        /// <param name="stream">Output stream representing the opened file</param>
        /// <returns>True if the stream opening succeeded, false otherwise</returns>
        private static bool GetFileStream(DatItem datItem, string file, bool? isZip, out Stream? stream)
        {
            // Get a generic stream for the file
            stream = null;

            // If we have a zipfile, extract the stream to memory
            if (isZip != null)
            {
                BaseArchive? archive = FileTypeTool.CreateArchiveType(file);
                if (archive == null)
                    return false;

                try
                {
                    ItemType itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType();
                    (stream, _) = archive.GetEntryStream(datItem.GetName() ?? itemType.AsStringValue() ?? string.Empty);
                }
                catch
                {
                    // Ignore the exception for now -- usually an over-large file
                    stream = null;
                    return false;
                }
            }
            // Otherwise, just open the filestream
            else
            {
                stream = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            // If the stream is null, then continue
            if (stream == null)
                return false;

            // Seek to the beginning of the stream
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            return true;
        }

        /// <summary>
        /// Get the default OutputFormat associated with each PackingFlag
        /// </summary>
        private static OutputFormat GetOutputFormat(PackingFlag packing)
        {
            return packing switch
            {
                PackingFlag.Zip => OutputFormat.TorrentZip,
                PackingFlag.Unzip => OutputFormat.Folder,
                PackingFlag.Partial => OutputFormat.Folder,
                PackingFlag.Flat => OutputFormat.ParentFolder,
                PackingFlag.FileOnly => OutputFormat.Folder,
                PackingFlag.None => OutputFormat.Folder,
                _ => OutputFormat.Folder,
            };
        }

        /// <summary>
        /// Get preconfigured Folder for rebuilding
        /// </summary>
        /// <param name="datFile">Current DatFile object to rebuild from</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <returns>Folder configured with proper flags</returns>
        private static IParent? GetPreconfiguredFolder(DatFile datFile, bool date, OutputFormat outputFormat)
        {
            IParent? outputArchive = FileTypeTool.CreateFolderType(outputFormat);
            if (outputArchive is BaseArchive baseArchive && date)
                baseArchive.SetRealDates(date);

            // Set the depth fields where appropriate
            if (outputArchive is GZipArchive gzipArchive)
                gzipArchive.Depth = datFile.Modifiers.OutputDepot?.Depth ?? 0;
            else if (outputArchive is XZArchive xzArchive)
                xzArchive.Depth = datFile.Modifiers.OutputDepot?.Depth ?? 0;

            return outputArchive;
        }

        /// <summary>
        /// Get string value from input OutputFormat
        /// </summary>
        /// <param name="itemType">OutputFormat to get value from</param>
        /// <returns>String value corresponding to the OutputFormat</returns>
        private static string? FromOutputFormat(OutputFormat itemType)
        {
            return itemType switch
            {
                OutputFormat.Folder => "directory",
                OutputFormat.ParentFolder => "directory",
                OutputFormat.TapeArchive => "TAR",
                OutputFormat.Torrent7Zip => "Torrent7Z",
                OutputFormat.TorrentGzip => "TorrentGZ",
                OutputFormat.TorrentGzipRomba => "TorrentGZ",
                OutputFormat.TorrentRar => "TorrentRAR",
                OutputFormat.TorrentXZ => "TorrentXZ",
                OutputFormat.TorrentXZRomba => "TorrentXZ",
                OutputFormat.TorrentZip => "TorrentZip",
                _ => null,
            };
        }
    }
}