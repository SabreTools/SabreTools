using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
    /// Represents parsing and writing of a SofwareList, M1, or MAME XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class SoftwareList : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SoftwareList(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse an SofwareList XML DAT and return all found games and roms within
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
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        case "softwarelist":
                            Name = (string.IsNullOrWhiteSpace(Name) ? xtr.GetAttribute("name") ?? string.Empty : Name);
                            Description = (string.IsNullOrWhiteSpace(Description) ? xtr.GetAttribute("description") ?? string.Empty : Description);
                            if (ForceMerging == ForceMerging.None)
                                ForceMerging = Utilities.GetForceMerging(xtr.GetAttribute("forcemerging"));

                            if (ForceNodump == ForceNodump.None)
                                ForceNodump = Utilities.GetForceNodump(xtr.GetAttribute("forcenodump"));

                            if (ForcePacking == ForcePacking.None)
                                ForcePacking = Utilities.GetForcePacking(xtr.GetAttribute("forcepacking"));

                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the machine
                        case "software":
                            ReadSoftware(xtr.ReadSubtree(), filename, sysid, srcid, keep, clean, remUnicode);

                            // Skip the software now that we've processed it
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
        /// Read software information
        /// </summary>
        /// <param name="reader">XmlReader representing a software block</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadSoftware(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // If we have an empty software, skip it
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

            if (Utilities.GetYesNo(reader.GetAttribute("isdevice")) == true)
                machineType |= MachineType.Device;

            if (Utilities.GetYesNo(reader.GetAttribute("ismechanical")) == true)
                machineType |= MachineType.Mechanical;

            Machine machine = new Machine
            {
                Name = reader.GetAttribute("name"),
                Description = reader.GetAttribute("name"),
                Supported = Utilities.GetYesNo(reader.GetAttribute("supported")), // (yes|partial|no) "yes"

                CloneOf = reader.GetAttribute("cloneof") ?? string.Empty,
                Infos = new List<Tuple<string, string>>(),

                MachineType = (machineType == MachineType.NULL ? MachineType.None : machineType),
            };

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "description":
                        machine.Description = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "publisher":
                        machine.Publisher = reader.ReadElementContentAsString();
                        break;

                    case "info":
                        machine.Infos.Add(new Tuple<string, string>(reader.GetAttribute("name"), reader.GetAttribute("value")));
                        reader.Read();
                        break;

                    case "sharedfeat":
                        // string sharedfeat_name = reader.GetAttribute("name");
                        // string sharedfeat_value = reader.GetAttribute("value");
                        reader.Read();
                        break;

                    case "part": // Contains all rom and disk information
                        containsItems = ReadPart(reader.ReadSubtree(), machine, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the part now that we've processed it
                        reader.Skip();
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
        /// Read part information
        /// </summary>
        /// <param name="reader">XmlReader representing a part block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadPart(
            XmlReader reader,
            Machine machine,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty, areaname = string.Empty, partname = string.Empty, partinterface = string.Empty;
            string temptype = reader.Name;
            long? areasize = null;
            List<Tuple<string, string>> features = new List<Tuple<string, string>>();
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "part")
                    {
                        partname = string.Empty;
                        partinterface = string.Empty;
                        features = new List<Tuple<string, string>>();
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && (reader.Name == "dataarea" || reader.Name == "diskarea"))
                    {
                        areaname = string.Empty;
                        areasize = null;
                    }

                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "part":
                        partname = reader.GetAttribute("name");
                        partinterface = reader.GetAttribute("interface");
                        reader.Read();
                        break;

                    case "feature":
                        features.Add(new Tuple<string, string>(reader.GetAttribute("name"), reader.GetAttribute("feature")));
                        reader.Read();
                        break;

                    case "dataarea":
                        areaname = reader.GetAttribute("name");
                        if (reader.GetAttribute("size") != null)
                        {
                            if (Int64.TryParse(reader.GetAttribute("size"), out long tempas))
                                areasize = tempas;
                        }

                        // string dataarea_width = reader.GetAttribute("width"); // (8|16|32|64) "8"
                        // string dataarea_endianness = reader.GetAttribute("endianness"); // endianness (big|little) "little"

                        containsItems = ReadDataArea(reader.ReadSubtree(), machine, features, areaname, areasize, 
                            partname, partinterface, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the dataarea now that we've processed it
                        reader.Skip();
                        break;

                    case "diskarea":
                        areaname = reader.GetAttribute("name");

                        containsItems = ReadDiskArea(reader.ReadSubtree(), machine, features, areaname, areasize,
                            partname, partinterface, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the diskarea now that we've processed it
                        reader.Skip();
                        break;

                    case "dipswitch":
                        // string dipswitch_name = reader.GetAttribute("name");
                        // string dipswitch_tag = reader.GetAttribute("tag");
                        // string dipswitch_mask = reader.GetAttribute("mask");

                        // For every <dipvalue> element...
                        // string dipvalue_name = reader.GetAttribute("name");
                        // string dipvalue_value = reader.GetAttribute("value");
                        // bool? dipvalue_default = Utilities.GetYesNo(reader.GetAttribute("default")); // (yes|no) "no"

                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
        }

        /// <summary>
        /// Read dataarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a dataarea block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="features">List of features from the parent part</param>
        /// <param name="areaname">Name of the containing area</param>
        /// <param name="areasize">Size of the containing area</param>
        /// <param name="partname">Name of the containing part</param>
        /// <param name="partinterface">Interface of the containing part</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDataArea(
            XmlReader reader,
            Machine machine,
            List<Tuple<string, string>> features,
            string areaname,
            long? areasize,
            string partname,
            string partinterface,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "rom":
                        containsItems = true;

                        // If the rom is continue or ignore, add the size to the previous rom
                        if (reader.GetAttribute("loadflag") == "continue" || reader.GetAttribute("loadflag") == "ignore")
                        {
                            int index = this[key].Count - 1;
                            DatItem lastrom = this[key][index];
                            if (lastrom.ItemType == ItemType.Rom)
                            {
                                ((Rom)lastrom).Size += Utilities.GetSize(reader.GetAttribute("size"));
                            }
                            this[key].RemoveAt(index);
                            this[key].Add(lastrom);
                            reader.Read();
                            continue;
                        }

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
                            Offset = reader.GetAttribute("offset"),
                            // Value = reader.GetAttribute("value");
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            // LoadFlag = reader.GetAttribute("loadflag"), // (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore)

                            AreaName = areaname,
                            AreaSize = areasize,
                            Features = features,
                            PartName = partname,
                            PartInterface = partinterface,

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        rom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(rom, clean, remUnicode);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
        }

        /// <summary>
        /// Read diskarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="features">List of features from the parent part</param>
        /// <param name="areaname">Name of the containing area</param>
        /// <param name="areasize">Size of the containing area</param>
        /// <param name="partname">Name of the containing part</param>
        /// <param name="partinterface">Interface of the containing part</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDiskArea(
            XmlReader reader,
            Machine machine,
            List<Tuple<string, string>> features,
            string areaname,
            long? areasize,
            string partname,
            string partinterface,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
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
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            Writable = Utilities.GetYesNo(reader.GetAttribute("writable")),

                            AreaName = areaname,
                            AreaSize = areasize,
                            Features = features,
                            PartName = partname,
                            PartInterface = partinterface,

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        disk.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(disk, clean, remUnicode);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return containsItems;
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

                            lastgame = rom.MachineName;
                            continue;
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
                string header = "<?xml version=\"1.0\"?>\n" +
                            "<!DOCTYPE softwarelist SYSTEM \"softwarelist.dtd\">\n\n" +
                            "<softwarelist name=\"" + WebUtility.HtmlEncode(Name) + "\"" +
                                " description=\"" + WebUtility.HtmlEncode(Description) + "\"" +
                                (ForcePacking == ForcePacking.Unzip ? " forcepacking=\"unzip\"" : string.Empty) +
                                (ForcePacking == ForcePacking.Zip ? " forcepacking=\"zip\"" : string.Empty) +
                                (ForceMerging == ForceMerging.Full ? " forcemerging=\"full\"" : string.Empty) +
                                (ForceMerging == ForceMerging.Split ? " forcemerging=\"split\"" : string.Empty) +
                                (ForceMerging == ForceMerging.Merged ? " forcemerging=\"merged\"" : string.Empty) +
                                (ForceMerging == ForceMerging.NonMerged ? " forcemerging=\"nonmerged\"" : string.Empty) +
                                (ForceNodump == ForceNodump.Ignore ? " forcenodump=\"ignore\"" : string.Empty) +
                                (ForceNodump == ForceNodump.Obsolete ? " forcenodump=\"obsolete\"" : string.Empty) +
                                (ForceNodump == ForceNodump.Required ? " forcenodump=\"required\"" : string.Empty) +
                                ">\n\n";

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

                string state = "\t<software name=\"" + (!ExcludeFields[(int)Field.MachineName] ? WebUtility.HtmlEncode(rom.MachineName) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.CloneOf] && !string.IsNullOrWhiteSpace(rom.CloneOf) && (rom.MachineName.ToLowerInvariant() != rom.CloneOf.ToLowerInvariant())
                                ? " cloneof=\"" + WebUtility.HtmlEncode(rom.CloneOf) + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Supported] ? " supported=\"" + (rom.Supported == true ? "yes" : rom.Supported == false ? "no" : "partial") + "\">\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Description] ? "\t\t<description>" + WebUtility.HtmlEncode(rom.MachineDescription) + "</description>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Year] && rom.Year != null ? "\t\t<year>" + WebUtility.HtmlEncode(rom.Year) + "</year>\n" : string.Empty)
                            + (!ExcludeFields[(int)Field.Publisher] && rom.Publisher != null ? "\t\t<publisher>" + WebUtility.HtmlEncode(rom.Publisher) + "</publisher>\n" : string.Empty);

                if (!ExcludeFields[(int)Field.Infos])
                {
                    foreach (Tuple<string, string> kvp in rom.Infos)
                    {
                        state += "\t\t<info name=\"" + WebUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + WebUtility.HtmlEncode(kvp.Item2) + "\" />\n";
                    }
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
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw)
        {
            try
            {
                string state = "\t</software>\n\n";

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

                state += "\t\t<part name=\"" + (!ExcludeFields[(int)Field.PartName] ? rom.PartName : string.Empty) + "\" interface=\""
                    + (!ExcludeFields[(int)Field.PartInterface] ? rom.PartInterface : string.Empty) + "\">\n";

                if (!ExcludeFields[(int)Field.Features])
                {
                    foreach (Tuple<string, string> kvp in rom.Features)
                    {
                        state += "\t\t\t<feature name=\"" + WebUtility.HtmlEncode(kvp.Item1) + "\" value=\"" + WebUtility.HtmlEncode(kvp.Item2) + "\"/>\n";
                    }
                }

                switch (rom.ItemType)
                {
                    case ItemType.Disk:
                        state += "\t\t\t<diskarea name=\"" + (!ExcludeFields[(int)Field.AreaName] ? (string.IsNullOrWhiteSpace(rom.AreaName) ? "cdrom" : rom.AreaName) : string.Empty) + "\""
                                + (!ExcludeFields[(int)Field.AreaSize] && rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : string.Empty) + ">\n"
                            + "\t\t\t\t<disk name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(((Disk)rom).MD5) ? " md5=\"" + ((Disk)rom).MD5.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(((Disk)rom).RIPEMD160) ? " ripemd160=\"" + ((Disk)rom).RIPEMD160.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA1) ? " sha1=\"" + ((Disk)rom).SHA1.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA256) ? " sha256=\"" + ((Disk)rom).SHA256.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA384) ? " sha384=\"" + ((Disk)rom).SHA384.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(((Disk)rom).SHA512) ? " sha512=\"" + ((Disk)rom).SHA512.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Status] && ((Disk)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Disk)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Writable] && ((Disk)rom).Writable != null ? " writable=\"" + (((Disk)rom).Writable == true ? "yes" : "no") + "\"" : string.Empty)
                            + "/>\n"
                            + "\t\t\t</diskarea>\n";
                        break;
                    case ItemType.Rom:
                        state += "\t\t\t<dataarea name=\"" + (!ExcludeFields[(int)Field.AreaName] ? (string.IsNullOrWhiteSpace(rom.AreaName) ? "rom" : rom.AreaName) : string.Empty) + "\""
                                + (!ExcludeFields[(int)Field.AreaSize] && rom.AreaSize != null ? " size=\"" + rom.AreaSize + "\"" : string.Empty) + ">\n"
                            + "\t\t\t\t<rom name=\"" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "\""
                            + (!ExcludeFields[(int)Field.Size] && ((Rom)rom).Size != -1 ? " size=\"" + ((Rom)rom).Size + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.CRC] && !string.IsNullOrWhiteSpace(((Rom)rom).CRC) ? " crc=\"" + ((Rom)rom).CRC.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(((Rom)rom).MD5) ? " md5=\"" + ((Rom)rom).MD5.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(((Rom)rom).RIPEMD160) ? " ripemd160=\"" + ((Rom)rom).RIPEMD160.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA1) ? " sha1=\"" + ((Rom)rom).SHA1.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA256) ? " sha256=\"" + ((Rom)rom).SHA256.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA384) ? " sha384=\"" + ((Rom)rom).SHA384.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(((Rom)rom).SHA512) ? " sha512=\"" + ((Rom)rom).SHA512.ToLowerInvariant() + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Offset] && !string.IsNullOrWhiteSpace(((Rom)rom).Offset) ? " offset=\"" + ((Rom)rom).Offset + "\"" : string.Empty)
                            // + (!ExcludeFields[(int)Field.Value] && !string.IsNullOrWhiteSpace(((Rom)rom).Value) ? " value=\"" + ((Rom)rom).Value + "\"" : string.Empty)
                            + (!ExcludeFields[(int)Field.Status] && ((Rom)rom).ItemStatus != ItemStatus.None ? " status=\"" + ((Rom)rom).ItemStatus.ToString().ToLowerInvariant() + "\"" : string.Empty)
                            // + (!ExcludeFields[(int)Field.Loadflag] && !string.IsNullOrWhiteSpace(((Rom)rom).Loadflag) ? " loadflag=\"" + ((Rom)rom).Loadflag + "\"" : string.Empty)
                            + "/>\n"
                            + "\t\t\t</dataarea>\n";
                        break;
                }

                state += "\t\t</part>\n";

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
                string footer = "\t</software>\n\n</softwarelist>\n";

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
