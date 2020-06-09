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
    /// Represents parsing and writing of a hashfile such as an SFV, MD5, or SHA-1 file
    /// </summary>
    internal class Hashfile : DatFile
    {
        // Private instance variables specific to Hashfile DATs
        Hash _hash;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="hash">Type of hash that is associated with this DAT</param> 
        public Hashfile(DatFile datFile, Hash hash)
            : base(datFile, cloneHeader: false)
        {
            _hash = hash;
        }

        /// <summary>
        /// Parse a hashfile or SFV and return all found games and roms within
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

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                // Split the line and get the name and hash
                string[] split = line.Split(' ');
                string name = string.Empty;
                string hash = string.Empty;

                // If we have CRC, then it's an SFV file and the name is first are
                if ((_hash & Hash.CRC) != 0)
                {
                    name = split[0].Replace("*", String.Empty);
                    hash = split[1];
                }
                // Otherwise, the name is second
                else
                {
                    name = split[1].Replace("*", String.Empty);
                    hash = split[0];
                }

                Rom rom = new Rom
                {
                    Name = name,
                    Size = -1,
                    CRC = ((_hash & Hash.CRC) != 0 ? Utilities.CleanHashData(hash, Constants.CRCLength) : null),
                    MD5 = ((_hash & Hash.MD5) != 0 ? Utilities.CleanHashData(hash, Constants.MD5Length) : null),
                    RIPEMD160 = ((_hash & Hash.RIPEMD160) != 0 ? Utilities.CleanHashData(hash, Constants.RIPEMD160Length) : null),
                    SHA1 = ((_hash & Hash.SHA1) != 0 ? Utilities.CleanHashData(hash, Constants.SHA1Length) : null),
                    SHA256 = ((_hash & Hash.SHA256) != 0 ? Utilities.CleanHashData(hash, Constants.SHA256Length) : null),
                    SHA384 = ((_hash & Hash.SHA384) != 0 ? Utilities.CleanHashData(hash, Constants.SHA384Length) : null),
                    SHA512 = ((_hash & Hash.SHA512) != 0 ? Utilities.CleanHashData(hash, Constants.SHA512Length) : null),
                    ItemStatus = ItemStatus.None,

                    MachineName = Path.GetFileNameWithoutExtension(filename),

                    SystemID = sysid,
                    SourceID = srcid,
                };

                // Now process and add the rom
                ParseAddHelper(rom, clean, remUnicode);
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
                Globals.Logger.User("Opening file for writing: {0}", outfile);
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning("File '{0}' could not be created for writing! Please check to see if the file is writable", outfile);
                    return false;
                }

                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

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
                            Globals.Logger.Verbose("Empty folder found: {0}", rom.MachineName);
                        }

                        // Now, output the rom data
                        WriteDatItem(sw, rom, ignoreblanks);
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

                switch (_hash)
                {
                    case Hash.CRC:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty)
                                + " " + (!ExcludeFields[(int)Field.CRC] ? ((Rom)rom).CRC : string.Empty) + "\n";
                        }
                        break;
                    case Hash.MD5:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.MD5] ? ((Rom)rom).MD5 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.MD5] ? ((Disk)rom).MD5 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        break;
                    case Hash.RIPEMD160:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.RIPEMD160] ? ((Rom)rom).RIPEMD160 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.RIPEMD160] ? ((Disk)rom).RIPEMD160 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        break;
                    case Hash.SHA1:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.SHA1] ? ((Rom)rom).SHA1 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.SHA1] ? ((Disk)rom).SHA1 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        break;
                    case Hash.SHA256:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.SHA256] ? ((Rom)rom).SHA256 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.SHA256] ? ((Disk)rom).SHA256 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        break;
                    case Hash.SHA384:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.SHA384] ? ((Rom)rom).SHA384 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.SHA384] ? ((Disk)rom).SHA384 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        break;
                    case Hash.SHA512:
                        if (rom.ItemType == ItemType.Rom)
                        {
                            state += (!ExcludeFields[(int)Field.SHA512] ? ((Rom)rom).SHA512 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
                        else if (rom.ItemType == ItemType.Disk)
                        {
                            state += (!ExcludeFields[(int)Field.SHA512] ? ((Disk)rom).SHA512 : string.Empty)
                                + " *" + (!ExcludeFields[(int)Field.MachineName] && GameName ? rom.MachineName + Path.DirectorySeparatorChar : string.Empty)
                                + (!ExcludeFields[(int)Field.Name] ? rom.Name : string.Empty) + "\n";
                        }
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
