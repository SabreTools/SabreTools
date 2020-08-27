using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a value-separated DAT
    /// </summary>
    internal class SeparatedValue : DatFile
    {
        // Private instance variables specific to Separated Value DATs
        private readonly char _delim;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="delim">Delimiter for parsing individual lines</param>
        public SeparatedValue(DatFile datFile, char delim)
            : base(datFile)
        {
            _delim = delim;
        }

        /// <summary>
        /// Parse a character-separated value DAT and return all found games and roms within
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
            SeparatedValueReader svr = new SeparatedValueReader(FileExtensions.TryOpenRead(filename), enc)
            {
                Header = true,
                Quotes = true,
                Separator = _delim,
                VerifyFieldCount = true
            };

            // If we're somehow at the end of the stream already, we can't do anything
            if (svr.EndOfStream)
                return;

            // Read in the header
            svr.ReadHeader();

            // Loop through all of the data lines
            while (!svr.EndOfStream)
            {
                try
                {
                    // Get the current line, split and parse
                    svr.ReadNextLine();
                }
                catch (InvalidDataException)
                {
                    Globals.Logger.Warning($"Malformed line found in '{filename}' at line {svr.LineNumber}");
                    continue;
                }
                
                // Create mapping dictionary
                var mappings = new Dictionary<Field, string>();

                // Now we loop through and get values for everything
                for (int i = 0; i < svr.HeaderValues.Count; i++)
                {
                    Field key = svr.HeaderValues[i].AsField();
                    string value = svr.Line[i];
                    mappings[key] = value;
                }

                // Set DatHeader fields
                DatHeader header = new DatHeader();
                header.SetFields(mappings);
                Header.ConditionalCopy(header);

                // Set Machine and DatItem fields
                if (mappings.ContainsKey(Field.DatItem_Type))
                {
                    DatItem datItem = DatItem.Create(mappings[Field.DatItem_Type].AsItemType());
                    datItem.SetFields(mappings);
                    datItem.Source = new Source(indexId, filename);
                    ParseAddHelper(datItem);
                }
            }
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
                    Quotes = true,
                    Separator = this._delim,
                    VerifyFieldCount = true
                };

                // Write out the header
                WriteHeader(svw);

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

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && (rom as Rom).Size == -1
                            && (rom as Rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.Machine.Name}");
                        }

                        // Now, output the rom data
                        WriteDatItem(svw, rom, ignoreblanks);
                    }
                }

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
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
        /// Write out DAT header using the supplied StreamWriter
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(SeparatedValueWriter svw)
        {
            try
            {
                string[] headers = new string[]
                {
                    "File Name",
                    "Internal Name",
                    "Description",
                    "Game Name",
                    "Game Description",
                    "Type",
                    "Rom Name",
                    "Disk Name",
                    "Size",
                    "CRC",
                    "MD5",
                    //"RIPEMD160",
                    "SHA1",
                    "SHA256",
                    //"SHA384",
                    //"SHA512",
                    "Nodump",
                };

                svw.WriteHeader(headers);

                svw.Flush();
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
                // Separated values should only output Rom and Disk
                if (datItem.ItemType != ItemType.Disk && datItem.ItemType != ItemType.Rom)
                    return true;

                // Build the state
                // TODO: Can we have some way of saying what fields to write out? Support for read extends to all fields now
                string[] fields = new string[14]; // 17;
                fields[0] = Header.FileName;
                fields[1] = Header.Name;
                fields[2] = Header.Description;
                fields[3] = datItem.Machine.Name;
                fields[4] = datItem.Machine.Description;

                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        fields[5] = "disk";
                        fields[6] = string.Empty;
                        fields[7] = disk.Name;
                        fields[8] = string.Empty;
                        fields[9] = string.Empty;
                        fields[10] = disk.MD5.ToLowerInvariant();
                        //fields[11] = string.Empty;
                        fields[11] = disk.SHA1.ToLowerInvariant();
                        fields[12] = string.Empty;
                        //fields[13] = string.Empty;
                        //fields[14] = string.Empty;
                        fields[13] = disk.ItemStatus.ToString();
                        break;

                    case ItemType.Media:
                        var media = datItem as Media;
                        fields[5] = "media";
                        fields[6] = string.Empty;
                        fields[7] = media.Name;
                        fields[8] = string.Empty;
                        fields[9] = string.Empty;
                        fields[10] = media.MD5.ToLowerInvariant();
                        //fields[11] = string.Empty;
                        fields[11] = media.SHA1.ToLowerInvariant();
                        fields[12] = media.SHA256?.ToLowerInvariant();
                        //fields[13] = string.Empty;
                        //fields[14] = string.Empty;
                        fields[13] = string.Empty;
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        fields[5] = "rom";
                        fields[6] = rom.Name;
                        fields[7] = string.Empty;
                        fields[8] = rom.Size.ToString();
                        fields[9] = rom.CRC?.ToLowerInvariant();
                        fields[10] = rom.MD5?.ToLowerInvariant();
                        //fields[11] = rom.RIPEMD160?.ToLowerInvariant();
                        fields[11] = rom.SHA1?.ToLowerInvariant();
                        fields[12] = rom.SHA256?.ToLowerInvariant();
                        //fields[13] = rom.SHA384?.ToLowerInvariant();
                        //fields[14] = rom.SHA512?.ToLowerInvariant();
                        fields[13] = rom.ItemStatus.ToString();
                        break;
                }

                svw.WriteString(CreatePrefixPostfix(datItem, true));
                svw.WriteValues(fields, false);
                svw.WriteString(CreatePrefixPostfix(datItem, false));
                svw.WriteLine();

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
