using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a RomCenter DAT
    /// </summary>
    /// TODO: Port over the INI parser from DICUI to make this flow a little better
    internal class RomCenter : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public RomCenter(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a RomCenter DAT and return all found games and roms within
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
            // Open a file reader
            Encoding enc = Utilities.GetEncoding(filename);
            StreamReader sr = new StreamReader(Utilities.TryOpenRead(filename), enc);

            string blocktype = string.Empty;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                // If the line is the start of the credits section
                if (line.ToLowerInvariant().StartsWith("[credits]"))
                {
                    blocktype = "credits";
                }

                // If the line is the start of the dat section
                else if (line.ToLowerInvariant().StartsWith("[dat]"))
                {
                    blocktype = "dat";
                }

                // If the line is the start of the emulator section
                else if (line.ToLowerInvariant().StartsWith("[emulator]"))
                {
                    blocktype = "emulator";
                }

                // If the line is the start of the game section
                else if (line.ToLowerInvariant().StartsWith("[games]"))
                {
                    blocktype = "games";
                }

                // Otherwise, it's not a section and it's data, so get out all data
                else
                {
                    // If we have an author
                    if (line.ToLowerInvariant().StartsWith("author="))
                    {
                        Author = (string.IsNullOrWhiteSpace(Author) ? line.Split('=')[1] : Author);
                    }

                    // If we have one of the three version tags
                    else if (line.ToLowerInvariant().StartsWith("version="))
                    {
                        switch (blocktype)
                        {
                            case "credits":
                                Version = (string.IsNullOrWhiteSpace(Version) ? line.Split('=')[1] : Version);
                                break;

                            case "emulator":
                                Description = (string.IsNullOrWhiteSpace(Description) ? line.Split('=')[1] : Description);
                                break;
                        }
                    }

                    // If we have a URL
                    else if (line.ToLowerInvariant().StartsWith("url="))
                    {
                        Url = (string.IsNullOrWhiteSpace(Url) ? line.Split('=')[1] : Url);
                    }

                    // If we have a comment
                    else if (line.ToLowerInvariant().StartsWith("comment="))
                    {
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? line.Split('=')[1] : Comment);
                    }

                    // If we have the split flag
                    else if (line.ToLowerInvariant().StartsWith("split="))
                    {
                        if (Int32.TryParse(line.Split('=')[1], out int split))
                        {
                            if (split == 1 && ForceMerging == ForceMerging.None)
                                ForceMerging = ForceMerging.Split;
                        }
                    }

                    // If we have the merge tag
                    else if (line.ToLowerInvariant().StartsWith("merge="))
                    {
                        if (Int32.TryParse(line.Split('=')[1], out int merge))
                        {
                            if (merge == 1 && ForceMerging == ForceMerging.None)
                                ForceMerging = ForceMerging.Full;
                        }
                    }

                    // If we have the refname tag
                    else if (line.ToLowerInvariant().StartsWith("refname="))
                    {
                        Name = (string.IsNullOrWhiteSpace(Name) ? line.Split('=')[1] : Name);
                    }

                    // If we have a rom
                    else if (line.StartsWith("¬"))
                    {
                        // Some old RC DATs have this behavior
                        if (line.Contains("¬N¬O"))
                            line = line.Replace("¬N¬O", string.Empty) + "¬¬";

                        /*
                        The rominfo order is as follows:
                        1 - parent name
                        2 - parent description
                        3 - game name
                        4 - game description
                        5 - rom name
                        6 - rom crc
                        7 - rom size
                        8 - romof name
                        9 - merge name
                        */
                        string[] rominfo = line.Split('¬');

                        // Try getting the size separately
                        if (!Int64.TryParse(rominfo[7], out long size))
                            size = 0;

                        Rom rom = new Rom
                        {
                            Name = rominfo[5],
                            Size = size,
                            CRC = Utilities.CleanHashData(rominfo[6], Constants.CRCLength),
                            ItemStatus = ItemStatus.None,

                            MachineName = rominfo[3],
                            MachineDescription = rominfo[4],
                            CloneOf = rominfo[1],
                            RomOf = rominfo[8],

                            SystemID = sysid,
                            SourceID = srcid,
                        };

                        // Now process and add the rom
                        ParseAddHelper(rom, clean, remUnicode);
                    }
                }
            }

            sr.Dispose();
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

                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

                // Write out the header
                WriteHeader(sw);

                // Write out each of the machines and roms
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
                        WriteDatItem(sw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

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
                string header = "[CREDITS]\n";
                header += $"author={Author}\n";
                header += $"version={Version}\n";
                header += $"comment={Comment}\n";
                header += "[DAT]\n";
                header += "version=2.50\n";
                header += $"split={(ForceMerging == ForceMerging.Split ? "1" : "0")}\n";
                header += $"merge={(ForceMerging == ForceMerging.Full || ForceMerging == ForceMerging.Merged ? "1" : "0")}\n";
                header += "[EMULATOR]\n";
                header += $"refname={Name}\n";
                header += $"version={Description}\n";
                header += "[GAMES]\n";

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
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(StreamWriter sw, DatItem datItem, bool ignoreblanks = false)
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
                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        state += "¬";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                            state += datItem.CloneOf;
                        state += "¬";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                            state += datItem.CloneOf;
                        state += $"¬{datItem.GetField(Field.MachineName, ExcludeFields)}";
                        if (string.IsNullOrWhiteSpace(datItem.MachineDescription))
                            state += $"¬{datItem.GetField(Field.MachineName, ExcludeFields)}";
                        else
                            state += $"¬{datItem.GetField(Field.Description, ExcludeFields)}";
                        state += $"¬{datItem.GetField(Field.Name, ExcludeFields)}";
                        state += "¬¬¬¬¬\n";
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        state += "¬";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                            state += datItem.CloneOf;
                        state += "¬";
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)))
                            state += datItem.CloneOf;
                        state += $"¬{datItem.GetField(Field.MachineName, ExcludeFields)}";
                        if (string.IsNullOrWhiteSpace(datItem.MachineDescription))
                            state += $"¬{datItem.GetField(Field.MachineName, ExcludeFields)}";
                        else
                            state += $"¬{datItem.GetField(Field.Description, ExcludeFields)}";
                        state += $"¬{datItem.GetField(Field.Name, ExcludeFields)}";
                        state += $"¬{datItem.GetField(Field.CRC, ExcludeFields)}";
                        state += $"¬{datItem.GetField(Field.Size, ExcludeFields)}";
                        state += "¬¬¬\n";
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
    }
}
