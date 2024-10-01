using System;
using System.Collections.Generic;
using System.Linq;
using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents writing a SoftwareList
    /// </summary>
    internal partial class SoftwareList : DatFile
    {
        /// <inheritdoc/>
        protected override ItemType[] GetSupportedTypes()
        {
            return new ItemType[]
            {
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Info,
                ItemType.Rom,
                ItemType.SharedFeature,
            };
        }

        /// <inheritdoc/>
        protected override List<DatItemField>? GetMissingRequiredFields(DatItem datItem)
        {
            var missingFields = new List<DatItemField>();

            switch (datItem)
            {
                case DipSwitch dipSwitch:
                    if (!dipSwitch.PartSpecified)
                    {
                        missingFields.Add(DatItemField.Part_Name);
                        missingFields.Add(DatItemField.Part_Interface);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(dipSwitch.Part.Name))
                            missingFields.Add(DatItemField.Part_Name);
                        if (string.IsNullOrWhiteSpace(dipSwitch.Part.Interface))
                            missingFields.Add(DatItemField.Part_Interface);
                    }
                    if (string.IsNullOrWhiteSpace(dipSwitch.Name))
                        missingFields.Add(DatItemField.Name);
                    if (string.IsNullOrWhiteSpace(dipSwitch.Tag))
                        missingFields.Add(DatItemField.Tag);
                    if (string.IsNullOrWhiteSpace(dipSwitch.Mask))
                        missingFields.Add(DatItemField.Mask);
                    if (dipSwitch.ValuesSpecified)
                    {
                        if (dipSwitch.Values.Any(dv => string.IsNullOrWhiteSpace(dv.Name)))
                            missingFields.Add(DatItemField.Part_Feature_Name);
                        if (dipSwitch.Values.Any(dv => string.IsNullOrWhiteSpace(dv.Value)))
                            missingFields.Add(DatItemField.Part_Feature_Value);
                    }

                    break;

                case Disk disk:
                    if (!disk.PartSpecified)
                    {
                        missingFields.Add(DatItemField.Part_Name);
                        missingFields.Add(DatItemField.Part_Interface);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(disk.Part.Name))
                            missingFields.Add(DatItemField.Part_Name);
                        if (string.IsNullOrWhiteSpace(disk.Part.Interface))
                            missingFields.Add(DatItemField.Part_Interface);
                    }
                    if (!disk.DiskAreaSpecified)
                    {
                        missingFields.Add(DatItemField.AreaName);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(disk.DiskArea.Name))
                            missingFields.Add(DatItemField.AreaName);
                    }
                    if (string.IsNullOrWhiteSpace(disk.Name))
                        missingFields.Add(DatItemField.Name);
                    break;

                case Info info:
                    if (string.IsNullOrWhiteSpace(info.Name))
                        missingFields.Add(DatItemField.Name);
                    break;

                case Rom rom:
                    if (!rom.PartSpecified)
                    {
                        missingFields.Add(DatItemField.Part_Name);
                        missingFields.Add(DatItemField.Part_Interface);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(rom.Part.Name))
                            missingFields.Add(DatItemField.Part_Name);
                        if (string.IsNullOrWhiteSpace(rom.Part.Interface))
                            missingFields.Add(DatItemField.Part_Interface);
                    }
                    if (!rom.DataAreaSpecified)
                    {
                        missingFields.Add(DatItemField.AreaName);
                        missingFields.Add(DatItemField.AreaSize);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(rom.DataArea.Name))
                            missingFields.Add(DatItemField.AreaName);
                        if (!rom.DataArea.SizeSpecified)
                            missingFields.Add(DatItemField.AreaSize);
                    }
                    break;

                case SharedFeature sharedFeat:
                    if (string.IsNullOrWhiteSpace(sharedFeat.Name))
                        missingFields.Add(DatItemField.Name);
                    break;
                default:
                    // Unsupported ItemTypes should be caught already
                    return null;
            }

            return missingFields;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                logger.User($"Writing to '{outfile}'...");

                var softwarelist = CreateSoftwareList(ignoreblanks);
                if (!Serialization.SoftawreList.SerializeToFileWithDocType(softwarelist, outfile))
                {
                    logger.Warning($"File '{outfile}' could not be written! See the log for more details.");
                    return false;
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex);
                return false;
            }

            logger.User($"'{outfile}' written!{Environment.NewLine}");
            return true;
        }

        #region Converters

        /// <summary>
        /// Create a SoftwareList from the current internal information
        /// <summary>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise</param>
        private Models.SoftwareList.SoftwareList CreateSoftwareList(bool ignoreblanks)
        {
            var softwarelist = new Models.SoftwareList.SoftwareList
            {
                Name = Header.Name,
                Description = Header.Description,
                Notes = Header.Comment,
                Software = CreateSoftware(ignoreblanks),
            };

            return softwarelist;
        }

        /// <summary>
        /// Create an array of Software from the current internal information
        /// <summary>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise</param>
        private Models.SoftwareList.Software[]? CreateSoftware(bool ignoreblanks)
        {
            // If we don't have items, we can't do anything
            if (this.Items == null || !this.Items.Any())
                return null;

            // Create a list of hold the games
            var software = new List<Models.SoftwareList.Software>();

            // Loop through the sorted items and create games for them
            foreach (string key in Items.SortedKeys)
            {
                var items = Items.FilteredItems(key);
                if (items == null || !items.Any())
                    continue;

                // Get the first item for game information
                var machine = items[0].Machine;
                var sw = CreateSoftware(machine);

                // Create holders for all item types
                var infos = new List<Models.SoftwareList.Info>();
                var sharedfeats = new List<Models.SoftwareList.SharedFeat>();
                var parts = new List<Models.SoftwareList.Part>();

                // Loop through and convert the items to respective lists
                for (int index = 0; index < items.Count; index++)
                {
                    // Get the item
                    var item = items[index];

                    // Check for a "null" item
                    item = ProcessNullifiedItem(item);

                    // Skip if we're ignoring the item
                    if (ShouldIgnore(item, ignoreblanks))
                        continue;

                    switch (item)
                    {
                        case Info info:
                            infos.Add(CreateInfo(info));
                            break;
                        case SharedFeature sharedFeature:
                            sharedfeats.Add(CreateSharedFeat(sharedFeature));
                            break;
                        case Rom rom:
                            parts.Add(CreatePart(rom));
                            break;
                        case Disk disk:
                            parts.Add(CreatePart(disk));
                            break;
                        case DipSwitch dipswitch:
                            parts.Add(CreatePart(dipswitch));
                            break;
                    }
                }

                // Process the parts to ensure we don't have duplicates
                parts = SantitizeParts(parts);

                // Assign the values to the game
                sw.Info = infos.ToArray();
                sw.SharedFeat = sharedfeats.ToArray();
                sw.Part = parts.ToArray();

                // Add the game to the list
                software.Add(sw);
            }

            return software.ToArray();
        }

        /// <summary>
        /// Create a Software from the current internal information
        /// <summary>
        private Models.SoftwareList.Software CreateSoftware(Machine machine)
        {
            var software = new Models.SoftwareList.Software
            {
                Name = machine.Name,
                CloneOf = machine.CloneOf,
                Supported = machine.Supported.FromSupported(verbose: true),
                Description = machine.Description,
                Year = machine.Year,
                Publisher = machine.Publisher,
                Notes = machine.Comment,
            };

            return software;
        }

        /// <summary>
        /// Create a Info from the current Info DatItem
        /// <summary>
        private static Models.SoftwareList.Info CreateInfo(Info item)
        {
            var info = new Models.SoftwareList.Info
            {
                Name = item.Name,
                Value = item.Value,
            };
            return info;
        }

        /// <summary>
        /// Create a SharedFeat from the current SharedFeature DatItem
        /// <summary>
        private static Models.SoftwareList.SharedFeat CreateSharedFeat(SharedFeature item)
        {
            var sharedfeat = new Models.SoftwareList.SharedFeat
            {
                Name = item.Name,
                Value = item.Value,
            };
            return sharedfeat;
        }

        /// <summary>
        /// Create a Part from the current Rom DatItem
        /// <summary>
        private static Models.SoftwareList.Part CreatePart(Rom item)
        {
            var part = new Models.SoftwareList.Part
            {
                Name = item.Part.Name,
                Interface = item.Part.Interface,
                Feature = CreateFeatures(item.Part.Features),
                DataArea = CreateDataAreas(item),
                DiskArea = null,
                DipSwitch = null,
            };
            return part;
        }

        /// <summary>
        /// Create a Part from the current Disk DatItem
        /// <summary>
        private static Models.SoftwareList.Part CreatePart(Disk item)
        {
            var part = new Models.SoftwareList.Part
            {
                Name = item.Part.Name,
                Interface = item.Part.Interface,
                Feature = CreateFeatures(item.Part.Features),
                DataArea = null,
                DiskArea = CreateDiskAreas(item),
                DipSwitch = null,
            };
            return part;
        }

        /// <summary>
        /// Create a Part from the current DipSwitch DatItem
        /// <summary>
        private static Models.SoftwareList.Part CreatePart(DipSwitch item)
        {
            var part = new Models.SoftwareList.Part
            {
                Name = item.Part.Name,
                Interface = item.Part.Interface,
                Feature = CreateFeatures(item.Part.Features),
                DataArea = null,
                DiskArea = null,
                DipSwitch = CreateDipSwitches(item),
            };
            return part;
        }

        /// <summary>
        /// Create a Feature array from the current list of PartFeature DatItems
        /// <summary>
        private static Models.SoftwareList.Feature[]? CreateFeatures(List<PartFeature> items)
        {
            // If we don't have features, we can't do anything
            if (items == null || !items.Any())
                return null;

            var features = new List<Models.SoftwareList.Feature>();
            foreach (var item in items)
            {
                var feature = new Models.SoftwareList.Feature
                {
                    Name = item.Name,
                    Value = item.Value,
                };
                features.Add(feature);
            }

            return features.ToArray();
        }

        /// <summary>
        /// Create a DataArea array from the current Rom DatItem
        /// <summary>
        private static Models.SoftwareList.DataArea[]? CreateDataAreas(Rom item)
        {
            var dataArea = new Models.SoftwareList.DataArea
            {
                Name = item.DataArea.Name,
                Size = item.DataArea.Size?.ToString(),
                Width = item.DataArea.Width?.ToString(),
                Endianness = item.DataArea.Endianness.FromEndianness(),
                Rom = CreateRom(item),
            };
            return new Models.SoftwareList.DataArea[] { dataArea };
        }

        /// <summary>
        /// Create a Rom array from the current Rom DatItem
        /// <summary>
        private static Models.SoftwareList.Rom[]? CreateRom(Rom item)
        {
            var rom = new Models.SoftwareList.Rom
            {
                Name = item.Name,
                Size = item.Size?.ToString(),
                Length = null,
                CRC = item.CRC,
                SHA1 = item.SHA1,
                Offset = item.Offset,
                Value = item.Value,
                Status = item.ItemStatus.FromItemStatus(yesno: false),
                LoadFlag = item.LoadFlag.FromLoadFlag(),
            };
            return new Models.SoftwareList.Rom[] { rom };
        }

        /// <summary>
        /// Create a DiskArea array from the current Disk DatItem
        /// <summary>
        private static Models.SoftwareList.DiskArea[]? CreateDiskAreas(Disk item)
        {
            var diskArea = new Models.SoftwareList.DiskArea
            {
                Name = item.DiskArea.Name,
                Disk = CreateDisk(item),
            };
            return new Models.SoftwareList.DiskArea[] { diskArea };
        }

        /// <summary>
        /// Create a Disk array from the current Disk DatItem
        /// <summary>
        private static Models.SoftwareList.Disk[]? CreateDisk(Disk item)
        {
            var disk = new Models.SoftwareList.Disk
            {
                Name = item.Name,
                MD5 = item.MD5,
                SHA1 = item.SHA1,
                Status = item.ItemStatus.FromItemStatus(yesno: false),
                Writeable = item.Writable?.ToString(),
            };
            return new Models.SoftwareList.Disk[] { disk };
        }

        /// <summary>
        /// Create a DipSwitch array from the current DipSwitch DatItem
        /// <summary>
        private static Models.SoftwareList.DipSwitch[]? CreateDipSwitches(DipSwitch item)
        {
            var dipValues = new List<Models.SoftwareList.DipValue>();
            foreach (var setting in item.Values ?? new List<Setting>())
            {
                var dipValue = new Models.SoftwareList.DipValue
                {
                    Name = setting.Name,
                    Value = setting.Value,
                    Default = setting.Default?.ToString(),
                };
                dipValues.Add(dipValue);
            }

            var dipSwitch = new Models.SoftwareList.DipSwitch { DipValue = dipValues.ToArray() };
            return new Models.SoftwareList.DipSwitch[] { dipSwitch };
        }

        /// <summary>
        /// Sanitize Parts list to ensure no duplicates exist
        /// <summary>
        private static List<Models.SoftwareList.Part> SantitizeParts(List<Models.SoftwareList.Part> parts)
        {
            // If we have no parts, we can't do anything
            if (!parts.Any())
                return parts;

            var grouped = parts.GroupBy(p => p.Name);

            var tempParts = new List<Models.SoftwareList.Part>();
            foreach (var grouping in grouped)
            {
                var tempPart = new Models.SoftwareList.Part();

                var tempFeatures = new List<Models.SoftwareList.Feature>();
                var tempDataAreas = new List<Models.SoftwareList.DataArea>();
                var tempDiskAreas = new List<Models.SoftwareList.DiskArea>();
                var tempDipSwitches = new List<Models.SoftwareList.DipSwitch>();

                foreach (var part in grouping)
                {
                    tempPart.Name ??= part.Name;
                    tempPart.Interface ??= part.Interface;

                    if (part.Feature != null)
                        tempFeatures.AddRange(part.Feature);
                    if (part.DataArea != null)
                        tempDataAreas.AddRange(part.DataArea);
                    if (part.DiskArea != null)
                        tempDiskAreas.AddRange(part.DiskArea);
                    if (part.DipSwitch != null)
                        tempDipSwitches.AddRange(part.DipSwitch);
                }

                tempDataAreas = SantitizeDataAreas(tempDataAreas);
                tempDiskAreas = SantitizeDiskAreas(tempDiskAreas);

                if (tempFeatures.Count > 0)
                    tempPart.Feature = tempFeatures.ToArray();
                if (tempDataAreas.Count > 0)
                    tempPart.DataArea = tempDataAreas.ToArray();
                if (tempDiskAreas.Count > 0)
                    tempPart.DiskArea = tempDiskAreas.ToArray();
                if (tempDipSwitches.Count > 0)
                    tempPart.DipSwitch = tempDipSwitches.ToArray();

                tempParts.Add(tempPart);
            }

            return tempParts;
        }

        /// <summary>
        /// Sanitize DataAreas list to ensure no duplicates exist
        /// <summary>
        private static List<Models.SoftwareList.DataArea> SantitizeDataAreas(List<Models.SoftwareList.DataArea> dataAreas)
        {
            // If we have no DataAreas, we can't do anything
            if (!dataAreas.Any())
                return dataAreas;

            var grouped = dataAreas.GroupBy(p => p.Name);

            var tempDataAreas = new List<Models.SoftwareList.DataArea>();
            foreach (var grouping in grouped)
            {
                var tempDataArea = new Models.SoftwareList.DataArea();
                var tempRoms = new List<Models.SoftwareList.Rom>();

                foreach (var dataArea in grouping)
                {
                    tempDataArea.Name ??= dataArea.Name;
                    tempDataArea.Size ??= dataArea.Size;
                    tempDataArea.Width ??= dataArea.Width;
                    tempDataArea.Endianness ??= dataArea.Endianness;

                    if (dataArea.Rom != null)
                        tempRoms.AddRange(dataArea.Rom);
                }

                if (tempRoms.Count > 0)
                    tempDataArea.Rom = tempRoms.ToArray();

                tempDataAreas.Add(tempDataArea);
            }

            return tempDataAreas;
        }

        /// <summary>
        /// Sanitize DiskArea list to ensure no duplicates exist
        /// <summary>
        private static List<Models.SoftwareList.DiskArea> SantitizeDiskAreas(List<Models.SoftwareList.DiskArea> diskAreas)
        {
            // If we have no DiskAreas, we can't do anything
            if (!diskAreas.Any())
                return diskAreas;

            var grouped = diskAreas.GroupBy(p => p.Name);

            var tempDiskAreas = new List<Models.SoftwareList.DiskArea>();
            foreach (var grouping in grouped)
            {
                var tempDiskArea = new Models.SoftwareList.DiskArea();
                var tempDisks = new List<Models.SoftwareList.Disk>();

                foreach (var dataArea in grouping)
                {
                    tempDiskArea.Name ??= dataArea.Name;
                    if (dataArea.Disk != null)
                        tempDisks.AddRange(dataArea.Disk);
                }

                if (tempDisks.Count > 0)
                    tempDiskArea.Disk = tempDisks.ToArray();

                tempDiskAreas.Add(tempDiskArea);
            }

            return tempDiskAreas;
        }

        #endregion
    }
}
