using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a MAME XML DAT
    /// </summary>
    internal class Listxml : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Listxml(DatFile datFile)
            : base(datFile)
        {
        }

        /// <summary>
        /// Parse a MAME XML DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <remarks>
        /// </remarks>
        protected override void ParseFile(
            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
        {
            // Prepare all internal variables
            XmlReader xtr = filename.GetXmlTextReader();

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
                            Header.Name = (Header.Name == null ? xtr.GetAttribute("build") : Header.Name);
                            Header.Description = (Header.Description == null ? Header.Name : Header.Description);
                            Header.Debug = (Header.Debug == null ? xtr.GetAttribute("debug").AsYesNo() : Header.Debug);
                            Header.MameConfig = (Header.MameConfig == null ? xtr.GetAttribute("mameconfig") : Header.MameConfig);
                            xtr.Read();
                            break;

                        // Handle M1 DATs since they're 99% the same as a SL DAT
                        case "m1":
                            Header.Name = (Header.Name == null ? "M1" : Header.Name);
                            Header.Description = (Header.Description == null ? "M1" : Header.Description);
                            Header.Version = (Header.Version == null ? xtr.GetAttribute("version") ?? string.Empty : Header.Version);
                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the machine
                        case "game": // Some older DATs still use "game"
                        case "machine":
                            ReadMachine(xtr.ReadSubtree(), filename, indexId);

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
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadMachine(
            XmlReader reader,

            // Standard Dat parsing
            string filename,
            int indexId)
        {
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            // Create a new machine
            MachineType machineType = MachineType.NULL;
            if (reader.GetAttribute("isbios").AsYesNo() == true)
                machineType |= MachineType.Bios;

            if (reader.GetAttribute("isdevice").AsYesNo() == true)
                machineType |= MachineType.Device;

            if (reader.GetAttribute("ismechanical").AsYesNo() == true)
                machineType |= MachineType.Mechanical;

            Machine machine = new Machine
            {
                Name = reader.GetAttribute("name"),
                Description = reader.GetAttribute("name"),
                CloneOf = reader.GetAttribute("cloneof"),
                RomOf = reader.GetAttribute("romof"),
                SampleOf = reader.GetAttribute("sampleof"),
                MachineType = (machineType == MachineType.NULL ? MachineType.NULL : machineType),

                SourceFile = reader.GetAttribute("sourcefile"),
                Runnable = reader.GetAttribute("runnable").AsRunnable(),
            };

            // Get list for new DatItems
            List<DatItem> datItems = new List<DatItem>();

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
                        datItems.Add(new BiosSet
                        {
                            Name = reader.GetAttribute("name"),
                            Description = reader.GetAttribute("description"),
                            Default = reader.GetAttribute("default").AsYesNo(),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        });

                        reader.Read();
                        break;

                    case "rom":
                        datItems.Add(new Rom
                        {
                            Name = reader.GetAttribute("name"),
                            Bios = reader.GetAttribute("bios"),
                            Size = Sanitizer.CleanSize(reader.GetAttribute("size")),
                            CRC = reader.GetAttribute("crc"),
                            MD5 = reader.GetAttribute("md5"),
#if NET_FRAMEWORK
                            RIPEMD160 = reader.GetAttribute("ripemd160"),
#endif
                            SHA1 = reader.GetAttribute("sha1"),
                            SHA256 = reader.GetAttribute("sha256"),
                            SHA384 = reader.GetAttribute("sha384"),
                            SHA512 = reader.GetAttribute("sha512"),
                            MergeTag = reader.GetAttribute("merge"),
                            Region = reader.GetAttribute("region"),
                            Offset = reader.GetAttribute("offset"),
                            ItemStatus = reader.GetAttribute("status").AsItemStatus(),
                            Optional = reader.GetAttribute("optional").AsYesNo(),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        });

                        reader.Read();
                        break;

                    case "disk":
                        datItems.Add(new Disk
                        {
                            Name = reader.GetAttribute("name"),
                            MD5 = reader.GetAttribute("md5"),
                            SHA1 = reader.GetAttribute("sha1"),
                            MergeTag = reader.GetAttribute("merge"),
                            Region = reader.GetAttribute("region"),
                            Index = reader.GetAttribute("index"),
                            Writable = reader.GetAttribute("writable").AsYesNo(),
                            ItemStatus = reader.GetAttribute("status").AsItemStatus(),
                            Optional = reader.GetAttribute("optional").AsYesNo(),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        });

                        reader.Read();
                        break;

                    case "device_ref":
                        var deviceReference = new ListXmlDeviceReference();
                        deviceReference.Name = reader.GetAttribute("name");

                        // Ensure the list exists
                        if (machine.DeviceReferences == null)
                            machine.DeviceReferences = new List<ListXmlDeviceReference>();

                        machine.DeviceReferences.Add(deviceReference);

                        reader.Read();
                        break;

                    case "sample":
                        datItems.Add(new Sample
                        {
                            Name = reader.GetAttribute("name"),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        });

                        reader.Read();
                        break;

                    case "chip":
                        datItems.Add(new Chip
                        {
                            Name = reader.GetAttribute("name"),
                            Tag = reader.GetAttribute("tag"),
                            ChipType = reader.GetAttribute("type"),
                            Clock = reader.GetAttribute("clock"),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        });

                        reader.Read();
                        break;

                    case "display":
                        var display = new ListXmlDisplay();
                        display.Tag = reader.GetAttribute("tag");
                        display.Type = reader.GetAttribute("type");
                        display.Rotate = reader.GetAttribute("rotate");
                        display.FlipX = reader.GetAttribute("flipx").AsYesNo();
                        display.Width = reader.GetAttribute("width");
                        display.Height = reader.GetAttribute("height");
                        display.Refresh = reader.GetAttribute("refresh");
                        display.PixClock = reader.GetAttribute("pixclock");
                        display.HTotal = reader.GetAttribute("htotal");
                        display.HBEnd = reader.GetAttribute("hbend");
                        display.HBStart = reader.GetAttribute("hbstart");
                        display.VTotal = reader.GetAttribute("vtotal");
                        display.VBEnd = reader.GetAttribute("vbend");
                        display.VBStart = reader.GetAttribute("vbstart");

                        // Ensure the list exists
                        if (machine.Displays == null)
                            machine.Displays = new List<ListXmlDisplay>();

                        machine.Displays.Add(display);

                        reader.Read();
                        break;

                    case "sound":
                        var sound = new ListXmlSound();
                        sound.Channels = reader.GetAttribute("channels");

                        // Ensure the list exists
                        if (machine.Sounds == null)
                            machine.Sounds = new List<ListXmlSound>();

                        machine.Sounds.Add(sound);

                        reader.Read();
                        break;

                    case "condition":
                        var condition = new ListXmlCondition();
                        condition.Tag = reader.GetAttribute("tag");
                        condition.Mask = reader.GetAttribute("mask");
                        condition.Relation = reader.GetAttribute("relation");
                        condition.Value = reader.GetAttribute("value");

                        // Ensure the list exists
                        if (machine.Conditions == null)
                            machine.Conditions = new List<ListXmlCondition>();

                        machine.Conditions.Add(condition);

                        reader.Read();
                        break;

                    case "input":
                        var input = new ListXmlInput();
                        input.Service = reader.GetAttribute("service").AsYesNo();
                        input.Tilt = reader.GetAttribute("tilt").AsYesNo();
                        input.Players = reader.GetAttribute("players");
                        input.Coins = reader.GetAttribute("coins");

                        // Now read the internal tags
                        ReadInput(reader.ReadSubtree(), input);

                        // Ensure the list exists
                        if (machine.Inputs == null)
                            machine.Inputs = new List<ListXmlInput>();

                        machine.Inputs.Add(input);

                        // Skip the input now that we've processed it
                        reader.Skip();
                        break;

                    case "dipswitch":
                        var dipSwitch = new ListXmlDipSwitch();
                        dipSwitch.Name = reader.GetAttribute("name");
                        dipSwitch.Tag = reader.GetAttribute("tag");
                        dipSwitch.Mask = reader.GetAttribute("mask");

                        // Now read the internal tags
                        ReadDipSwitch(reader.ReadSubtree(), dipSwitch);

                        // Ensure the list exists
                        if (machine.DipSwitches == null)
                            machine.DipSwitches = new List<ListXmlDipSwitch>();

                        machine.DipSwitches.Add(dipSwitch);

                        // Skip the dipswitch now that we've processed it
                        reader.Skip();
                        break;

                    case "configuration":
                        var configuration = new ListXmlConfiguration();
                        configuration.Name = reader.GetAttribute("name");
                        configuration.Tag = reader.GetAttribute("tag");
                        configuration.Mask = reader.GetAttribute("mask");

                        // Now read the internal tags
                        ReadConfiguration(reader.ReadSubtree(), configuration);

                        // Ensure the list exists
                        if (machine.Configurations == null)
                            machine.Configurations = new List<ListXmlConfiguration>();

                        machine.Configurations.Add(configuration);

                        // Skip the configuration now that we've processed it
                        reader.Skip();
                        break;

                    case "port":
                        var port = new ListXmlPort();
                        port.Tag = reader.GetAttribute("tag");

                        // Now read the internal tags
                        ReadPort(reader.ReadSubtree(), port);

                        // Ensure the list exists
                        if (machine.Ports == null)
                            machine.Ports = new List<ListXmlPort>();

                        machine.Ports.Add(port);

                        // Skip the port now that we've processed it
                        reader.Skip();
                        break;

                    case "adjuster":
                        var adjuster = new ListXmlAdjuster();
                        adjuster.Name = reader.GetAttribute("name");
                        adjuster.Default = reader.GetAttribute("default").AsYesNo();

                        // Now read the internal tags
                        ReadAdjuster(reader.ReadSubtree(), adjuster);

                        // Ensure the list exists
                        if (machine.Adjusters == null)
                            machine.Adjusters = new List<ListXmlAdjuster>();

                        machine.Adjusters.Add(adjuster);

                        // Skip the adjuster now that we've processed it
                        reader.Skip();
                        break;

                    case "driver":
                        var driver = new ListXmlDriver();
                        driver.Status = reader.GetAttribute("status");
                        driver.Emulation = reader.GetAttribute("emulation");
                        driver.Cocktail = reader.GetAttribute("cocktail");
                        driver.SaveState = reader.GetAttribute("savestate");

                        // Ensure the list exists
                        if (machine.Drivers == null)
                            machine.Drivers = new List<ListXmlDriver>();

                        machine.Drivers.Add(driver);

                        reader.Read();
                        break;

                    case "feature":
                        var feature = new ListXmlFeature();
                        feature.Type = reader.GetAttribute("type");
                        feature.Status = reader.GetAttribute("status");
                        feature.Overall = reader.GetAttribute("overall");

                        // Ensure the list exists
                        if (machine.Features == null)
                            machine.Features = new List<ListXmlFeature>();

                        machine.Features.Add(feature);

                        reader.Read();
                        break;

                    case "device":
                        var device = new ListXmlDevice();
                        device.Type = reader.GetAttribute("type");
                        device.Tag = reader.GetAttribute("tag");
                        device.FixedImage = reader.GetAttribute("fixed_image");
                        device.Mandatory = reader.GetAttribute("mandatory");
                        device.Interface = reader.GetAttribute("interface");

                        // Now read the internal tags
                        ReadDevice(reader.ReadSubtree(), device);

                        // Ensure the list exists
                        if (machine.Devices == null)
                            machine.Devices = new List<ListXmlDevice>();

                        machine.Devices.Add(device);

                        // Skip the device now that we've processed it
                        reader.Skip();
                        break;

                    case "slot":
                        var slot = new ListXmlSlot();
                        slot.Name = reader.GetAttribute("name");

                        // Now read the internal tags
                        ReadSlot(reader.ReadSubtree(), slot, machine);

                        // Ensure the list exists
                        if (machine.Slots == null)
                            machine.Slots = new List<ListXmlSlot>();

                        machine.Slots.Add(slot);

                        // Skip the slot now that we've processed it
                        reader.Skip();
                        break;

                    case "softwarelist":
                        var softwareList = new ListXmlSoftwareList();
                        softwareList.Name = reader.GetAttribute("name");
                        softwareList.Status = reader.GetAttribute("status");
                        softwareList.Filter = reader.GetAttribute("filter");

                        // Ensure the list exists
                        if (machine.SoftwareLists == null)
                            machine.SoftwareLists = new List<ListXmlSoftwareList>();

                        machine.SoftwareLists.Add(softwareList);

                        reader.Read();
                        break;

                    case "ramoption":
                        var ramOption = new ListXmlRamOption();
                        ramOption.Default = reader.GetAttribute("default").AsYesNo();

                        // Ensure the list exists
                        if (machine.RamOptions == null)
                            machine.RamOptions = new List<ListXmlRamOption>();

                        machine.RamOptions.Add(ramOption);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // If we found items, copy the machine info and add them
            if (datItems.Any())
            {
                foreach (DatItem datItem in datItems)
                {
                    datItem.CopyMachineInformation(machine);
                    ParseAddHelper(datItem);
                }
            }

            // If no items were found for this machine, add a Blank placeholder
            else
            {
                Blank blank = new Blank()
                {
                    Source = new Source
                    {
                        Index = indexId,
                        Name = filename,
                    },
                };

                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank);
            }
        }

        /// <summary>
        /// Read slot information
        /// </summary>
        /// <param name="reader">XmlReader representing a machine block</param>
        /// <param name="slot">ListXmlSlot to populate</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        private void ReadSlot(XmlReader reader, ListXmlSlot slot, Machine machine)
        {
            // If we have an empty machine, skip it
            if (reader == null)
                return;

            // Get list ready
            slot.SlotOptions = new List<ListXmlSlotOption>();

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
                        var slotOption = new ListXmlSlotOption();
                        slotOption.Name = reader.GetAttribute("name");
                        slotOption.DeviceName = reader.GetAttribute("devname");
                        slotOption.Default = reader.GetAttribute("default").AsYesNo();

                        slot.SlotOptions.Add(slotOption);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Input information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="input">ListXmlInput to populate</param>
        private void ReadInput(XmlReader reader, ListXmlInput input)
        {
            // If we have an empty input, skip it
            if (reader == null)
                return;

            // Get list ready
            input.Controls = new List<ListXmlControl>();

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

                // Get the information from the dipswitch
                switch (reader.Name)
                {
                    case "control":
                        var control = new ListXmlControl();
                        control.Type = reader.GetAttribute("type");
                        control.Player = reader.GetAttribute("player");
                        control.Buttons = reader.GetAttribute("buttons");
                        control.RegButtons = reader.GetAttribute("regbuttons");
                        control.Minimum = reader.GetAttribute("minimum");
                        control.Maximum = reader.GetAttribute("maximum");
                        control.Sensitivity = reader.GetAttribute("sensitivity");
                        control.KeyDelta = reader.GetAttribute("keydelta");
                        control.Reverse = reader.GetAttribute("reverse").AsYesNo();
                        control.Ways = reader.GetAttribute("ways");
                        control.Ways2 = reader.GetAttribute("ways2");
                        control.Ways3 = reader.GetAttribute("ways3");

                        input.Controls.Add(control);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read DipSwitch information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="dipSwitch">ListXmlDipSwitch to populate</param>
        private void ReadDipSwitch(XmlReader reader, ListXmlDipSwitch dipSwitch)
        {
            // If we have an empty dipswitch, skip it
            if (reader == null)
                return;

            // Get lists ready
            dipSwitch.Locations = new List<ListXmlDipLocation>();
            dipSwitch.Values = new List<ListXmlDipValue>();

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

                // Get the information from the dipswitch
                switch (reader.Name)
                {
                    case "diplocation":
                        var dipLocation = new ListXmlDipLocation();
                        dipLocation.Name = reader.GetAttribute("name");
                        dipLocation.Number = reader.GetAttribute("number");
                        dipLocation.Inverted = reader.GetAttribute("inverted").AsYesNo();

                        dipSwitch.Locations.Add(dipLocation);

                        reader.Read();
                        break;

                    case "dipvalue":
                        var dipValue = new ListXmlDipValue();
                        dipValue.Name = reader.GetAttribute("name");
                        dipValue.Value = reader.GetAttribute("value");
                        dipValue.Default = reader.GetAttribute("default").AsYesNo();

                        dipSwitch.Values.Add(dipValue);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Configuration information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="configuration">ListXmlConfiguration to populate</param>
        private void ReadConfiguration(XmlReader reader, ListXmlConfiguration configuration)
        {
            // If we have an empty configuration, skip it
            if (reader == null)
                return;

            // Get lists ready
            configuration.Locations = new List<ListXmlConfLocation>();
            configuration.Settings = new List<ListXmlConfSetting>();

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

                // Get the information from the dipswitch
                switch (reader.Name)
                {
                    case "conflocation":
                        var confLocation = new ListXmlConfLocation();
                        confLocation.Name = reader.GetAttribute("name");
                        confLocation.Number = reader.GetAttribute("number");
                        confLocation.Inverted = reader.GetAttribute("inverted").AsYesNo();

                        configuration.Locations.Add(confLocation);

                        reader.Read();
                        break;

                    case "confsetting":
                        var confSetting = new ListXmlConfSetting();
                        confSetting.Name = reader.GetAttribute("name");
                        confSetting.Value = reader.GetAttribute("value");
                        confSetting.Default = reader.GetAttribute("default").AsYesNo();

                        configuration.Settings.Add(confSetting);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Port information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="port">ListXmlPort to populate</param>
        private void ReadPort(XmlReader reader, ListXmlPort port)
        {
            // If we have an empty port, skip it
            if (reader == null)
                return;

            // Get list ready
            port.Analogs = new List<ListXmlAnalog>();

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

                // Get the information from the port
                switch (reader.Name)
                {
                    case "analog":
                        var analog = new ListXmlAnalog();
                        analog.Mask = reader.GetAttribute("mask");

                        port.Analogs.Add(analog);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Adjuster information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="adjuster">ListXmlAdjuster to populate</param>
        private void ReadAdjuster(XmlReader reader, ListXmlAdjuster adjuster)
        {
            // If we have an empty port, skip it
            if (reader == null)
                return;

            // Get list ready
            adjuster.Conditions = new List<ListXmlCondition>();

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

                // Get the information from the adjuster
                switch (reader.Name)
                {
                    case "condition":
                        var condition = new ListXmlCondition();
                        condition.Tag = reader.GetAttribute("tag");
                        condition.Mask = reader.GetAttribute("mask");
                        condition.Relation = reader.GetAttribute("relation");
                        condition.Value = reader.GetAttribute("value");

                        adjuster.Conditions.Add(condition);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Device information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="device">ListXmlDevice to populate</param>
        private void ReadDevice(XmlReader reader, ListXmlDevice device)
        {
            // If we have an empty port, skip it
            if (reader == null)
                return;

            // Get lists ready
            device.Instances = new List<ListXmlInstance>();
            device.Extensions = new List<ListXmlExtension>();

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

                // Get the information from the adjuster
                switch (reader.Name)
                {
                    case "instance":
                        var instance = new ListXmlInstance();
                        instance.Name = reader.GetAttribute("name");
                        instance.BriefName = reader.GetAttribute("briefname");

                        device.Instances.Add(instance);

                        reader.Read();
                        break;

                    case "extension":
                        var extension = new ListXmlExtension();
                        extension.Name = reader.GetAttribute("name");

                        device.Extensions.Add(extension);

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
                FileStream fs = FileExtensions.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                XmlTextWriter xtw = new XmlTextWriter(fs, new UTF8Encoding(false))
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                // Write out the header
                WriteHeader(xtw);

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
                        DatItem rom = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (rom.Name == null || rom.Machine.Name == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
                            WriteEndGame(xtw, rom);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.Machine.Name.ToLowerInvariant())
                            WriteStartGame(xtw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.Machine.Name}");

                            lastgame = rom.Machine.Name;
                            continue;
                        }

                        // Now, output the rom data
                        WriteDatItem(xtw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.Machine.Name;
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                xtw.Dispose();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(XmlTextWriter xtw)
        {
            try
            {
                xtw.WriteStartDocument();

                xtw.WriteStartElement("mame");
                xtw.WriteRequiredAttributeString("build", Header.Name);
                xtw.WriteOptionalAttributeString("debug", Header.Debug.FromYesNo());
                xtw.WriteOptionalAttributeString("mameconfig", Header.MameConfig);

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(XmlTextWriter xtw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.Machine.Name = datItem.Machine.Name.TrimStart(Path.DirectorySeparatorChar);

                // Build the state
                xtw.WriteStartElement("machine");
                xtw.WriteRequiredAttributeString("name", datItem.Machine.Name);
                xtw.WriteOptionalAttributeString("sourcefile", datItem.Machine.SourceFile);

                if (datItem.Machine.MachineType.HasFlag(MachineType.Bios))
                    xtw.WriteAttributeString("isbios", "yes");
                if (datItem.Machine.MachineType.HasFlag(MachineType.Device))
                    xtw.WriteAttributeString("isdevice", "yes");
                if (datItem.Machine.MachineType.HasFlag(MachineType.Mechanical))
                    xtw.WriteAttributeString("ismechanical", "yes");

                xtw.WriteOptionalAttributeString("runnable", datItem.Machine.Runnable.FromRunnable());

                if (!string.Equals(datItem.Machine.Name, datItem.Machine.CloneOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteOptionalAttributeString("cloneof", datItem.Machine.CloneOf);
                if (!string.Equals(datItem.Machine.Name, datItem.Machine.RomOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteOptionalAttributeString("romof", datItem.Machine.RomOf);
                if (!string.Equals(datItem.Machine.Name, datItem.Machine.SampleOf, StringComparison.OrdinalIgnoreCase))
                    xtw.WriteOptionalAttributeString("sampleof", datItem.Machine.SampleOf);

                xtw.WriteOptionalElementString("description", datItem.Machine.Description);
                xtw.WriteOptionalElementString("year", datItem.Machine.Year);
                xtw.WriteOptionalElementString("manufacturer", datItem.Machine.Manufacturer);

                // TODO: These should go *after* the datitems
                if (datItem.Machine.DeviceReferences != null)
                {
                    foreach (var deviceReference in datItem.Machine.DeviceReferences)
                    {
                        xtw.WriteStartElement("device_ref");

                        xtw.WriteOptionalAttributeString("name", deviceReference.Name);

                        // End device_ref
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Displays != null)
                {
                    foreach (var display in datItem.Machine.Displays)
                    {
                        xtw.WriteStartElement("display");

                        xtw.WriteOptionalAttributeString("tag", display.Tag);
                        xtw.WriteOptionalAttributeString("type", display.Type);
                        xtw.WriteOptionalAttributeString("rotate", display.Rotate);
                        xtw.WriteOptionalAttributeString("flipx", display.FlipX.FromYesNo());
                        xtw.WriteOptionalAttributeString("width", display.Width);
                        xtw.WriteOptionalAttributeString("height", display.Height);
                        xtw.WriteOptionalAttributeString("refresh", display.Refresh);
                        xtw.WriteOptionalAttributeString("pixclock", display.PixClock);
                        xtw.WriteOptionalAttributeString("htotal", display.HTotal);
                        xtw.WriteOptionalAttributeString("hbend", display.HBEnd);
                        xtw.WriteOptionalAttributeString("hstart", display.HBStart);
                        xtw.WriteOptionalAttributeString("vtotal", display.VTotal);
                        xtw.WriteOptionalAttributeString("vbend", display.VBEnd);
                        xtw.WriteOptionalAttributeString("vbstart", display.VBStart);

                        // End display
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Sounds != null)
                {
                    foreach (var sound in datItem.Machine.Sounds)
                    {
                        xtw.WriteStartElement("sound");

                        xtw.WriteOptionalAttributeString("channels", sound.Channels);

                        // End sound
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Conditions != null)
                {
                    foreach (var condition in datItem.Machine.Conditions)
                    {
                        xtw.WriteStartElement("condition");

                        xtw.WriteOptionalAttributeString("tag", condition.Tag);
                        xtw.WriteOptionalAttributeString("mask", condition.Mask);
                        xtw.WriteOptionalAttributeString("relation", condition.Relation);
                        xtw.WriteOptionalAttributeString("value", condition.Value);

                        // End condition
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Inputs != null)
                {
                    foreach (var input in datItem.Machine.Inputs)
                    {
                        xtw.WriteStartElement("input");

                        xtw.WriteOptionalAttributeString("service", input.Service.FromYesNo());
                        xtw.WriteOptionalAttributeString("tilt", input.Tilt.FromYesNo());
                        xtw.WriteOptionalAttributeString("players", input.Players);
                        xtw.WriteOptionalAttributeString("coins", input.Coins);

                        if (input.Controls != null)
                        {
                            foreach (var control in input.Controls)
                            {
                                xtw.WriteStartElement("control");

                                xtw.WriteOptionalAttributeString("type", control.Type);
                                xtw.WriteOptionalAttributeString("player", control.Player);
                                xtw.WriteOptionalAttributeString("buttons", control.Buttons);
                                xtw.WriteOptionalAttributeString("regbuttons", control.RegButtons);
                                xtw.WriteOptionalAttributeString("minimum", control.Minimum);
                                xtw.WriteOptionalAttributeString("maximum", control.Maximum);
                                xtw.WriteOptionalAttributeString("sensitivity", control.Sensitivity);
                                xtw.WriteOptionalAttributeString("keydelta", control.KeyDelta);
                                xtw.WriteOptionalAttributeString("reverse", control.Reverse.FromYesNo());
                                xtw.WriteOptionalAttributeString("ways", control.Ways);
                                xtw.WriteOptionalAttributeString("ways2", control.Ways2);
                                xtw.WriteOptionalAttributeString("ways3", control.Ways3);

                                // End control
                                xtw.WriteEndElement();
                            }
                        }

                        // End input
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.DipSwitches != null)
                {
                    foreach (var dipSwitch in datItem.Machine.DipSwitches)
                    {
                        xtw.WriteStartElement("dipswitch");

                        xtw.WriteOptionalAttributeString("name", dipSwitch.Name);
                        xtw.WriteOptionalAttributeString("tag", dipSwitch.Tag);
                        xtw.WriteOptionalAttributeString("mask", dipSwitch.Mask);

                        if (dipSwitch.Locations != null)
                        {
                            foreach (var location in dipSwitch.Locations)
                            {
                                xtw.WriteStartElement("diplocation");

                                xtw.WriteOptionalAttributeString("name", location.Name);
                                xtw.WriteOptionalAttributeString("number", location.Number);
                                xtw.WriteOptionalAttributeString("inverted", location.Inverted.FromYesNo());

                                // End diplocation
                                xtw.WriteEndElement();
                            }
                        }
                        if (dipSwitch.Values != null)
                        {
                            foreach (var value in dipSwitch.Values)
                            {
                                xtw.WriteStartElement("dipvalue");

                                xtw.WriteOptionalAttributeString("name", value.Name);
                                xtw.WriteOptionalAttributeString("value", value.Value);
                                xtw.WriteOptionalAttributeString("default", value.Default.FromYesNo());

                                // End dipvalue
                                xtw.WriteEndElement();
                            }
                        }

                        // End dipswitch
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Configurations != null)
                {
                    foreach (var configuration in datItem.Machine.Configurations)
                    {
                        xtw.WriteStartElement("configuration");

                        xtw.WriteOptionalAttributeString("name", configuration.Name);
                        xtw.WriteOptionalAttributeString("tag", configuration.Tag);
                        xtw.WriteOptionalAttributeString("mask", configuration.Mask);

                        if (configuration.Locations != null)
                        {
                            foreach (var location in configuration.Locations)
                            {
                                xtw.WriteStartElement("conflocation");

                                xtw.WriteOptionalAttributeString("name", location.Name);
                                xtw.WriteOptionalAttributeString("number", location.Number);
                                xtw.WriteOptionalAttributeString("inverted", location.Inverted.FromYesNo());

                                // End conflocation
                                xtw.WriteEndElement();
                            }
                        }
                        if (configuration.Settings != null)
                        {
                            foreach (var setting in configuration.Settings)
                            {
                                xtw.WriteStartElement("confsetting");

                                xtw.WriteOptionalAttributeString("name", setting.Name);
                                xtw.WriteOptionalAttributeString("value", setting.Value);
                                xtw.WriteOptionalAttributeString("default", setting.Default.FromYesNo());

                                // End confsetting
                                xtw.WriteEndElement();
                            }
                        }

                        // End configuration
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Ports != null)
                {
                    foreach (var port in datItem.Machine.Ports)
                    {
                        xtw.WriteStartElement("port");

                        xtw.WriteOptionalAttributeString("tag", port.Tag);

                        if (port.Analogs != null)
                        {
                            foreach (var analog in port.Analogs)
                            {
                                xtw.WriteStartElement("analog");

                                xtw.WriteOptionalAttributeString("mask", analog.Mask);

                                // End analog
                                xtw.WriteEndElement();
                            }
                        }

                        // End port
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Adjusters != null)
                {
                    foreach (var adjuster in datItem.Machine.Adjusters)
                    {
                        xtw.WriteStartElement("adjuster");

                        xtw.WriteOptionalAttributeString("name", adjuster.Name);
                        xtw.WriteOptionalAttributeString("default", adjuster.Default.FromYesNo());

                        if (adjuster.Conditions != null)
                        {
                            foreach (var condition in adjuster.Conditions)
                            {
                                xtw.WriteStartElement("condition");

                                xtw.WriteOptionalAttributeString("tag", condition.Tag);
                                xtw.WriteOptionalAttributeString("mask", condition.Mask);
                                xtw.WriteOptionalAttributeString("relation", condition.Relation);
                                xtw.WriteOptionalAttributeString("value", condition.Value);

                                // End condition
                                xtw.WriteEndElement();
                            }
                        }

                        // End adjuster
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Drivers != null)
                {
                    foreach (var driver in datItem.Machine.Drivers)
                    {
                        xtw.WriteStartElement("driver");

                        xtw.WriteOptionalAttributeString("status", driver.Status);
                        xtw.WriteOptionalAttributeString("emulation", driver.Emulation);
                        xtw.WriteOptionalAttributeString("cocktail", driver.Cocktail);
                        xtw.WriteOptionalAttributeString("savestate", driver.SaveState);

                        // End driver
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Features != null)
                {
                    foreach (var feature in datItem.Machine.Features)
                    {
                        xtw.WriteStartElement("feature");

                        xtw.WriteOptionalAttributeString("type", feature.Type);
                        xtw.WriteOptionalAttributeString("status", feature.Status);
                        xtw.WriteOptionalAttributeString("overall", feature.Overall);

                        // End feature
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Devices != null)
                {
                    foreach (var device in datItem.Machine.Devices)
                    {
                        xtw.WriteStartElement("device");

                        xtw.WriteOptionalAttributeString("type", device.Type);
                        xtw.WriteOptionalAttributeString("tag", device.Tag);
                        xtw.WriteOptionalAttributeString("fixed_image", device.FixedImage);
                        xtw.WriteOptionalAttributeString("mandatory", device.Mandatory);
                        xtw.WriteOptionalAttributeString("interface", device.Interface);

                        if (device.Instances != null)
                        {
                            foreach (var instance in device.Instances)
                            {
                                xtw.WriteStartElement("instance");

                                xtw.WriteOptionalAttributeString("name", instance.Name);
                                xtw.WriteOptionalAttributeString("briefname", instance.BriefName);

                                // End instance
                                xtw.WriteEndElement();
                            }
                        }
                        if (device.Extensions != null)
                        {
                            foreach (var extension in device.Extensions)
                            {
                                xtw.WriteStartElement("extension");

                                xtw.WriteOptionalAttributeString("name", extension.Name);

                                // End extension
                                xtw.WriteEndElement();
                            }
                        }

                        // End device
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.Slots != null)
                {
                    foreach (var slot in datItem.Machine.Slots)
                    {
                        xtw.WriteStartElement("slot");

                        xtw.WriteOptionalAttributeString("name", slot.Name);

                        if (slot.SlotOptions != null)
                        {
                            foreach (var slotOption in slot.SlotOptions)
                            {
                                xtw.WriteStartElement("slotoption");

                                xtw.WriteOptionalAttributeString("name", slotOption.Name);
                                xtw.WriteOptionalAttributeString("devname", slotOption.DeviceName);
                                xtw.WriteOptionalAttributeString("default", slotOption.Default.FromYesNo());

                                // End slotoption
                                xtw.WriteEndElement();
                            }
                        }

                        // End slot
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.SoftwareLists != null)
                {
                    foreach (var softwarelist in datItem.Machine.SoftwareLists)
                    {
                        xtw.WriteStartElement("softwarelist");

                        xtw.WriteOptionalAttributeString("name", softwarelist.Name);
                        xtw.WriteOptionalAttributeString("status", softwarelist.Status);
                        xtw.WriteOptionalAttributeString("filter", softwarelist.Filter);

                        // End softwarelist
                        xtw.WriteEndElement();
                    }
                }
                if (datItem.Machine.RamOptions != null)
                {
                    foreach (var ramOption in datItem.Machine.RamOptions)
                    {
                        xtw.WriteStartElement("ramoption");

                        xtw.WriteOptionalAttributeString("default", ramOption.Default.FromYesNo());

                        // End softwarelist
                        xtw.WriteEndElement();
                    }
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(XmlTextWriter xtw, DatItem datItem)
        {
            try
            {
                // End machine
                xtw.WriteEndElement();

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(XmlTextWriter xtw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state
                switch (datItem.ItemType)
                {
                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        xtw.WriteStartElement("biosset");
                        xtw.WriteRequiredAttributeString("name", biosSet.Name);
                        xtw.WriteOptionalAttributeString("description", biosSet.Description);
                        xtw.WriteOptionalAttributeString("default", biosSet.Default?.ToString().ToLowerInvariant());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Chip:
                        var chip = datItem as Chip;
                        xtw.WriteStartElement("chip");
                        xtw.WriteRequiredAttributeString("name", chip.Name);
                        xtw.WriteOptionalAttributeString("tag", chip.Tag);
                        xtw.WriteOptionalAttributeString("type", chip.ChipType);
                        xtw.WriteOptionalAttributeString("clock", chip.Clock);
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        xtw.WriteStartElement("disk");
                        xtw.WriteRequiredAttributeString("name", disk.Name);
                        xtw.WriteOptionalAttributeString("md5", disk.MD5?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha1", disk.SHA1?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("merge", disk.MergeTag);
                        xtw.WriteOptionalAttributeString("region", disk.Region);
                        xtw.WriteOptionalAttributeString("index", disk.Index);
                        xtw.WriteOptionalAttributeString("writable", disk.Writable.FromYesNo());
                        xtw.WriteOptionalAttributeString("status", disk.ItemStatus.FromItemStatus(false));
                        xtw.WriteOptionalAttributeString("optional", disk.Optional.FromYesNo());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        xtw.WriteStartElement("rom");
                        xtw.WriteRequiredAttributeString("name", rom.Name);
                        if (rom.Size != -1) xtw.WriteAttributeString("size", rom.Size.ToString());
                        xtw.WriteOptionalAttributeString("crc", rom.CRC?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("md5", rom.MD5?.ToLowerInvariant());
#if NET_FRAMEWORK
                        xtw.WriteOptionalAttributeString("ripemd160", rom?.RIPEMD160?.ToLowerInvariant());
#endif
                        xtw.WriteOptionalAttributeString("sha1", rom.SHA1?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha256", rom.SHA256?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha384", rom.SHA384?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("sha512", rom.SHA512?.ToLowerInvariant());
                        xtw.WriteOptionalAttributeString("bios", rom.Bios);
                        xtw.WriteOptionalAttributeString("merge", rom.MergeTag);
                        xtw.WriteOptionalAttributeString("region", rom.Region);
                        xtw.WriteOptionalAttributeString("offset", rom.Offset);
                        xtw.WriteOptionalAttributeString("status", rom.ItemStatus.FromItemStatus(false));
                        xtw.WriteOptionalAttributeString("optional", rom.Optional.FromYesNo());
                        xtw.WriteEndElement();
                        break;

                    case ItemType.Sample:
                        xtw.WriteStartElement("sample");
                        xtw.WriteRequiredAttributeString("name", datItem.Name);
                        xtw.WriteEndElement();
                        break;
                }

                xtw.Flush();
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
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(XmlTextWriter xtw)
        {
            try
            {
                // End machine
                xtw.WriteEndElement();

                // End mame
                xtw.WriteEndElement();

                xtw.Flush();
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
