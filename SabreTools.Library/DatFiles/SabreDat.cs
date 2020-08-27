using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of an SabreDat XML DAT
    /// </summary>
    internal class SabreDat : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SabreDat(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse an SabreDat XML DAT and return all found directories and files within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        protected override void ParseFile(
            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
        {
            // Prepare all internal variables
            bool empty = true;
            string key;
            List<string> parent = new List<string>();

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
                    // If we're ending a folder or game, take care of possibly empty games and removing from the parent
                    if (xtr.NodeType == XmlNodeType.EndElement && (xtr.Name == "directory" || xtr.Name == "dir"))
                    {
                        // If we didn't find any items in the folder, make sure to add the blank rom
                        if (empty)
                        {
                            string tempgame = string.Join("\\", parent);
                            Rom rom = new Rom("null", tempgame);

                            // Now process and add the rom
                            key = ParseAddHelper(rom);
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
                                Header.Type = (string.IsNullOrWhiteSpace(Header.Type) ? "SuperDAT" : Header.Type);
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
                            empty = ReadDirectory(xtr.ReadSubtree(), parent, filename, indexId, keep);

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
                string content;
                switch (reader.Name)
                {
                    case "name":
                        content = reader.ReadElementContentAsString(); ;
                        Header.Name = (Header.Name== null ? content : Header.Name);
                        superdat = superdat || content.Contains(" - SuperDAT");
                        if (keep && superdat)
                        {
                            Header.Type = (Header.Type == null ? "SuperDAT" : Header.Type);
                        }
                        break;

                    case "description":
                        content = reader.ReadElementContentAsString();
                        Header.Description = (Header.Description== null ? content : Header.Description);
                        break;

                    case "rootdir":
                        content = reader.ReadElementContentAsString();
                        Header.RootDir = (Header.RootDir== null ? content : Header.RootDir);
                        break;

                    case "category":
                        content = reader.ReadElementContentAsString();
                        Header.Category = (Header.Category== null ? content : Header.Category);
                        break;

                    case "version":
                        content = reader.ReadElementContentAsString();
                        Header.Version = (Header.Version== null ? content : Header.Version);
                        break;

                    case "date":
                        content = reader.ReadElementContentAsString();
                        Header.Date = (Header.Date== null ? content.Replace(".", "/") : Header.Date);
                        break;

                    case "author":
                        content = reader.ReadElementContentAsString();
                        Header.Author = (Header.Author== null ? content : Header.Author);
                        Header.Email = (Header.Email == null ? reader.GetAttribute("email") : Header.Email);
                        Header.Homepage = (Header.Homepage == null ? reader.GetAttribute("homepage") : Header.Homepage);
                        Header.Url = (Header.Url == null ? reader.GetAttribute("url") : Header.Url);
                        break;

                    case "comment":
                        content = reader.ReadElementContentAsString();
                        Header.Comment = (Header.Comment== null ? content : Header.Comment);
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
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private bool ReadDirectory(XmlReader reader,
            List<string> parent,

            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
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
                        Rom rom = new Rom("null", tempgame);

                        // Now process and add the rom
                        key = ParseAddHelper(rom);
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
                            Header.Type = (string.IsNullOrWhiteSpace(Header.Type) ? "SuperDAT" : Header.Type);
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
                        ReadDirectory(reader.ReadSubtree(), parent, filename, indexId, keep);

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
                                        its = flagreader.GetAttribute("name").AsItemStatus();
                                    }
                                    break;
                            }

                            flagreader.Read();
                        }

                        // If the rom has a Date attached, read it in and then sanitize it
                        date = Sanitizer.CleanDate(reader.GetAttribute("date"));

                        // Take care of hex-sized files
                        size = Sanitizer.CleanSize(reader.GetAttribute("size"));

                        Machine dir = new Machine
                        {
                            // Get the name of the game from the parent
                            Name = string.Join("\\", parent),
                            Description = string.Join("\\", parent),
                        };

                        DatItem datItem;
                        switch (reader.GetAttribute("type").ToLowerInvariant())
                        {
                            case "archive":
                                datItem = new Archive
                                {
                                    Name = reader.GetAttribute("name"),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "biosset":
                                datItem = new BiosSet
                                {
                                    Name = reader.GetAttribute("name"),
                                    Description = reader.GetAttribute("description"),
                                    Default = reader.GetAttribute("default").AsYesNo(),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "chip":
                                datItem = new Chip
                                {
                                    Name = reader.GetAttribute("name"),
                                    Tag = reader.GetAttribute("tag"),
                                    ChipType = reader.GetAttribute("chiptype"),
                                    Clock = reader.GetAttribute("clock"),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "disk":
                                datItem = new Disk
                                {
                                    Name = reader.GetAttribute("name"),
                                    MD5 = reader.GetAttribute("md5"),
                                    SHA1 = reader.GetAttribute("sha1"),
                                    ItemStatus = its,

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "media":
                                datItem = new Media
                                {
                                    Name = reader.GetAttribute("name"),
                                    MD5 = reader.GetAttribute("md5"),
                                    SHA1 = reader.GetAttribute("sha1"),
                                    SHA256 = reader.GetAttribute("sha256"),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "release":
                                datItem = new Release
                                {
                                    Name = reader.GetAttribute("name"),
                                    Region = reader.GetAttribute("region"),
                                    Language = reader.GetAttribute("language"),
                                    Date = reader.GetAttribute("date"),
                                    Default = reader.GetAttribute("default").AsYesNo(),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "rom":
                                datItem = new Rom
                                {
                                    Name = reader.GetAttribute("name"),
                                    Size = size,
                                    CRC = reader.GetAttribute("crc"),
                                    MD5 = reader.GetAttribute("md5"),
#if NET_FRAMEWORK
                                    RIPEMD160 = reader.GetAttribute("ripemd160"),
#endif
                                    SHA1 = reader.GetAttribute("sha1"),
                                    SHA256 = reader.GetAttribute("sha256"),
                                    SHA384 = reader.GetAttribute("sha384"),
                                    SHA512 = reader.GetAttribute("sha512"),
                                    ItemStatus = its,
                                    Date = date,

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            case "sample":
                                datItem = new Sample
                                {
                                    Name = reader.GetAttribute("name"),

                                    Source = new Source
                                    {
                                        Index = indexId,
                                        Name = filename,
                                    },
                                };
                                break;

                            default:
                                // By default, create a new Blank, just in case
                                datItem = new Blank();
                                break;
                        }

                        datItem?.CopyMachineInformation(dir);

                        // Now process and add the rom
                        key = ParseAddHelper(datItem);

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
            string content;

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
                                    Header.Type = (Header.Type == null ? content : Header.Type);
                                    superdat = superdat || content.Contains("SuperDAT");
                                    break;

                                case "forcemerging":
                                    if (Header.ForceMerging == MergingFlag.None)
                                        Header.ForceMerging = content.AsMergingFlag();

                                    break;

                                case "forcenodump":
                                    if (Header.ForceNodump == NodumpFlag.None)
                                        Header.ForceNodump = content.AsNodumpFlag();

                                    break;

                                case "forcepacking":
                                    if (Header.ForcePacking == PackingFlag.None)
                                        Header.ForcePacking = content.AsPackingFlag();

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
                int depth = 2, last = -1;
                string lastgame = null;
                List<string> splitpath = new List<string>();

                // Use a sorted list of games to output
                foreach (string key in Items.SortedKeys)
                {
                    List<DatItem> roms = Items[key];

                    // Resolve the names in the block
                    roms = DatItem.ResolveNames(roms);

                    for (int index = 0; index < roms.Count; index++)
                    {
                        DatItem rom = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (rom.Name == null || rom.Machine.Name == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        List<string> newsplit = rom.Machine.Name.Split('\\').ToList();

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
                            depth = WriteEndGame(xtw, splitpath, newsplit, depth, out last);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
                            depth = WriteStartGame(xtw, rom, newsplit, depth, last);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && (rom as Rom).Size == -1
                            && (rom as Rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.Machine.Name}");

                            splitpath = newsplit;
                            lastgame = rom.Machine.Name;
                            continue;
                        }

                        // Now, output the rom data
                        WriteDatItem(xtw, rom, depth, ignoreblanks);

                        // Set the new data to compare against
                        splitpath = newsplit;
                        lastgame = rom.Machine.Name;
                    }
                }

                // Write the file footer out
                WriteFooter(xtw, depth);

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
                xtw.WriteDocType("sabredat", null, "newdat.xsd", null);

                xtw.WriteStartElement("datafile");

                xtw.WriteStartElement("header");

                xtw.WriteRequiredElementString("name", Header.Name);
                xtw.WriteRequiredElementString("description", Header.Description);
                xtw.WriteOptionalElementString("rootdir", Header.RootDir);
                xtw.WriteOptionalElementString("category", Header.Category);
                xtw.WriteRequiredElementString("version", Header.Version);
                xtw.WriteOptionalElementString("date", Header.Date);
                xtw.WriteRequiredElementString("author", Header.Author);
                xtw.WriteOptionalElementString("comment", Header.Comment);
                if (!string.IsNullOrWhiteSpace(Header.Type)
                    || Header.ForcePacking != PackingFlag.None
                    || Header.ForceMerging != MergingFlag.None
                    || Header.ForceNodump != NodumpFlag.None)
                {
                    xtw.WriteStartElement("flags");

                    if (!string.IsNullOrWhiteSpace(Header.Type))
                    {
                        xtw.WriteStartElement("flag");
                        xtw.WriteAttributeString("name", "type");
                        xtw.WriteRequiredAttributeString("value", Header.Type);
                        xtw.WriteEndElement();
                    }

                    if (Header.ForcePacking != PackingFlag.None)
                    {
                        xtw.WriteStartElement("flag");
                        xtw.WriteAttributeString("name", "forcepacking");
                        xtw.WriteOptionalAttributeString("value", Header.ForcePacking.FromPackingFlag(false));
                        xtw.WriteEndElement();
                    }

                    if (Header.ForceMerging != MergingFlag.None)
                    {
                        xtw.WriteStartElement("flag");
                        xtw.WriteAttributeString("name", "forcemerging");
                        xtw.WriteAttributeString("value", Header.ForceMerging.FromMergingFlag(false));
                        xtw.WriteEndElement();
                    }

                    if (Header.ForceNodump != NodumpFlag.None)
                    {
                        xtw.WriteStartElement("flag");
                        xtw.WriteAttributeString("name", "forcenodump");
                        xtw.WriteAttributeString("value", Header.ForceNodump.FromNodumpFlag());
                        xtw.WriteEndElement();
                    }

                    // End flags
                    xtw.WriteEndElement();
                }

                // End header
                xtw.WriteEndElement();

                xtw.WriteStartElement("data");

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
        /// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
        /// <returns>The new depth of the tag</returns>
        private int WriteStartGame(XmlTextWriter xtw, DatItem datItem, List<string> newsplit, int depth, int last)
        {
            try
            {
                // No game should start with a path separator
                datItem.Machine.Name = datItem.Machine.Name?.TrimStart(Path.DirectorySeparatorChar) ?? string.Empty;

                // Build the state
                for (int i = (last == -1 ? 0 : last); i < newsplit.Count; i++)
                {
                    xtw.WriteStartElement("directory");
                    xtw.WriteRequiredAttributeString("name", newsplit[i]);
                    xtw.WriteRequiredAttributeString("description", newsplit[i]);
                }

                depth = depth - (last == -1 ? 0 : last) + newsplit.Count;

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="splitpath">Split path representing last kwown parent game (SabreDAT only)</param>
        /// <param name="newsplit">Split path representing the parent game (SabreDAT only)</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="last">Last known depth to cycle back from (SabreDAT only)</param>
        /// <returns>The new depth of the tag</returns>
        private int WriteEndGame(XmlTextWriter xtw, List<string> splitpath, List<string> newsplit, int depth, out int last)
        {
            last = 0;

            try
            {
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
                        // End directory
                        xtw.WriteEndElement();
                    }

                    // Reset the current depth
                    depth = 2 + last;
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(XmlTextWriter xtw, DatItem datItem, int depth, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state
                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "archive");
                        xtw.WriteRequiredAttributeString("name", datItem.Name);
                        xtw.WriteEndElement();
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "biosset");
                        xtw.WriteRequiredAttributeString("name", biosSet.Name);
                        xtw.WriteOptionalAttributeString("description", biosSet.Description);
                        xtw.WriteOptionalAttributeString("default", biosSet.Default.FromYesNo());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Chip:
                        var chip = datItem as Chip;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "chip");
                        xtw.WriteRequiredAttributeString("name", chip.Name);
                        xtw.WriteOptionalAttributeString("tag", chip.Tag);
                        xtw.WriteOptionalAttributeString("chiptype", chip.ChipType);
                        xtw.WriteOptionalAttributeString("clock", chip.Clock);
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "disk");
                        xtw.WriteRequiredAttributeString("name", disk.Name);
                        xtw.WriteOptionalAttributeString("md5", disk.MD5?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha1", disk.SHA1?.ToLowerInvariant());
                        if (disk.ItemStatus != ItemStatus.None)
                        {
                            xtw.WriteStartElement("flags");

                            xtw.WriteStartElement("flag");
                            xtw.WriteAttributeString("name", "status");
                            xtw.WriteAttributeString("value", disk.ItemStatus.FromItemStatus(false));
                            xtw.WriteEndElement();

                            // End flags
                            xtw.WriteEndElement();
                        }
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Media:
                        var media = datItem as Media;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "media");
                        xtw.WriteRequiredAttributeString("name", media.Name);
                        xtw.WriteOptionalAttributeString("md5", media.MD5?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha1", media.SHA1?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha256", media.SHA256?.ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "release");
                        xtw.WriteRequiredAttributeString("name", release.Name);
                        xtw.WriteOptionalAttributeString("region", release.Region);
                        xtw.WriteOptionalAttributeString("language", release.Language);
                        xtw.WriteOptionalAttributeString("date", release.Date);
                        xtw.WriteOptionalAttributeString("default", release.Default.FromYesNo());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "rom");
                        xtw.WriteRequiredAttributeString("name", rom.Name);
                        if (rom.Size != -1) xtw.WriteAttributeString("size", rom.Size.ToString());
                        xtw.WriteOptionalAttributeString("crc", rom.CRC?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("md5", rom.MD5?.ToLowerInvariant());
#if NET_FRAMEWORK
                        xtw.WriteOptionalAttributeString("ripemd160", rom.RIPEMD160?.ToLowerInvariant());
#endif
                        xtw.WriteOptionalAttributeString("sha1", rom.SHA1?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha256", rom.SHA256?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha384", rom.SHA384?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha512", rom.SHA512?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("date", rom.Date);
                        if (rom.ItemStatus != ItemStatus.None)
                        {
                            xtw.WriteStartElement("flags");

                            xtw.WriteStartElement("flag");
                            xtw.WriteAttributeString("name", "status");
                            xtw.WriteAttributeString("value", rom.ItemStatus.FromItemStatus(false));
                            xtw.WriteEndElement();

                            // End flags
                            xtw.WriteEndElement();
                        }
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Sample:
                        xtw.WriteStartElement("file");
                        xtw.WriteAttributeString("type", "sample");
                        xtw.WriteRequiredAttributeString("name", datItem.Name);
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
        /// <param name="depth">Current depth to output file at (SabreDAT only)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(XmlTextWriter xtw, int depth)
        {
            try
            {
                for (int i = depth - 1; i >= 2; i--)
                {
                    // End directory
                    xtw.WriteEndElement();
                }

                // End data
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
