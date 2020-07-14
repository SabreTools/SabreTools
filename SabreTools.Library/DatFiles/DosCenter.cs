using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Readers;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a DosCenter DAT
    /// </summary>
    internal class DosCenter : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public DosCenter(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse a DOSCenter DAT and return all found games and roms within
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
            ClrMameProReader cmpr = new ClrMameProReader(FileExtensions.TryOpenRead(filename), enc)
            {
                DosCenter = true
            };

            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore everything not top-level
                if (cmpr.RowType != CmpRowType.TopLevel)
                    continue;

                // Switch on the top-level name
                switch (cmpr.TopLevel.ToLowerInvariant())
                {
                    // Header values
                    case "doscenter":
                        ReadHeader(cmpr);
                        break;

                    // Sets
                    case "game":
                        ReadGame(cmpr, filename, indexId);
                        break;

                    default:
                        break;
                }
            }

            cmpr.Dispose();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="cmpr">ClrMameProReader to use to parse the header</param>
        private void ReadHeader(ClrMameProReader cmpr)
        {
            // If there's no subtree to the header, skip it
            if (cmpr == null || cmpr.EndOfStream)
                return;

            // While we don't hit an end element or end of stream
            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore comments, internal items, and nothingness
                if (cmpr.RowType == CmpRowType.None || cmpr.RowType == CmpRowType.Comment || cmpr.RowType == CmpRowType.Internal)
                    continue;

                // If we reached the end of a section, break
                if (cmpr.RowType == CmpRowType.EndTopLevel)
                    break;

                // If the standalone value is null, we skip
                if (cmpr.Standalone == null)
                    continue;

                string itemKey = cmpr.Standalone?.Key.ToLowerInvariant().TrimEnd(':');
                string itemVal = cmpr.Standalone?.Value;

                // For all other cases
                switch (itemKey)
                {
                    case "name":
                        DatHeader.Name = (string.IsNullOrWhiteSpace(DatHeader.Name) ? itemVal : DatHeader.Name);
                        break;
                    case "description":
                        DatHeader.Description = (string.IsNullOrWhiteSpace(DatHeader.Description) ? itemVal : DatHeader.Description);
                        break;
                    case "dersion":
                        DatHeader.Version = (string.IsNullOrWhiteSpace(DatHeader.Version) ? itemVal : DatHeader.Version);
                        break;
                    case "date":
                        DatHeader.Date = (string.IsNullOrWhiteSpace(DatHeader.Date) ? itemVal : DatHeader.Date);
                        break;
                    case "author":
                        DatHeader.Author = (string.IsNullOrWhiteSpace(DatHeader.Author) ? itemVal : DatHeader.Author);
                        break;
                    case "homepage":
                        DatHeader.Homepage = (string.IsNullOrWhiteSpace(DatHeader.Homepage) ? itemVal : DatHeader.Homepage);
                        break;
                    case "comment":
                        DatHeader.Comment = (string.IsNullOrWhiteSpace(DatHeader.Comment) ? itemVal : DatHeader.Comment);
                        break;
                }
            }
        }

        /// <summary>
        /// Read set information
        /// </summary>
        /// <param name="cmpr">ClrMameProReader to use to parse the header</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadGame(
            ClrMameProReader cmpr,

            // Standard Dat parsing
            string filename,
            int indexId)
        {
            // Prepare all internal variables
            bool containsItems = false;
            Machine machine = new Machine()
            {
                MachineType = MachineType.None,
            };

            // If there's no subtree to the header, skip it
            if (cmpr == null || cmpr.EndOfStream)
                return;

            // While we don't hit an end element or end of stream
            while (!cmpr.EndOfStream)
            {
                cmpr.ReadNextLine();

                // Ignore comments and nothingness
                if (cmpr.RowType == CmpRowType.None || cmpr.RowType == CmpRowType.Comment)
                    continue;

                // If we reached the end of a section, break
                if (cmpr.RowType == CmpRowType.EndTopLevel)
                    break;

                // Handle any standalone items
                if (cmpr.RowType == CmpRowType.Standalone && cmpr.Standalone != null)
                {
                    string itemKey = cmpr.Standalone?.Key.ToLowerInvariant();
                    string itemVal = cmpr.Standalone?.Value;

                    switch (itemKey)
                    {
                        case "name":
                            machine.Name = (itemVal.ToLowerInvariant().EndsWith(".zip") ? itemVal.Remove(itemVal.Length - 4) : itemVal);
                            machine.Description = (itemVal.ToLowerInvariant().EndsWith(".zip") ? itemVal.Remove(itemVal.Length - 4) : itemVal);
                            break;
                    }
                }

                // Handle any internal items
                else if (cmpr.RowType == CmpRowType.Internal
                    && string.Equals(cmpr.InternalName, "file", StringComparison.OrdinalIgnoreCase)
                    && cmpr.Internal != null)
                {
                    containsItems = true;

                    // Create the proper DatItem based on the type
                    Rom item = DatItem.Create(ItemType.Rom) as Rom;

                    // Then populate it with information
                    item.CopyMachineInformation(machine);

                    item.IndexId = indexId;
                    item.IndexSource = filename;

                    // Loop through all of the attributes
                    foreach (var kvp in cmpr.Internal)
                    {
                        string attrKey = kvp.Key;
                        string attrVal = kvp.Value;

                        switch (attrKey)
                        {
                            //If the item is empty, we automatically skip it because it's a fluke
                            case "":
                                continue;

                            // Regular attributes
                            case "name":
                                item.Name = attrVal;
                                break;

                            case "size":
                                if (Int64.TryParse(attrVal, out long size))
                                    item.Size = size;
                                else
                                    item.Size = -1;

                                break;

                            case "crc":
                                item.CRC = attrVal;
                                break;
                            case "date":
                                item.Date = attrVal;
                                break;
                        }
                    }

                    // Now process and add the rom
                    ParseAddHelper(item);
                }
            }

            // If no items were found for this machine, add a Blank placeholder
            if (!containsItems)
            {
                Blank blank = new Blank()
                {
                    IndexId = indexId,
                    IndexSource = filename,
                };

                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank);
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

                ClrMameProWriter cmpw = new ClrMameProWriter(fs, new UTF8Encoding(false))
                {
                    Quotes = false
                };

                // Write out the header
                WriteHeader(cmpw);

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

                        List<string> newsplit = rom.MachineName.Split('\\').ToList();

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteEndGame(cmpw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(cmpw, rom);

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
#if NET_FRAMEWORK
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
#endif
                            ((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
                            ((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
                            ((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
                            ((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
                        }

                        // Now, output the rom data
                        WriteDatItem(cmpw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(cmpw);

                Globals.Logger.Verbose($"File written!{Environment.NewLine}");
                cmpw.Dispose();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(ClrMameProWriter cmpw)
        {
            try
            {
                cmpw.WriteStartElement("DOSCenter");
                cmpw.WriteStandalone("Name:", DatHeader.Name, false);
                cmpw.WriteStandalone("Description:", DatHeader.Description, false);
                cmpw.WriteStandalone("Version:", DatHeader.Version, false);
                cmpw.WriteStandalone("Date:", DatHeader.Date, false);
                cmpw.WriteStandalone("Author:", DatHeader.Author, false);
                cmpw.WriteStandalone("Homepage:", DatHeader.Homepage, false);
                cmpw.WriteStandalone("Comment:", DatHeader.Comment, false);
                cmpw.WriteEndElement();

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(ClrMameProWriter cmpw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                cmpw.WriteStartElement("game");
                cmpw.WriteStandalone("name", $"{datItem.GetField(Field.MachineName, DatHeader.ExcludeFields)}.zip", true);

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(ClrMameProWriter cmpw)
        {
            try
            {
                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(ClrMameProWriter cmpw, DatItem datItem, bool ignoreblanks = false)
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
                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        cmpw.WriteStartElement("file");
                        cmpw.WriteAttributeString("name", datItem.GetField(Field.Name, DatHeader.ExcludeFields));
                        if (!DatHeader.ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            cmpw.WriteAttributeString("size", rom.Size.ToString());
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Date, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("date", rom.Date);
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CRC, DatHeader.ExcludeFields)))
                            cmpw.WriteAttributeString("crc", rom.CRC.ToLowerInvariant());
                        cmpw.WriteEndElement();
                        break;
                }

                cmpw.Flush();
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
        /// <param name="cmpw">ClrMameProWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(ClrMameProWriter cmpw)
        {
            try
            {
                // End game
                cmpw.WriteEndElement();

                cmpw.Flush();
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
