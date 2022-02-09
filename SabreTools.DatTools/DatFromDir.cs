using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SabreTools.Core;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Archives;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// This file represents all methods related to populating a DatFile
    /// from a set of files and directories
    /// </summary>
    public class DatFromDir
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Create a new Dat from a directory
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="hashes">Hashes to include in the information</param>
        public static bool PopulateFromDir(
            DatFile datFile,
            string basePath,
            TreatAsFile asFiles = 0x00,
            bool addAsFiles = false,
            SkipFileType skipFileType = SkipFileType.None,
            bool addBlanks = false,
            Hash hashes = Hash.Standard)
        {
            // Set the progress variables
            long totalSize = 0;
            long currentSize = 0;

            InternalStopwatch watch = new InternalStopwatch($"Populating DAT from {basePath}");

            // Process the input
            if (Directory.Exists(basePath))
            {
                logger.Verbose($"Folder found: {basePath}");

                // Get a list of all files to process
                List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();

                // Loop through and add the file sizes
                Parallel.ForEach(files, Globals.ParallelOptions, item =>
                {
                    Interlocked.Add(ref totalSize, new FileInfo(item).Length);
                });

                // Process the files in the main folder or any subfolder
                logger.User(totalSize, currentSize);
                foreach (string item in files)
                {
                    CheckFileForHashes(datFile, item, basePath, asFiles, addAsFiles, skipFileType, addBlanks, hashes);
                    currentSize += new FileInfo(item).Length;
                    logger.User(totalSize, currentSize, item);
                }

                // Now find all folders that are empty, if we are supposed to
                if (addBlanks)
                    ProcessDirectoryBlanks(datFile, basePath);
            }
            else if (File.Exists(basePath))
            {
                logger.Verbose($"File found: {basePath}");

                totalSize = new FileInfo(basePath).Length;
                logger.User(totalSize, currentSize);

                string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(basePath));
                CheckFileForHashes(datFile, basePath, parentPath, asFiles, addAsFiles, skipFileType, addBlanks, hashes);
                logger.User(totalSize, totalSize, basePath);
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
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="hashes">Hashes to include in the information</param>
        private static void CheckFileForHashes(
            DatFile datFile,
            string item,
            string basePath,
            TreatAsFile asFiles,
            bool addAsFiles,
            SkipFileType skipFileType,
            bool addBlanks,
            Hash hashes)
        {
            // If we're in depot mode, process it separately
            if (CheckDepotFile(datFile, item))
                return;

            // Initialize possible archive variables
            BaseArchive archive = BaseArchive.Create(item);

            // Process archives according to flags
            if (archive != null)
            {
                // Set the archive flags
                archive.AvailableHashes = hashes;

                // Skip if we're treating archives as files and skipping files
                if (asFiles.HasFlag(TreatAsFile.Archive) && skipFileType == SkipFileType.File)
                {
                    return;
                }

                // Skip if we're skipping archives
                else if (skipFileType == SkipFileType.Archive)
                {
                    return;
                }

                // Process as archive if we're not treating archives as files
                else if (!asFiles.HasFlag(TreatAsFile.Archive) || addAsFiles)
                {
                    var extracted = archive.GetChildren();

                    // If we have internal items to process, do so
                    if (extracted != null)
                        ProcessArchive(datFile, item, basePath, extracted);

                    // Now find all folders that are empty, if we are supposed to
                    if (addBlanks)
                        ProcessArchiveBlanks(datFile, item, basePath, archive);
                }

                // Process as file if we're treating archives as files
                if (asFiles.HasFlag(TreatAsFile.Archive) || addAsFiles)
                    ProcessFile(datFile, item, basePath, hashes, asFiles, addAsFiles);
            }

            // Process non-archives according to flags
            else
            {
                // Skip if we're skipping files
                if (skipFileType == SkipFileType.File)
                    return;

                // Process as file
                else
                    ProcessFile(datFile, item, basePath, hashes, asFiles, addAsFiles);
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
            if (datFile.Header.OutputDepot?.IsActive != true)
                return false;

            // Check the file as if it were in a depot
            GZipArchive gzarc = new GZipArchive(item);
            BaseFile baseFile = gzarc.GetTorrentGZFileInfo();

            // If the rom is valid, add it
            if (baseFile != null && baseFile.Filename != null)
            {
                // Add the list if it doesn't exist already
                Rom rom = new Rom(baseFile);
                datFile.Items.Add(rom.GetKey(ItemKey.CRC), rom);
                logger.Verbose($"File added: {Path.GetFileNameWithoutExtension(item)}");
            }
            else
            {
                logger.Verbose($"File not added: {Path.GetFileNameWithoutExtension(item)}");
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
        private static void ProcessArchive(DatFile datFile, string item, string basePath, List<BaseFile> extracted)
        {
            // Get the parent path for all items
            string parent = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item);

            // First take care of the found items
            Parallel.ForEach(extracted, Globals.ParallelOptions, baseFile =>
            {
                DatItem datItem = DatItem.Create(baseFile);
                ProcessFileHelper(datFile, item, datItem, basePath, parent);
            });
        }

        /// <summary>
        /// Process blank folders in an archive
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File containing the blanks</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="archive">BaseArchive to get blanks from</param>
        private static void ProcessArchiveBlanks(DatFile datFile, string item, string basePath, BaseArchive archive)
        {
            List<string> empties = new List<string>();

            // Get the parent path for all items
            string parent = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item);

            // Now get all blank folders from the archive
            if (archive != null)
                empties = archive.GetEmptyFolders();

            // Add add all of the found empties to the DAT
            Parallel.ForEach(empties, Globals.ParallelOptions, empty =>
            {
                Rom emptyRom = new Rom(Path.Combine(empty, "_"), item);
                ProcessFileHelper(datFile, item, emptyRom, basePath, parent);
            });
        }

        /// <summary>
        /// Process blank folders in a directory
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        private static void ProcessDirectoryBlanks(DatFile datFile, string basePath)
        {
            // If we're in depot mode, we don't process blanks
            if (datFile.Header.OutputDepot?.IsActive == true)
                return;

            List<string> empties = basePath.ListEmpty();
            Parallel.ForEach(empties, Globals.ParallelOptions, dir =>
            {
                // Get the full path for the directory
                string fulldir = Path.GetFullPath(dir);

                // Set the temporary variables
                string gamename = string.Empty;
                string romname = string.Empty;

                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (datFile.Header.Type == "SuperDAT")
                {
                    gamename = fulldir.Remove(0, basePath.Length + 1);
                    romname = "_";
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
                    romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
                }

                // Sanitize the names
                gamename = gamename.Trim(Path.DirectorySeparatorChar);
                romname = romname.Trim(Path.DirectorySeparatorChar);

                logger.Verbose($"Adding blank empty folder: {gamename}");
                datFile.Items["null"].Add(new Rom(romname, gamename));
            });
        }

        /// <summary>
        /// Process a single file as a file
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File to be added</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="hashes">Hashes to include in the information</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="addAsFiles">TreatAsFiles representing CHD and Archive and as whole file scanning</param>
        private static void ProcessFile(DatFile datFile, string item, string basePath, Hash hashes, TreatAsFile asFiles, bool addAsFiles)
        {
            logger.Verbose($"'{Path.GetFileName(item)}' treated like a file");
            BaseFile baseFile = BaseFile.GetInfo(item, header: datFile.Header.HeaderSkipper, hashes: hashes, asFiles: asFiles);
            DatItem datItem = DatItem.Create(baseFile);
            
            // Get the parent path for all items
            string parent = string.Empty;
            if (addAsFiles)
                parent = (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item);

            ProcessFileHelper(datFile, item, datItem, basePath, parent);
        }


        /// <summary>
        /// Process a single file as a file (with found Rom data)
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="item">File to be added</param>
        /// <param name="item">Rom data to be used to write to file</param>
        /// <param name="basepath">Path the represents the parent directory</param>
        /// <param name="parent">Parent game to be used</param>
        private static void ProcessFileHelper(DatFile datFile, string item, DatItem datItem, string basepath, string parent)
        {
            // If we didn't get an accepted parsed type somehow, cancel out
            List<ItemType> parsed = new List<ItemType> { ItemType.Disk, ItemType.Media, ItemType.Rom };
            if (!parsed.Contains(datItem.ItemType))
                return;

            try
            {
                // If the basepath doesn't end with a directory separator, add it
                if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basepath += Path.DirectorySeparatorChar.ToString();

                // Make sure we have the full item path
                item = Path.GetFullPath(item);

                // Process the item to sanitize names based on input
                SetDatItemInfo(datFile, datItem, item, parent, basepath);

                // Add the file information to the DAT
                string key = datItem.GetKey(ItemKey.CRC);
                datFile.Items.Add(key, datItem);

                logger.Verbose($"File added: {datItem.GetName() ?? string.Empty}");
            }
            catch (IOException ex)
            {
                logger.Error(ex);
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
        private static void SetDatItemInfo(DatFile datFile, DatItem datItem, string item, string parent, string basepath)
        {
            // Get the data to be added as game and item names
            string machineName, itemName;

            // If the parent is blank, then we have a non-archive file
            if (string.IsNullOrWhiteSpace(parent))
            {
                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (datFile.Header.Type == "SuperDAT")
                {
                    machineName = Path.GetDirectoryName(item.Remove(0, basepath.Length));
                    itemName = Path.GetFileName(item);
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    machineName = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
                    itemName = item.Remove(0, (Path.Combine(basepath, machineName).Length));
                }
            }

            // Otherwise, we assume that we have an archive
            else
            {
                // If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
                if (datFile.Header.Type == "SuperDAT")
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
            machineName = machineName.Trim(Path.DirectorySeparatorChar);
            itemName = itemName?.Trim(Path.DirectorySeparatorChar) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(machineName) && string.IsNullOrWhiteSpace(itemName))
            {
                itemName = machineName;
                machineName = "Default";
            }

            // Update machine information
            datItem.Machine.Name = machineName;
            datItem.Machine.Description = machineName;

            // If we have a Disk, then the ".chd" extension needs to be removed
            if (datItem.ItemType == ItemType.Disk && itemName.EndsWith(".chd"))
            {
                itemName = itemName[0..^4];
            }

            // If we have a Media, then the extension needs to be removed
            else if (datItem.ItemType == ItemType.Media)
            {
                if (itemName.EndsWith(".dicf"))
                    itemName = itemName[0..^5];
                else if (itemName.EndsWith(".aaru"))
                    itemName = itemName[0..^5];
                else if (itemName.EndsWith(".aaruformat"))
                    itemName = itemName[0..^11];
                else if (itemName.EndsWith(".aaruf"))
                    itemName = itemName[0..^6];
                else if (itemName.EndsWith(".aif"))
                    itemName = itemName[0..^4];
            }

            // Set the item name back
            datItem.SetName(itemName);
        }
    }
}
