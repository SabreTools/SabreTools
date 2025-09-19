using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatFiles.Formats;
using SabreTools.DatItems;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;
using SabreTools.Reports;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for parsing into DatFiles
    /// </summary>
    public static class Parser
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        #region DatFile

        /// <summary>
        /// Create a generic DatFile to be used
        /// </summary>
        /// <returns>Empty, default DatFile implementation</returns>
        public static DatFile CreateDatFile() => CreateDatFile(DatFormat.Logiqx, baseDat: null);

        /// <summary>
        /// Create a specific type of DatFile to be used based on a format and a base DAT
        /// </summary>
        /// <param name="datFormat">Format of the DAT to be created</param>
        /// <param name="baseDat">DatFile containing the information to use in specific operations</param>
        /// <returns>DatFile of the specific internal type that corresponds to the inputs</returns>
        public static DatFile CreateDatFile(DatFormat datFormat, DatFile? baseDat)
        {
            return datFormat switch
            {
                DatFormat.ArchiveDotOrg => new ArchiveDotOrg(baseDat),
                DatFormat.AttractMode => new AttractMode(baseDat),
                DatFormat.ClrMamePro => new ClrMamePro(baseDat),
                DatFormat.CSV => new CommaSeparatedValue(baseDat),
                DatFormat.DOSCenter => new DosCenter(baseDat),
                DatFormat.EverdriveSMDB => new EverdriveSMDB(baseDat),
                DatFormat.Listrom => new Listrom(baseDat),
                DatFormat.Listxml => new Listxml(baseDat),
                DatFormat.Logiqx => new Logiqx(baseDat, false),
                DatFormat.LogiqxDeprecated => new Logiqx(baseDat, true),
                DatFormat.MissFile => new Missfile(baseDat),
                DatFormat.OfflineList => new OfflineList(baseDat),
                DatFormat.OpenMSX => new OpenMSX(baseDat),
                DatFormat.RedumpMD2 => new Md2File(baseDat),
                DatFormat.RedumpMD4 => new Md4File(baseDat),
                DatFormat.RedumpMD5 => new Md5File(baseDat),
                DatFormat.RedumpRIPEMD128 => new RipeMD128File(baseDat),
                DatFormat.RedumpRIPEMD160 => new RipeMD160File(baseDat),
                DatFormat.RedumpSFV => new SfvFile(baseDat),
                DatFormat.RedumpSHA1 => new Sha1File(baseDat),
                DatFormat.RedumpSHA256 => new Sha256File(baseDat),
                DatFormat.RedumpSHA384 => new Sha384File(baseDat),
                DatFormat.RedumpSHA512 => new Sha512File(baseDat),
                DatFormat.RedumpSpamSum => new SpamSumFile(baseDat),
                DatFormat.RomCenter => new RomCenter(baseDat),
                DatFormat.SabreJSON => new SabreJSON(baseDat),
                DatFormat.SabreXML => new SabreXML(baseDat),
                DatFormat.SoftwareList => new SoftwareList(baseDat),
                DatFormat.SSV => new SemicolonSeparatedValue(baseDat),
                DatFormat.TSV => new TabSeparatedValue(baseDat),

                // We use new-style Logiqx as a backup for generic DatFile
                _ => new Logiqx(baseDat, false),
            };
        }

        /// <summary>
        /// Create a new DatFile from an existing DatHeader
        /// </summary>
        /// <param name="datHeader">DatHeader to get the values from</param>
        /// <param name="datModifiers">DatModifiers to get the values from</param>
        public static DatFile CreateDatFile(DatHeader datHeader, DatModifiers datModifiers)
        {
            DatFormat format = datHeader.GetFieldValue<DatFormat>(DatHeader.DatFormatKey);
            DatFile datFile = CreateDatFile(format, baseDat: null);
            datFile.SetHeader(datHeader);
            datFile.SetModifiers(datModifiers);
            return datFile;
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public static void ParseInto(
            DatFile datFile,
            string filename,
            int indexId = 0,
            bool keep = false,
            bool keepext = false,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false)
        {
            // Check the file extension first as a safeguard
            if (!Utilities.HasValidDatExtension(filename))
                return;

            // If the output filename isn't set already, get the internal filename
            string? outputFilename = datFile.Header.GetStringFieldValue(DatHeader.FileNameKey);
            if (string.IsNullOrEmpty(outputFilename))
                outputFilename = keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename);

            // If the output type isn't set already, try to derive one
            DatFormat datFormat = datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey);
            if (datFormat == 0)
                datFormat = GetDatFormat(filename);

            // Set values back to the header and set bucketing
            datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, outputFilename);
            datFile.Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, datFormat);
            datFile.Items.SetBucketedBy(ItemKey.Machine); // Setting this because it can reduce issues later

            var watch = new InternalStopwatch($"Parsing '{filename}' into internal DAT");

            // Now parse the correct type of DAT
            try
            {
                DatFile parsingDatFile = CreateDatFile(datFormat, datFile);
                parsingDatFile.ParseFile(filename,
                    indexId,
                    keep,
                    statsOnly: statsOnly,
                    filterRunner: filterRunner,
                    throwOnError: throwOnError);

                // Copy the header values back
                datFile.SetHeader(parsingDatFile.Header);
            }
            catch (Exception ex) when (!throwOnError)
            {
                _staticLogger.Error(ex, $"Error with file '{filename}'");
            }

            watch.Stop();
        }

        /// <summary>
        /// Create a DatFile and parse statistics into it
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// <remarks>
        /// Code must remove the existing format in order to ensure the format is derived
        /// from the input file instead. This should be addressed later by either always
        /// deriving the format, or by setting a flag for this to be done automatically.
        // </remarks>
        public static DatFile ParseStatistics(string? filename, FilterRunner? filterRunner = null, bool throwOnError = false)
        {
            // Null filenames are invalid
            if (filename == null)
            {
                DatFile empty = CreateDatFile();
                empty.Header.RemoveField(DatHeader.DatFormatKey);
                return empty;
            }

            DatFile datFile = CreateDatFile();
            datFile.Header.RemoveField(DatHeader.DatFormatKey);
            ParseInto(datFile, filename, statsOnly: true, filterRunner: filterRunner, throwOnError: throwOnError);
            return datFile;
        }

        /// <summary>
        /// Populate from multiple paths while returning the invividual headers
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// <returns>List of DatHeader objects representing headers</returns>
        public static List<DatHeader> PopulateUserData(DatFile datFile, List<string> inputs, FilterRunner? filterRunner = null)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return PopulateUserData(datFile, paths, filterRunner);
        }

        /// <summary>
        /// Populate from multiple paths while returning the invividual headers
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// <returns>List of DatHeader objects representing headers</returns>
        public static List<DatHeader> PopulateUserData(DatFile datFile, List<ParentablePath> inputs, FilterRunner? filterRunner = null)
        {
            DatFile[] datFiles = new DatFile[inputs.Count];
            InternalStopwatch watch = new("Processing individual DATs");

            // Parse all of the DATs into their own DatFiles in the array
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, inputs.Count, Core.Globals.ParallelOptions, i =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, i =>
#else
            for (int i = 0; i < inputs.Count; i++)
#endif
            {
                var input = inputs[i];
                _staticLogger.User($"Adding DAT: {input.CurrentPath}");
                datFiles[i] = CreateDatFile(datFile.Header.CloneFormat(), datFile.Modifiers);

                // Ensure the format is reset after parsing
                DatFormat currentFormat = datFiles[i].Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey);
                datFiles[i].Header.RemoveField(DatHeader.DatFormatKey);
                ParseInto(datFiles[i], input.CurrentPath, indexId: i, keep: true, filterRunner: filterRunner);
                datFiles[i].Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, currentFormat);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            watch.Start("Populating internal DAT");
            for (int i = 0; i < inputs.Count; i++)
            {
                AddFromExisting(datFile, datFiles[i], true);
                AddFromExistingDB(datFile, datFiles[i], true);
            }

            watch.Stop();

            return [.. Array.ConvertAll(datFiles, d => d.Header)];
        }

        /// <summary>
        /// Add items from another DatFile to the existing DatFile
        /// </summary>
        /// <param name="addTo">DatFile to add to</param>
        /// <param name="addFrom">DatFile to add from</param>
        /// <param name="delete">If items should be deleted from the source DatFile</param>
        private static void AddFromExisting(DatFile addTo, DatFile addFrom, bool delete = false)
        {
            // Get the list of keys from the DAT
            foreach (string key in addFrom.Items.SortedKeys)
            {
                // Add everything from the key to the internal DAT
                addFrom.GetItemsForBucket(key).ForEach(item => addTo.AddItem(item, statsOnly: false));

                // Now remove the key from the source DAT
                if (delete)
                    addFrom.RemoveBucket(key);
            }

            // Now remove the file dictionary from the source DAT
            if (delete)
                addFrom.ResetDictionary();
        }

        /// <summary>
        /// Add items from another DatFile to the existing DatFile
        /// </summary>
        /// <param name="addTo">DatFile to add to</param>
        /// <param name="addFrom">DatFile to add from</param>
        /// <param name="delete">If items should be deleted from the source DatFile</param>
        private static void AddFromExistingDB(DatFile addTo, DatFile addFrom, bool delete = false)
        {
            // Get all current items, machines, and mappings
            var datItems = addFrom.ItemsDB.GetItems();
            var machines = addFrom.GetMachinesDB();
            var sources = addFrom.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = addTo.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = addTo.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = addFrom.GetMachineForItemDB(item.Key).Key;
                long sourceIndex = addFrom.GetSourceForItemDB(item.Key).Key;

                addTo.AddItemDB(item.Value, machineRemapping[machineIndex], sourceRemapping[sourceIndex], statsOnly: false);

                // Now remove the key from the source DAT
                if (delete)
                    addFrom.RemoveItemDB(item.Key);

#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            // Now remove the file dictionary from the source DAT
            if (delete)
                addFrom.ResetDictionary();
        }

        /// <summary>
        /// Get what type of DAT the input file is
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <returns>The DatFormat corresponding to the DAT</returns>
        private static DatFormat GetDatFormat(string filename)
        {
            // Limit the output formats based on extension
            if (!Utilities.HasValidDatExtension(filename))
                return 0;

            // Get the extension from the filename
            string? ext = filename.GetNormalizedExtension();

            // Check if file exists
            if (!System.IO.File.Exists(filename))
                return 0;

            // Some formats should only require the extension to know
            switch (ext)
            {
                case "csv":
                    return DatFormat.CSV;
                case "json":
                    return DatFormat.SabreJSON;
                case "md2":
                    return DatFormat.RedumpMD2;
                case "md4":
                    return DatFormat.RedumpMD4;
                case "md5":
                    return DatFormat.RedumpMD5;
                case "ripemd128":
                    return DatFormat.RedumpRIPEMD128;
                case "ripemd160":
                    return DatFormat.RedumpRIPEMD160;
                case "sfv":
                    return DatFormat.RedumpSFV;
                case "sha1":
                    return DatFormat.RedumpSHA1;
                case "sha256":
                    return DatFormat.RedumpSHA256;
                case "sha384":
                    return DatFormat.RedumpSHA384;
                case "sha512":
                    return DatFormat.RedumpSHA512;
                case "spamsum":
                    return DatFormat.RedumpSpamSum;
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
            }

            // For everything else, we need to read it
            // Get the first two non-whitespace, non-comment lines to check, if possible
            string first = string.Empty, second = string.Empty;

            try
            {
                using StreamReader sr = System.IO.File.OpenText(filename);
                first = FindNextLine(sr);
                second = FindNextLine(sr);
            }
            catch { }

            // If we have an XML-based DAT
            if (first.Contains("<?xml") && first.Contains("?>"))
            {
                if (second.StartsWith("<!doctype datafile"))
                    return DatFormat.Logiqx;
                else if (second.StartsWith("<datafile xmlns:xsi"))
                    return DatFormat.Logiqx;

                else if (second.StartsWith("<!doctype mame")
                    || second.StartsWith("<!doctype m1")
                    || second.StartsWith("<mame")
                    || second.StartsWith("<m1"))
                    return DatFormat.Listxml;

                else if (second.StartsWith("<!doctype softwaredb"))
                    return DatFormat.OpenMSX;

                else if (second.StartsWith("<!doctype softwarelist"))
                    return DatFormat.SoftwareList;

                else if (second.StartsWith("<!doctype sabredat"))
                    return DatFormat.SabreXML;

                else if ((second.StartsWith("<dat") && !second.StartsWith("<datafile"))
                    || second.StartsWith("<?xml-stylesheet"))
                    return DatFormat.OfflineList;

                else if (second.StartsWith("<files"))
                    return DatFormat.ArchiveDotOrg;

                // Older and non-compliant DATs
                else
                    return DatFormat.Logiqx;
            }

            // If we have an SMDB (SHA-256, Filename, SHA-1, MD5, CRC32)
            else if (Regex.IsMatch(first, @"[0-9a-f]{64}\t.*?\t[0-9a-f]{40}\t[0-9a-f]{32}\t[0-9a-f]{8}"))
                return DatFormat.EverdriveSMDB;

            // If we have an INI-based DAT
#if NETFRAMEWORK
            else if (first.Contains("[") && first.Contains("]"))
#else
            else if (first.Contains('[') && first.Contains(']'))
#endif
                return DatFormat.RomCenter;

            // If we have a listroms DAT
            else if (first.StartsWith("roms required for driver"))
                return DatFormat.Listrom;

            // If we have a CMP-based DAT
            else if (first.Contains("clrmamepro"))
                return DatFormat.ClrMamePro;

            else if (first.Contains("romvault"))
                return DatFormat.ClrMamePro;

            else if (first.Contains("doscenter"))
                return DatFormat.DOSCenter;

#if NETFRAMEWORK
            else if (first.ToLowerInvariant().Contains("#name;title;emulator;cloneof;year;manufacturer;category;players;rotation;control;status;displaycount;displaytype;altromname;alttitle;extra"))
#else
            else if (first.Contains("#Name;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra", StringComparison.InvariantCultureIgnoreCase))
#endif
                return DatFormat.AttractMode;

#if NETFRAMEWORK
            else if (first.ToLowerInvariant().Contains("#romname;title;emulator;cloneof;year;manufacturer;category;players;rotation;control;status;displaycount;displaytype;altromname;alttitle;extra"))
#else
            else if (first.Contains("#RomName;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra", StringComparison.InvariantCultureIgnoreCase))
#endif
                return DatFormat.AttractMode;

            else
                return DatFormat.ClrMamePro;
        }

        /// <summary>
        /// Find the next non-whitespace, non-comment line from an input
        /// </summary>
        /// <param name="sr">StreamReader representing the input</param>
        /// <returns>The next complete line, if possible</returns>
        private static string FindNextLine(StreamReader sr)
        {
            // If we're at the end of the stream, we can't do anything
            if (sr.EndOfStream)
                return string.Empty;

            // Find the first line that's not whitespace or an XML comment
            string? line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
            bool inComment = line?.StartsWith("<!--") ?? false;
            while ((string.IsNullOrEmpty(line) || inComment) && !sr.EndOfStream)
            {
                // Null lines should not happen
                if (line == null)
                {
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                    continue;
                }

                // Self-contained comment lines
                if (line.StartsWith("<!--") && line.EndsWith("-->"))
                {
                    inComment = false;
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                }

                // Start of block comments
                else if (line.StartsWith("<!--"))
                {
                    inComment = true;
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                }

                // End of block comments
                else if (inComment && line.EndsWith("-->"))
                {
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                    inComment = line?.StartsWith("<!--") ?? false;
                }

                // Empty lines are just skipped
                else if (string.IsNullOrEmpty(line))
                {
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                    inComment |= line?.StartsWith("<!--") ?? false;
                }

                // In-comment lines
                else if (inComment)
                {
                    line = sr.ReadLine()?.ToLowerInvariant()?.Trim();
                }
            }

            // If we ended in a comment, return an empty string
            if (inComment)
                return string.Empty;

            return line ?? string.Empty;
        }
    
        #endregion

        #region BaseReport

        /// <summary>
        /// Create a specific type of BaseReport to be used based on a format and user inputs
        /// </summary>
        /// <param name="statReportFormat">Format of the Statistics Report to be created</param>
        /// <param name="statsList">List of statistics objects to set</param>
        /// <returns>BaseReport of the specific internal type that corresponds to the inputs</returns>
        public static BaseReport CreateReport(StatReportFormat statReportFormat, List<DatStatistics> statsList)
        {
            return statReportFormat switch
            {
                StatReportFormat.None => new Reports.Formats.Textfile(statsList),
                StatReportFormat.Textfile => new Reports.Formats.Textfile(statsList),
                StatReportFormat.CSV => new Reports.Formats.CommaSeparatedValue(statsList),
                StatReportFormat.HTML => new Reports.Formats.Html(statsList),
                StatReportFormat.SSV => new Reports.Formats.SemicolonSeparatedValue(statsList),
                StatReportFormat.TSV => new Reports.Formats.TabSeparatedValue(statsList),
                
                // We use textfile output as a backup for generic BaseReport
                _ => new Reports.Formats.Textfile(statsList),
            };
        }

        #endregion
    }
}