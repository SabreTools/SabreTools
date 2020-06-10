using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a MAME XML DAT
    /// </summary>
    /// TODO: Verify that all write for this DatFile type is correct
    internal class Listxml : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Listxml(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a MAME XML DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <remarks>
        /// </remarks>
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
            // Prepare all internal variables
            Encoding enc = Utilities.GetEncoding(filename);
            XmlReader xtr = Utilities.GetXmlTextReader(filename);

            // If we got a null reader, just return
            if (xtr == null)
                return;

            // Otherwise, read the file to the end
            try
            {
                xtr.MoveToContent();
                while (!xtr.EOF)
                {
                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        case "mame":
                            Name = (string.IsNullOrWhiteSpace(Name) ? xtr.GetAttribute("build") : Name);
                            Description = (string.IsNullOrWhiteSpace(Description) ? Name : Name);
                            // string mame_debug = xtr.GetAttribute("debug"); // (yes|no) "no"
                            // string mame_mameconfig = xtr.GetAttribute("mameconfig"); CDATA
                            xtr.Read();
                            break;

                        // Handle M1 DATs since they're 99% the same as a SL DAT
                        case "m1":
                            Name = (string.IsNullOrWhiteSpace(Name) ? "M1" : Name);
                            Description = (string.IsNullOrWhiteSpace(Description) ? "M1" : Description);
                            Version = (string.IsNullOrWhiteSpace(Version) ? xtr.GetAttribute("version") ?? string.Empty : Version);
                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the machine
                        case "game": // Some older DATs still use "game"
                        case "machine":
                            ReadMachine(xtr.ReadSubtree(), filename, sysid, srcid, clean, remUnicode);

                            // Skip the machine now that we've processed it
                            xtr.Skip();
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
        /// Read machine information
        /// </summary>
        /// <param name="reader">XmlReader representing a machine block</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadMachine(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool clean,
            bool remUnicode)
        {
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            string key = string.Empty;
            string temptype = reader.Name;
            bool containsItems = false;

            // Create a new machine
            MachineType machineType = MachineType.NULL;
            if (Utilities.GetYesNo(reader.GetAttribute("isbios")) == true)
                machineType |= MachineType.Bios;

            if (Utilities.GetYesNo(reader.GetAttribute("isdevice")) == true)
                machineType |= MachineType.Device;

            if (Utilities.GetYesNo(reader.GetAttribute("ismechanical")) == true)
                machineType |= MachineType.Mechanical;

            Machine machine = new Machine
            {
                Name = reader.GetAttribute("name"),
                Description = reader.GetAttribute("name"),
                SourceFile = reader.GetAttribute("sourcefile"),
                Runnable = Utilities.GetYesNo(reader.GetAttribute("runnable")),

                Comment = string.Empty,

                CloneOf = reader.GetAttribute("cloneof") ?? string.Empty,
                RomOf = reader.GetAttribute("romof") ?? string.Empty,
                SampleOf = reader.GetAttribute("sampleof") ?? string.Empty,
                Devices = new List<string>(),
                SlotOptions = new List<string>(),

                MachineType = (machineType == MachineType.NULL ? MachineType.None : machineType),
            };

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the roms from the machine
                switch (reader.Name)
                {
                    case "description":
                        machine.Description = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "manufacturer":
                        machine.Manufacturer = reader.ReadElementContentAsString();
                        break;

                    case "biosset":
                        containsItems = true;

                        DatItem biosset = new BiosSet
                        {
                            Name = reader.GetAttribute("name"),
                            Description = reader.GetAttribute("description"),
                            Default = Utilities.GetYesNo(reader.GetAttribute("default")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        biosset.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(biosset, clean, remUnicode);

                        reader.Read();
                        break;

                    case "rom":
                        containsItems = true;

                        DatItem rom = new Rom
                        {
                            Name = reader.GetAttribute("name"),
                            Bios = reader.GetAttribute("bios"),
                            Size = Utilities.GetSize(reader.GetAttribute("size")),
                            CRC = Utilities.CleanHashData(reader.GetAttribute("crc"), Constants.CRCLength),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.SHA1Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            MergeTag = reader.GetAttribute("merge"),
                            Region = reader.GetAttribute("region"),
                            Offset = reader.GetAttribute("offset"),
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            Optional = Utilities.GetYesNo(reader.GetAttribute("optional")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        rom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(rom, clean, remUnicode);

                        reader.Read();
                        break;

                    case "disk":
                        containsItems = true;

                        DatItem disk = new Disk
                        {
                            Name = reader.GetAttribute("name"),
                            MD5 = Utilities.CleanHashData(reader.GetAttribute("md5"), Constants.MD5Length),
                            RIPEMD160 = Utilities.CleanHashData(reader.GetAttribute("ripemd160"), Constants.SHA1Length),
                            SHA1 = Utilities.CleanHashData(reader.GetAttribute("sha1"), Constants.SHA1Length),
                            SHA256 = Utilities.CleanHashData(reader.GetAttribute("sha256"), Constants.SHA256Length),
                            SHA384 = Utilities.CleanHashData(reader.GetAttribute("sha384"), Constants.SHA384Length),
                            SHA512 = Utilities.CleanHashData(reader.GetAttribute("sha512"), Constants.SHA512Length),
                            MergeTag = reader.GetAttribute("merge"),
                            Region = reader.GetAttribute("region"),
                            Index = reader.GetAttribute("index"),
                            Writable = Utilities.GetYesNo(reader.GetAttribute("writable")),
                            ItemStatus = Utilities.GetItemStatus(reader.GetAttribute("status")),
                            Optional = Utilities.GetYesNo(reader.GetAttribute("optional")),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        disk.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(disk, clean, remUnicode);

                        reader.Read();
                        break;

                    case "device_ref":
                        string device_ref_name = reader.GetAttribute("name");
                        if (!machine.Devices.Contains(device_ref_name))
                            machine.Devices.Add(device_ref_name);

                        reader.Read();
                        break;

                    case "sample":
                        containsItems = true;

                        DatItem samplerom = new Sample
                        {
                            Name = reader.GetAttribute("name"),

                            SystemID = sysid,
                            System = filename,
                            SourceID = srcid,
                        };

                        samplerom.CopyMachineInformation(machine);

                        // Now process and add the rom
                        key = ParseAddHelper(samplerom, clean, remUnicode);

                        reader.Read();
                        break;

                    case "chip":
                        // string chip_name = reader.GetAttribute("name");
                        // string chip_tag = reader.GetAttribute("tag");
                        // string chip_type = reader.GetAttribute("type"); // (cpu|audio)
                        // string chip_clock = reader.GetAttribute("clock");

                        reader.Read();
                        break;

                    case "display":
                        // string display_tag = reader.GetAttribute("tag");
                        // string display_type = reader.GetAttribute("type"); // (raster|vector|lcd|svg|unknown)
                        // string display_rotate = reader.GetAttribute("rotate"); // (0|90|180|270)
                        // bool? display_flipx = Utilities.GetYesNo(reader.GetAttribute("flipx"));
                        // string display_width = reader.GetAttribute("width");
                        // string display_height = reader.GetAttribute("height");
                        // string display_refresh = reader.GetAttribute("refresh");
                        // string display_pixclock = reader.GetAttribute("pixclock");
                        // string display_htotal = reader.GetAttribute("htotal");
                        // string display_hbend = reader.GetAttribute("hbend");
                        // string display_hbstart = reader.GetAttribute("hbstart");
                        // string display_vtotal = reader.GetAttribute("vtotal");
                        // string display_vbend = reader.GetAttribute("vbend");
                        // string display_vbstart = reader.GetAttribute("vbstart");

                        reader.Read();
                        break;

                    case "sound":
                        // string sound_channels = reader.GetAttribute("channels");

                        reader.Read();
                        break;

                    case "condition":
                        // string condition_tag = reader.GetAttribute("tag");
                        // string condition_mask = reader.GetAttribute("mask");
                        // string condition_relation = reader.GetAttribute("relation"); // (eq|ne|gt|le|lt|ge)
                        // string condition_value = reader.GetAttribute("value");

                        reader.Read();
                        break;

                    case "input":
                        // bool? input_service = Utilities.GetYesNo(reader.GetAttribute("service"));
                        // bool? input_tilt = Utilities.GetYesNo(reader.GetAttribute("tilt"));
                        // string input_players = reader.GetAttribute("players");
                        // string input_coins = reader.GetAttribute("coins");

                        // // While the subtree contains <control> elements...
                        // string control_type = reader.GetAttribute("type");
                        // string control_player = reader.GetAttribute("player");
                        // string control_buttons = reader.GetAttribute("buttons");
                        // string control_regbuttons = reader.GetAttribute("regbuttons");
                        // string control_minimum = reader.GetAttribute("minimum");
                        // string control_maximum = reader.GetAttribute("maximum");
                        // string control_sensitivity = reader.GetAttribute("sensitivity");
                        // string control_keydelta = reader.GetAttribute("keydelta");
                        // bool? control_reverse = Utilities.GetYesNo(reader.GetAttribute("reverse"));
                        // string control_ways = reader.GetAttribute("ways");
                        // string control_ways2 = reader.GetAttribute("ways2");
                        // string control_ways3 = reader.GetAttribute("ways3");

                        reader.Skip();
                        break;

                    case "dipswitch":
                        // string dipswitch_name = reader.GetAttribute("name");
                        // string dipswitch_tag = reader.GetAttribute("tag");
                        // string dipswitch_mask = reader.GetAttribute("mask");

                        // // While the subtree contains <diplocation> elements...
                        // string diplocation_name = reader.GetAttribute("name");
                        // string diplocation_number = reader.GetAttribute("number");
                        // bool? diplocation_inverted = Utilities.GetYesNo(reader.GetAttribute("inverted"));

                        // // While the subtree contains <dipvalue> elements...
                        // string dipvalue_name = reader.GetAttribute("name");
                        // string dipvalue_value = reader.GetAttribute("value");
                        // bool? dipvalue_default = Utilities.GetYesNo(reader.GetAttribute("default"));

                        reader.Skip();
                        break;

                    case "configuration":
                        // string configuration_name = reader.GetAttribute("name");
                        // string configuration_tag = reader.GetAttribute("tag");
                        // string configuration_mask = reader.GetAttribute("mask");

                        // // While the subtree contains <conflocation> elements...
                        // string conflocation_name = reader.GetAttribute("name");
                        // string conflocation_number = reader.GetAttribute("number");
                        // bool? conflocation_inverted = Utilities.GetYesNo(reader.GetAttribute("inverted"));

                        // // While the subtree contains <confsetting> elements...
                        // string confsetting_name = reader.GetAttribute("name");
                        // string confsetting_value = reader.GetAttribute("value");
                        // bool? confsetting_default = Utilities.GetYesNo(reader.GetAttribute("default"));

                        reader.Skip();
                        break;

                    case "port":
                        // string port_tag = reader.GetAttribute("tag");

                        // // While the subtree contains <analog> elements...
                        // string analog_mask = reader.GetAttribute("mask");

                        reader.Skip();
                        break;

                    case "adjuster":
                        // string adjuster_name = reader.GetAttribute("name");
                        // bool? adjuster_default = Utilities.GetYesNo(reader.GetAttribute("default"));

                        // // For the one possible <condition> element...
                        // string condition_tag = reader.GetAttribute("tag");
                        // string condition_mask = reader.GetAttribute("mask");
                        // string condition_relation = reader.GetAttribute("relation"); // (eq|ne|gt|le|lt|ge)
                        // string condition_value = reader.GetAttribute("value");

                        reader.Skip();
                        break;
                    case "driver":
                        // string driver_status = reader.GetAttribute("status"); // (good|imperfect|preliminary)
                        // string driver_emulation = reader.GetAttribute("emulation"); // (good|imperfect|preliminary)
                        // string driver_cocktail = reader.GetAttribute("cocktail"); // (good|imperfect|preliminary)
                        // string driver_savestate = reader.GetAttribute("savestate"); // (supported|unsupported)

                        reader.Read();
                        break;

                    case "feature":
                        // string feature_type = reader.GetAttribute("type"); // (protection|palette|graphics|sound|controls|keyboard|mouse|microphone|camera|disk|printer|lan|wan|timing)
                        // string feature_status = reader.GetAttribute("status"); // (unemulated|imperfect)
                        // string feature_overall = reader.GetAttribute("overall"); // (unemulated|imperfect)

                        reader.Read();
                        break;
                    case "device":
                        // string device_type = reader.GetAttribute("type");
                        // string device_tag = reader.GetAttribute("tag");
                        // string device_fixed_image = reader.GetAttribute("fixed_image");
                        // string device_mandatory = reader.GetAttribute("mandatory");
                        // string device_interface = reader.GetAttribute("interface");

                        // // For the one possible <instance> element...
                        // string instance_name = reader.GetAttribute("name");
                        // string instance_briefname = reader.GetAttribute("briefname");

                        // // While the subtree contains <extension> elements...
                        // string extension_name = reader.GetAttribute("name");

                        reader.Skip();
                        break;

                    case "slot":
                        // string slot_name = reader.GetAttribute("name");
                        ReadSlot(reader.ReadSubtree(), machine);

                        // Skip the slot now that we've processed it
                        reader.Skip();
                        break;

                    case "softwarelist":
                        // string softwarelist_name = reader.GetAttribute("name");
                        // string softwarelist_status = reader.GetAttribute("status"); // (original|compatible)
                        // string softwarelist_filter = reader.GetAttribute("filter");

                        reader.Read();
                        break;

                    case "ramoption":
                        // string ramoption_default = reader.GetAttribute("default");

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // If no items were found for this machine, add a Blank placeholder
            if (!containsItems)
            {
                Blank blank = new Blank()
                {
                    SystemID = sysid,
                    System = filename,
                    SourceID = srcid,
                };
                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank, clean, remUnicode);
            }
        }

        /// <summary>
        /// Read slot information
        /// </summary>
        /// <param name="reader">XmlReader representing a machine block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        private void ReadSlot(XmlReader reader,	Machine machine)
        {
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the roms from the machine
                switch (reader.Name)
                {
                    case "slotoption":
                        // string slotoption_name = reader.GetAttribute("name");
                        string devname = reader.GetAttribute("devname");
                        if (!machine.SlotOptions.Contains(devname))
                        {
                            machine.SlotOptions.Add(devname);
                        }
                        // bool? slotoption_default = Utilities.GetYesNo(reader.GetAttribute("default"));
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

                            lastgame = rom.MachineName;
                            continue;
                        }

                        // Now, output the rom data
                        WriteDatItem(sw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(sw);

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
                string header = "<?xml version=\"1.0\"?>\n";
                header += $"<mame build=\"{WebUtility.HtmlEncode(Name)}\"";
                //header += $" debug=\"{Debug}\"";
                //header += $" mameconfig=\"{MameConfig}\"";
                header += ">\n\n";

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
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(StreamWriter sw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                string state = $"\t<machine name=\"{WebUtility.HtmlEncode(datItem.GetField(Field.MachineName, ExcludeFields) as string)}\"";
                if (!ExcludeFields[(int)Field.SourceFile] && datItem.SourceFile != null)
                    state += $" sourcefile=\"{datItem.SourceFile}\"";
                if (!ExcludeFields[(int)Field.MachineType])
                {
                    if ((datItem.MachineType & MachineType.Bios) != 0)
                        state += " isbios=\"yes\"";
                    if ((datItem.MachineType & MachineType.Device) != 0)
                        state += " isdevice=\"yes\"";
                    if ((datItem.MachineType & MachineType.Mechanical) != 0)
                        state += " ismechanical=\"yes\"";
                }
                if (!ExcludeFields[(int)Field.Runnable])
                {
                    if (datItem.Runnable == true)
                        state += " runnable=\"yes\"";
                    else if (datItem.Runnable == false)
                        state += " runnable=\"no\"";
                }
                if (!ExcludeFields[(int)Field.CloneOf] && !string.IsNullOrWhiteSpace(datItem.CloneOf) && !string.Equals(datItem.MachineName, datItem.CloneOf, StringComparison.OrdinalIgnoreCase))
                    state += $" cloneof=\"{WebUtility.HtmlEncode(datItem.CloneOf)}\"";
                if (!ExcludeFields[(int)Field.RomOf] && !string.IsNullOrWhiteSpace(datItem.RomOf) && !string.Equals(datItem.MachineName, datItem.RomOf, StringComparison.OrdinalIgnoreCase))
                    state += $" romof=\"{WebUtility.HtmlEncode(datItem.RomOf)}\"";
                if (!ExcludeFields[(int)Field.SampleOf] && !string.IsNullOrWhiteSpace(datItem.SampleOf) && !string.Equals(datItem.MachineName, datItem.SampleOf, StringComparison.OrdinalIgnoreCase))
                    state += $" sampleof=\"{WebUtility.HtmlEncode(datItem.SampleOf)}\"";
                state += ">\n";
                if (!ExcludeFields[(int)Field.Description] && !string.IsNullOrWhiteSpace(datItem.MachineDescription))
                    state += $"\t\t<description>{WebUtility.HtmlEncode(datItem.MachineDescription)}</description>\n";
                if (!ExcludeFields[(int)Field.Year] && !string.IsNullOrWhiteSpace(datItem.Year))
                    state += $"\t\t<year>{WebUtility.HtmlEncode(datItem.Year)}</year>\n";
                if (!ExcludeFields[(int)Field.Publisher] && !string.IsNullOrWhiteSpace(datItem.Publisher))
                    state += $"\t\t<publisher>{WebUtility.HtmlEncode(datItem.Publisher)}</publisher>\n";
                if (!ExcludeFields[(int)Field.Infos] && datItem.Infos != null && datItem.Infos.Count > 0)
                {
                    foreach (Tuple<string, string> kvp in datItem.Infos)
                    {
                        state += $"\t\t<info name=\"{WebUtility.HtmlEncode(kvp.Item1)}\" value=\"{WebUtility.HtmlEncode(kvp.Item2)}\" />\n";
                    }
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

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(StreamWriter sw)
        {
            try
            {
                string state = "\t</machine>\n";

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
                    case ItemType.Archive:
                        //TODO: Am I missing this?
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        state += $"\t\t<biosset name\"{WebUtility.HtmlEncode(biosSet.GetField(Field.Name, ExcludeFields) as string)}\"";
                        if (!ExcludeFields[(int)Field.BiosDescription] && !string.IsNullOrWhiteSpace(biosSet.Description))
                            state += $" description=\"{WebUtility.HtmlEncode(biosSet.Description)}\"";
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                            state += $" default=\"{WebUtility.HtmlEncode(biosSet.Default.ToString().ToLowerInvariant())}\"";
                        state += "/>\n";
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        state += $"\t\t<disk name\"{WebUtility.HtmlEncode(disk.GetField(Field.Name, ExcludeFields) as string)}\"";
                        if (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(disk.MD5))
                            state += $" md5=\"{disk.MD5.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(disk.RIPEMD160))
                            state += $" ripemd160=\"{disk.RIPEMD160.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(disk.SHA1))
                            state += $" sha1=\"{disk.SHA1.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(disk.SHA256))
                            state += $" sha256=\"{disk.SHA256.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(disk.SHA384))
                            state += $" sha384=\"{disk.SHA384.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(disk.SHA512))
                            state += $" sha512=\"{disk.SHA512.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Merge] && !string.IsNullOrWhiteSpace(disk.MergeTag))
                            state += $" merge=\"{WebUtility.HtmlEncode(disk.MergeTag)}\"";
                        if (!ExcludeFields[(int)Field.Region] && !string.IsNullOrWhiteSpace(disk.Region))
                            state += $" region=\"{WebUtility.HtmlEncode(disk.Region)}\"";
                        if (!ExcludeFields[(int)Field.Index] && !string.IsNullOrWhiteSpace(disk.Index))
                            state += $" index=\"{WebUtility.HtmlEncode(disk.Index)}\"";
                        if (!ExcludeFields[(int)Field.Writable] && disk.Writable != null)
                            state += $" writable=\"{(disk.Writable == true ? "yes" : "no")}\"";
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                            state += $" status=\"{disk.ItemStatus.ToString().ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Optional] && disk.Optional != null)
                            state += $" optional=\"{(disk.Optional == true ? "yes" : "no")}\"";
                        state += "/>\n";
                        break;

                    case ItemType.Release:
                        //TODO: Am I missing this?
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        state += $"\t\t<rom name\"{WebUtility.HtmlEncode(rom.GetField(Field.Name, ExcludeFields) as string)}\"";
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                            state += $" size=\"{rom.Size}\"";
                        if (!ExcludeFields[(int)Field.CRC] && !string.IsNullOrWhiteSpace(rom.CRC))
                            state += $" crc=\"{rom.CRC.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.MD5] && !string.IsNullOrWhiteSpace(rom.MD5))
                            state += $" md5=\"{rom.MD5.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.RIPEMD160] && !string.IsNullOrWhiteSpace(rom.RIPEMD160))
                            state += $" ripemd160=\"{rom.RIPEMD160.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA1] && !string.IsNullOrWhiteSpace(rom.SHA1))
                            state += $" sha1=\"{rom.SHA1.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA256] && !string.IsNullOrWhiteSpace(rom.SHA256))
                            state += $" sha256=\"{rom.SHA256.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA384] && !string.IsNullOrWhiteSpace(rom.SHA384))
                            state += $" sha384=\"{rom.SHA384.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.SHA512] && !string.IsNullOrWhiteSpace(rom.SHA512))
                            state += $" sha512=\"{rom.SHA512.ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Bios] && !string.IsNullOrWhiteSpace(rom.Bios))
                            state += $" bios=\"{WebUtility.HtmlEncode(rom.Bios)}\"";
                        if (!ExcludeFields[(int)Field.Merge] && !string.IsNullOrWhiteSpace(rom.MergeTag))
                            state += $" merge=\"{WebUtility.HtmlEncode(rom.MergeTag)}\"";
                        if (!ExcludeFields[(int)Field.Region] && !string.IsNullOrWhiteSpace(rom.Region))
                            state += $" region=\"{WebUtility.HtmlEncode(rom.Region)}\"";
                        if (!ExcludeFields[(int)Field.Offset] && !string.IsNullOrWhiteSpace(rom.Offset))
                            state += $" offset=\"{rom.Offset}\"";
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                            state += $" status=\"{rom.ItemStatus.ToString().ToLowerInvariant()}\"";
                        if (!ExcludeFields[(int)Field.Optional] && rom.Optional != null)
                            state += $" optional=\"{(rom.Optional == true ? "yes" : "no")}\"";
                        state += "/>\n";
                        break;

                    case ItemType.Sample:
                        state += $"\t\t<sample name\"{WebUtility.HtmlEncode(datItem.GetField(Field.Name, ExcludeFields) as string)}\"";
                        state += "/>\n";
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

        /// <summary>
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(StreamWriter sw)
        {
            try
            {
                string footer = "\t</machine>\n</mame>\n";

                // Write the footer out
                sw.Write(footer);
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
