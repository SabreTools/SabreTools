using System;
using System.Collections.Generic;
using System.IO;
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
                Globals.Logger.Warning($"Exception found while parsing '{filename}': {ex}");

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

                            lastgame = rom.MachineName;
                            continue;
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
                xtw.WriteDocType("softwaredb", null, "softwaredb1.dtd", null);

                xtw.WriteStartElement("softwaredb");
                //xtw.WriteAttributeString("timestamp", timestamp);

                // TODO: Figure out how to fix the issue with removed formatting after this point
                //xtw.WriteComment("Credits");
                //xtw.WriteCData(@"The softwaredb.xml file contains information about rom mapper types

//Copyright 2003 Nicolas Beyaert (Initial Database)
//Copyright 2004-2013 BlueMSX Team
//Copyright 2005-2018 openMSX Team
//Generation MSXIDs by www.generation-msx.nl");

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
                xtw.WriteStartElement("software");
                xtw.WriteElementString("title", datItem.GetField(Field.MachineName, ExcludeFields));
                //xtw.WriteElementString("genmsxid", msxid);
                //xtw.WriteElementString("system", system));
                xtw.WriteElementString("company", datItem.GetField(Field.Manufacturer, ExcludeFields));
                xtw.WriteElementString("year", datItem.GetField(Field.Year, ExcludeFields));
                //xtw.WriteElementString("country", country);

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
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(XmlTextWriter xtw)
        {
            try
            {
                // End software
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
                    case ItemType.Rom: // Currently this encapsulates rom, megarom, and sccpluscart
                        var rom = datItem as Rom;
                        xtw.WriteStartElement("dump");

                        //xtw.WriteStartElement("original");
                        //xtw.WriteAttributeString("value", "true");
                        //xtw.WriteString("GoodMSX");
                        //xtw.WriteEndElement();

                        xtw.WriteStartElement("rom");
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Offset, ExcludeFields)))
                            xtw.WriteElementString("start", rom.Offset);
                        //xtw.WriteElementString("type", "Normal");
                        xtw.WriteElementString("hash", rom.GetField(Field.SHA1, ExcludeFields).ToLowerInvariant());
                        //xtw.WriteElementString("remark", "");

                        // End rom
                        xtw.WriteEndElement();

                        // End dump
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
                // End software
                xtw.WriteEndElement();

                // End softwaredb
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
