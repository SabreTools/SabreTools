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
    /// Represents parsing and writing of an OfflineList XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class OfflineList : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public OfflineList(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse an OfflineList XML DAT and return all found games and roms within
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
                        case "configuration":
                            ReadConfiguration(xtr.ReadSubtree(), keep);

                            // Skip the configuration node now that we've processed it
                            xtr.Skip();
                            break;

                        case "games":
                            ReadGames(xtr.ReadSubtree(), keep, clean, remUnicode);

                            // Skip the games node now that we've processed it
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
        /// Read configuration information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadConfiguration(XmlReader reader, bool keep)
        {
            bool superdat = false;

            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all configuration items (ONLY OVERWRITE IF THERE'S NO DATA)
                string content = string.Empty;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "datname":
                        content = reader.ReadElementContentAsString();
                        Name = (string.IsNullOrWhiteSpace(Name) ? content : Name);
                        superdat = superdat || content.Contains(" - SuperDAT");
                        if (keep && superdat)
                        {
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
                        }
                        break;

                    case "datversion":
                        content = reader.ReadElementContentAsString();
                        Version = (string.IsNullOrWhiteSpace(Version) ? content : Version);
                        break;

                    case "system":
                        content = reader.ReadElementContentAsString();
                        // string system = content;
                        break;

                    case "screenshotswidth":
                        content = reader.ReadElementContentAsString();
                        // string screenshotsWidth = content; // Int32?
                        break;

                    case "screenshotsheight":
                        content = reader.ReadElementContentAsString();
                        // string screenshotsHeight = content; // Int32?
                        break;

                    case "infos":
                        ReadInfos(reader.ReadSubtree());

                        // Skip the infos node now that we've processed it
                        reader.Skip();
                        break;

                    case "canopen":
                        ReadCanOpen(reader.ReadSubtree());

                        // Skip the canopen node now that we've processed it
                        reader.Skip();
                        break;

                    case "newdat":
                        ReadNewDat(reader.ReadSubtree());

                        // Skip the newdat node now that we've processed it
                        reader.Skip();
                        break;

                    case "search":
                        ReadSearch(reader.ReadSubtree());

                        // Skip the search node now that we've processed it
                        reader.Skip();
                        break;

                    case "romtitle":
                        content = reader.ReadElementContentAsString();
                        // string romtitle = content;

                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read infos information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        private void ReadInfos(XmlReader reader)
        {
            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all infos items
                switch (reader.Name.ToLowerInvariant())
                {
                    case "title":
                        // string title_visible = reader.GetAttribute("visible"); // (true|false)
                        // string title_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string title_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "location":
                        // string location_visible = reader.GetAttribute("visible"); // (true|false)
                        // string location_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string location_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "publisher":
                        // string publisher_visible = reader.GetAttribute("visible"); // (true|false)
                        // string publisher_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string publisher_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "sourcerom":
                        // string sourceRom_visible = reader.GetAttribute("visible"); // (true|false)
                        // string sourceRom_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string sourceRom_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "savetype":
                        // string saveType_visible = reader.GetAttribute("visible"); // (true|false)
                        // string saveType_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string saveType_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "romsize":
                        // string romSize_visible = reader.GetAttribute("visible"); // (true|false)
                        // string romSize_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string romSize_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "releasenumber":
                        // string releaseNumber_visible = reader.GetAttribute("visible"); // (true|false)
                        // string releaseNumber_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string releaseNumber_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "languagenumber":
                        // string languageNumber_visible = reader.GetAttribute("visible"); // (true|false)
                        // string languageNumber_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string languageNumber_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "comment":
                        // string comment_visible = reader.GetAttribute("visible"); // (true|false)
                        // string comment_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string comment_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "romcrc":
                        // string romCRC_visible = reader.GetAttribute("visible"); // (true|false)
                        // string romCRC_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string romCRC_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "im1crc":
                        // string im1CRC_visible = reader.GetAttribute("visible"); // (true|false)
                        // string im1CRC_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string im1CRC_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "im2crc":
                        // string im2CRC_visible = reader.GetAttribute("visible"); // (true|false)
                        // string im2CRC_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string im2CRC_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "languages":
                        // string languages_visible = reader.GetAttribute("visible"); // (true|false)
                        // string languages_inNamingOption = reader.GetAttribute("inNamingOption"); // (true|false)
                        // string languages_default = reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read canopen information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        private void ReadCanOpen(XmlReader reader)
        {
            // Prepare all internal variables
            List<string> extensions = new List<string>();

            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all canopen items
                switch (reader.Name.ToLowerInvariant())
                {
                    case "extension":
                        extensions.Add(reader.ReadElementContentAsString());
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read newdat information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        private void ReadNewDat(XmlReader reader)
        {
            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all newdat items
                string content = string.Empty;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "datversionurl":
                        content = reader.ReadElementContentAsString();
                        Url = (string.IsNullOrWhiteSpace(Name) ? content : Url);
                        break;

                    case "daturl":
                        // string fileName = reader.GetAttribute("fileName");
                        content = reader.ReadElementContentAsString();
                        // string url = content;
                        break;

                    case "imurl":
                        content = reader.ReadElementContentAsString();
                        // string url = content;
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read search information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        private void ReadSearch(XmlReader reader)
        {
            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all search items
                string content = string.Empty;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "to":
                        // string value = reader.GetAttribute("value");
                        // string default = reader.GetAttribute("default"); (true|false)
                        // string auto = reader.GetAttribute("auto"); (true|false)

                        ReadTo(reader.ReadSubtree());

                        // Skip the to node now that we've processed it
                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read to information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        private void ReadTo(XmlReader reader)
        {
            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all search items
                string content = string.Empty;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "find":
                        // string operation = reader.GetAttribute("operation");
                        // string value = reader.GetAttribute("value"); // Int32?
                        content = reader.ReadElementContentAsString();
                        // string findValue = content;
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read games information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadGames(XmlReader reader,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all games items (ONLY OVERWRITE IF THERE'S NO DATA)
                switch (reader.Name.ToLowerInvariant())
                {
                    case "game":
                        ReadGame(reader.ReadSubtree(), keep, clean, remUnicode);

                        // Skip the game node now that we've processed it
                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read game information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadGame(XmlReader reader,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // Prepare all internal variables
            string releaseNumber = string.Empty, key = string.Empty, publisher = string.Empty, duplicateid = string.Empty;
            long size = -1;
            List<Rom> roms = new List<Rom>();
            Machine machine = new Machine();

            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all games items
                string content = string.Empty;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "imagenumber":
                        content = reader.ReadElementContentAsString();
                        // string imageNumber = content;
                        break;

                    case "releasenumber":
                        releaseNumber = reader.ReadElementContentAsString();
                        break;

                    case "title":
                        content = reader.ReadElementContentAsString();
                        machine.Name = content;
                        break;

                    case "savetype":
                        content = reader.ReadElementContentAsString();
                        // string saveType = content;
                        break;

                    case "romsize":
                        if (!Int64.TryParse(reader.ReadElementContentAsString(), out size))
                            size = -1;

                        break;

                    case "publisher":
                        publisher = reader.ReadElementContentAsString();
                        break;

                    case "location":
                        content = reader.ReadElementContentAsString();
                        // string location = content;
                        break;

                    case "sourcerom":
                        content = reader.ReadElementContentAsString();
                        // string sourceRom = content;
                        break;

                    case "language":
                        content = reader.ReadElementContentAsString();
                        // string language = content;
                        break;

                    case "files":
                        roms = ReadFiles(reader.ReadSubtree(), releaseNumber, machine.Name, keep, clean, remUnicode);
                        // Skip the files node now that we've processed it
                        reader.Skip();
                        break;

                    case "im1crc":
                        content = reader.ReadElementContentAsString();
                        // string im1crc = content;
                        break;

                    case "im2crc":
                        content = reader.ReadElementContentAsString();
                        // string im2crc = content;
                        break;

                    case "comment":
                        machine.Comment = reader.ReadElementContentAsString();
                        break;

                    case "duplicateid":
                        duplicateid = reader.ReadElementContentAsString();
                        if (duplicateid != "0")
                            machine.CloneOf = duplicateid;

                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Add information accordingly for each rom
            for (int i = 0; i < roms.Count; i++)
            {
                roms[i].Size = size;
                roms[i].Publisher = publisher;
                roms[i].CopyMachineInformation(machine);

                // Now process and add the rom
                key = ParseAddHelper(roms[i], clean, remUnicode);
            }
        }

        /// <summary>
        /// Read files information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="releaseNumber">Release number from the parent game</param>
        /// <param name="machineName">Name of the parent game to use</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private List<Rom> ReadFiles(XmlReader reader,
            string releaseNumber,
            string machineName,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // Prepare all internal variables
            List<Tuple<string, string>> extensionToCrc = new List<Tuple<string, string>>();
            List<Rom> roms = new List<Rom>();

            // If there's no subtree to the configuration, skip it
            if (reader == null)
                return roms;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Otherwise, read what we can from the header
            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all romCRC items
                switch (reader.Name.ToLowerInvariant())
                {
                    case "romcrc":
                        extensionToCrc.Add(
                            new Tuple<string, string>(
                                reader.GetAttribute("extension") ?? string.Empty,
                                reader.ReadElementContentAsString().ToLowerInvariant()));
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Now process the roms with the proper information
            foreach (Tuple<string, string> pair in extensionToCrc)
            {
                roms.Add(new Rom()
                {
                    Name = (releaseNumber != "0" ? releaseNumber + " - " : string.Empty) + machineName + pair.Item1,
                    CRC = Utilities.CleanHashData(pair.Item2, Constants.CRCLength),

                    ItemStatus = ItemStatus.None,
                });
            }

            return roms;
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
                xtw.WriteStartDocument(false);

                xtw.WriteStartElement("dat");
                xtw.WriteAttributeString("xsi", "xmlns", "http://www.w3.org/2001/XMLSchema-instance");
                xtw.WriteAttributeString("noNamespaceSchemaLocation", "xsi", "datas.xsd");

                xtw.WriteStartElement("configuration");
                xtw.WriteElementString("datName", Name);
                xtw.WriteElementString("datVersion", Count.ToString());
                xtw.WriteElementString("system", "none");
                xtw.WriteElementString("screenshotsWidth", "240");
                xtw.WriteElementString("screenshotsHeight", "160");

                xtw.WriteStartElement("infos");

                xtw.WriteStartElement("title");
                xtw.WriteAttributeString("visible", "false");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("location");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("publisher");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("sourceRom");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("saveType");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("romSize");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("releaseNumber");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("languageNumber");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("comment");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("romCRC");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("im1CRC");
                xtw.WriteAttributeString("visible", "false");
                xtw.WriteAttributeString("inNamingOption", "false");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("im2CRC");
                xtw.WriteAttributeString("visible", "false");
                xtw.WriteAttributeString("inNamingOption", "false");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("languages");
                xtw.WriteAttributeString("visible", "true");
                xtw.WriteAttributeString("inNamingOption", "true");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteEndElement();

                // End infos
                xtw.WriteEndElement();

                xtw.WriteStartElement("canOpen");
                xtw.WriteElementString("extension", ".bin");
                xtw.WriteEndElement();

                xtw.WriteStartElement("newDat");
                xtw.WriteElementString("datVersionURL", Url);

                xtw.WriteStartElement("datUrl");
                xtw.WriteAttributeString("fileName", $"{FileName}.zip");
                xtw.WriteString(Url);
                xtw.WriteEndElement();

                xtw.WriteElementString("imURL", Url);

                // End newDat
                xtw.WriteEndElement();

                xtw.WriteStartElement("search");

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "location");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteAttributeString("auto", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "romSize");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteAttributeString("auto", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "languages");
                xtw.WriteAttributeString("default", "true");
                xtw.WriteAttributeString("auto", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "saveType");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteAttributeString("auto", "false");
                xtw.WriteEndElement();

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "publisher");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteAttributeString("auto", "true");
                xtw.WriteEndElement();

                xtw.WriteStartElement("to");
                xtw.WriteAttributeString("value", "sourceRom");
                xtw.WriteAttributeString("default", "false");
                xtw.WriteAttributeString("auto", "true");
                xtw.WriteEndElement();

                // End search
                xtw.WriteEndElement();

                xtw.WriteElementString("romTitle", "%u - %n");

                // End configuration
                xtw.WriteEndElement();

                xtw.WriteStartElement("games");

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
                string state = string.Empty;

                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                xtw.WriteStartElement("game");
                xtw.WriteElementString("imageNumber", "1");
                xtw.WriteElementString("releaseNumber", "1");
                xtw.WriteElementString("title", datItem.GetField(Field.Name, ExcludeFields));
                xtw.WriteElementString("saveType", "None");

                if (datItem.ItemType == ItemType.Rom)
                {
                    var rom = datItem as Rom;
                    xtw.WriteElementString("romSize", datItem.GetField(Field.Size, ExcludeFields));
                }

                xtw.WriteElementString("publisher", "None");
                xtw.WriteElementString("location", "0");
                xtw.WriteElementString("sourceRom", "None");
                xtw.WriteElementString("language", "0");

                if (datItem.ItemType == ItemType.Disk)
                {
                    var disk = datItem as Disk;
                    xtw.WriteStartElement("files");
                    if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                    {
                        xtw.WriteStartElement("romMD5");
                        xtw.WriteAttributeString("extension", ".chd");
                        xtw.WriteString(disk.MD5.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                    {
                        xtw.WriteStartElement("romSHA1");
                        xtw.WriteAttributeString("extension", ".chd");
                        xtw.WriteString(disk.SHA1.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }

                    // End files
                    xtw.WriteEndElement();
                }
                else if (datItem.ItemType == ItemType.Rom)
                {
                    var rom = datItem as Rom;
                    string tempext = "." + Utilities.GetExtension(rom.Name);

                    xtw.WriteStartElement("files");
                    if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, ExcludeFields)))
                    {
                        xtw.WriteStartElement("romCRC");
                        xtw.WriteAttributeString("extension", tempext);
                        xtw.WriteString(rom.CRC.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, ExcludeFields)))
                    {
                        xtw.WriteStartElement("romMD5");
                        xtw.WriteAttributeString("extension", tempext);
                        xtw.WriteString(rom.MD5.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, ExcludeFields)))
                    {
                        xtw.WriteStartElement("romSHA1");
                        xtw.WriteAttributeString("extension", tempext);
                        xtw.WriteString(rom.SHA1.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }

                    // End files
                    xtw.WriteEndElement();
                }

                xtw.WriteElementString("im1CRC", "00000000");
                xtw.WriteElementString("im2CRC", "00000000");
                xtw.WriteElementString("comment", "");
                xtw.WriteElementString("duplicateID", "0");
                
                // End game
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
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(XmlTextWriter xtw)
        {
            try
            {
                // End games
                xtw.WriteEndElement();

                xtw.WriteStartElement("gui");

                xtw.WriteStartElement("images");
                xtw.WriteAttributeString("width", "487");
                xtw.WriteAttributeString("height", "162");

                xtw.WriteStartElement("image");
                xtw.WriteAttributeString("x", "0");
                xtw.WriteAttributeString("y", "0");
                xtw.WriteAttributeString("width", "240");
                xtw.WriteAttributeString("height", "160");
                xtw.WriteEndElement();

                xtw.WriteStartElement("image");
                xtw.WriteAttributeString("x", "245");
                xtw.WriteAttributeString("y", "0");
                xtw.WriteAttributeString("width", "240");
                xtw.WriteAttributeString("height", "160");
                xtw.WriteEndElement();

                // End images
                xtw.WriteEndElement();

                // End gui
                xtw.WriteEndElement();

                // End dat
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
