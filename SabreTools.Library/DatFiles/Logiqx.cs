using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using FileStream = System.IO.FileStream;
using StreamWriter = System.IO.StreamWriter;
#endif
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
        bool _deprecated;

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
                Globals.Logger.Warning("Exception found while parsing '{0}': {1}", filename, ex);

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
                Globals.Logger.User("Opening file for writing: {0}", outfile);
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning("File '{0}' could not be created for writing! Please check to see if the file is writable", outfile);
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
                            WriteEndGame(sw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(sw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose("Empty folder found: {0}", rom.MachineName);

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

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
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
                string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">\n\n" +
                            "<datafile>\n" +
                            "\t<header>\n" +
                            "\t\t<name>" + WebUtility.HtmlEncode(Name) + "</name>\n" +
                            "\t\t<description>" + WebUtility.HtmlEncode(Description) + "</description>\n" +
                            (!string.IsNullOrWhiteSpace(RootDir) ? "\t\t<rootdir>" + WebUtility.HtmlEncode(RootDir) + "</rootdir>\n" : string.Empty) +
                            (!string.IsNullOrWhiteSpace(Category) ? "\t\t<category>" + WebUtility.HtmlEncode(Category) + "</category>\n" : string.Empty) +
                            "\t\t<version>" + WebUtility.HtmlEncode(Version) + "</version>\n" +
                            (!string.IsNullOrWhiteSpace(Date) ? "\t\t<date>" + WebUtility.HtmlEncode(Date) + "</date>\n" : string.Empty) +
                            "\t\t<author>" + WebUtility.HtmlEncode(Author) + "</author>\n" +
                            (!string.IsNullOrWhiteSpace(Email) ? "\t\t<email>" + WebUtility.HtmlEncode(Email) + "</email>\n" : string.Empty) +
                            (!string.IsNullOrWhiteSpace(Homepage) ? "\t\t<homepage>" + WebUtility.HtmlEncode(Homepage) + "</homepage>\n" : string.Empty) +
                            (!string.IsNullOrWhiteSpace(Url) ? "\t\t<url>" + WebUtility.HtmlEncode(Url) + "</url>\n" : string.Empty) +
                            (!string.IsNullOrWhiteSpace(Comment) ? "\t\t<comment>" + WebUtility.HtmlEncode(Comment) + "</comment>\n" : string.Empty) +
                            (!string.IsNullOrWhiteSpace(Type) ? "\t\t<type>" + WebUtility.HtmlEncode(Type) + "</type>\n" : string.Empty) +
                            (ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None || !string.IsNullOrWhiteSpace(Header) ?
                                "\t\t<clrmamepro" +
                                    (ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : string.Empty) +
                                    (ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : string.Empty) +
                                    (ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : string.Empty) +
                                    (ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : string.Empty) +
                                    (ForceMerging == ForceMerging.Merged ? " forcemerging=\"merged\"" : string.Empty) +
                                    (ForceMerging == ForceMerging.NonMerged ? " forcemerging=\"nonmerged\"" : string.Empty) +
                                    (ForceNodump == ForceNodump.Ignore ? " forcenodump=\"ignore\"" : string.Empty) +
                                    (ForceNodump == ForceNodump.Obsolete ? " forcenodump=\"obsolete\"" : string.Empty) +
                                    (ForceNodump == ForceNodump.Required ? " forcenodump=\"required\"" : string.Empty) +
                                    (!string.IsNullOrWhiteSpace(Header) ? " header=\"" + Header + "\"" : string.Empty) +
                                    " />\n"
                            : string.Empty) +
                            "\t</header>\n";

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
        /// <param name="rom">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(StreamWriter sw, DatItem rom)
        {
            try
            {
                // No game should start with a path separator
                rom.MachineName = rom.MachineName.TrimStart(Path.DirectorySeparatorChar);

                string state = "\t<" + (_deprecated ? "game" : "machine") + " name=\"" + (!ExcludeFields[(int)Field.MachineName] ? WebUtility.HtmlEncode(rom.MachineName) : string.Empty) + "\""
                                + (!ExcludeFields[(int)Field.MachineType] && (rom.MachineType & MachineType.Bios) != 0 ? " isbios=\"yes\"" : string.Empty)
                                + (!ExcludeFields[(int)Field.MachineType] && (rom.MachineType & MachineType.Device) != 0 ? " isdevice=\"yes\"" : string.Empty)
                                + (!ExcludeFields[(int)Field.MachineType] && (rom.MachineType & MachineType.Mechanical) != 0 ? " ismechanical=\"yes\"" : string.Empty)
                                + (!ExcludeFields[(int)Field.Runnable] && rom.Runnable == true
                                    ? " runnable=\"yes\""
                                    : (!ExcludeFields[(int)Field.Runnable] && rom.Runnable == false ? " runnable=\"no\"" : string.Empty))
                                + (!ExcludeFields[(int)Field.CloneOf] && !string.IsNullOrWhiteSpace(rom.CloneOf) && (rom.MachineName.ToLowerInvariant() != rom.CloneOf.ToLowerInvariant())
                                    ? " cloneof=\"" + WebUtility.HtmlEncode(rom.CloneOf) + "\""
                                    : string.Empty)
                                + (!ExcludeFields[(int)Field.RomOf] && !string.IsNullOrWhiteSpace(rom.RomOf) && (rom.MachineName.ToLowerInvariant() != rom.RomOf.ToLowerInvariant())
                                    ? " romof=\"" + WebUtility.HtmlEncode(rom.RomOf) + "\""
                                    : string.Empty)
                                + (!ExcludeFields[(int)Field.SampleOf] && !string.IsNullOrWhiteSpace(rom.SampleOf) && (rom.MachineName.ToLowerInvariant() != rom.SampleOf.ToLowerInvariant())
                                    ? " sampleof=\"" + WebUtility.HtmlEncode(rom.SampleOf) + "\""
                                    : string.Empty)
                                + ">\n"
                            + (!ExcludeFields[(int)Field.Comment] && !string.IsNullOrWhiteSpace(rom.Comment) ? "\t\t<comment>" + WebUtility.HtmlEncode(rom.Comment) + "</comment>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Description] ? "\t\t<description>" + WebUtility.HtmlEncode((string.IsNullOrWhiteSpace(rom.MachineDescription) ? rom.MachineName : rom.MachineDescription)) + "</description>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Year] && !string.IsNullOrWhiteSpace(rom.Year) ? "\t\t<year>" + WebUtility.HtmlEncode(rom.Year) + "</year>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Publisher] && !string.IsNullOrWhiteSpace(rom.Publisher) ? "\t\t<publisher>" + WebUtility.HtmlEncode(rom.Publisher) + "</publisher>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Manufacturer] && !string.IsNullOrWhiteSpace(rom.Manufacturer) ? "\t\t<manufacturer>" + WebUtility.HtmlEncode(rom.Manufacturer) + "</manufacturer>\n" : string.Empty);

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
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw)
        {
            try
            {
                string state = "\t</" + (_deprecated ? "game" : "machine") + ">\n";

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
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="rom">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(StreamWriter sw, DatItem rom, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks
                && (rom.ItemType == ItemType.Rom
                && (((Rom)rom).Size == 0 || ((Rom)rom).Size == -1)))
            {
                return true;
            }

            try
            {
                string state = string.Empty;

                // Pre-process the item name
                ProcessItemName(rom, true);

                switch (rom.ItemType)
                {
                    case ItemType.Archive:
                        state += "\t\t<archive name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + "/>\n";
                        break;
                    case ItemType.BiosSet:
                        state += "\t\t<biosset name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.BiosDescription] && !string.IsNullOrWhiteSpace(((BiosSet)rom).Description) ? " description=\"" + WebUtility.HtmlEncode(((BiosSet)rom).Description) + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Default] && ((BiosSet)rom).Default != null
                                ? ((BiosSet)rom).Default.ToString().ToLowerInvariant()
                                : string.Empty)
                            + "/>\n";
                        break;
                    case ItemType.Disk:
                        state += "\t\t<disk name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(((Disk)rom).RIPEMD160) ? " ripemd160=\"" + ((Disk)rom).RIPEMD160.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256=\"" + ((Disk)rom).SHA256.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384=\"" + ((Disk)rom).SHA384.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512=\"" + ((Disk)rom).SHA512.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Status] && ((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : string.Empty)
                            + "/>\n";
                        break;
                    case ItemType.Release:
                        state += "\t\t<release name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.Region] && !string.IsNullOrWhiteSpace(((Release)rom).Region) ? " region=\"" + WebUtility.HtmlEncode(((Release)rom).Region) + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Language] && !string.IsNullOrWhiteSpace(((Release)rom).Language) ? " language=\"" + WebUtility.HtmlEncode(((Release)rom).Language) + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Date] && !string.IsNullOrWhiteSpace(((Release)rom).Date) ? " date=\"" + WebUtility.HtmlEncode(((Release)rom).Date) + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Default] && ((Release)rom).Default != null
                                ? ((Release)rom).Default.ToString().ToLowerInvariant()
                                : string.Empty)
                            + "/>\n";
                        break;
                    case ItemType.Rom:
                        state += "\t\t<rom name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.Size] && ((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.CRC] && !string.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(((Rom)rom).RIPEMD160) ? " ripemd160=\"" + ((Rom)rom).RIPEMD160.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256=\"" + ((Rom)rom).SHA256.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384=\"" + ((Rom)rom).SHA384.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512=\"" + ((Rom)rom).SHA512.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Date] && !string.IsNullOrWhiteSpace(((Rom)rom).Date) ? " date=\"" + ((Rom)rom).Date + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Status] && ((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : string.Empty)
                            + "/>\n";
                        break;
                    case ItemType.Sample:
                        state += "\t\t<sample name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + "/>\n";
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
                string footer = "\t</" + (_deprecated ? "game" : "machine") + ">\n</datafile>\n";

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
