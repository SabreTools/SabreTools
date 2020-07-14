using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Readers;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a ClrMamePro DAT
    /// </summary>
    internal class ClrMamePro : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public ClrMamePro(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse a ClrMamePro DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        protected override void ParseFile(
            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
        {
            // Open a file reader
            Encoding enc = FileExtensions.GetEncoding(filename);
            ClrMameProReader cmpr = new ClrMameProReader(FileExtensions.TryOpenRead(filename), enc)
            {
                DosCenter = false
            };

            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore everything not top-level
                if (cmpr.RowType != CmpRowType.TopLevel)
                    continue;

                // Switch on the top-level name
                switch (cmpr.TopLevel.ToLowerInvariant())
                {
                    // Header values
                    case "clrmamepro":
                    case "romvault":
                        ReadHeader(cmpr, keep);
                        break;

                    // Sets
                    case "set":         // Used by the most ancient DATs
                    case "game":        // Used by most CMP DATs
                    case "machine":     // Possibly used by MAME CMP DATs
                        ReadSet(cmpr, false, filename, indexId);
                        break;
                    case "resource":    // Used by some other DATs to denote a BIOS set
                        ReadSet(cmpr, true, filename, indexId);
                        break;

                    default:
                        break;
                }
            }

            cmpr.Dispose();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="cmpr">ClrMameProReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadHeader(ClrMameProReader cmpr, bool keep)
        {
            bool superdat = false;

            // If there's no subtree to the header, skip it
            if (cmpr == null || cmpr.EndOfStream)
                return;

            // While we don't hit an end element or end of stream
            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore comments, internal items, and nothingness
                if (cmpr.RowType == CmpRowType.None || cmpr.RowType == CmpRowType.Comment || cmpr.RowType == CmpRowType.Internal)
                    continue;

                // If we reached the end of a section, break
                if (cmpr.RowType == CmpRowType.EndTopLevel)
                    break;

                // If the standalone value is null, we skip
                if (cmpr.Standalone == null)
                    continue;

                string itemKey = cmpr.Standalone?.Key.ToLowerInvariant();
                string itemVal = cmpr.Standalone?.Value;

                // For all other cases
                switch (itemKey)
                {
                    case "name":
                        DatHeader.Name = (string.IsNullOrWhiteSpace(DatHeader.Name) ? itemVal : DatHeader.Name);
                        superdat = superdat || itemVal.Contains(" - SuperDAT");

                        if (keep && superdat)
                            DatHeader.Type = (string.IsNullOrWhiteSpace(DatHeader.Type) ? "SuperDAT" : DatHeader.Type);

                        break;
                    case "description":
                        DatHeader.Description = (string.IsNullOrWhiteSpace(DatHeader.Description) ? itemVal : DatHeader.Description);
                        break;
                    case "rootdir":
                        DatHeader.RootDir = (string.IsNullOrWhiteSpace(DatHeader.RootDir) ? itemVal : DatHeader.RootDir);
                        break;
                    case "category":
                        DatHeader.Category = (string.IsNullOrWhiteSpace(DatHeader.Category) ? itemVal : DatHeader.Category);
                        break;
                    case "version":
                        DatHeader.Version = (string.IsNullOrWhiteSpace(DatHeader.Version) ? itemVal : DatHeader.Version);
                        break;
                    case "date":
                        DatHeader.Date = (string.IsNullOrWhiteSpace(DatHeader.Date) ? itemVal : DatHeader.Date);
                        break;
                    case "author":
                        DatHeader.Author = (string.IsNullOrWhiteSpace(DatHeader.Author) ? itemVal : DatHeader.Author);
                        break;
                    case "email":
                        DatHeader.Email = (string.IsNullOrWhiteSpace(DatHeader.Email) ? itemVal : DatHeader.Email);
                        break;
                    case "homepage":
                        DatHeader.Homepage = (string.IsNullOrWhiteSpace(DatHeader.Homepage) ? itemVal : DatHeader.Homepage);
                        break;
                    case "url":
                        DatHeader.Url = (string.IsNullOrWhiteSpace(DatHeader.Url) ? itemVal : DatHeader.Url);
                        break;
                    case "comment":
                        DatHeader.Comment = (string.IsNullOrWhiteSpace(DatHeader.Comment) ? itemVal : DatHeader.Comment);
                        break;
                    case "header":
                        DatHeader.Header = (string.IsNullOrWhiteSpace(DatHeader.Header) ? itemVal : DatHeader.Header);
                        break;
                    case "type":
                        DatHeader.Type = (string.IsNullOrWhiteSpace(DatHeader.Type) ? itemVal : DatHeader.Type);
                        superdat = superdat || itemVal.Contains("SuperDAT");
                        break;
                    case "forcemerging":
                        if (DatHeader.ForceMerging == ForceMerging.None)
                            DatHeader.ForceMerging = itemVal.AsForceMerging();

                        break;
                    case "forcezipping":
                        if (DatHeader.ForcePacking == ForcePacking.None)
                            DatHeader.ForcePacking = itemVal.AsForcePacking();

                        break;
                    case "forcepacking":
                        if (DatHeader.ForcePacking == ForcePacking.None)
                            DatHeader.ForcePacking = itemVal.AsForcePacking();

                        break;
                }
            }
        }

        /// <summary>
        /// Read set information
        /// </summary>
        /// <param name="cmpr">ClrMameProReader to use to parse the header</param>
        /// <param name="resource">True if the item is a resource (bios), false otherwise</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadSet(
            ClrMameProReader cmpr,
            bool resource,

            // Standard Dat parsing
            string filename,
            int indexId)
        {
            // Prepare all internal variables
            bool containsItems = false;
            Machine machine = new Machine()
            {
                MachineType = (resource ? MachineType.Bios : MachineType.None),
            };

            // If there's no subtree to the header, skip it
            if (cmpr == null || cmpr.EndOfStream)
                return;

            // While we don't hit an end element or end of stream
            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore comments and nothingness
                if (cmpr.RowType == CmpRowType.None || cmpr.RowType == CmpRowType.Comment)
                    continue;

                // If we reached the end of a section, break
                if (cmpr.RowType == CmpRowType.EndTopLevel)
                    break;

                // Handle any standalone items
                if (cmpr.RowType == CmpRowType.Standalone && cmpr.Standalone != null)
                {
                    string itemKey = cmpr.Standalone?.Key.ToLowerInvariant();
                    string itemVal = cmpr.Standalone?.Value;

                    switch (itemKey)
                    {
                        case "name":
                            machine.Name = itemVal;
                            break;
                        case "description":
                            machine.Description = itemVal;
                            break;
                        case "year":
                            machine.Year = itemVal;
                            break;
                        case "manufacturer":
                            machine.Manufacturer = itemVal;
                            break;
                        case "cloneof":
                            machine.CloneOf = itemVal;
                            break;
                        case "romof":
                            machine.RomOf = itemVal;
                            break;
                        case "sampleof":
                            machine.SampleOf = itemVal;
                            break;
                    }
                }

                // Handle any internal items
                else if (cmpr.RowType == CmpRowType.Internal
                    && !string.IsNullOrWhiteSpace(cmpr.InternalName)
                    && cmpr.Internal != null)
                {
                    containsItems = true;
                    string itemKey = cmpr.InternalName;

                    ItemType itemType = ItemType.Rom;
                    switch (itemKey)
                    {
                        case "archive":
                            itemType = ItemType.Archive;
                            break;
                        case "biosset":
                            itemType = ItemType.BiosSet;
                            break;
                        case "disk":
                            itemType = ItemType.Disk;
                            break;
                        case "release":
                            itemType = ItemType.Release;
                            break;
                        case "rom":
                            itemType = ItemType.Rom;
                            break;
                        case "sample":
                            itemType = ItemType.Sample;
                            break;
                    }

                    // Create the proper DatItem based on the type
                    DatItem item = DatItem.Create(itemType);

                    // Then populate it with information
                    item.CopyMachineInformation(machine);

                    item.IndexId = indexId;
                    item.IndexSource = filename;

                    // Loop through all of the attributes
                    foreach (var kvp in cmpr.Internal)
                    {
                        string attrKey = kvp.Key;
                        string attrVal = kvp.Value;

                        switch (attrKey)
                        {
                            //If the item is empty, we automatically skip it because it's a fluke
                            case "":
                                continue;

                            // Regular attributes
                            case "name":
                                item.Name = attrVal;
                                break;

                            case "size":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    if (Int64.TryParse(attrVal, out long size))
                                        ((Rom)item).Size = size;
                                    else
                                        ((Rom)item).Size = -1;
                                }

                                break;
                            case "crc":
                                if (item.ItemType == ItemType.Rom)
                                    (item as Rom).CRC = attrVal;

                                break;
                            case "md5":
                                if (item.ItemType == ItemType.Rom)
                                    (item as Rom).MD5 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).MD5 = attrVal;

                                break;
#if NET_FRAMEWORK
                            case "ripemd160":
                                if (item.ItemType == ItemType.Rom)
                                    (item as Rom).RIPEMD160 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).RIPEMD160 = attrVal;

                                break;
#endif
                            case "sha1":
                                if (item.ItemType == ItemType.Rom)
                                    (item as Rom).SHA1 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).SHA1 = attrVal;

                                break;
                            case "sha256":
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).SHA256 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).SHA256 = attrVal;

                                break;
                            case "sha384":
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).SHA384 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).SHA384 = attrVal;

                                break;
                            case "sha512":
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).SHA512 = attrVal;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).SHA512 = attrVal;

                                break;
                            case "status":
                                ItemStatus tempFlagStatus = attrVal.AsItemStatus();
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).ItemStatus = tempFlagStatus;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).ItemStatus = tempFlagStatus;

                                break;
                            case "date":
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).Date = attrVal;
                                else if (item.ItemType == ItemType.Release)
                                    ((Release)item).Date = attrVal;

                                break;
                            case "default":
                                if (item.ItemType == ItemType.BiosSet)
                                    ((BiosSet)item).Default = attrVal.ToLowerInvariant().AsYesNo();
                                else if (item.ItemType == ItemType.Release)
                                    ((Release)item).Default = attrVal.ToLowerInvariant().AsYesNo();

                                break;
                            case "description":
                                if (item.ItemType == ItemType.BiosSet)
                                    ((BiosSet)item).Description = attrVal.ToLowerInvariant();

                                break;
                            case "region":
                                if (item.ItemType == ItemType.Release)
                                    ((Release)item).Region = attrVal.ToLowerInvariant();

                                break;
                            case "language":
                                if (item.ItemType == ItemType.Release)
                                    ((Release)item).Language = attrVal.ToLowerInvariant();

                                break;
                        }
                    }

                    // Now process and add the rom
                    ParseAddHelper(item);
                }
            }

            // If no items were found for this machine, add a Blank placeholder
            if (!containsItems)
            {
                Blank blank = new Blank()
                {
                    IndexId = indexId,
                    IndexSource = filename,
                };

                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank);
            }
        }

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false)
        {
            try
            {
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = FileExtensions.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                ClrMameProWriter cmpw = new ClrMameProWriter(fs, new UTF8Encoding(false))
                {
                    Quotes = true
                };

                // Write out the header
                WriteHeader(cmpw);

                // Write out each of the machines and roms
                string lastgame = null;

                // Get a properly sorted set of keys
                List<string> keys = Keys;
                keys.Sort(new NaturalComparer());

                foreach (string key in keys)
                {
                    List<DatItem> roms = this[key];

                    // Resolve the names in the block
                    roms = DatItem.ResolveNames(roms);

                    for (int index = 0; index < roms.Count; index++)
                    {
                        DatItem rom = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (rom.Name == null || rom.MachineName == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteEndGame(cmpw, rom);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(cmpw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

                            // If we're in a mode that doesn't allow for actual empty folders, add the blank info
                            rom.Name = (rom.Name == "null" ? "-" : rom.Name);
                            ((Rom)rom).Size = Constants.SizeZero;
                            ((Rom)rom).CRC = ((Rom)rom).CRC == "null" ? Constants.CRCZero : null;
                            ((Rom)rom).MD5 = ((Rom)rom).MD5 == "null" ? Constants.MD5Zero : null;
#if NET_FRAMEWORK
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
#endif
                            ((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
                            ((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
                            ((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
                            ((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
                        }

                        // Now, output the rom data
                        WriteDatItem(cmpw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(cmpw);

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                cmpw.Dispose();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT header using the supplied StreamWriter
        /// </summary>
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(ClrMameProWriter cmpw)
        {
            try
            {
                cmpw.WriteStartElement("clrmamepro");

                cmpw.WriteStandalone("name", DatHeader.Name);
                cmpw.WriteStandalone("description", DatHeader.Description);
                if (!string.IsNullOrWhiteSpace(DatHeader.Category))
                    cmpw.WriteStandalone("category", DatHeader.Category);
                cmpw.WriteStandalone("version", DatHeader.Version);
                if (!string.IsNullOrWhiteSpace(DatHeader.Date))
                    cmpw.WriteStandalone("date", DatHeader.Date);
                cmpw.WriteStandalone("author", DatHeader.Author);
                if (!string.IsNullOrWhiteSpace(DatHeader.Email))
                    cmpw.WriteStandalone("email", DatHeader.Email);
                if (!string.IsNullOrWhiteSpace(DatHeader.Homepage))
                    cmpw.WriteStandalone("homepage", DatHeader.Homepage);
                if (!string.IsNullOrWhiteSpace(DatHeader.Url))
                    cmpw.WriteStandalone("url", DatHeader.Url);
                if (!string.IsNullOrWhiteSpace(DatHeader.Comment))
                    cmpw.WriteStandalone("comment", DatHeader.Comment);

                switch (DatHeader.ForcePacking)
                {
                    case ForcePacking.Unzip:
                        cmpw.WriteStandalone("forcezipping", "no", false);
                        break;
                    case ForcePacking.Zip:
                        cmpw.WriteStandalone("forcezipping", "yes", false);
                        break;
                }

                switch (DatHeader.ForceMerging)
                {
                    case ForceMerging.Full:
                        cmpw.WriteStandalone("forcemerging", "full", false);
                        break;
                    case ForceMerging.Split:
                        cmpw.WriteStandalone("forcemerging", "split", false);
                        break;
                    case ForceMerging.Merged:
                        cmpw.WriteStandalone("forcemerging", "merged", false);
                        break;
                    case ForceMerging.NonMerged:
                        cmpw.WriteStandalone("forcemerging", "nonmerged", false);
                        break;
                }

                // End clrmamepro
                cmpw.WriteEndElement();

                cmpw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(ClrMameProWriter cmpw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                cmpw.WriteStartElement(datItem.MachineType == MachineType.Bios ? "resource" : "game");
                cmpw.WriteStandalone("name", datItem.GetField(Field.MachineName, DatHeader.ExcludeFields));
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RomOf, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("romof", datItem.RomOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("cloneof", datItem.CloneOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("sampleof", datItem.SampleOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("description", datItem.MachineDescription);
                else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("description", datItem.MachineName);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("year", datItem.Year);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Manufacturer, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("manufacturer", datItem.Manufacturer);

                cmpw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out Game end using the supplied StreamWriter
        /// </summary>
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(ClrMameProWriter cmpw, DatItem datItem)
        {
            try
            {
                // Build the state based on excluded fields
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, DatHeader.ExcludeFields)))
                    cmpw.WriteStandalone("sampleof", datItem.SampleOf);

                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="datFile">DatFile to write out from</param>
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(ClrMameProWriter cmpw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                        cmpw.WriteStartElement("archive");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, DatHeader.ExcludeFields));
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        cmpw.WriteStartElement("biosset");
                        cmpw.WriteAttributeString("name", biosSet.GetField(Field.Name, DatHeader.ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(biosSet.GetField(Field.BiosDescription, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("description", biosSet.Description);
                        if (!DatHeader.ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            cmpw.WriteAttributeString("default", biosSet.Default.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        cmpw.WriteStartElement("disk");
                        cmpw.WriteAttributeString("name", disk.GetField(Field.Name, DatHeader.ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("md5", disk.MD5.ToLowerInvariant());
#if NET_FRAMEWORK
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("ripemd160", disk.RIPEMD160.ToLowerInvariant());
#endif
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha1", disk.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha256", disk.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha384", disk.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha512", disk.SHA512.ToLowerInvariant());
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            cmpw.WriteAttributeString("flags", disk.ItemStatus.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        cmpw.WriteStartElement("release");
                        cmpw.WriteAttributeString("name", release.GetField(Field.Name, DatHeader.ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Region, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("region", release.Region);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Language, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("language", release.Language);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("date", release.Date);
                        if (!DatHeader.ExcludeFields[(int)Field.Default] && release.Default != null)
                            cmpw.WriteAttributeString("default", release.Default.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        cmpw.WriteStartElement("rom");
                        cmpw.WriteAttributeString("name", rom.GetField(Field.Name, DatHeader.ExcludeFields));
                        if (!DatHeader.ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            cmpw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("md5", rom.MD5.ToLowerInvariant());
#if NET_FRAMEWORK
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("ripemd160", rom.RIPEMD160.ToLowerInvariant());
#endif
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha1", rom.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha256", rom.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha384", rom.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("sha512", rom.SHA512.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("date", rom.Date);
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            cmpw.WriteAttributeString("flags", rom.ItemStatus.ToString().ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;

                    case ItemType.Sample:
                        cmpw.WriteStartElement("sample");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, DatHeader.ExcludeFields));
                        cmpw.WriteEndElement();
                        break;
                }

                cmpw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(ClrMameProWriter cmpw)
        {
            try
            {
                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
