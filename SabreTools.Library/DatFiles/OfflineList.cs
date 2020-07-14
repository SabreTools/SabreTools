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
    internal class OfflineList : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public OfflineList(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse an OfflineList XML DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <remarks>
        /// </remarks>
        protected override void ParseFile(
            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
        {
            XmlReader xtr = filename.GetXmlTextReader();

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
                            ReadGames(xtr.ReadSubtree(), filename, indexId);

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
                string content;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "datname":
                        content = reader.ReadElementContentAsString();
                        DatHeader.Name = (string.IsNullOrWhiteSpace(DatHeader.Name) ? content : DatHeader.Name);
                        superdat = superdat || content.Contains(" - SuperDAT");
                        if (keep && superdat)
                        {
                            DatHeader.Type = (string.IsNullOrWhiteSpace(DatHeader.Type) ? "SuperDAT" : DatHeader.Type);
                        }
                        break;

                    case "datversion":
                        content = reader.ReadElementContentAsString();
                        DatHeader.Version = (string.IsNullOrWhiteSpace(DatHeader.Version) ? content : DatHeader.Version);
                        break;

                    case "system":
                        reader.ReadElementContentAsString();
                        break;

                    case "screenshotswidth":
                        reader.ReadElementContentAsString(); // Int32?
                        break;

                    case "screenshotsheight":
                        reader.ReadElementContentAsString(); // Int32?
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
                        reader.ReadElementContentAsString();
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
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "location":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "publisher":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "sourcerom":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "savetype":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "romsize":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "releasenumber":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "languagenumber":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "comment":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "romcrc":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "im1crc":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "im2crc":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
                        reader.Read();
                        break;

                    case "languages":
                        reader.GetAttribute("visible"); // (true|false)
                        reader.GetAttribute("inNamingOption"); // (true|false)
                        reader.GetAttribute("default"); // (true|false)
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
                string content;
                switch (reader.Name.ToLowerInvariant())
                {
                    case "datversionurl":
                        content = reader.ReadElementContentAsString();
                        DatHeader.Url = (string.IsNullOrWhiteSpace(DatHeader.Url) ? content : DatHeader.Url);
                        break;

                    case "daturl":
                        reader.GetAttribute("fileName");
                        reader.ReadElementContentAsString();
                        break;

                    case "imurl":
                        reader.ReadElementContentAsString();
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
                switch (reader.Name.ToLowerInvariant())
                {
                    case "to":
                        reader.GetAttribute("value");
                        reader.GetAttribute("default"); // (true|false)
                        reader.GetAttribute("auto"); // (true|false)

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
                switch (reader.Name.ToLowerInvariant())
                {
                    case "find":
                        reader.GetAttribute("operation");
                        reader.GetAttribute("value"); // Int32?
                        reader.ReadElementContentAsString();
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
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadGames(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int indexId)
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
                        ReadGame(reader.ReadSubtree(), filename, indexId);

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
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadGame(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int indexId)
        {
            // Prepare all internal variables
            string releaseNumber = string.Empty, publisher = string.Empty, duplicateid;
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
                switch (reader.Name.ToLowerInvariant())
                {
                    case "imagenumber":
                        reader.ReadElementContentAsString();
                        break;

                    case "releasenumber":
                        releaseNumber = reader.ReadElementContentAsString();
                        break;

                    case "title":
                        machine.Name = reader.ReadElementContentAsString();
                        break;

                    case "savetype":
                        reader.ReadElementContentAsString();
                        break;

                    case "romsize":
                        if (!Int64.TryParse(reader.ReadElementContentAsString(), out size))
                            size = -1;

                        break;

                    case "publisher":
                        publisher = reader.ReadElementContentAsString();
                        break;

                    case "location":
                        reader.ReadElementContentAsString();
                        break;

                    case "sourcerom":
                        reader.ReadElementContentAsString();
                        break;

                    case "language":
                        reader.ReadElementContentAsString();
                        break;

                    case "files":
                        roms = ReadFiles(reader.ReadSubtree(), releaseNumber, machine.Name, filename, indexId);
                        
                        // Skip the files node now that we've processed it
                        reader.Skip();
                        break;

                    case "im1crc":
                        reader.ReadElementContentAsString();
                        break;

                    case "im2crc":
                        reader.ReadElementContentAsString();
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
                ParseAddHelper(roms[i]);
            }
        }

        /// <summary>
        /// Read files information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="releaseNumber">Release number from the parent game</param>
        /// <param name="machineName">Name of the parent game to use</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private List<Rom> ReadFiles(
            XmlReader reader,
            string releaseNumber,
            string machineName,

            // Standard Dat parsing
            string filename,
            int indexId)
        {
            // Prepare all internal variables
            var extensionToCrc = new List<KeyValuePair<string, string>>();
            var roms = new List<Rom>();

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
                            new KeyValuePair<string, string>(
                                reader.GetAttribute("extension") ?? string.Empty,
                                reader.ReadElementContentAsString().ToLowerInvariant()));
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Now process the roms with the proper information
            foreach (KeyValuePair<string, string> pair in extensionToCrc)
            {
                roms.Add(new Rom()
                {
                    Name = (releaseNumber != "0" ? releaseNumber + " - " : string.Empty) + machineName + pair.Key,
                    CRC = pair.Value,

                    ItemStatus = ItemStatus.None,

                    IndexId = indexId,
                    IndexSource = filename,
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
                FileStream fs = FileExtensions.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                XmlTextWriter xtw = new XmlTextWriter(fs, new UTF8Encoding(false))
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

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
#if NET_FRAMEWORK
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
#endif
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
                xtw.WriteElementString("datName", DatHeader.Name);
                xtw.WriteElementString("datVersion", DatStats.Count.ToString());
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
                xtw.WriteElementString("datVersionURL", DatHeader.Url);

                xtw.WriteStartElement("datUrl");
                xtw.WriteAttributeString("fileName", $"{DatHeader.FileName}.zip");
                xtw.WriteString(DatHeader.Url);
                xtw.WriteEndElement();

                xtw.WriteElementString("imURL", DatHeader.Url);

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
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                xtw.WriteStartElement("game");
                xtw.WriteElementString("imageNumber", "1");
                xtw.WriteElementString("releaseNumber", "1");
                xtw.WriteElementString("title", datItem.GetField(Field.Name, DatHeader.ExcludeFields));
                xtw.WriteElementString("saveType", "None");

                if (datItem.ItemType == ItemType.Rom)
                {
                    var rom = datItem as Rom;
                    xtw.WriteElementString("romSize", datItem.GetField(Field.Size, DatHeader.ExcludeFields));
                }

                xtw.WriteElementString("publisher", "None");
                xtw.WriteElementString("location", "0");
                xtw.WriteElementString("sourceRom", "None");
                xtw.WriteElementString("language", "0");

                if (datItem.ItemType == ItemType.Disk)
                {
                    var disk = datItem as Disk;
                    xtw.WriteStartElement("files");
                    if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, DatHeader.ExcludeFields)))
                    {
                        xtw.WriteStartElement("romMD5");
                        xtw.WriteAttributeString("extension", ".chd");
                        xtw.WriteString(disk.MD5.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
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
                    string tempext = "." + PathExtensions.GetNormalizedExtension(rom.Name);

                    xtw.WriteStartElement("files");
                    if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, DatHeader.ExcludeFields)))
                    {
                        xtw.WriteStartElement("romCRC");
                        xtw.WriteAttributeString("extension", tempext);
                        xtw.WriteString(rom.CRC.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.MD5, DatHeader.ExcludeFields)))
                    {
                        xtw.WriteStartElement("romMD5");
                        xtw.WriteAttributeString("extension", tempext);
                        xtw.WriteString(rom.MD5.ToUpperInvariant());
                        xtw.WriteEndElement();
                    }
                    else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
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
