using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
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
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse an Everdrive SMDB file and return all found games within
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
            // Open a file reader
            Encoding enc = FileExtensions.GetEncoding(filename);
            StreamReader sr = new StreamReader(FileExtensions.TryOpenRead(filename), enc);

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
                    CRC = gameinfo[4],
                    MD5 = gameinfo[3],
                    SHA1 = gameinfo[2],
                    SHA256 = gameinfo[0],
                    ItemStatus = ItemStatus.None,

                    MachineName = fullname[0],
                    MachineDescription = fullname[0],

                    IndexId = indexId,
                    IndexSource = filename,
                };

                // Now process and add the rom
                ParseAddHelper(rom);
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
                FileStream fs = FileExtensions.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                SeparatedValueWriter svw = new SeparatedValueWriter(fs, new UTF8Encoding(false))
                {
                    Quotes = false,
                    Separator = '\t',
                    VerifyFieldCount = true
                };

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

                        WriteDatItem(svw, item, ignoreblanks);
                    }
                }

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                svw.Dispose();
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
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(SeparatedValueWriter svw, DatItem datItem, bool ignoreblanks = false)
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

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Rom:
                        var rom = datItem as Rom;

                        string[] fields = new string[]
                        {
                            rom.GetField(Field.SHA256, DatHeader.ExcludeFields),
                            $"{rom.GetField(Field.MachineName, DatHeader.ExcludeFields)}/",
                            rom.GetField(Field.Name, DatHeader.ExcludeFields),
                            rom.GetField(Field.SHA1, DatHeader.ExcludeFields),
                            rom.GetField(Field.MD5, DatHeader.ExcludeFields),
                            rom.GetField(Field.CRC, DatHeader.ExcludeFields),
                        };

                        svw.WriteValues(fields);

                        break;
                }

                svw.Flush();
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
