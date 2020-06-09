using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a openMSX softawre list XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class OpenMSX : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public OpenMSX(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a openMSX softawre list XML DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <remarks>
        /// </remarks>
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
                        case "softwaredb":
                            Name = (string.IsNullOrWhiteSpace(Name) ? "openMSX Software List" : Name);
                            Description = (string.IsNullOrWhiteSpace(Description) ? Name : Name);
                            // string timestamp = xtr.GetAttribute("timestamp"); // CDATA
                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the software
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
        /// <param name="reader">XmlReader representing a machine block</param>
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
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            int diskno = 0;
            bool containsItems = false;

            // Create a new machine
            Machine machine = new Machine();

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
                    case "title":
                        machine.Name = reader.ReadElementContentAsString();
                        break;

                    case "genmsxid":
                        // string id = reader.ReadElementContentAsString();
                        reader.Read();
                        break;

                    case "system":
                        // string system = reader.ReadElementContentAsString();
                        reader.Read();
                        break;

                    case "company":
                        machine.Manufacturer = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "country":
                        // string country = reader.ReadElementContentAsString();
                        reader.Read();
                        break;

                    case "dump":
                        containsItems = ReadDump(reader.ReadSubtree(), machine, diskno, filename, sysid, srcid, keep, clean, remUnicode);
                        diskno++;

                        // Skip the dump now that we've processed it
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
        /// Read dump information
        /// </summary>
        /// <param name="reader">XmlReader representing a part block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="diskno">Disk number to use when outputting to other DAT formats</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDump(
            XmlReader reader,
            Machine machine,
            int diskno,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the dump
                switch (reader.Name)
                {
                    case "rom":
                        containsItems = ReadRom(reader.ReadSubtree(), machine, diskno, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the rom now that we've processed it
                        reader.Skip();
                        break;

                    case "megarom":
                        containsItems = ReadMegaRom(reader.ReadSubtree(), machine, diskno, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the megarom now that we've processed it
                        reader.Skip();
                        break;

                    case "sccpluscart":
                        containsItems = ReadSccPlusCart(reader.ReadSubtree(), machine, diskno, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the sccpluscart now that we've processed it
                        reader.Skip();
                        break;

                    case "original":
                        // bool value = Utilities.GetYesNo(reader.GetAttribute("value");
                        // string original = reader.ReadElementContentAsString();
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
        /// Read rom information
        /// </summary>
        /// <param name="reader">XmlReader representing a rom block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="diskno">Disk number to use when outputting to other DAT formats</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadRom(
            XmlReader reader,
            Machine machine,
            int diskno,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string hash = string.Empty, offset = string.Empty, type = string.Empty, remark = string.Empty;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the rom
                switch (reader.Name)
                {
                    case "hash":
                        containsItems = true;
                        hash = reader.ReadElementContentAsString();
                        break;

                    case "start":
                        offset = reader.ReadElementContentAsString();
                        break;

                    case "type":
                        type = reader.ReadElementContentAsString();
                        break;

                    case "remark":
                        remark = reader.ReadElementContentAsString();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Create and add the new rom
            Rom rom = new Rom
            {
                Name = machine.Name + "_" + diskno + (!string.IsNullOrWhiteSpace(remark) ? " " + remark : string.Empty),
                Offset = offset,
                Size = -1,
                SHA1 = Utilities.CleanHashData(hash, Constants.SHA1Length),
            };

            rom.CopyMachineInformation(machine);
            ParseAddHelper(rom, clean, remUnicode);

            return containsItems;
        }

        /// <summary>
        /// Read megarom information
        /// </summary>
        /// <param name="reader">XmlReader representing a megarom block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="diskno">Disk number to use when outputting to other DAT formats</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadMegaRom(
            XmlReader reader,
            Machine machine,
            int diskno,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string hash = string.Empty, offset = string.Empty, type = string.Empty, remark = string.Empty;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the dump
                switch (reader.Name)
                {
                    case "hash":
                        containsItems = true;
                        hash = reader.ReadElementContentAsString();
                        break;

                    case "start":
                        offset = reader.ReadElementContentAsString();
                        break;

                    case "type":
                        type = reader.ReadElementContentAsString();
                        break;

                    case "remark":
                        remark = reader.ReadElementContentAsString();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Create and add the new rom
            Rom rom = new Rom
            {
                Name = machine.Name + "_" + diskno + (!string.IsNullOrWhiteSpace(remark) ? " " + remark : string.Empty),
                Offset = offset,
                Size = -1,
                SHA1 = Utilities.CleanHashData(hash, Constants.SHA1Length),
            };

            rom.CopyMachineInformation(machine);
            ParseAddHelper(rom, clean, remUnicode);

            return containsItems;
        }

        /// <summary>
        /// Read sccpluscart information
        /// </summary>
        /// <param name="reader">XmlReader representing a sccpluscart block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="diskno">Disk number to use when outputting to other DAT formats</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadSccPlusCart(
            XmlReader reader,
            Machine machine,
            int diskno,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            string hash = string.Empty, boot = string.Empty, remark = string.Empty;
            bool containsItems = false;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the dump
                switch (reader.Name)
                {
                    case "boot":
                        boot = reader.ReadElementContentAsString();
                        break;

                    case "hash":
                        containsItems = true;
                        hash = reader.ReadElementContentAsString();
                        break;

                    case "remark":
                        remark = reader.ReadElementContentAsString();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Create and add the new rom
            Rom rom = new Rom
            {
                Name = machine.Name + "_" + diskno + (!string.IsNullOrWhiteSpace(remark) ? " " + remark : string.Empty),
                Size = -1,
                SHA1 = Utilities.CleanHashData(hash, Constants.SHA1Length),
            };

            rom.CopyMachineInformation(machine);
            ParseAddHelper(rom, clean, remUnicode);

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
                            "<!DOCTYPE softwaredb SYSTEM \"softwaredb1.dtd\">\n" + 
                            "<softwaredb" +
                                // " timestamp=\"" + timestamp + "\"" +
                                ">\n" +
                                @"<!-- Credits -->
<![CDATA[
The softwaredb.xml file contains information about rom mapper types

Copyright 2003 Nicolas Beyaert (Initial Database)
Copyright 2004-2013 BlueMSX Team
Copyright 2005-2018 openMSX Team
Generation MSXIDs by www.generation-msx.nl

]]>
";

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

                string state = "<software>\n"
                            + "\t<title>" + (!ExcludeFields[(int)Field.MachineName] ? WebUtility.HtmlEncode(rom.MachineName) : string.Empty) + "</title>\n"
                            // + "\t<genmsxid>" + msxid + "</genmsxid>\n"
                            // + "\t<system>" + system + "</system>\n"
                            + "\t<company>" + (!ExcludeFields[(int)Field.Manufacturer] ? rom.Manufacturer : string.Empty) + "</company>\n"
                            + "\t<year>" + (!ExcludeFields[(int)Field.Year] ? rom.Year : string.Empty) + "</year>\n";
                            // + "\t<country>" + country + "</country>\n";

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
                string state = "</software>\n";

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
                        break;

                    case ItemType.BiosSet:
                        break;

                    case ItemType.Disk:
                        break;

                    case ItemType.Release:
                        break;

                    case ItemType.Rom: // Currently this encapsulates rom, megarom, and sccpluscart
                        state += "\t\t<dump>"
                            // + "<original value=\"true\">GoodMSX</original>"
                            + "<rom>"
                            + (!ExcludeFields[(int)Field.Offset] && !string.IsNullOrWhiteSpace(((Rom)rom).Offset) ? "<start>" + ((Rom)rom).Offset + "</start>" : string.Empty)
                            // + "<type>Normal</type>"
                            + "<hash>" + (!ExcludeFields[(int)Field.SHA1] ? ((Rom)rom).SHA1 : string.Empty) + "</hash>"
                            // + "<remark></remark>"
                            + "</rom></dump>\n";
                            break;

                    case ItemType.Sample:
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
                string footer = "</software>\n</softwaredb>\n";

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
