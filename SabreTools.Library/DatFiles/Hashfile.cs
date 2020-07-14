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
    /// Represents parsing and writing of a hashfile such as an SFV, MD5, or SHA-1 file
    /// </summary>
    internal class Hashfile : DatFile
    {
        // Private instance variables specific to Hashfile DATs
        private readonly Hash _hash;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="hash">Type of hash that is associated with this DAT</param> 
        public Hashfile(DatFile datFile, Hash hash)
            : base(datFile)
        {
            _hash = hash;
        }

        /// <summary>
        /// Parse a hashfile or SFV and return all found games and roms within
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

                // Split the line and get the name and hash
                string[] split = line.Split(' ');
                string name = string.Empty;
                string hash = string.Empty;

                // If we have CRC, then it's an SFV file and the name is first are
                if (_hash.HasFlag(Hash.CRC))
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
                    CRC = (_hash.HasFlag(Hash.CRC) ? hash : null),
                    MD5 = (_hash.HasFlag(Hash.MD5) ? hash : null),
#if NET_FRAMEWORK
                    RIPEMD160 = (_hash.HasFlag(Hash.RIPEMD160) ? hash : null),
#endif
                    SHA1 = (_hash.HasFlag(Hash.SHA1) ? hash : null),
                    SHA256 = (_hash.HasFlag(Hash.SHA256) ? hash : null),
                    SHA384 = (_hash.HasFlag(Hash.SHA384) ? hash : null),
                    SHA512 = (_hash.HasFlag(Hash.SHA512) ? hash : null),
                    ItemStatus = ItemStatus.None,

                    MachineName = Path.GetFileNameWithoutExtension(filename),

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
                    Separator = ' ',
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
                        }

                        // Now, output the rom data
                        WriteDatItem(svw, rom, ignoreblanks);
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
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(SeparatedValueWriter svw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Build the state based on excluded fields
                string[] fields = new string[2];
                switch (_hash)
                {
                    case Hash.CRC:
                        switch (datItem.ItemType)
                        {
                            case ItemType.Rom:
                                var rom = datItem as Rom;
                                fields[0] = string.Empty;
                                if (DatHeader.GameName)
                                    fields[0] = $"{rom.GetField(Field.MachineName, DatHeader.ExcludeFields)}{Path.DirectorySeparatorChar}";
                                fields[0] += rom.GetField(Field.Name, DatHeader.ExcludeFields);
                                fields[1] = rom.GetField(Field.CRC, DatHeader.ExcludeFields);
                                break;
                        }
                        break;

                    case Hash.MD5:
#if NET_FRAMEWORK
                    case Hash.RIPEMD160:
#endif
                    case Hash.SHA1:
                    case Hash.SHA256:
                    case Hash.SHA384:
                    case Hash.SHA512:
                        Field hashField = _hash.AsField();

                        switch (datItem.ItemType)
                        {
                            case ItemType.Disk:
                                var disk = datItem as Disk;
                                fields[0] = disk.GetField(hashField, DatHeader.ExcludeFields);
                                fields[1] = string.Empty;
                                if (DatHeader.GameName)
                                    fields[1] = $"{disk.GetField(Field.MachineName, DatHeader.ExcludeFields)}{Path.DirectorySeparatorChar}";
                                fields[1] += disk.GetField(Field.Name, DatHeader.ExcludeFields);
                                break;

                            case ItemType.Rom:
                                var rom = datItem as Rom;
                                fields[0] = rom.GetField(hashField, DatHeader.ExcludeFields);
                                fields[1] = string.Empty;
                                if (DatHeader.GameName)
                                    fields[1] = $"{rom.GetField(Field.MachineName, DatHeader.ExcludeFields)}{Path.DirectorySeparatorChar}";
                                fields[1] += rom.GetField(Field.Name, DatHeader.ExcludeFields);
                                break;
                        }
                        break;
                }

                // If we had at least one field filled in
                if (!string.IsNullOrEmpty(fields[0]) || !string.IsNullOrEmpty(fields[1]))
                    svw.WriteValues(fields);

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
