using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.IO;

// TODO: Use softwarelist.dtd and *try* to make this write more correctly
namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of a SoftwareList
    /// </summary>
    internal class SoftwareList : DatFile
    {
        /// <summary>
        /// DTD for original MAME Software List DATs
        /// </summary>
        private const string SoftwareListDTD = @"<!ELEMENT softwarelist (software+)>
    <!ATTLIST softwarelist name CDATA #REQUIRED>
    <!ATTLIST softwarelist description CDATA #IMPLIED>
    <!ELEMENT software (description, year, publisher, info*, sharedfeat*, part*)>
        <!ATTLIST software name CDATA #REQUIRED>
        <!ATTLIST software cloneof CDATA #IMPLIED>
        <!ATTLIST software supported (yes|partial|no) string.Emptyyesstring.Empty>
        <!ELEMENT description (#PCDATA)>
        <!ELEMENT year (#PCDATA)>
        <!ELEMENT publisher (#PCDATA)>
        <!ELEMENT info EMPTY>
            <!ATTLIST info name CDATA #REQUIRED>
            <!ATTLIST info value CDATA #IMPLIED>
        <!ELEMENT sharedfeat EMPTY>
            <!ATTLIST sharedfeat name CDATA #REQUIRED>
            <!ATTLIST sharedfeat value CDATA #IMPLIED>
        <!ELEMENT part (feature*, dataarea*, diskarea*, dipswitch*)>
            <!ATTLIST part name CDATA #REQUIRED>
            <!ATTLIST part interface CDATA #REQUIRED>
            <!-- feature is used to store things like pcb-type, mapper type, etc. Specific values depend on the system. -->
            <!ELEMENT feature EMPTY>
                <!ATTLIST feature name CDATA #REQUIRED>
                <!ATTLIST feature value CDATA #IMPLIED>
            <!ELEMENT dataarea (rom*)>
                <!ATTLIST dataarea name CDATA #REQUIRED>
                <!ATTLIST dataarea size CDATA #REQUIRED>
                <!ATTLIST dataarea width (8|16|32|64) string.Empty8string.Empty>
                <!ATTLIST dataarea endianness (big|little) string.Emptylittlestring.Empty>
                <!ELEMENT rom EMPTY>
                    <!ATTLIST rom name CDATA #IMPLIED>
                    <!ATTLIST rom size CDATA #IMPLIED>
                    <!ATTLIST rom crc CDATA #IMPLIED>
                    <!ATTLIST rom md5 CDATA #IMPLIED>
                    <!ATTLIST rom sha1 CDATA #IMPLIED>
                    <!ATTLIST rom sha256 CDATA #IMPLIED>
                    <!ATTLIST rom sha384 CDATA #IMPLIED>
                    <!ATTLIST rom sha512 CDATA #IMPLIED>
                    <!ATTLIST rom offset CDATA #IMPLIED>
                    <!ATTLIST rom value CDATA #IMPLIED>
                    <!ATTLIST rom status (baddump|nodump|good) string.Emptygoodstring.Empty>
                    <!ATTLIST rom loadflag (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore) #IMPLIED>
            <!ELEMENT diskarea (disk*)>
                <!ATTLIST diskarea name CDATA #REQUIRED>
                <!ELEMENT disk EMPTY>
                    <!ATTLIST disk name CDATA #REQUIRED>
                    <!ATTLIST disk md5 CDATA #IMPLIED>
                    <!ATTLIST disk sha1 CDATA #IMPLIED>
                    <!ATTLIST disk status (baddump|nodump|good) string.Emptygoodstring.Empty>
                    <!ATTLIST disk writeable (yes|no) string.Emptynostring.Empty>
            <!ELEMENT dipswitch (dipvalue*)>
                <!ATTLIST dipswitch name CDATA #REQUIRED>
                <!ATTLIST dipswitch tag CDATA #REQUIRED>
                <!ATTLIST dipswitch mask CDATA #REQUIRED>
                <!ELEMENT dipvalue EMPTY>
                    <!ATTLIST dipvalue name CDATA #REQUIRED>
                    <!ATTLIST dipvalue value CDATA #REQUIRED>
                    <!ATTLIST dipvalue default (yes|no) string.Emptynostring.Empty>
";

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SoftwareList(DatFile datFile)
            : base(datFile)
        {
        }

        /// <inheritdoc/>
        public override void ParseFile(string filename, int indexId, bool keep, bool statsOnly = false, bool throwOnError = false)
        {
            // Prepare all internal variables
            XmlReader xtr = XmlReader.Create(filename, new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            });

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
                        case "softwarelist":
                            Header.Name ??= xtr.GetAttribute("name") ?? string.Empty;
                            Header.Description ??= xtr.GetAttribute("description") ?? string.Empty;

                            xtr.Read();
                            break;

                        // We want to process the entire subtree of the machine
                        case "software":
                            ReadSoftware(xtr.ReadSubtree(), statsOnly, filename, indexId);

                            // Skip the software now that we've processed it
                            xtr.Skip();
                            break;

                        default:
                            xtr.Read();
                            break;
                    }
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Warning(ex, $"Exception found while parsing '{filename}'");

                // For XML errors, just skip the affected node
                xtr?.Read();
            }

            xtr.Dispose();
        }

        /// <summary>
        /// Read software information
        /// </summary>
        /// <param name="reader">XmlReader representing a software block</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private void ReadSoftware(XmlReader reader, bool statsOnly, string filename, int indexId)
        {
            // If we have an empty software, skip it
            if (reader == null)
                return;

            // Otherwise, add what is possible
            reader.MoveToContent();

            bool containsItems = false;

            // Create a new machine
            Machine machine = new Machine
            {
                Name = reader.GetAttribute("name"),
                CloneOf = reader.GetAttribute("cloneof"),
                Supported = reader.GetAttribute("supported").AsSupported(),
            };

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "description":
                        machine.Description = reader.ReadElementContentAsString();
                        break;

                    case "year":
                        machine.Year = reader.ReadElementContentAsString();
                        break;

                    case "publisher":
                        machine.Publisher = reader.ReadElementContentAsString();
                        break;

                    case "info":
                       DatItem item = new Info
                       {
                            Name = reader.GetAttribute("name"),
                            Value = reader.GetAttribute("value"),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        }, statsOnly);
                        item.CopyMachineInformation(machine);
                        ParseAddHelper(item, statsOnly);
                        
                        reader.Read();
                        break;

                    case "sharedfeat":
                        DatItem item = new SharedFeature
                        {
                            Name = reader.GetAttribute("name"),
                            Value = reader.GetAttribute("value"),

                            Source = new Source
                            {
                                Index = indexId,
                                Name = filename,
                            },
                        }, statsOnly);
                        item.CopyMachineInformation(machine);
                        ParseAddHelper(item, statsOnly);
                        
                        reader.Read();
                        break;

                    case "part": // Contains all rom and disk information
                        var part = new Part()
                        {
                            Name = reader.GetAttribute("name"),
                            Interface = reader.GetAttribute("interface"),
                        };

                        // Now read the internal tags
                        containsItems = ReadPart(reader.ReadSubtree(), machine, part, statsOnly, filename, indexId);

                        // Skip the part now that we've processed it
                        reader.Skip();
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
                    Source = new Source
                    {
                        Index = indexId,
                        Name = filename,
                    },
                };

                blank.CopyMachineInformation(machine);

                // Now process and add the rom
                ParseAddHelper(blank, statsOnly);
            }
        }

        /// <summary>
        /// Read part information
        /// </summary>
        /// <param name="reader">XmlReader representing a part block</param>
        /// <param name="machine">Machine information to pass to contained items</param>
        /// <param name="part">Part information to pass to contained items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        private bool ReadPart(XmlReader reader, Machine machine, Part part, bool statsOnly, string filename, int indexId)
        {
            // If we have an empty port, skip it
            if (reader == null)
                return false;

            // Get lists ready
            part.Features = new List<PartFeature>();
            part.DataAreas = new List<DataArea>();
            List<DatItem> items = new List<DatItem>();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "feature":
                        var feature = new PartFeature()
                        {
                            Name = reader.GetAttribute("name"),
                            Value = reader.GetAttribute("value"),
                        };

                        part.Features.Add(feature);

                        reader.Read();
                        break;

                    case "dataarea":
                        var dataArea = new DataArea
                        {
                            Name = reader.GetAttribute("name"),
                            Size = Utilities.CleanLong(reader.GetAttribute("size")),
                            Width = Utilities.CleanLong(reader.GetAttribute("width")),
                            Endianness = reader.GetAttribute("endianness").AsEndianness(),
                        };

                        List<DatItem> roms = ReadDataArea(reader.ReadSubtree(), dataArea);

                        // If we got valid roms, add them to the list
                        if (!String.IsNullOrEmpty(string.Join(", ", roms)))
                                items.AddRange(roms);
                        else
                                part.DataAreas.Add(dataArea);

                        // Skip the dataarea now that we've processed it
                        reader.Skip();
                        break;

                    case "diskarea":
                        var diskArea = new DiskArea
                        {
                            Name = reader.GetAttribute("name"),
                        };

                        List<DatItem> disks = ReadDiskArea(reader.ReadSubtree(), diskArea);

                        // If we got valid disks, add them to the list
                        if (disks != null)
                            items.AddRange(disks);

                        // Skip the diskarea now that we've processed it
                        reader.Skip();
                        break;

                    case "dipswitch":
                        var dipSwitch = new DipSwitch
                        {
                            Name = reader.GetAttribute("name"),
                            Tag = reader.GetAttribute("tag"),
                            Mask = reader.GetAttribute("mask"),
                        };

                        // Now read the internal tags
                        ReadDipSwitch(reader.ReadSubtree(), dipSwitch);

                        items.Add(dipSwitch);

                        // Skip the dipswitch now that we've processed it
                        reader.Skip();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            // Loop over all of the items, if they exist
            string key = string.Empty;
            foreach (DatItem item in items)
            {
                // Add all missing information
                switch (item.ItemType)
                {
                    case ItemType.DipSwitch:
                        (item as DipSwitch).Part = part;
                        break;
                    case ItemType.Disk:
                        (item as Disk).Part = part;
                        break;
                    case ItemType.Rom:
                        (item as Rom).Part = part;

                        // If the rom is continue or ignore, add the size to the previous rom
                        // TODO: Can this be done on write? We technically lose information this way.
                        // Order is not guaranteed, and since these don't tend to have any way
                        // of determining what the "previous" item was after this, that info would
                        // have to be stored *with* the item somehow
                        if ((item as Rom).LoadFlag == LoadFlag.Continue || (item as Rom).LoadFlag == LoadFlag.Ignore)
                        {
                            int index = Items[key].Count - 1;
                            DatItem lastrom = Items[key][index];
                            if (lastrom.ItemType == ItemType.Rom)
                            {
                                (lastrom as Rom).Size += (item as Rom).Size;
                                Items[key].RemoveAt(index);
                                Items[key].Add(lastrom);
                            }

                            continue;
                        }

                        break;
                }

                item.Source = new Source(indexId, filename);
                item.CopyMachineInformation(machine);

                // Finally add each item
                key = ParseAddHelper(item, statsOnly);
            }

            return items.Any();
        }

        /// <summary>
        /// Read dataarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a dataarea block</param>
        /// <param name="dataArea">DataArea representing the enclosing area</param>
        private List<DatItem> ReadDataArea(XmlReader reader, DataArea dataArea)
        {
            List<DatItem> items = new List<DatItem>();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "rom":
                        var rom = new Rom
                        {
                            Name = reader.GetAttribute("name"),
                            Size = Utilities.CleanLong(reader.GetAttribute("size")),
                            CRC = reader.GetAttribute("crc"),
                            SHA1 = reader.GetAttribute("sha1"),
                            Offset = reader.GetAttribute("offset"),
                            Value = reader.GetAttribute("value"),
                            ItemStatus = reader.GetAttribute("status").AsItemStatus(),
                            LoadFlag = reader.GetAttribute("loadflag").AsLoadFlag(),

                            DataArea = dataArea,
                        };

                        items.Add(rom);
                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return items;
        }

        /// <summary>
        /// Read diskarea information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="diskArea">DiskArea representing the enclosing area</param>
        private List<DatItem> ReadDiskArea(XmlReader reader, DiskArea diskArea)
        {
            List<DatItem> items = new List<DatItem>();

            while (!reader.EOF)
            {
                // We only want elements
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                // Get the elements from the software
                switch (reader.Name)
                {
                    case "disk":
                        DatItem disk = new Disk
                        {
                            Name = reader.GetAttribute("name"),
                            SHA1 = reader.GetAttribute("sha1"),
                            ItemStatus = reader.GetAttribute("status").AsItemStatus(),
                            Writable = reader.GetAttribute("writable").AsYesNo(),

                            DiskArea = diskArea,
                        };

                        items.Add(disk);
                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }

            return items;
        }

        /// <summary>
        /// Read DipSwitch DipValues information
        /// </summary>
        /// <param name="reader">XmlReader representing a diskarea block</param>
        /// <param name="dipSwitch">DipSwitch to populate</param>
        private void ReadDipSwitch(XmlReader reader, DipSwitch dipSwitch)
        {
            // If we have an empty dipswitch, skip it
            if (reader == null)
                return;

            // Get list ready
            dipSwitch.Values = new List<Setting>();

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
                    case "dipvalue":
                        var dipValue = new Setting
                        {
                            Name = reader.GetAttribute("name"),
                            Value = reader.GetAttribute("value"),
                            Default = reader.GetAttribute("default").AsYesNo(),
                        };

                        dipSwitch.Values.Add(dipValue);

                        reader.Read();
                        break;

                    default:
                        reader.Read();
                        break;
                }
            }
        }

        /// <inheritdoc/>
        protected override ItemType[] GetSupportedTypes()
        {
            return new ItemType[]
            {
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Info,
                ItemType.DataArea,
                ItemType.Rom,
                ItemType.SharedFeature,
            };
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                logger.User($"Writing to '{outfile}'...");
                FileStream fs = File.Create(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
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
                    ConcurrentList<DatItem> datItems = Items.FilteredItems(key);

                    // If this machine doesn't contain any writable items, skip
                    if (!ContainsWritable(datItems))
                        continue;

                    // Resolve the names in the block
                    datItems = DatItem.ResolveNames(datItems);

                    for (int index = 0; index < datItems.Count; index++)
                    {
                        DatItem datItem = datItems[index];

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != datItem.Machine.Name.ToLowerInvariant())
                            WriteEndGame(xtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != datItem.Machine.Name.ToLowerInvariant())
                            WriteStartGame(xtw, datItem);

                        // Check for a "null" item
                        datItem = ProcessNullifiedItem(datItem);

                        // Write out the item if we're not ignoring
                        if (!ShouldIgnore(datItem, ignoreblanks))
                            WriteDatItem(xtw, datItem);

                        // Set the new data to compare against
                        lastgame = datItem.Machine.Name;
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                logger.User($"'{outfile}' written!{Environment.NewLine}");
                xtw.Dispose();
                fs.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT header using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private void WriteHeader(XmlTextWriter xtw)
        {
            xtw.WriteStartDocument();
            xtw.WriteDocType("softwarelist", null, "softwarelist.dtd", null);

            xtw.WriteStartElement("softwarelist");
            xtw.WriteRequiredAttributeString("name", Header.Name);
            xtw.WriteRequiredAttributeString("description", Header.Description);

            xtw.Flush();
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        private void WriteStartGame(XmlTextWriter xtw, DatItem datItem)
        {
            // No game should start with a path separator
            datItem.Machine.Name = datItem.Machine.Name.TrimStart(Path.DirectorySeparatorChar);

            // Build the state
            xtw.WriteStartElement("software");
            xtw.WriteRequiredAttributeString("name", datItem.Machine.Name);
            if (!string.Equals(datItem.Machine.Name, datItem.Machine.CloneOf, StringComparison.OrdinalIgnoreCase))
                xtw.WriteOptionalAttributeString("cloneof", datItem.Machine.CloneOf);
            xtw.WriteOptionalAttributeString("supported", datItem.Machine.Supported.FromSupported(false));

            xtw.WriteOptionalElementString("description", datItem.Machine.Description);
            xtw.WriteOptionalElementString("year", datItem.Machine.Year);
            xtw.WriteOptionalElementString("publisher", datItem.Machine.Publisher);

            xtw.Flush();
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private void WriteEndGame(XmlTextWriter xtw)
        {
            // End software
            xtw.WriteEndElement();

            xtw.Flush();
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        private void WriteDatItem(XmlTextWriter xtw, DatItem datItem)
        {
            // Pre-process the item name
            ProcessItemName(datItem, true);

            // Build the state
            switch (datItem.ItemType)
            {
                case ItemType.DipSwitch:
                    var dipSwitch = datItem as DipSwitch;
                    xtw.WriteStartElement("dipswitch");
                    xtw.WriteRequiredAttributeString("name", dipSwitch.Name);
                    xtw.WriteRequiredAttributeString("tag", dipSwitch.Tag);
                    xtw.WriteRequiredAttributeString("mask", dipSwitch.Mask);
                    if (dipSwitch.ValuesSpecified)
                    {
                        foreach (Setting dipValue in dipSwitch.Values)
                        {
                            xtw.WriteStartElement("dipvalue");
                            xtw.WriteRequiredAttributeString("name", dipValue.Name);
                            xtw.WriteOptionalAttributeString("value", dipValue.Value);
                            xtw.WriteOptionalAttributeString("default", dipValue.Default.FromYesNo());
                            xtw.WriteEndElement();
                        }
                    }
                    xtw.WriteEndElement();
                    break;

                case ItemType.Disk:
                    var disk = datItem as Disk;
                    string diskAreaName = disk.DiskArea?.Name;
                    if (string.IsNullOrWhiteSpace(diskAreaName))
                        diskAreaName = "cdrom";

                    xtw.WriteStartElement("part");
                    xtw.WriteRequiredAttributeString("name", disk.Part?.Name);
                    xtw.WriteRequiredAttributeString("interface", disk.Part?.Interface);

                    if (disk.Part?.FeaturesSpecified == true)
                    {
                        foreach (PartFeature partFeature in disk.Part.Features)
                        {
                            xtw.WriteStartElement("feature");
                            xtw.WriteRequiredAttributeString("name", partFeature.Name);
                            xtw.WriteRequiredAttributeString("value", partFeature.Value);
                            xtw.WriteEndElement();
                        }
                    }

                    xtw.WriteStartElement("diskarea");
                    xtw.WriteRequiredAttributeString("name", diskAreaName);

                    xtw.WriteStartElement("disk");
                    xtw.WriteRequiredAttributeString("name", disk.Name);
                    xtw.WriteOptionalAttributeString("md5", disk.MD5?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("sha1", disk.SHA1?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("status", disk.ItemStatus.FromItemStatus(false));
                    xtw.WriteOptionalAttributeString("writable", disk.Writable.FromYesNo());
                    xtw.WriteEndElement();

                    // End diskarea
                    xtw.WriteEndElement();

                    // End part
                    xtw.WriteEndElement();
                    break;

                case ItemType.Info:
                    var info = datItem as Info;
                    xtw.WriteStartElement("info");
                    xtw.WriteRequiredAttributeString("name", info.Name);
                    xtw.WriteRequiredAttributeString("value", info.Value);
                    xtw.WriteEndElement();
                    break;

                case ItemType.Rom:
                    var rom = datItem as Rom;
                    string dataAreaName = rom.DataArea?.Name;
                    if (string.IsNullOrWhiteSpace(dataAreaName))
                        dataAreaName = "rom";

                    xtw.WriteStartElement("part");
                    xtw.WriteRequiredAttributeString("name", rom.Part?.Name);
                    xtw.WriteRequiredAttributeString("interface", rom.Part?.Interface);

                    if (rom.Part?.FeaturesSpecified == true)
                    {
                        foreach (PartFeature kvp in rom.Part.Features)
                        {
                            xtw.WriteStartElement("feature");
                            xtw.WriteRequiredAttributeString("name", kvp.Name);
                            xtw.WriteRequiredAttributeString("value", kvp.Value);
                            xtw.WriteEndElement();
                        }
                    }

                    xtw.WriteStartElement("dataarea");
                    xtw.WriteRequiredAttributeString("name", dataAreaName);
                    xtw.WriteOptionalAttributeString("size", rom.DataArea?.Size.ToString());
                    xtw.WriteOptionalAttributeString("width", rom.DataArea?.Width?.ToString());
                    xtw.WriteOptionalAttributeString("endianness", rom.DataArea?.Endianness.FromEndianness());

                    xtw.WriteStartElement("rom");
                    xtw.WriteRequiredAttributeString("name", rom.Name);
                    xtw.WriteOptionalAttributeString("size", rom.Size?.ToString());
                    xtw.WriteOptionalAttributeString("crc", rom.CRC?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("md5", rom.MD5?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("sha1", rom.SHA1?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("sha256", rom.SHA256?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("sha384", rom.SHA384?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("sha512", rom.SHA512?.ToLowerInvariant());
                    xtw.WriteOptionalAttributeString("offset", rom.Offset);
                    xtw.WriteOptionalAttributeString("value", rom.Value);
                    xtw.WriteOptionalAttributeString("status", rom.ItemStatus.FromItemStatus(false));
                    xtw.WriteOptionalAttributeString("loadflag", rom.LoadFlag.FromLoadFlag());
                    xtw.WriteEndElement();

                    // End dataarea
                    xtw.WriteEndElement();
                    
                    // additional dataareas
                    if (rom.Part?.DataAreasSpecified == true)
                    {
                        foreach (DataArea kvp in rom.Part.DataAreas)
                        {
                            xtw.WriteStartElement("dataarea");
                            xtw.WriteRequiredAttributeString("name", kvp.Name);
                            xtw.WriteOptionalAttributeString("size", kvp.Size.ToString());
                            xtw.WriteOptionalAttributeString("width", kvp.Width?.ToString());
                            xtw.WriteOptionalAttributeString("endianness", kvp.Endianness.FromEndianness());
                            xtw.WriteEndElement();
                        }
                    }

                    // End part
                    xtw.WriteEndElement();
                    break;

                case ItemType.SharedFeature:
                    var sharedFeature = datItem as SharedFeature;
                    xtw.WriteStartElement("sharedfeat");
                    xtw.WriteRequiredAttributeString("name", sharedFeature.Name);
                    xtw.WriteRequiredAttributeString("value", sharedFeature.Value);
                    xtw.WriteEndElement();
                    break;
            }

            xtw.Flush();
        }

        /// <summary>
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private void WriteFooter(XmlTextWriter xtw)
        {
            // End software
            xtw.WriteEndElement();

            // End softwarelist
            xtw.WriteEndElement();

            xtw.Flush();
        }
    }
}
