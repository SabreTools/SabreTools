using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of an AttractMode DAT
    /// </summary>
    internal class AttractMode : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public AttractMode(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse an AttractMode DAT and return all found games within
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

            sr.ReadLine(); // Skip the first line since it's the header
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                /*
                The gameinfo order is as follows
                0 - game name
                1 - game description
                2 - emulator name (filename)
                3 - cloneof
                4 - year
                5 - manufacturer
                6 - category
                7 - players
                8 - rotation
                9 - control
                10 - status
                11 - displaycount
                12 - displaytype
                13 - alt romname
                14 - alt title
                15 - extra
                16 - buttons
                */

                string[] gameinfo = line.Split(';');

                Rom rom = new Rom
                {
                    Name = "-",
                    Size = Constants.SizeZero,
                    CRC = Constants.CRCZero,
                    MD5 = Constants.MD5Zero,
#if NET_FRAMEWORK
                    RIPEMD160 = Constants.RIPEMD160Zero,
#endif
                    SHA1 = Constants.SHA1Zero,
                    ItemStatus = ItemStatus.None,

                    Machine = new Machine
                    {
                        Name = gameinfo[0],
                        Description = gameinfo[1],
                        CloneOf = gameinfo[3],
                        Year = gameinfo[4],
                        Manufacturer = gameinfo[5],
                        Category = gameinfo[6],
                        Players = gameinfo[7],
                        Rotation = gameinfo[8],
                        Control = gameinfo[9],
                        Status = gameinfo[10],
                        DisplayCount = gameinfo[11],
                        DisplayType = gameinfo[12],
                        Comment = gameinfo[15],
                        Buttons = gameinfo[16],
                    },

                    AltName = gameinfo[13],
                    AltTitle = gameinfo[14],

                    Source = new Source
                    {
                        Index = indexId,
                        Name = filename,
                    },                    
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
                    Separator = ';',
                    VerifyFieldCount = true
                };

                // Write out the header
                WriteHeader(svw);

                // Write out each of the machines and roms
                string lastgame = null;

                // Use a sorted list of games to output
                foreach (string key in Items.SortedKeys)
                {
                    List<DatItem> roms = Items[key];

                    // Resolve the names in the block
                    roms = DatItem.ResolveNames(roms);

                    for (int index = 0; index < roms.Count; index++)
                    {
                        DatItem item = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (item.Name == null || item.Machine.Name == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != item.Machine.Name.ToLowerInvariant())
                            WriteDatItem(svw, item, ignoreblanks);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (item.ItemType == ItemType.Rom
                            && (item as Rom).Size == -1
                            && (item as Rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {item.Machine.Name}");

                            item.Name = (item.Name == "null" ? "-" : item.Name);
                            (item as Rom).Size = Constants.SizeZero;
                        }

                        // Set the new data to compare against
                        lastgame = item.Machine.Name;
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
                    "#Title",
                    "Name",
                    "Emulator",
                    "CloneOf",
                    "Year",
                    "Manufacturer",
                    "Category",
                    "Players",
                    "Rotation",
                    "Control",
                    "Status",
                    "DisplayCount",
                    "DisplayType",
                    "AltRomname",
                    "AltTitle",
                    "Extra",
                    "Buttons",
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
        /// Write out Game start using the supplied StreamWriter
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
                // No game should start with a path separator
                datItem.Machine.Name = datItem.Machine.Name.TrimStart(Path.DirectorySeparatorChar);

                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state
                switch (datItem.ItemType)
                {
                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        string[] fields = new string[]
                        {
                            rom.Machine.Name,
                            rom.Machine.Description,
                            Header.FileName,
                            rom.Machine.CloneOf,
                            rom.Machine.Year,
                            rom.Machine.Manufacturer,
                            rom.Machine.Category,
                            rom.Machine.Players,
                            rom.Machine.Rotation,
                            rom.Machine.Control,
                            rom.ItemStatus.ToString(),
                            rom.Machine.DisplayCount,
                            rom.Machine.DisplayType,
                            rom.AltName,
                            rom.AltTitle,
                            rom.Machine.Comment,
                            rom.Machine.Buttons,
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
