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
                Globals.Logger.Warning("Exception found while parsing '{0}': {1}", filename, ex);

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
                string header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n"
                            + "<dat xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"datas.xsd\">\n"
                            + "\t<configuration>\n"
                            + "\t\t<datName>" + WebUtility.HtmlEncode(Name) + "</datName>\n"
                            + "\t\t<datVersion>" + Count + "</datVersion>\n"
                            + "\t\t<system>none</system>\n"
                            + "\t\t<screenshotsWidth>240</screenshotsWidth>\n"
                            + "\t\t<screenshotsHeight>160</screenshotsHeight>\n"
                            + "\t\t<infos>\n"
                            + "\t\t\t<title visible=\"false\" inNamingOption=\"true\" default=\"false\"/>\n"
                            + "\t\t\t<location visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t\t<publisher visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t\t<sourceRom visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t\t<saveType visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t\t<romSize visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t\t<releaseNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
                            + "\t\t\t<languageNumber visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
                            + "\t\t\t<comment visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
                            + "\t\t\t<romCRC visible=\"true\" inNamingOption=\"true\" default=\"false\"/>\n"
                            + "\t\t\t<im1CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
                            + "\t\t\t<im2CRC visible=\"false\" inNamingOption=\"false\" default=\"false\"/>\n"
                            + "\t\t\t<languages visible=\"true\" inNamingOption=\"true\" default=\"true\"/>\n"
                            + "\t\t</infos>\n"
                            + "\t\t<canOpen>\n"
                            + "\t\t\t<extension>.bin</extension>\n"
                            + "\t\t</canOpen>\n"
                            + "\t\t<newDat>\n"
                            + "\t\t\t<datVersionURL>" + WebUtility.HtmlEncode(Url) + "</datVersionURL>\n"
                            + "\t\t\t<datURL fileName=\"" + WebUtility.HtmlEncode(FileName) + ".zip\">" + WebUtility.HtmlEncode(Url) + "</datURL>\n"
                            + "\t\t\t<imURL>" + WebUtility.HtmlEncode(Url) + "</imURL>\n"
                            + "\t\t</newDat>\n"
                            + "\t\t<search>\n"
                            + "\t\t\t<to value=\"location\" default=\"true\" auto=\"true\"/>\n"
                            + "\t\t\t<to value=\"romSize\" default=\"true\" auto=\"false\"/>\n"
                            + "\t\t\t<to value=\"languages\" default=\"true\" auto=\"true\"/>\n"
                            + "\t\t\t<to value=\"saveType\" default=\"false\" auto=\"false\"/>\n"
                            + "\t\t\t<to value=\"publisher\" default=\"false\" auto=\"true\"/>\n"
                            + "\t\t\t<to value=\"sourceRom\" default=\"false\" auto=\"true\"/>\n"
                            + "\t\t</search>\n"
                            + "\t\t<romTitle >%u - %n</romTitle>\n"
                            + "\t</configuration>\n"
                            + "\t<games>\n";

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
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw)
        {
            try
            {
                string state = "\t\t</game>\n";

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

                state += "\t\t<game>\n"
                            + "\t\t\t<imageNumber>1</imageNumber>\n"
                            + "\t\t\t<releaseNumber>1</releaseNumber>\n"
                            + "\t\t\t<title>" + (!ExcludeFields[(int)Field.Name] ? WebUtility.HtmlEncode(rom.Name) : string.Empty) + "</title>\n"
                            + "\t\t\t<saveType>None</saveType>\n";

                if (rom.ItemType == ItemType.Rom)
                    state += "\t\t\t<romSize>" + (!ExcludeFields[(int)Field.Size] ? ((Rom)rom).Size.ToString() : string.Empty) + "</romSize>\n";

                state += "\t\t\t<publisher>None</publisher>\n"
                    + "\t\t\t<location>0</location>\n"
                    + "\t\t\t<sourceRom>None</sourceRom>\n"
                    + "\t\t\t<language>0</language>\n";

                if (rom.ItemType == ItemType.Disk)
                {
                    state += "\t\t\t<files>\n"
                        + (((Disk)rom).MD5 != null
                            ? "\t\t\t\t<romMD5 extension=\".chd\">" + (!ExcludeFields[(int)Field.MD5] ? ((Disk)rom).MD5.ToUpperInvariant() : string.Empty) + "</romMD5>\n"
                            : "\t\t\t\t<romSHA1 extension=\".chd\">" + (!ExcludeFields[(int)Field.SHA1] ? ((Disk)rom).SHA1.ToUpperInvariant() : string.Empty) + "</romSHA1>\n")
                        + "\t\t\t</files>\n";
                }
                else if (rom.ItemType == ItemType.Rom)
                {
                    string tempext = "." + Utilities.GetExtension(((Rom)rom).Name);

                    state += "\t\t\t<files>\n"
                        + (((Rom)rom).CRC != null
                            ? "\t\t\t\t<romCRC extension=\"" + tempext + "\">" + (!ExcludeFields[(int)Field.CRC] ? ((Rom)rom).CRC.ToUpperInvariant() : string.Empty) + "</romCRC>\n"
                            : ((Rom)rom).MD5 != null
                                ? "\t\t\t\t<romMD5 extension=\"" + tempext + "\">" + (!ExcludeFields[(int)Field.MD5] ? ((Rom)rom).MD5.ToUpperInvariant() : string.Empty) + "</romMD5>\n"
                                : "\t\t\t\t<romSHA1 extension=\"" + tempext + "\">" + (!ExcludeFields[(int)Field.SHA1] ? ((Rom)rom).SHA1.ToUpperInvariant() : string.Empty) + "</romSHA1>\n")
                        + "\t\t\t</files>\n";
                }

                state += "\t\t\t<im1CRC>00000000</im1CRC>\n"
                    + "\t\t\t<im2CRC>00000000</im2CRC>\n"
                    + "\t\t\t<comment></comment>\n"
                    + "\t\t\t<duplicateID>0</duplicateID>\n"
                    + "\t\t</game>\n";

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
                string footer = "\t\t</game>"
                            + "\t</games>\n"
                            + "\t<gui>\n"
                            + "\t\t<images width=\"487\" height=\"162\">\n"
                            + "\t\t\t<image x=\"0\" y=\"0\" width=\"240\" height=\"160\"/>\n"
                            + "\t\t\t<image x=\"245\" y=\"0\" width=\"240\" height=\"160\"/>\n"
                            + "\t\t</images>\n"
                            + "\t</gui>\n"
                            + "</dat>";

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
