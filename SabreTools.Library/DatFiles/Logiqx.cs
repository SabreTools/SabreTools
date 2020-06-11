using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a Logiqx-derived DAT
    /// </summary>
    /// TODO: Add XSD validation for all XML DAT types (maybe?)
    /// TODO: Verify that all write for this DatFile type is correct
    internal class Logiqx : DatFile
    {
        // Private instance variables specific to Logiqx DATs
        private readonly bool _deprecated;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="deprecated">True if the output uses "game", false if the output uses "machine"</param>
        public Logiqx(DatFile datFile, bool deprecated)
            : base(datFile, cloneHeader: false)
        {
            _deprecated = deprecated;
        }

        /// <summary>
        /// Parse a Logiqx XML DAT and return all found games and roms within
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
            // Prepare all internal variables
            Encoding enc = Utilities.GetEncoding(filename);
            XmlReader xtr = Utilities.GetXmlTextReader(filename);
            List<string> dirs = new List<string>();

            // If we got a null reader, just return
            if (xtr == null)
                return;

            // Otherwise, read the file to the end
            try
            {
                xtr.MoveToContent();
                while (!xtr.EOF)
                {
                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        // If we're ending a dir, remove the last item from the dirs list, if possible
                        if (xtr.Name == "dir" && dirs.Count > 0)
                            dirs.RemoveAt(dirs.Count - 1);

                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        // The datafile tag can have some attributes
                        case "datafile":
                            // string build = xtr.GetAttribute("build");
                            // string debug = xtr.GetAttribute("debug"); // (yes|no) "no"
                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the header
                        case "header":
                            ReadHeader(xtr.ReadSubtree(), keep);

                            // Skip the header node now that we've processed it
                            xtr.Skip();
                            break;

                        // Unique to RomVault-created DATs
                        case "dir":
                            Type = "SuperDAT";
                            dirs.Add(xtr.GetAttribute("name") ?? string.Empty);
                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the game
                        case "machine": // New-style Logiqx
                        case "game": // Old-style Logiqx
                            ReadMachine(xtr.ReadSubtree(), dirs, filename, sysid, srcid, keep, clean, remUnicode);

                            // Skip the machine now that we've processed it
                            xtr.Skip();
                            break;

                        default:
                            xtr.Read();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning($"Exception found while parsing '{filename}': {ex}");

                // For XML errors, just skip the affected node
                xtr?.Read();
            }

            xtr.Dispose();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadHeader(XmlReader reader, bool keep)
        {
            bool superdat = false;

            // If there's no subtree to the header, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element || reader.Name == "header")
                {
                    reader.Read();
                    continue;
                }

                // Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
                string content = string.Empty;
                switch (reader.Name)
                {
                    case "name":
                        content = reader.ReadElementContentAsString(); ;
                        Name = (string.IsNullOrWhiteSpace(Name) ? content : Name);
                        superdat = superdat || content.Contains(" - SuperDAT");
                        if (keep && superdat)
                        {
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
                        }
                        break;

                    case "description":
                        content = reader.ReadElementContentAsString();
                        Description = (string.IsNullOrWhiteSpace(Description) ? content : Description);
                        break;

                    case "rootdir": // This is exclusive to TruRip XML
                        content = reader.ReadElementContentAsString();
                        RootDir = (string.IsNullOrWhiteSpace(RootDir) ? content : RootDir);
                        break;

                    case "category":
                        content = reader.ReadElementContentAsString();
                        Category = (string.IsNullOrWhiteSpace(Category) ? content : Category);
                        break;

                    case "version":
                        content = reader.ReadElementContentAsString();
                        Version = (string.IsNullOrWhiteSpace(Version) ? content : Version);
                        break;

                    case "date":
                        content = reader.ReadElementContentAsString();
                        Date = (string.IsNullOrWhiteSpace(Date) ? content.Replace(".", "/") : Date);
                        break;

                    case "author":
                        content = reader.ReadElementContentAsString();
                        Author = (string.IsNullOrWhiteSpace(Author) ? content : Author);
                        break;

                    case "email":
                        content = reader.ReadElementContentAsString();
                        Email = (string.IsNullOrWhiteSpace(Email) ? content : Email);
                        break;

                    case "homepage":
                        content = reader.ReadElementContentAsString();
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? content : Homepage);
                        break;

                    case "url":
                        content = reader.ReadElementContentAsString();
                        Url = (string.IsNullOrWhiteSpace(Url) ? content : Url);
                        break;

                    case "comment":
                        content = reader.ReadElementContentAsString();
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? content : Comment);
                        break;

                    case "type": // This is exclusive to TruRip XML
                        content = reader.ReadElementContentAsString();
                        Type = (string.IsNullOrWhiteSpace(Type) ? content : Type);
                        superdat = superdat || content.Contains("SuperDAT");
                        break;

                    case "clrmamepro":
                        if (string.IsNullOrWhiteSpace(Header))
                            Header = reader.GetAttribute("header");

                        if (ForceMerging == ForceMerging.None)
                            ForceMerging = Utilities.GetForceMerging(reader.GetAttribute("forcemerging"));

                        if (ForceNodump == ForceNodump.None)
                            ForceNodump = Utilities.GetForceNodump(reader.GetAttribute("forcenodump"));

                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = Utilities.GetForcePacking(reader.GetAttribute("forcepacking"));

                        reader.Read();
                        break;

                    case "romcenter":
                        if (reader.GetAttribute("plugin") != null)
                        {
                            // CDATA
                        }

                        if (reader.GetAttribute("rommode") != null)
                        {
                            // (merged|split|unmerged) "split"
                        }

                        if (reader.GetAttribute("biosmode") != null)
                        {
                            // merged|split|unmerged) "split"
                        }

                        if (reader.GetAttribute("samplemode") != null)
                        {
                            // (merged|unmerged) "merged"
                        }

                        if (reader.GetAttribute("lockrommode") != null)
                        {
                            // (yes|no) "no"
                        }

                        if (reader.GetAttribute("lockbiosmode") != null)
                        {
                            // (yes|no) "no"
                        }

                        if (reader.GetAttribute("locksamplemode") != null)
                        {
                            // (yes|no) "no"
                        }

                        reader.Read();
                        break;
                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read game/machine information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the machine</param>
        /// <param name="dirs">List of dirs to prepend to the game name</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadMachine(
            XmlReader reader,
            List<string> dirs,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            // Create a new machine
            MachineType machineType = MachineType.NULL;
            if (Utilities.GetYesNo(reader.GetAttribute("isbios")) == true)
                machineType |= MachineType.Bios;

            if (Utilities.GetYesNo(reader.GetAttribute("isdevice")) == true) // Listxml-specific, used by older DATs
                machineType |= MachineType.Device;
            
            if (Utilities.GetYesNo(reader.GetAttribute("ismechanical")) == true) // Listxml-specific, used by older DATs
                machineType |= MachineType.Mechanical;

            string dirsString = (dirs != null && dirs.Count() > 0 ? string.Join("/", dirs) + "/" : string.Empty);
            Machine machine = new Machine
            {
                Name = dirsString + reader.GetAttribute("name"),
                Description = dirsString + reader.GetAttribute("name"),
                SourceFile = reader.GetAttribute("sourcefile"),
                Board = reader.GetAttribute("board"),
                RebuildTo = reader.GetAttribute("rebuildto"),
                Runnable = Utilities.GetYesNo(reader.GetAttribute("runnable")), // Listxml-specific, used by older DATs

                Comment = string.Empty,

                CloneOf = reader.GetAttribute("cloneof") ?? string.Empty,
                RomOf = reader.GetAttribute("romof") ?? string.Empty,
                SampleOf = reader.GetAttribute("sampleof") ?? string.Empty,

                MachineType = (machineType == MachineType.NULL ? MachineType.None : machineType),
            };

            if (Type == "SuperDAT" && !keep)
            {
                string tempout = Regex.Match(machine.Name, @".*?\\(.*)").Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(tempout))
                    machine.Name = tempout;
            }

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the roms from the machine
                switch (reader.Name)
                {
                    case "comment": // There can be multiple comments by spec
                        machine.Comment += reader.ReadElementContentAsString();
                        break;

                    case "description":
                        machine.Description = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "manufacturer":
                        machine.Manufacturer = reader.ReadElementContentAsString();
                        break;

                    case "publisher": // Not technically supported but used by some legacy DATs
                        machine.Publisher = reader.ReadElementContentAsString();
                        break;

                    case "trurip": // This is special metadata unique to TruRip
                        ReadTruRip(reader.ReadSubtree(), machine);

                        // Skip the trurip node now that we've processed it
                        reader.Skip();
                        break;

                    case "release":
                        containsItems = true;

                        DatItem release = new Release
                        {
                            Name = reader.GetAttribute("name"),
                            Region = reader.GetAttribute("region"),
                            Language = reader.GetAttribute("language"),
                            Date = reader.GetAttribute("date"),
                            Default = Utilities.GetYesNo(reader.GetAttribute("default")),
                        };

                        release.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(release, clean, remUnicode);

                        reader.Read();
                        break;

                    case "biosset":
                        containsItems = true;

                        DatItem biosset = new BiosSet
                        {
                            Name = reader.GetAttribute("name"),
                            Description = reader.GetAttribute("description"),
                            Default = Utilities.GetYesNo(reader.GetAttribute("default")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        biosset.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(biosset, clean, remUnicode);

                        reader.Read();
                        break;

                    case "rom":
                        containsItems = true;

                        DatItem rom = new Rom
                        {
                            Name = reader.GetAttribute("name"),
                            Size = Utilities.GetSize(reader.GetAttribute("size")),
                            CRC = Utilities.CleanHashData(reader.GetAttribute("crc"), Constants.CRCLength),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            MergeTag = reader.GetAttribute("merge"),
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            Date = Utilities.GetDate(reader.GetAttribute("date")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        rom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(rom, clean, remUnicode);

                        reader.Read();
                        break;

                    case "disk":
                        containsItems = true;

                        DatItem disk = new Disk
                        {
                            Name = reader.GetAttribute("name"),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            MergeTag = reader.GetAttribute("merge"),
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        disk.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(disk, clean, remUnicode);

                        reader.Read();
                        break;

                    case "sample":
                        containsItems = true;

                        DatItem samplerom = new Sample
                        {
                            Name = reader.GetAttribute("name"),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        samplerom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(samplerom, clean, remUnicode);

                        reader.Read();
                        break;

                    case "archive":
                        containsItems = true;

                        DatItem archiverom = new Archive
                        {
                            Name = reader.GetAttribute("name"),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        archiverom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(archiverom, clean, remUnicode);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
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
        /// Read TruRip information
        /// </summary>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        private void ReadTruRip(XmlReader reader, Machine machine)
        {
            // If we have an empty trurip, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the information from the trurip
                string content = string.Empty;
                switch (reader.Name)
                {
                    case "titleid":
                        content = reader.ReadElementContentAsString();
                        // string titleid = content;
                        break;

                    case "publisher":
                        machine.Publisher = reader.ReadElementContentAsString();
                        break;

                    case "developer": // Manufacturer is as close as this gets
                        machine.Manufacturer = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "genre":
                        content = reader.ReadElementContentAsString();
                        // string genre = content;
                        break;

                    case "subgenre":
                        content = reader.ReadElementContentAsString();
                        // string subgenre = content;
                        break;

                    case "ratings":
                        content = reader.ReadElementContentAsString();
                        // string ratings = content;
                        break;

                    case "score":
                        content = reader.ReadElementContentAsString();
                        // string score = content;
                        break;

                    case "players":
                        content = reader.ReadElementContentAsString();
                        // string players = content;
                        break;

                    case "enabled":
                        content = reader.ReadElementContentAsString();
                        // string enabled = content;
                        break;

                    case "crc":
                        content = reader.ReadElementContentAsString();
                        // string crc = Utilities.GetYesNo(content);
                        break;

                    case "source":
                        machine.SourceFile = reader.ReadElementContentAsString();
                        break;

                    case "cloneof":
                        machine.CloneOf = reader.ReadElementContentAsString();
                        break;

                    case "relatedto":
                        content = reader.ReadElementContentAsString();
                        // string relatedto = content;
                        break;

                    default:
                        reader.Read();
                        break;
                }
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

                XmlTextWriter xtw = new XmlTextWriter(fs, new UTF8Encoding(false));
                xtw.Formatting = Formatting.Indented;

                // Write out the header
                WriteHeader(xtw);

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
                            WriteEndGame(xtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(xtw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

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
                        WriteDatItem(xtw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                xtw.Dispose();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(XmlTextWriter xtw)
        {
            try
            {
                xtw.WriteStartDocument();
                xtw.WriteDocType("datafile", "-//Logiqx//DTD ROM Management Datafile//EN", "http://www.logiqx.com/Dats/datafile.dtd", null);
                
                xtw.WriteStartElement("datafile");
                
                xtw.WriteStartElement("header");
                xtw.WriteElementString("name", Name);
                xtw.WriteElementString("description", Description);
                if (!string.IsNullOrWhiteSpace(RootDir))
                    xtw.WriteElementString("rootdir", RootDir);
                if (!string.IsNullOrWhiteSpace(Category))
                    xtw.WriteElementString("category", Category);
                xtw.WriteElementString("version", Version);
                if (!string.IsNullOrWhiteSpace(Date))
                    xtw.WriteElementString("date", Date);
                xtw.WriteElementString("author", Author);
                if (!string.IsNullOrWhiteSpace(Email))
                    xtw.WriteElementString("email", Email);
                if (!string.IsNullOrWhiteSpace(Homepage))
                    xtw.WriteElementString("homepage", Homepage);
                if (!string.IsNullOrWhiteSpace(Url))
                    xtw.WriteElementString("url", Url);
                if (!string.IsNullOrWhiteSpace(Comment))
                    xtw.WriteElementString("comment", Comment);
                if (!string.IsNullOrWhiteSpace(Type))
                    xtw.WriteElementString("type", Type);

                if (ForcePacking != ForcePacking.None
                    || ForceMerging != ForceMerging.None
                    || ForceNodump != ForceNodump.None
                    || !string.IsNullOrWhiteSpace(Header))
                {
                    xtw.WriteStartElement("clrmamepro");
                    switch (ForcePacking)
                    {
                        case ForcePacking.Unzip:
                            xtw.WriteAttributeString("forcepacking", "unzip");
                            break;
                        case ForcePacking.Zip:
                            xtw.WriteAttributeString("forcepacking", "zip");
                            break;
                    }

                    switch (ForceMerging)
                    {
                        case ForceMerging.Full:
                            xtw.WriteAttributeString("forcemerging", "full");
                            break;
                        case ForceMerging.Split:
                            xtw.WriteAttributeString("forcemerging", "split");
                            break;
                        case ForceMerging.Merged:
                            xtw.WriteAttributeString("forcemerging", "merged");
                            break;
                        case ForceMerging.NonMerged:
                            xtw.WriteAttributeString("forcemerging", "nonmerged");
                            break;
                    }

                    switch (ForceNodump)
                    {
                        case ForceNodump.Ignore:
                            xtw.WriteAttributeString("forcenodump", "ignore");
                            break;
                        case ForceNodump.Obsolete:
                            xtw.WriteAttributeString("forcenodump", "obsolete");
                            break;
                        case ForceNodump.Required:
                            xtw.WriteAttributeString("forcenodump", "required");
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(Header))
                        xtw.WriteAttributeString("header", Header);

                    xtw.WriteEndElement();
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(XmlTextWriter xtw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                xtw.WriteStartElement(_deprecated ? "game" : "machine");
                xtw.WriteAttributeString("name", datItem.GetField(Field.MachineName, ExcludeFields));
                if (!ExcludeFields[(int)Field.MachineType])
                {
                    if ((datItem.MachineType & MachineType.Bios) != 0)
                        xtw.WriteAttributeString("isbios", "yes");
                    if ((datItem.MachineType & MachineType.Device) != 0)
                        xtw.WriteAttributeString("isdevice", "yes");
                    if ((datItem.MachineType & MachineType.Mechanical) != 0)
                        xtw.WriteAttributeString("ismechanical", "yes");
                }

                if (!ExcludeFields[(int)Field.Runnable] && datItem.Runnable != null)
                {
                    if (datItem.Runnable == true)
                        xtw.WriteAttributeString("runnable", "yes");
                    else if (datItem.Runnable == false)
                        xtw.WriteAttributeString("runnable", "no");
                }

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.CloneOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteAttributeString("cloneof", datItem.CloneOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RomOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.RomOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteAttributeString("romof", datItem.RomOf);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.SampleOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteAttributeString("sampleof", datItem.SampleOf);

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Comment, ExcludeFields)))
                    xtw.WriteElementString("comment", datItem.Comment);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                    xtw.WriteElementString("description", datItem.MachineDescription);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, ExcludeFields)))
                    xtw.WriteElementString("year", datItem.Year);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Publisher, ExcludeFields)))
                    xtw.WriteElementString("publisher", datItem.Publisher);
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Manufacturer, ExcludeFields)))
                    xtw.WriteElementString("manufacturer", datItem.Manufacturer);

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(XmlTextWriter xtw)
        {
            try
            {
                // End machine
                xtw.WriteEndElement();

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(XmlTextWriter xtw, DatItem datItem, bool ignoreblanks = false)
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
                        xtw.WriteStartElement("archive");
                        xtw.WriteAttributeString("name", datItem.GetField(Field.Name, ExcludeFields));
                        xtw.WriteEndElement();
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        xtw.WriteStartElement("biosset");
                        xtw.WriteAttributeString("name", biosSet.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.BiosDescription, ExcludeFields)))
                            xtw.WriteAttributeString("description", biosSet.Description);
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            xtw.WriteAttributeString("default", biosSet.Default.ToString().ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        xtw.WriteStartElement("disk");
                        xtw.WriteAttributeString("name", disk.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            xtw.WriteAttributeString("md5", disk.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            xtw.WriteAttributeString("ripemd160", disk.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            xtw.WriteAttributeString("sha1", disk.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            xtw.WriteAttributeString("sha256", disk.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            xtw.WriteAttributeString("sha384", disk.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            xtw.WriteAttributeString("sha512", disk.SHA512.ToLowerInvariant());
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            xtw.WriteAttributeString("status", disk.ItemStatus.ToString().ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        xtw.WriteStartElement("release");
                        xtw.WriteAttributeString("name", release.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Region, ExcludeFields)))
                            xtw.WriteAttributeString("region", release.Region);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Language, ExcludeFields)))
                            xtw.WriteAttributeString("language", release.Language);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            xtw.WriteAttributeString("date", release.Date);
                        if (!ExcludeFields[(int)Field.Default] && release.Default != null)
                            xtw.WriteAttributeString("default", release.Default.ToString().ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        xtw.WriteStartElement("rom");
                        xtw.WriteAttributeString("name", rom.GetField(Field.Name, ExcludeFields));
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            xtw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            xtw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                            xtw.WriteAttributeString("md5", rom.MD5.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RIPEMD160, ExcludeFields)))
                            xtw.WriteAttributeString("ripemd160", rom.RIPEMD160.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                            xtw.WriteAttributeString("sha1", rom.SHA1.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA256, ExcludeFields)))
                            xtw.WriteAttributeString("sha256", rom.SHA256.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA384, ExcludeFields)))
                            xtw.WriteAttributeString("sha384", rom.SHA384.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA512, ExcludeFields)))
                            xtw.WriteAttributeString("sha512", rom.SHA512.ToLowerInvariant());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, ExcludeFields)))
                            xtw.WriteAttributeString("date", rom.Date);
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            xtw.WriteAttributeString("status", rom.ItemStatus.ToString().ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Sample:
                        xtw.WriteStartElement("sample");
                        xtw.WriteAttributeString("name", datItem.GetField(Field.Name, ExcludeFields));
                        xtw.WriteEndElement();
                        break;
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(XmlTextWriter xtw)
        {
            try
            {
                // End machine
                xtw.WriteEndElement();

                // End datafile
                xtw.WriteEndElement();

                xtw.Flush();
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
