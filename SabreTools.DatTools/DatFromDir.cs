using System;
using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Archives;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// This file represents all methods related to populating a DatFile
    /// from a set of files and directories
    /// </summary>
    public class DatFromDir
    {
        #region Fields

        /// <summary>
        /// Hashes to include in the information
        /// </summary>
        private readonly HashType[] _hashes;

        /// <summary>
        /// Type of files that should be skipped
        /// </summary>
        private readonly SkipFileType _skipFileType;

        /// <summary>
        /// TreatAsFile representing CHD and Archive scanning</param>
        /// </summary>
        private readonly TreatAsFile _treatAsFile;

        /// <summary>
        /// Indicates if blank items should be created for empty folders
        /// </summary>
        private readonly bool _addBlanks;

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        #region Constructors

        public DatFromDir(HashType[] hashes, SkipFileType skipFileType, TreatAsFile treatAsFile, bool addBlanks)
        {
            if (hashes.Length == 0)
                throw new ArgumentException($"{nameof(hashes)} must contain at least one valid HashType");

            _hashes = hashes;
            _skipFileType = skipFileType;
            _treatAsFile = treatAsFile;
            _addBlanks = addBlanks;
        }

        #endregion

        /// <summary>
        /// Create a new Dat from a directory
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        public bool PopulateFromDir(DatFile datFile, string basePath)
        {
            InternalStopwatch watch = new($"Populating DAT from {basePath}");

            // Process the input
            if (Directory.Exists(basePath))
            {
                _staticLogger.Verbose($"Folder found: {basePath}");

                // Get a list of all files to process
                string[] files = IOExtensions.SafeGetFiles(basePath, "*", SearchOption.AllDirectories);

                // Set intiial progress values
                long totalCount = files.Length;
                long currentCount = 0;
                _staticLogger.User(totalCount, currentCount);

                // Process the files in the main folder or any subfolder
                foreach (string item in files)
                {
                    currentCount++;
                    CheckFileForHashes(datFile, item, basePath);

                    _staticLogger.User(totalCount, currentCount, item);
                }

                // Now find all folders that are empty, if we are supposed to
                if (_addBlanks)
                    ProcessDirectoryBlanks(datFile, basePath);
            }
            else if (System.IO.File.Exists(basePath))
            {
                _staticLogger.Verbose($"File found: {basePath}");

                // Set intiial progress values
                long totalCount = 1;
                _staticLogger.User(totalCount, 0);

                // Process the input file
                string? parentPath = Path.GetDirectoryName(Path.GetDirectoryName(basePath));
                CheckFileForHashes(datFile, basePath, parentPath);

                _staticLogger.User(totalCount, totalCount, basePath);
            }
            else
            {
                _staticLogger.Verbose($"Invalid path found: {basePath}");
            }

            watch.Stop();
            return true;
        }

        /// <summary>
        /// Check a given file for hashes, based on current settings
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">Filename of the item to be checked</param>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="asFile">TreatAsFile representing CHD and Archive scanning</param>
        private void CheckFileForHashes(DatFile datFile, string item, string? basePath)
        {
            // If we're in depot mode, process it separately
            if (CheckDepotFile(datFile, item))
                return;

            // Initialize possible archive variables
            BaseArchive? archive = FileTypeTool.CreateArchiveType(item);

            // Process archives according to flags
            if (archive != null)
            {
                // Set the archive flags
                archive.SetHashTypes(_hashes);

                // Skip if we're treating archives as files and skipping files
#if NET20 || NET35
                if ((_treatAsFile & TreatAsFile.Archive) != 0 && _skipFileType == SkipFileType.File)
#else
                if (_treatAsFile.HasFlag(TreatAsFile.Archive) && _skipFileType == SkipFileType.File)
#endif
                {
                    return;
                }

                // Skip if we're skipping archives
                else if (_skipFileType == SkipFileType.Archive)
                {
                    return;
                }

                // Process as archive if we're not treating archives as files
#if NET20 || NET35
                else if ((_treatAsFile & TreatAsFile.Archive) == 0)
#else
                else if (!_treatAsFile.HasFlag(TreatAsFile.Archive))
#endif
                {
                    var extracted = archive.GetChildren();

                    // If we have internal items to process, do so
                    if (extracted != null)
                        ProcessArchive(datFile, item, basePath, extracted);

                    // Now find all folders that are empty, if we are supposed to
                    if (_addBlanks)
                        ProcessArchiveBlanks(datFile, item, basePath, archive);
                }

                // Process as file if we're treating archives as files
                else
                {
                    ProcessFile(datFile, item, basePath);
                }
            }

            // Process non-archives according to flags
            else
            {
                // Skip if we're skipping files
                if (_skipFileType == SkipFileType.File)
                    return;

                // Process as file
                else
                    ProcessFile(datFile, item, basePath);
            }
        }

        /// <summary>
        /// Check an item as if it's supposed to be in a depot
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">Filename of the item to be checked</param>
        /// <returns>True if we checked a depot file, false otherwise</returns>
        private static bool CheckDepotFile(DatFile datFile, string item)
        {
            // If we're not in Depot mode, return false
            if (datFile.Modifiers.OutputDepot?.IsActive != true)
                return false;

            // Check the file as if it were in a depot
            GZipArchive gzarc = new(item);
            BaseFile? baseFile = gzarc.GetTorrentGZFileInfo();

            // If the rom is valid, add it
            if (baseFile != null && baseFile.Filename != null)
            {
                // Add the list if it doesn't exist already
                Rom rom = baseFile.ConvertToRom();
                datFile.AddItem(rom, statsOnly: false);
                _staticLogger.Verbose($"File added: {Path.GetFileNameWithoutExtension(item)}");
            }
            else
            {
                _staticLogger.Verbose($"File not added: {Path.GetFileNameWithoutExtension(item)}");
                return true;
            }

            return true;
        }

        /// <summary>
        /// Process a single file as an archive
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File to be added</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="extracted">List of BaseFiles representing the internal files</param>
        private static void ProcessArchive(DatFile datFile, string item, string? basePath, List<BaseFile> extracted)
        {
            // Get the parent path for all items
            string parent = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath?.Length ?? 0) + Path.GetFileNameWithoutExtension(item);

            // First take care of the found items
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(extracted, Core.Globals.ParallelOptions, baseFile =>
#elif NET40_OR_GREATER
            Parallel.ForEach(extracted, baseFile =>
#else
            foreach (var baseFile in extracted)
#endif
            {
                DatItem? datItem = DatItemTool.CreateDatItem(baseFile);
                if (datItem == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                ProcessFileHelper(datFile, item, datItem, basePath, parent);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Process blank folders in an archive
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File containing the blanks</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="archive">BaseArchive to get blanks from</param>
        private static void ProcessArchiveBlanks(DatFile datFile, string item, string? basePath, BaseArchive archive)
        {
            List<string> empties = [];

            // Get the parent path for all items
            string parent = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath?.Length ?? 0) + Path.GetFileNameWithoutExtension(item);

            // Now get all blank folders from the archive
            if (archive != null)
                empties = archive.GetEmptyFolders();

            // Add add all of the found empties to the DAT
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(empties, Core.Globals.ParallelOptions, empty =>
#elif NET40_OR_GREATER
            Parallel.ForEach(empties, empty =>
#else
            foreach (var empty in empties)
#endif
            {
                var emptyMachine = new Machine();
                emptyMachine.SetName(item);

                var emptyRom = new Rom();
                emptyRom.SetName(Path.Combine(empty, "_"));
                emptyRom.SetFieldValue<Machine?>(DatItem.MachineKey, emptyMachine);

                ProcessFileHelper(datFile, item, emptyRom, basePath, parent);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Process blank folders in a directory
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        private static void ProcessDirectoryBlanks(DatFile datFile, string? basePath)
        {
            // If we're in depot mode, we don't process blanks
            if (datFile.Modifiers.OutputDepot?.IsActive == true)
                return;

            List<string> empties = basePath.ListEmpty() ?? [];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(empties, Core.Globals.ParallelOptions, dir =>
#elif NET40_OR_GREATER
            Parallel.ForEach(empties, dir =>
#else
            foreach (var dir in empties)
#endif
            {
                // Get the full path for the directory
                string fulldir = Path.GetFullPath(dir);

                // Set the temporary variables
                string gamename = string.Empty;
                string romname = string.Empty;

                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey) == "SuperDAT")
                {
                    if (basePath != null)
                        gamename = fulldir.Remove(0, basePath.Length + 1);
                    else
                        gamename = fulldir;

                    romname = "_";
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    if (basePath != null)
                    {
                        gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
                        romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
                    }
                    else
                    {
                        gamename = fulldir;
                        romname = Path.Combine(fulldir, "_");
                    }
                }

                // Sanitize the names
                gamename = gamename.Trim(Path.DirectorySeparatorChar);
                romname = romname.Trim(Path.DirectorySeparatorChar);

                _staticLogger.Verbose($"Adding blank empty folder: {gamename}");

                var blankMachine = new Machine();
                blankMachine.SetName(gamename);

                var blankRom = new Blank();
                blankRom.SetName(romname);
                blankRom.SetFieldValue<Machine?>(DatItem.MachineKey, blankMachine);

                datFile.AddItem(blankRom, statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Process a single file as a file
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File to be added</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="asFile">TreatAsFile representing CHD and Archive scanning</param>
        private void ProcessFile(DatFile datFile, string item, string? basePath)
        {
            _staticLogger.Verbose($"'{Path.GetFileName(item)}' treated like a file");
            var header = datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey);
            BaseFile? baseFile = FileTypeTool.GetInfo(item, _hashes, header);
            DatItem? datItem = DatItemTool.CreateDatItem(baseFile, _treatAsFile);
            if (datItem != null)
                ProcessFileHelper(datFile, item, datItem, basePath, string.Empty);
        }

        /// <summary>
        /// Process a single file as a file (with found Rom data)
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File to be added</param>
        /// <param name="item">Rom data to be used to write to file</param>
        /// <param name="basepath">Path the represents the parent directory</param>
        /// <param name="parent">Parent game to be used</param>
        private static void ProcessFileHelper(DatFile datFile, string item, DatItem datItem, string? basepath, string parent)
        {
            // If we didn't get an accepted parsed type somehow, cancel out
            List<ItemType> parsed = [ItemType.Disk, ItemType.File, ItemType.Media, ItemType.Rom];
            if (!parsed.Contains(datItem.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType()))
                return;

            try
            {
                // If the basepath doesn't end with a directory separator, add it
                if (basepath != null && !basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basepath += Path.DirectorySeparatorChar.ToString();

                // Make sure we have the full item path
                item = Path.GetFullPath(item);

                // Process the item to sanitize names based on input
                SetDatItemInfo(datFile, datItem, item, parent, basepath);

                // Add the file information to the DAT
                datFile.AddItem(datItem, statsOnly: false);

                _staticLogger.Verbose($"File added: {datItem.GetName() ?? string.Empty}");
            }
            catch (IOException ex)
            {
                _staticLogger.Error(ex);
                return;
            }
        }

        /// <summary>
        /// Set proper Game and Rom names from user inputs
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="datItem">DatItem representing the input file</param>
        /// <param name="item">Item name to use</param>
        /// <param name="parent">Parent name to use</param>
        /// <param name="basepath">Base path to use</param>
        private static void SetDatItemInfo(DatFile datFile, DatItem datItem, string item, string parent, string? basepath)
        {
            // Get the data to be added as game and item names
            string? machineName, itemName;

            // If the parent is blank, then we have a non-archive file
            if (string.IsNullOrEmpty(parent))
            {
                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey) == "SuperDAT")
                {
                    machineName = Path.GetDirectoryName(item.Remove(0, basepath?.Length ?? 0));
                    itemName = Path.GetFileName(item);
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    machineName = item.Remove(0, basepath?.Length ?? 0).Split(Path.DirectorySeparatorChar)[0];
                    if (basepath != null)
                        itemName = item.Remove(0, (Path.Combine(basepath, machineName).Length));
                    else
                        itemName = item.Remove(0, (machineName.Length));
                }
            }

            // Otherwise, we assume that we have an archive
            else
            {
                // If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
                if (datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey) == "SuperDAT")
                {
                    machineName = parent;
                    itemName = datItem.GetName();
                }

                // Otherwise, we want the archive name as the game, and the file as everything else
                else
                {
                    machineName = parent;
                    itemName = datItem.GetName();
                }
            }

            // Sanitize the names
            machineName = machineName?.Trim(Path.DirectorySeparatorChar);
            itemName = itemName?.Trim(Path.DirectorySeparatorChar) ?? string.Empty;

            if (!string.IsNullOrEmpty(machineName) && string.IsNullOrEmpty(itemName))
            {
                itemName = machineName;
                machineName = "Default";
            }

            // Update machine information
            datItem.GetMachine()!.SetFieldValue<string?>(Data.Models.Metadata.Machine.DescriptionKey, machineName);
            datItem.GetMachine()!.SetName(machineName);

            // If we have a Disk, then the ".chd" extension needs to be removed
            if (datItem is Disk && itemName!.EndsWith(".chd"))
            {
                itemName = itemName.Substring(0, itemName.Length - 4);
            }

            // If we have a Media, then the extension needs to be removed
            else if (datItem is Media)
            {
                if (itemName!.EndsWith(".dicf"))
                    itemName = itemName.Substring(0, itemName.Length - 5);
                else if (itemName.EndsWith(".aaru"))
                    itemName = itemName.Substring(0, itemName.Length - 5);
                else if (itemName.EndsWith(".aaruformat"))
                    itemName = itemName.Substring(0, itemName.Length - 11);
                else if (itemName.EndsWith(".aaruf"))
                    itemName = itemName.Substring(0, itemName.Length - 6);
                else if (itemName.EndsWith(".aif"))
                    itemName = itemName.Substring(0, itemName.Length - 4);
            }

            // Set the item name back
            datItem.SetName(itemName);
        }
    }
}
