using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a ClrMamePro DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class ClrMamePro : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public ClrMamePro(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a ClrMamePro DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        public override void ParseFile(
            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // Open a file reader
            Encoding enc = Utilities.GetEncoding(filename);
            StreamReader sr = new StreamReader(Utilities.TryOpenRead(filename), enc);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                // Comments in CMP DATs start with a #
                if (line.Trim().StartsWith("#"))
                    continue;

                // If the line is the header or a game
                if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
                {
                    GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;
                    string normalizedValue = gc[1].Value.ToLowerInvariant();

                    // If we have a known header
                    if (normalizedValue == "clrmamepro"
                        || normalizedValue == "romvault"
                        || normalizedValue == "doscenter")
                    {
                        ReadHeader(sr, keep);
                    }
                    // If we have a known set type
                    else if (normalizedValue == "set"      // Used by the most ancient DATs
                        || normalizedValue == "game"       // Used by most CMP DATs
                        || normalizedValue == "machine")   // Possibly used by MAME CMP DATs
                    {
                        ReadSet(sr, false, filename, sysid, srcid, clean, remUnicode);
                    }
                    else if (normalizedValue == "resource")  // Used by some other DATs to denote a BIOS set
                    {
                        ReadSet(sr, true, filename, sysid, srcid, clean, remUnicode);
                    }
                }
            }

            sr.Dispose();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="reader">StreamReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadHeader(StreamReader reader, bool keep)
        {
            bool superdat = false;

            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // We only want elements
                if (line.Trim().StartsWith("#"))
                {
                    line = reader.ReadLine();
                    continue;
                }

                // Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
                GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = gc[2].Value.Replace("\"", string.Empty);

                if (line.Trim().StartsWith("Name:"))
                {
                    Name = (string.IsNullOrWhiteSpace(Name) ? line.Substring(6) : Name);
                    superdat = superdat || itemval.Contains(" - SuperDAT");

                    if (keep && superdat)
                        Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);

                    line = reader.ReadLine();
                    continue;
                }

                switch (gc[1].Value)
                {
                    case "name":
                    case "Name:":
                        Name = (string.IsNullOrWhiteSpace(Name) ? itemval : Name);
                        superdat = superdat || itemval.Contains(" - SuperDAT");

                        if (keep && superdat)
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);

                        break;
                    case "description":
                    case "Description:":
                        Description = (string.IsNullOrWhiteSpace(Description) ? itemval : Description);
                        break;
                    case "rootdir":
                    case "Rootdir:":
                        RootDir = (string.IsNullOrWhiteSpace(RootDir) ? itemval : RootDir);
                        break;
                    case "category":
                    case "Category:":
                        Category = (string.IsNullOrWhiteSpace(Category) ? itemval : Category);
                        break;
                    case "version":
                    case "Version:":
                        Version = (string.IsNullOrWhiteSpace(Version) ? itemval : Version);
                        break;
                    case "date":
                    case "Date:":
                        Date = (string.IsNullOrWhiteSpace(Date) ? itemval : Date);
                        break;
                    case "author":
                    case "Author:":
                        Author = (string.IsNullOrWhiteSpace(Author) ? itemval : Author);
                        break;
                    case "email":
                    case "Email:":
                        Email = (string.IsNullOrWhiteSpace(Email) ? itemval : Email);
                        break;
                    case "homepage":
                    case "Homepage:":
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? itemval : Homepage);
                        break;
                    case "url":
                    case "Url:":
                        Url = (string.IsNullOrWhiteSpace(Url) ? itemval : Url);
                        break;
                    case "comment":
                    case "Comment:":
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? itemval : Comment);
                        break;
                    case "header":
                    case "Header:":
                        Header = (string.IsNullOrWhiteSpace(Header) ? itemval : Header);
                        break;
                    case "type":
                    case "Type:":
                        Type = (string.IsNullOrWhiteSpace(Type) ? itemval : Type);
                        superdat = superdat || itemval.Contains("SuperDAT");
                        break;
                    case "forcemerging":
                        if (ForceMerging == ForceMerging.None)
                            ForceMerging = Utilities.GetForceMerging(itemval);
                        
                        break;
                    case "forcezipping":
                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = Utilities.GetForcePacking(itemval);

                        break;
                    case "forcepacking":
                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = Utilities.GetForcePacking(itemval);

                        break;
                }

                line = reader.ReadLine();
            }
        }

        /// <summary>
        /// Read set information
        /// </summary>
        /// <param name="reader">StreamReader to use to parse the header</param>
        /// <param name="resource">True if the item is a resource (bios), false otherwise</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadSet(
            StreamReader reader,
            bool resource,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool clean,
            bool remUnicode)
        {
            // Prepare all internal variables
            bool containsItems = false;
            Machine machine = new Machine()
            {
                MachineType = (resource ? MachineType.Bios : MachineType.None),
            };

            // If there's no subtree to the header, skip it
            if (reader == null || reader.EndOfStream)
                return;

            // Otherwise, add what is possible
            string line = reader.ReadLine();
            while (!Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                // We only want elements
                if (line.Trim().StartsWith("#"))
                {
                    line = reader.ReadLine();
                    continue;
                }

                // Item-specific lines have a known pattern
                string trimmedline = line.Trim();
                if (trimmedline.StartsWith("archive (")
                    || trimmedline.StartsWith("biosset (")
                    || trimmedline.StartsWith("disk (")
                    || trimmedline.StartsWith("file (") // This is a DOSCenter file, not a SabreDAT file
                    || trimmedline.StartsWith("release (")
                    || trimmedline.StartsWith("rom (")
                    || (trimmedline.StartsWith("sample") && !trimmedline.StartsWith("sampleof")))
                {
                    containsItems = true;
                    ItemType temptype = ItemType.Rom;
                    if (line.Trim().StartsWith("rom ("))
                        temptype = ItemType.Rom;
                    else if (line.Trim().StartsWith("disk ("))
                        temptype = ItemType.Disk;
                    else if (line.Trim().StartsWith("file ("))
                        temptype = ItemType.Rom;
                    else if (line.Trim().StartsWith("sample"))
                        temptype = ItemType.Sample;

                    // Create the proper DatItem based on the type
                    DatItem item = Utilities.GetDatItem(temptype);

                    // Then populate it with information
                    item.CopyMachineInformation(machine);

                    item.SystemID = sysid;
                    item.System = filename;
                    item.SourceID = srcid;

                    // If we have a sample, treat it special
                    if (temptype == ItemType.Sample)
                    {
                        line = line.Trim().Remove(0, 6).Trim().Replace("\"", string.Empty); // Remove "sample" from the input string
                        item.Name = line;

                        // Now process and add the sample
                        ParseAddHelper(item, clean, remUnicode);
                        line = reader.ReadLine();
                        continue;
                    }

                    // Get the line split by spaces and quotes
                    string[] linegc = Utilities.SplitLineAsCMP(line);

                    // Special cases for DOSCenter DATs only because of how the lines are arranged
                    if (line.Trim().StartsWith("file ("))
                    {
                        // Loop over the specifics
                        for (int i = 0; i < linegc.Length; i++)
                        {
                            // Names are not quoted, for some stupid reason
                            if (linegc[i] == "name")
                            {
                                // Get the name in order until we find the next flag
                                while (++i < linegc.Length && linegc[i] != "size"
                                    && linegc[i] != "date"
                                    && linegc[i] != "crc"
                                    && linegc[i] != "md5"
                                    && linegc[i] != "ripemd160"
                                    && linegc[i] != "sha1"
                                    && linegc[i] != "sha256"
                                    && linegc[i] != "sha384"
                                    && linegc[i] != "sha512")
                                {
                                    item.Name += $" {linegc[i]}";
                                }

                                // Perform correction
                                item.Name = item.Name.TrimStart();
                                i--;
                            }

                            // Get the size from the next part
                            else if (linegc[i] == "size")
                            {
                                long tempsize = -1;
                                if (!Int64.TryParse(linegc[++i], out tempsize))
                                    tempsize = 0;

                                ((Rom)item).Size = tempsize;
                            }

                            // Get the date from the next part
                            else if (linegc[i] == "date")
                            {
                                ((Rom)item).Date = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";
                            }

                            // Get the CRC from the next part
                            else if (linegc[i] == "crc")
                            {
                                ((Rom)item).CRC = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the MD5 from the next part
                            else if (linegc[i] == "md5")
                            {
                                ((Rom)item).MD5 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the RIPEMD160 from the next part
                            else if (linegc[i] == "ripemd160")
                            {
                                ((Rom)item).RIPEMD160 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the SHA1 from the next part
                            else if (linegc[i] == "sha1")
                            {
                                ((Rom)item).SHA1 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the SHA256 from the next part
                            else if (linegc[i] == "sha256")
                            {
                                ((Rom)item).SHA256 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the SHA384 from the next part
                            else if (linegc[i] == "sha384")
                            {
                                ((Rom)item).SHA384 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }

                            // Get the SHA512 from the next part
                            else if (linegc[i] == "sha512")
                            {
                                ((Rom)item).SHA512 = linegc[++i].Replace("\"", string.Empty).ToLowerInvariant();
                            }
                        }

                        // Now process and add the rom
                        ParseAddHelper(item, clean, remUnicode);
                        line = reader.ReadLine();
                        continue;
                    }

                    // Loop over all attributes normally and add them if possible
                    for (int i = 0; i < linegc.Length; i++)
                    {
                        // Look at the current item and use it if possible
                        string quoteless = linegc[i].Replace("\"", string.Empty);
                        switch (quoteless)
                        {
                            //If the item is empty, we automatically skip it because it's a fluke
                            case "":
                                continue;

                            // Special cases for standalone item statuses
                            case "baddump":
                            case "good":
                            case "nodump":
                            case "verified":
                                ItemStatus tempStandaloneStatus = Utilities.GetItemStatus(quoteless);
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).ItemStatus = tempStandaloneStatus;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).ItemStatus = tempStandaloneStatus;

                                break;

                            // Regular attributes
                            case "name":
                                quoteless = linegc[++i].Replace("\"", string.Empty);
                                item.Name = quoteless;
                                break;
                            case "size":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    if (Int64.TryParse(quoteless, out long size))
                                        ((Rom)item).Size = size;
                                    else
                                        ((Rom)item).Size = -1;
                                }

                                break;
                            case "crc":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).CRC = Utilities.CleanHashData(quoteless, Constants.CRCLength);
                                }

                                break;
                            case "md5":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).MD5 = Utilities.CleanHashData(quoteless, Constants.MD5Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    i++;
                                    quoteless = linegc[i].Replace("\"", string.Empty);
                                    ((Disk)item).MD5 = Utilities.CleanHashData(quoteless, Constants.MD5Length);
                                }

                                break;
                            case "ripemd160":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).RIPEMD160 = Utilities.CleanHashData(quoteless, Constants.RIPEMD160Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).RIPEMD160 = Utilities.CleanHashData(quoteless, Constants.RIPEMD160Length);
                                }

                                break;
                            case "sha1":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA1 = Utilities.CleanHashData(quoteless, Constants.SHA1Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA1 = Utilities.CleanHashData(quoteless, Constants.SHA1Length);
                                }

                                break;
                            case "sha256":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA256 = Utilities.CleanHashData(quoteless, Constants.SHA256Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA256 = Utilities.CleanHashData(quoteless, Constants.SHA256Length);
                                }

                                break;
                            case "sha384":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA384 = Utilities.CleanHashData(quoteless, Constants.SHA384Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA384 = Utilities.CleanHashData(quoteless, Constants.SHA384Length);
                                }

                                break;
                            case "sha512":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Rom)item).SHA512 = Utilities.CleanHashData(quoteless, Constants.SHA512Length);
                                }
                                else if (item.ItemType == ItemType.Disk)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Disk)item).SHA512 = Utilities.CleanHashData(quoteless, Constants.SHA512Length);
                                }

                                break;
                            case "status":
                            case "flags":
                                quoteless = linegc[++i].Replace("\"", string.Empty);
                                ItemStatus tempFlagStatus = Utilities.GetItemStatus(quoteless);
                                if (item.ItemType == ItemType.Rom)
                                    ((Rom)item).ItemStatus = tempFlagStatus;
                                else if (item.ItemType == ItemType.Disk)
                                    ((Disk)item).ItemStatus = tempFlagStatus;

                                break;
                            case "date":
                                if (item.ItemType == ItemType.Rom)
                                {
                                    // If we have quotes in the next item, assume only one item
                                    if (linegc[i + 1].Contains("\""))
                                        quoteless = linegc[++i].Replace("\"", string.Empty);

                                    // Otherwise, we assume we need to read the next two items
                                    else
                                        quoteless = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";

                                    ((Rom)item).Date = quoteless;
                                }
                                else if (item.ItemType == ItemType.Release)
                                {
                                    // If we have quotes in the next item, assume only one item
                                    if (linegc[i + 1].Contains("\""))
                                        quoteless = linegc[++i].Replace("\"", string.Empty);

                                    // Otherwise, we assume we need to read the next two items
                                    else
                                        quoteless = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";

                                    ((Release)item).Date = quoteless;
                                }

                                break;
                            case "default":
                                if (item.ItemType == ItemType.BiosSet)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((BiosSet)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
                                }
                                else if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Default = Utilities.GetYesNo(quoteless.ToLowerInvariant());
                                }

                                break;
                            case "description":
                                if (item.ItemType == ItemType.BiosSet)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((BiosSet)item).Description = quoteless.ToLowerInvariant();
                                }

                                break;
                            case "region":
                                if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Region = quoteless.ToLowerInvariant();
                                }

                                break;
                            case "language":
                                if (item.ItemType == ItemType.Release)
                                {
                                    quoteless = linegc[++i].Replace("\"", string.Empty);
                                    ((Release)item).Language = quoteless.ToLowerInvariant();
                                }

                                break;
                        }
                    }

                    // Now process and add the rom
                    ParseAddHelper(item, clean, remUnicode);

                    line = reader.ReadLine();
                    continue;
                }

                // Set-specific lines have a known pattern
                GroupCollection setgc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = setgc[2].Value.Replace("\"", string.Empty);

                switch (setgc[1].Value)
                {
                    case "name":
                        machine.Name = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        machine.Description = (itemval.ToLowerInvariant().EndsWith(".zip") ? itemval.Remove(itemval.Length - 4) : itemval);
                        break;
                    case "description":
                        machine.Description = itemval;
                        break;
                    case "year":
                        machine.Year = itemval;
                        break;
                    case "manufacturer":
                        machine.Manufacturer = itemval;
                        break;
                    case "cloneof":
                        machine.CloneOf = itemval;
                        break;
                    case "romof":
                        machine.RomOf = itemval;
                        break;
                    case "sampleof":
                        machine.SampleOf = itemval;
                        break;
                }

                line = reader.ReadLine();
            }

            // If no items were found for this machine, add a Blank placeholder
            if (!containsItems)
            {
                Blank blank = new Blank()
                {
                    SystemID = sysid,
                    System = filename,
                    SourceID = srcid,
                };

                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank, clean, remUnicode);
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
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

                // Write out the header
                WriteHeader(sw);

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
                            WriteEndGame(sw, rom);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(sw, rom);

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
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
                            ((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
                            ((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
                            ((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
                            ((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
                        }

                        // Now, output the rom data
                        WriteDatItem(sw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(sw);

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                sw.Dispose();
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
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(StreamWriter sw)
        {
            try
            {
                string header = "clrmamepro (\n";
                header += $"\tname \"{Name}\"\n";
                header += $"\tdescription \"{Description}\"\n";
                if (!string.IsNullOrWhiteSpace(Category))
                    header += $"\tcategory \"{Category}\"\n";
                header += $"\tversion \"{Version}\"\n";
                if (!string.IsNullOrWhiteSpace(Date))
                    header += $"\tdate \"{Date}\"\n";
                header += $"\tauthor \"{Author}\"\n";
                if (!string.IsNullOrWhiteSpace(Email))
                    header += $"\temail \"{Email}\"\n";
                if (!string.IsNullOrWhiteSpace(Homepage))
                    header += $"\thomepage \"{Homepage}\"\n";
                if (!string.IsNullOrWhiteSpace(Url))
                    header += $"\turl \"{Url}\"\n";
                if (!string.IsNullOrWhiteSpace(Comment))
                    header += $"\tcomment \"{Comment}\"\n";
                switch (ForcePacking)
                {
                    case ForcePacking.Unzip:
                        header += "\tforcezipping no\n";
                        break;
                    case ForcePacking.Zip:
                        header += "\tforcezipping yes\n";
                        break;
                }
                switch (ForceMerging)
                {
                    case ForceMerging.Full:
                        header += "\tforcemerging full\n";
                        break;
                    case ForceMerging.Split:
                        header += "\tforcemerging split\n";
                        break;
                    case ForceMerging.Merged:
                        header += "\tforcemerging merged\n";
                        break;
                    case ForceMerging.NonMerged:
                        header += "\tforcemerging nonmerged\n";
                        break;
                }
                header += ")\n";

                // Write the header out
                sw.Write(header);
                sw.Flush();
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
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(StreamWriter sw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                string state = (datItem.MachineType == MachineType.Bios ? "resource" : "game");
                state += $" (\n\tname \"{datItem.MachineName}\"\n";
                    state += $"\tromof \"{datItem.RomOf}\"\n";
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                    state += $"\tcloneof \"{datItem.CloneOf}\"\n";
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)))
                    state += $"\tsampleof \"{datItem.SampleOf}\"\n";
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    state += $"\tdescription \"{datItem.MachineDescription}\"\n";
                else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    state += $"\tdescription \"{datItem.MachineName}\"\n";
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, ExcludeFields)))
                    state += $"\tyear \"{datItem.Year}\"\n";
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Manufacturer, ExcludeFields)))
                    state += $"\tmanufacturer \"{datItem.Manufacturer}\"\n";

                sw.Write(state);
                sw.Flush();
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
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw, DatItem datItem)
        {
            try
            {
                string state = string.Empty;

                // Build the state based on excluded fields
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)))
                    state += $"\tsampleof \"{datItem.SampleOf}\"\n";

                state += ")\n";

                sw.Write(state);
                sw.Flush();
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
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(StreamWriter sw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                string state = string.Empty;

                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                        state += $"\tarchive ( name\"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        state += " )\n";
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        state += $"\tbiosset ( name\"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.BiosDescription, ExcludeFields)))
                            state += $" description \"{biosSet.Description}\"";
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            state += $" default \"{biosSet.Default.ToString().ToLowerInvariant()}\"";
                        state += " )\n";
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        state += $"\tdisk ( name \"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            state += $" md5 \"{disk.MD5.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            state += $" ripemd160 \"{disk.RIPEMD160.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            state += $" sha1 \"{disk.SHA1.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            state += $" sha256 \"{disk.SHA256.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            state += $" sha384 \"{disk.SHA384.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            state += $" sha512 \"{disk.SHA512.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            state += $" flags \"{disk.ItemStatus.ToString().ToLowerInvariant()}\"";
                        state += " )\n";
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        state += $"\trelease ( name\"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Region, ExcludeFields)))
                            state += $" region \"{release.Region}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Language, ExcludeFields)))
                            state += $" language \"{release.Language}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            state += $" date \"{release.Date}\"";
                        if (!ExcludeFields[(int)Field.Default] && release.Default != null)
                            state += $" default \"{release.Default.ToString().ToLowerInvariant()}\"";
                        state += " )\n";
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        state += $"\trom ( name \"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            state += $" size \"{rom.Size}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, ExcludeFields)))
                            state += $" crc \"{rom.CRC.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            state += $" md5 \"{rom.MD5.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            state += $" ripemd160 \"{rom.RIPEMD160.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            state += $" sha1 \"{rom.SHA1.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            state += $" sha256 \"{rom.SHA256.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            state += $" sha384 \"{rom.SHA384.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            state += $" sha512 \"{rom.SHA512.ToLowerInvariant()}\"";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            state += $" date \"{rom.Date}\"";
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            state += $" flags \"{rom.ItemStatus.ToString().ToLowerInvariant()}\"";
                        state += " )\n";
                        break;

                    case ItemType.Sample:
                        state += $"\tsample ( name\"{datItem.GetField(Field.Name, ExcludeFields)}\"";
                        state += " )\n";
                        break;
                }

                sw.Write(state);
                sw.Flush();
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
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(StreamWriter sw)
        {
            try
            {
                string footer = footer = ")\n";

                // Write the footer out
                sw.Write(footer);
                sw.Flush();
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
