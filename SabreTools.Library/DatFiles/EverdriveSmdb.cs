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
    /// Represents parsing and writing of an Everdrive SMDB file
    /// </summary>
    internal class EverdriveSMDB : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public EverdriveSMDB(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse an Everdrive SMDB file and return all found games within
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

                /*
                The gameinfo order is as follows
                0 - SHA-256
                1 - Machine Name/Filename
                2 - SHA-1
                3 - MD5
                4 - CRC32
                */

                string[] gameinfo = line.Split('\t');
                string[] fullname = gameinfo[1].Split('/');

                Rom rom = new Rom
                {
                    Name = gameinfo[1].Substring(fullname[0].Length + 1),
                    Size = -1, // No size provided, but we don't want the size being 0
                    CRC = Utilities.CleanHashData(gameinfo[4], Constants.CRCLength),
                    MD5 = Utilities.CleanHashData(gameinfo[3], Constants.MD5Length),
                    SHA1 = Utilities.CleanHashData(gameinfo[2], Constants.SHA1Length),
                    SHA256 = Utilities.CleanHashData(gameinfo[0], Constants.SHA256Length),
                    ItemStatus = ItemStatus.None,

                    MachineName = fullname[0],
                    MachineDescription = fullname[0],
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
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = Utilities.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
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
                        DatItem item = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (item.Name == null || item.MachineName == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (item.ItemType == ItemType.Rom
                            && ((Rom)item).Size == -1
                            && ((Rom)item).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {item.MachineName}");

                            item.Name = (item.Name == "null" ? "-" : item.Name);
                            ((Rom)item).Size = Constants.SizeZero;
                        }

                        WriteDatItem(sw, item, ignoreblanks);
                    }
                }

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
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
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(StreamWriter sw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Pre-process the item name
                ProcessItemName(datItem, true);

                string state = string.Empty;

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                    case ItemType.BiosSet:
                    case ItemType.Disk:
                    case ItemType.Release:
                    case ItemType.Sample:
                        // We don't output these at all for Everdrive SMDB
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        state += $"{rom.GetField(Field.SHA256, ExcludeFields)}\t";
                        state += $"{rom.GetField(Field.MachineName, ExcludeFields)}/\t";
                        state += $"{rom.GetField(Field.Name, ExcludeFields)}\t";
                        state += $"{rom.GetField(Field.SHA1, ExcludeFields)}\t";
                        state += $"{rom.GetField(Field.MD5, ExcludeFields)}\t";
                        state += $"{rom.GetField(Field.CRC, ExcludeFields)}";
                        state += "\n";
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
