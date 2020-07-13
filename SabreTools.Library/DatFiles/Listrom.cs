using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a MAME Listrom DAT
    /// </summary>
    internal class Listrom : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Listrom(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse a MAME Listrom DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <remarks>
        /// In a new style MAME listrom DAT, each game has the following format:
        /// 
        /// ROMs required for driver "005".
        /// Name                                   Size Checksum
        /// 1346b.cpu-u25                          2048 CRC(8e68533e) SHA1(a257c556d31691068ed5c991f1fb2b51da4826db)
        /// 6331.sound-u8                            32 BAD CRC(1d298cb0) SHA1(bb0bb62365402543e3154b9a77be9c75010e6abc) BAD_DUMP
        /// 16v8h-blue.u24                          279 NO GOOD DUMP KNOWN
        /// </remarks>
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

            string gamename = string.Empty;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();

                // If we have a blank line, we just skip it
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // If we have the descriptor line, ignore it
                else if (line == "Name                                   Size Checksum")
                {
                    continue;
                }

                // If we have the beginning of a game, set the name of the game
                else if (line.StartsWith("ROMs required for"))
                {
                    gamename = Regex.Match(line, @"^ROMs required for \S*? string.Empty(.*?)string.Empty\.").Groups[1].Value;
                }

                // If we have a machine with no required roms (usually internal devices), skip it
                else if (line.StartsWith("No ROMs required for"))
                {
                    continue;
                }

                // Otherwise, we assume we have a rom that we need to add
                else
                {
                    // First, we preprocess the line so that the rom name is consistently correct
                    string[] split = line.Split(new string[] { "    " }, StringSplitOptions.RemoveEmptyEntries);

                    // If the line doesn't have the 4 spaces of padding, check for 3
                    if (split.Length == 1)
                        split = line.Split(new string[] { "   " }, StringSplitOptions.RemoveEmptyEntries);

                    // If the split is still unsuccessful, log it and skip
                    if (split.Length == 1)
                        Globals.Logger.Warning($"Possibly malformed line: '{line}'");

                    string romname = split[0];
                    line = line.Substring(romname.Length);

                    // Next we separate the ROM into pieces
                    split = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                    // Standard Disks have 2 pieces (name, sha1)
                    if (split.Length == 1)
                    {
                        Disk disk = new Disk()
                        {
                            Name = romname,
                            SHA1 = Sanitizer.CleanListromHashData(split[0]),

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(disk);
                    }

                    // Baddump Disks have 4 pieces (name, BAD, sha1, BAD_DUMP)
                    else if (split.Length == 3 && line.EndsWith("BAD_DUMP"))
                    {
                        Disk disk = new Disk()
                        {
                            Name = romname,
                            SHA1 = Sanitizer.CleanListromHashData(split[1]),
                            ItemStatus = ItemStatus.BadDump,

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(disk);
                    }

                    // Standard ROMs have 4 pieces (name, size, crc, sha1)
                    else if (split.Length == 3)
                    {
                        if (!Int64.TryParse(split[0], out long size))
                            size = 0;

                        Rom rom = new Rom()
                        {
                            Name = romname,
                            Size = size,
                            CRC = Sanitizer.CleanListromHashData(split[1]),
                            SHA1 = Sanitizer.CleanListromHashData(split[2]),

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(rom);
                    }

                    // Nodump Disks have 5 pieces (name, NO, GOOD, DUMP, KNOWN)
                    else if (split.Length == 4 && line.EndsWith("NO GOOD DUMP KNOWN"))
                    {
                        Disk disk = new Disk()
                        {
                            Name = romname,
                            ItemStatus = ItemStatus.Nodump,

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(disk);
                    }

                    // Baddump ROMs have 6 pieces (name, size, BAD, crc, sha1, BAD_DUMP)
                    else if (split.Length == 5 && line.EndsWith("BAD_DUMP"))
                    {
                        if (!Int64.TryParse(split[0], out long size))
                            size = 0;

                        Rom rom = new Rom()
                        {
                            Name = romname,
                            Size = size,
                            CRC = Sanitizer.CleanListromHashData(split[2]),
                            SHA1 = Sanitizer.CleanListromHashData(split[3]),
                            ItemStatus = ItemStatus.BadDump,

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(rom);
                    }

                    // Nodump ROMs have 6 pieces (name, size, NO, GOOD, DUMP, KNOWN)
                    else if (split.Length == 5 && line.EndsWith("NO GOOD DUMP KNOWN"))
                    {
                        if (!Int64.TryParse(split[0], out long size))
                            size = 0;

                        Rom rom = new Rom()
                        {
                            Name = romname,
                            Size = size,
                            ItemStatus = ItemStatus.Nodump,

                            MachineName = gamename,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        ParseAddHelper(rom);
                    }

                    // If we have something else, it's invalid
                    else
                    {
                        Globals.Logger.Warning($"Invalid line detected: '{romname} {line}'");
                    }
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

                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));

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

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(sw, rom);

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
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="rom">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(StreamWriter sw, DatItem rom)
        {
            try
            {
                // No game should start with a path separator
                rom.MachineName = rom.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                sw.Write($"ROMs required for driver \"{rom.GetField(Field.MachineName, DatHeader.ExcludeFields)}\".\n");
                sw.Write("Name                                   Size Checksum\n");

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
        /// Write out Game end using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw)
        {
            try
            {
                // End driver
                sw.Write("\n");

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
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        var disk = datItem as Disk;

                        // The name is padded out to a particular length
                        if (disk.Name.Length < 43)
                            sw.Write(disk.Name.PadRight(43, ' '));
                        else
                            sw.Write($"{disk.Name}          ");

                        // If we have a baddump, put the first indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && disk.ItemStatus == ItemStatus.BadDump)
                            sw.Write(" BAD");

                        // If we have a nodump, write out the indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && disk.ItemStatus == ItemStatus.Nodump)
                            sw.Write(" NO GOOD DUMP KNOWN");

                        // Otherwise, write out the SHA-1 hash
                        else if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
                            sw.Write($" SHA1({disk.SHA1})");

                        // If we have a baddump, put the second indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && disk.ItemStatus == ItemStatus.BadDump)
                            sw.Write(" BAD_DUMP");

                        sw.Write("\n");
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;

                        // The name is padded out to a particular length
                        if (rom.Name.Length < 43)
                            sw.Write(rom.Name.PadRight(43 - rom.Size.ToString().Length, ' '));
                        else
                            sw.Write($"{rom.Name}          ");

                        // If we don't have a nodump, write out the size
                        if (rom.ItemStatus != ItemStatus.Nodump)
                            sw.Write(rom.Size);

                        // If we have a baddump, put the first indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && rom.ItemStatus == ItemStatus.BadDump)
                            sw.Write(" BAD");

                        // If we have a nodump, write out the indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && rom.ItemStatus == ItemStatus.Nodump)
                        {
                            sw.Write(" NO GOOD DUMP KNOWN");
                        }
                        // Otherwise, write out the CRC and SHA-1 hashes
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, DatHeader.ExcludeFields)))
                                sw.Write($" CRC({rom.CRC})");
                            if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SHA1, DatHeader.ExcludeFields)))
                                sw.Write($" SHA1({rom.SHA1})");
                        }

                        // If we have a baddump, put the second indicator
                        if (!DatHeader.ExcludeFields[(int)Field.Status] && rom.ItemStatus == ItemStatus.BadDump)
                            sw.Write(" BAD_DUMP");

                        sw.Write("\n");
                        break;
                }

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
