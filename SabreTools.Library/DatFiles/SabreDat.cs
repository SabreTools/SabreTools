using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Represents parsing and writing of an SabreDat XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class SabreDat : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SabreDat(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse an SabreDat XML DAT and return all found directories and files within
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
            bool empty = true;
            string key = string.Empty;
            List<string> parent = new List<string>();

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
                    // If we're ending a folder or game, take care of possibly empty games and removing from the parent
                    if (xtr.NodeType == XmlNodeType.EndElement && (xtr.Name == "directory" || xtr.Name == "dir"))
                    {
                        // If we didn't find any items in the folder, make sure to add the blank rom
                        if (empty)
                        {
                            string tempgame = string.Join("\\", parent);
                            Rom rom = new Rom("null", tempgame, omitFromScan: Hash.DeepHashes); // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually

                            // Now process and add the rom
                            key = ParseAddHelper(rom, clean, remUnicode);
                        }

                        // Regardless, end the current folder
                        int parentcount = parent.Count;
                        if (parentcount == 0)
                        {
                            Globals.Logger.Verbose($"Empty parent '{string.Join("\\", parent)}' found in '{filename}'");
                            empty = true;
                        }

                        // If we have an end folder element, remove one item from the parent, if possible
                        if (parentcount > 0)
                        {
                            parent.RemoveAt(parent.Count - 1);
                            if (keep && parentcount > 1)
                                Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
                        }
                    }

                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        // We want to process the entire subtree of the header
                        case "header":
                            ReadHeader(xtr.ReadSubtree(), keep);

                            // Skip the header node now that we've processed it
                            xtr.Skip();
                            break;

                        case "dir":
                        case "directory":
                            empty = ReadDirectory(xtr.ReadSubtree(), parent, filename, sysid, srcid, keep, clean, remUnicode);

                            // Skip the directory node now that we've processed it
                            xtr.Read();
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

            // Otherwise, read what we can from the header
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

                    case "rootdir":
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
                        Email = (string.IsNullOrWhiteSpace(Email) ?	reader.GetAttribute("email") : Email);
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? reader.GetAttribute("homepage") : Homepage);
                        Url = (string.IsNullOrWhiteSpace(Url) ? reader.GetAttribute("url") : Url);
                        break;

                    case "comment":
                        content = reader.ReadElementContentAsString();
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? content : Comment);
                        break;

                    case "flags":
                        ReadFlags(reader.ReadSubtree(), superdat);

                        // Skip the flags node now that we've processed it
                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read directory information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private bool ReadDirectory(XmlReader reader,
            List<string> parent,

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
            XmlReader flagreader;
            bool empty = true;
            string key = string.Empty, date = string.Empty;
            long size = -1;
            ItemStatus its = ItemStatus.None;

            // If there's no subtree to the header, skip it
            if (reader == null)
                return empty;

            string foldername = (reader.GetAttribute("name") ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(foldername))
                parent.Add(foldername);

            // Otherwise, read what we can from the directory
            while (!reader.EOF)
            {
                // If we're ending a folder or game, take care of possibly empty games and removing from the parent
                if (reader.NodeType == XmlNodeType.EndElement && (reader.Name == "directory" || reader.Name == "dir"))
                {
                    // If we didn't find any items in the folder, make sure to add the blank rom
                    if (empty)
                    {
                        string tempgame = string.Join("\\", parent);
                        Rom rom = new Rom("null", tempgame, omitFromScan: Hash.DeepHashes); // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually

                        // Now process and add the rom
                        key = ParseAddHelper(rom, clean, remUnicode);
                    }

                    // Regardless, end the current folder
                    int parentcount = parent.Count;
                    if (parentcount == 0)
                    {
                        Globals.Logger.Verbose($"Empty parent '{string.Join("\\", parent)}' found in '{filename}'");
                        empty = true;
                    }

                    // If we have an end folder element, remove one item from the parent, if possible
                    if (parentcount > 0)
                    {
                        parent.RemoveAt(parent.Count - 1);
                        if (keep && parentcount > 1)
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
                    }
                }

                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get all directory items
                string content = string.Empty;
                switch (reader.Name)
                {
                    // Directories can contain directories
                    case "dir":
                    case "directory":
                        ReadDirectory(reader.ReadSubtree(), parent, filename, sysid, srcid, keep, clean, remUnicode);

                        // Skip the directory node now that we've processed it
                        reader.Read();
                        break;

                    case "file":
                        empty = false;

                        // If the rom is itemStatus, flag it
                        its = ItemStatus.None;
                        flagreader = reader.ReadSubtree();

                        // If the subtree is empty, skip it
                        if (flagreader == null)
                        {
                            reader.Skip();
                            continue;
                        }

                        while (!flagreader.EOF)
                        {
                            // We only want elements
                            if (flagreader.NodeType != XmlNodeType.Element || flagreader.Name == "flags")
                            {
                                flagreader.Read();
                                continue;
                            }

                            switch (flagreader.Name)
                            {
                                case "flag":
                                    if (flagreader.GetAttribute("name") != null && flagreader.GetAttribute("value") != null)
                                    {
                                        content = flagreader.GetAttribute("value");
                                        its = Utilities.GetItemStatus(flagreader.GetAttribute("name"));
                                    }
                                    break;
                            }

                            flagreader.Read();
                        }

                        // If the rom has a Date attached, read it in and then sanitize it
                        date = Utilities.GetDate(reader.GetAttribute("date"));

                        // Take care of hex-sized files
                        size = Utilities.GetSize(reader.GetAttribute("size"));

                        Machine dir = new Machine();

                        // Get the name of the game from the parent
                        dir.Name = string.Join("\\", parent);
                        dir.Description = dir.Name;

                        DatItem datItem;
                        switch (reader.GetAttribute("type").ToLowerInvariant())
                        {
                            case "archive":
                                datItem = new Archive
                                {
                                    Name = reader.GetAttribute("name"),

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            case "biosset":
                                datItem = new BiosSet
                                {
                                    Name = reader.GetAttribute("name"),
                                    Description = reader.GetAttribute("description"),
                                    Default = Utilities.GetYesNo(reader.GetAttribute("default")),

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            case "disk":
                                datItem = new Disk
                                {
                                    Name = reader.GetAttribute("name"),
                                    MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                                    RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                                    SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                                    SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                                    SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                                    SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                                    ItemStatus = its,

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            case "release":
                                datItem = new Release
                                {
                                    Name = reader.GetAttribute("name"),
                                    Region = reader.GetAttribute("region"),
                                    Language = reader.GetAttribute("language"),
                                    Date = reader.GetAttribute("date"),
                                    Default = Utilities.GetYesNo(reader.GetAttribute("default")),

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            case "rom":
                                datItem = new Rom
                                {
                                    Name = reader.GetAttribute("name"),
                                    Size = size,
                                    CRC = Utilities.CleanHashData(reader.GetAttribute("crc"), Constants.CRCLength),
                                    MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                                    RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.RIPEMD160Length),
                                    SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                                    SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                                    SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                                    SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                                    ItemStatus = its,
                                    Date = date,

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            case "sample":
                                datItem = new Sample
                                {
                                    Name = reader.GetAttribute("name"),

                                    SystemID = sysid,
                                    System = filename,
                                    SourceID = srcid,
                                };
                                break;

                            default:
                                // By default, create a new Blank, just in case
                                datItem = new Blank();
                                break;
                        }

                        datItem?.CopyMachineInformation(dir);

                        // Now process and add the rom
                        key = ParseAddHelper(datItem, clean, remUnicode);

                        reader.Read();
                        break;
                }
            }

            return empty;
        }

        /// <summary>
        /// Read flags information
        /// </summary>
        /// <param name="reader">XmlReader to use to parse the header</param>
        /// <param name="superdat">True if superdat has already been set externally, false otherwise</param>
        private void ReadFlags(XmlReader reader, bool superdat)
        {
            // Prepare all internal variables
            string content = string.Empty;

            // If we somehow have a null flag section, skip it
            if (reader == null)
                return;

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element || reader.Name == "flags")
                {
                    reader.Read();
                    continue;
                }

                switch (reader.Name)
                {
                    case "flag":
                        if (reader.GetAttribute("name") != null && reader.GetAttribute("value") != null)
                        {
                            content = reader.GetAttribute("value");
                            switch (reader.GetAttribute("name").ToLowerInvariant())
                            {
                                case "type":
                                    Type = (string.IsNullOrWhiteSpace(Type) ? content : Type);
                                    superdat = superdat || content.Contains("SuperDAT");
                                    break;

                                case "forcemerging":
                                    if (ForceMerging == ForceMerging.None)
                                    {
                                        ForceMerging = Utilities.GetForceMerging(content);
                                    }
                                    break;

                                case "forcenodump":
                                    if (ForceNodump == ForceNodump.None)
                                    {
                                        ForceNodump = Utilities.GetForceNodump(content);
                                    }
                                    break;

                                case "forcepacking":
                                    if (ForcePacking == ForcePacking.None)
                                    {
                                        ForcePacking = Utilities.GetForcePacking(content);
                                    }
                                    break;
                            }
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
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        /// TODO: Fix writing out files that have a path in the name
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
                int depth = 2, last = -1;
                string lastgame = null;
                List<string> splitpath = new List<string>();

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

                        List<string> newsplit = rom.MachineName.Split('\\').ToList();

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            depth = WriteEndGame(sw, splitpath, newsplit, depth, out last);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            depth = WriteStartGame(sw, rom, newsplit, lastgame, depth, last);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

                            splitpath = newsplit;
                            lastgame = rom.MachineName;
                            continue;
                        }

                        // Now, output the rom data
                        WriteDatItem(sw, rom, depth, ignoreblanks);

                        // Set the new data to compare against
                        splitpath = newsplit;
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(sw, depth);

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
                string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                header += "<!DOCTYPE sabredat SYSTEM \"newdat.xsd\">\n\n";
                header += "<datafile>\n";
                header += "\t<header>\n";
                header += $"\t\t<name>{WebUtility.HtmlEncode(Name)}</name>\n";
                header += $"\t\t<description>{WebUtility.HtmlEncode(Description)}</description>\n";
                if (!string.IsNullOrWhiteSpace(RootDir))
                    header += $"\t\t<rootdir>{WebUtility.HtmlEncode(RootDir)}</rootdir>\n";
                if (!string.IsNullOrWhiteSpace(Category))
                    header += $"\t\t<category>{WebUtility.HtmlEncode(Category)}</category>\n";
                header += $"\t\t<version>{WebUtility.HtmlEncode(Version)}</version>\n";
                if (!string.IsNullOrWhiteSpace(Date))
                    header += $"\t\t<date>{WebUtility.HtmlEncode(Date)}</date>\n";
                header += $"\t\t<author>{WebUtility.HtmlEncode(Author)}</author>\n";
                if (!string.IsNullOrWhiteSpace(Comment))
                    header += $"\t\t<comment>{WebUtility.HtmlEncode(Comment)}</comment>\n";
                if (!string.IsNullOrWhiteSpace(Type) || ForcePacking != ForcePacking.None || ForceMerging != ForceMerging.None || ForceNodump != ForceNodump.None)
                {
                    header += "\t\t<flags>\n";
                    if (!string.IsNullOrWhiteSpace(Type))
                        header += $"\t\t\t<flag name=\"type\" value=\"{WebUtility.HtmlEncode(Type)}\"/>\n";
                    switch (ForcePacking)
                    {
                        case ForcePacking.Unzip:
                            header += "\t\t\t<flag name=\"forcepacking\" value=\"unzip\"/>\n";
                            break;
                        case ForcePacking.Zip:
                            header += "\t\t\t<flag name=\"forcepacking\" value=\"zip\"/>\n";
                            break;
                    }
                    switch (ForceMerging)
                    {
                        case ForceMerging.Full:
                            header += "\t\t\t<flag name=\"forcemerging\" value=\"full\"/>\n";
                            break;
                        case ForceMerging.Split:
                            header += "\t\t\t<flag name=\"forcemerging\" value=\"split\"/>\n";
                            break;
                        case ForceMerging.Merged:
                            header += "\t\t\t<flag name=\"forcemerging\" value=\"merged\"/>\n";
                            break;
                        case ForceMerging.NonMerged:
                            header += "\t\t\t<flag name=\"forcemerging\" value=\"nonmerged\"/>\n";
                            break;
                    }
                    switch (ForceNodump)
                    {
                        case ForceNodump.Ignore:
                            header += "\t\t\t<flag name=\"forcenodump\" value=\"ignore\"/>\n";
                            break;
                        case ForceNodump.Obsolete:
                            header += "\t\t\t<flag name=\"forcenodump\" value=\"obsolete\"/>\n";
                            break;
                        case ForceNodump.Required:
                            header += "\t\t\t<flag name=\"forcenodump\" value=\"required\"/>\n";
                            break;
                    }
                    header += "\t\t</flags>\n";
                }
                header += "\t<data>\n";

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
        /// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
        /// <param name="lastgame">The name of the last game to be output</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
        /// <returns>The new depth of the tag</returns>
        private int WriteStartGame(StreamWriter sw, DatItem datItem, List<string> newsplit, string lastgame, int depth, int last)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                string state = string.Empty;

                // Build the state based on excluded fields
                for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
                {
                    for (int j = 0; j < depth - last + i - (lastgame == null ? 1 : 0); j++)
                    {
                        state += "\t";
                    }

                    state += $"<directory";
                    state += $" name=\"{(!ExcludeFields[(int)Field.MachineName] ? WebUtility.HtmlEncode(newsplit[i]) : string.Empty)}\"";
                    state += $" description=\"{(!ExcludeFields[(int)Field.MachineName] ? WebUtility.HtmlEncode(newsplit[i]) : string.Empty)}\">\n";
                }

                depth = depth - (last == -1 ? 0 : last) + newsplit.Count;

                sw.Write(state);
                sw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return depth;
            }

            return depth;
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
        /// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
        /// <returns>The new depth of the tag</returns>
        private int WriteEndGame(StreamWriter sw, List<string> splitpath, List<string> newsplit, int depth, out int last)
        {
            last = 0;

            try
            {
                string state = string.Empty;
                if (splitpath != null)
                {
                    for (int i = 0; i < newsplit.Count && i < splitpath.Count; i++)
                    {
                        // Always keep track of the last seen item
                        last = i;

                        // If we find a difference, break
                        if (newsplit[i] != splitpath[i])
                            break;
                    }

                    // Now that we have the last known position, take down all open folders
                    for (int i = depth - 1; i > last + 1; i--)
                    {
                        // Print out the number of tabs and the end folder
                        for (int j = 0; j < i; j++)
                        {
                            state += "\t";
                        }

                        state += "</directory>\n";
                    }

                    // Reset the current depth
                    depth = 2 + last;
                }

                sw.Write(state);
                sw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return depth;
            }

            return depth;
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(StreamWriter sw, DatItem datItem, int depth, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                string state = string.Empty, prefix = string.Empty;

                // Pre-process the item name
                ProcessItemName(datItem, true);

                for (int i = 0; i < depth; i++)
                {
                    prefix += "\t";
                }

                state += prefix;

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                        state += $"<file type=\"archive\" name=\"{WebUtility.HtmlEncode(datItem.GetField(Field.Name, ExcludeFields))}\"";
                        state += "/>\n";
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        state += $"<file type=\"biosset\" name\"{WebUtility.HtmlEncode(biosSet.GetField(Field.Name, ExcludeFields))}\"";
                        if (!ExcludeFields[(int)Field.BiosDescription] && !string.IsNullOrWhiteSpace(biosSet.Description))
                            state += $" description=\"{WebUtility.HtmlEncode(biosSet.Description)}\"";
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            state += $" default=\"{biosSet.Default.ToString().ToLowerInvariant()}\"";
                        state += "/>\n";
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        state += $"<file type=\"disk\" name=\"{WebUtility.HtmlEncode(disk.GetField(Field.Name, ExcludeFields))}\"";
                        if (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(disk.MD5))
                            state += $" md5=\"{disk.MD5.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(disk.RIPEMD160))
                            state += $" ripemd160=\"{disk.RIPEMD160.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(disk.SHA1))
                            state += $" sha1=\"{disk.SHA1.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(disk.SHA256))
                            state += $" sha256=\"{disk.SHA256.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(disk.SHA384))
                            state += $" sha384=\"{disk.SHA384.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(disk.SHA512))
                            state += $" sha512=\"{disk.SHA512.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                        {
                            state = $"{prefix}/>\n";
                            state += $"{prefix}\t<flags>\n";
                            state += $"{prefix}\t\t<flag name=\"status\" value=\"{disk.ItemStatus.ToString().ToLowerInvariant()}\"/>\n";
                            state += $"{prefix}\t</flags>\n";
                            state += $"{prefix}</file>\n";
                        }
                        else
                        {
                            state += "/>\n";
                        }
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        state += $"<file type=\"release\" name\"{WebUtility.HtmlEncode(release.GetField(Field.Name, ExcludeFields))}\"";
                        if (!ExcludeFields[(int)Field.Region] && !string.IsNullOrWhiteSpace(release.Region))
                            state += $" region=\"{WebUtility.HtmlEncode(release.Region)}\"";
                        if (!ExcludeFields[(int)Field.Language] && !string.IsNullOrWhiteSpace(release.Language))
                            state += $" language=\"{WebUtility.HtmlEncode(release.Language)}\"";
                        if (!ExcludeFields[(int)Field.Date] && !string.IsNullOrWhiteSpace(release.Date))
                            state += $" date=\"{WebUtility.HtmlEncode(release.Date)}\"";
                        if (!ExcludeFields[(int)Field.Default] && release.Default != null)
                            state += $" default=\"{release.Default.ToString().ToLowerInvariant()}\"";
                        state += "/>\n";
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        state += $"<file type=\"rom\" name=\"{WebUtility.HtmlEncode(rom.GetField(Field.Name, ExcludeFields))}\"";
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            state += $" size=\"{rom.Size}\"";
                        if (!ExcludeFields[(int)Field.CRC] && !string.IsNullOrWhiteSpace(rom.CRC))
                            state += $" crc=\"{rom.CRC.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(rom.MD5))
                            state += $" md5=\"{rom.MD5.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(rom.RIPEMD160))
                            state += $" ripemd160=\"{rom.RIPEMD160.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(rom.SHA1))
                            state += $" sha1=\"{rom.SHA1.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(rom.SHA256))
                            state += $" sha256=\"{rom.SHA256.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(rom.SHA384))
                            state += $" sha384=\"{rom.SHA384.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(rom.SHA512))
                            state += $" sha512=\"{rom.SHA512.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Date] && !string.IsNullOrWhiteSpace(rom.Date))
                            state += $" date=\"{WebUtility.HtmlEncode(rom.Date)}\"";
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                        {
                            state = $"{prefix}/>\n";
                            state += $"{prefix}\t<flags>\n";
                            state += $"{prefix}\t\t<flag name=\"status\" value=\"{rom.ItemStatus.ToString().ToLowerInvariant()}\"/>\n";
                            state += $"{prefix}\t</flags>\n";
                            state += $"{prefix}</file>\n";
                        }
                        else
                        {
                            state += "/>\n";
                        }
                        break;

                    case ItemType.Sample:
                        state += $"<file type=\"sample\" name=\"{WebUtility.HtmlEncode(datItem.GetField(Field.Name, ExcludeFields))}\"";
                        state += "/>\n";
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
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(StreamWriter sw, int depth)
        {
            try
            {
                string footer = string.Empty;
                for (int i = depth - 1; i >= 2; i--)
                {
                    // Print out the number of tabs and the end folder
                    for (int j = 0; j < i; j++)
                    {
                        footer += "\t";
                    }

                    footer += "</directory>\n";
                }

                footer += "\t</data>\n</datafile>\n";

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
