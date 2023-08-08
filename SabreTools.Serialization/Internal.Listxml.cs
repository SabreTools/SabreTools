using System.Collections.Generic;
using System.Linq;

namespace SabreTools.Serialization
{
    /// <summary>
    /// Serializer for Listxml models to internal structure
    /// </summary>
    public partial class Internal
    {
        #region Serialize

        /// <summary>
        /// Convert from <cref="Models.Listxml.GameBase"/> to <cref="Models.Internal.Machine"/>
        /// </summary>
        public static Models.Internal.Machine ConvertMachineFromListxml(Models.Listxml.GameBase item)
        {
            var machine = new Models.Internal.Machine
            {
                [Models.Internal.Machine.NameKey] = item.Name,
                [Models.Internal.Machine.SourceFileKey] = item.SourceFile,
                [Models.Internal.Machine.IsBiosKey] = item.IsBios,
                [Models.Internal.Machine.IsDeviceKey] = item.IsDevice,
                [Models.Internal.Machine.IsMechanicalKey] = item.IsMechanical,
                [Models.Internal.Machine.RunnableKey] = item.Runnable,
                [Models.Internal.Machine.CloneOfKey] = item.CloneOf,
                [Models.Internal.Machine.RomOfKey] = item.RomOf,
                [Models.Internal.Machine.SampleOfKey] = item.SampleOf,
                [Models.Internal.Machine.DescriptionKey] = item.Description,
                [Models.Internal.Machine.YearKey] = item.Year,
                [Models.Internal.Machine.ManufacturerKey] = item.Manufacturer,
                [Models.Internal.Machine.HistoryKey] = item.History,
            };

            if (item.BiosSet != null && item.BiosSet.Any())
            {
                var biosSets = new List<Models.Internal.BiosSet>();
                foreach (var biosSet in item.BiosSet)
                {
                    biosSets.Add(ConvertFromListxml(biosSet));
                }
                machine[Models.Internal.Machine.BiosSetKey] = biosSets.ToArray();
            }

            if (item.Rom != null && item.Rom.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var rom in item.Rom)
                {
                    roms.Add(ConvertFromListxml(rom));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            if (item.Disk != null && item.Disk.Any())
            {
                var disks = new List<Models.Internal.Disk>();
                foreach (var disk in item.Disk)
                {
                    disks.Add(ConvertFromListxml(disk));
                }
                machine[Models.Internal.Machine.DiskKey] = disks.ToArray();
            }

            if (item.DeviceRef != null && item.DeviceRef.Any())
            {
                var deviceRefs = new List<Models.Internal.DeviceRef>();
                foreach (var deviceRef in item.DeviceRef)
                {
                    deviceRefs.Add(ConvertFromListxml(deviceRef));
                }
                machine[Models.Internal.Machine.DeviceRefKey] = deviceRefs.ToArray();
            }

            if (item.Sample != null && item.Sample.Any())
            {
                var samples = new List<Models.Internal.Sample>();
                foreach (var sample in item.Sample)
                {
                    samples.Add(ConvertFromListxml(sample));
                }
                machine[Models.Internal.Machine.SampleKey] = samples.ToArray();
            }

            if (item.Chip != null && item.Chip.Any())
            {
                var chips = new List<Models.Internal.Chip>();
                foreach (var chip in item.Chip)
                {
                    chips.Add(ConvertFromListxml(chip));
                }
                machine[Models.Internal.Machine.ChipKey] = chips.ToArray();
            }

            if (item.Display != null && item.Display.Any())
            {
                var displays = new List<Models.Internal.Display>();
                foreach (var display in item.Display)
                {
                    displays.Add(ConvertFromListxml(display));
                }
                machine[Models.Internal.Machine.DisplayKey] = displays.ToArray();
            }

            if (item.Video != null && item.Video.Any())
            {
                var videos = new List<Models.Internal.Video>();
                foreach (var video in item.Video)
                {
                    videos.Add(ConvertFromListxml(video));
                }
                machine[Models.Internal.Machine.VideoKey] = videos.ToArray();
            }

            if (item.Sound != null)
                machine[Models.Internal.Machine.SoundKey] = ConvertFromListxml(item.Sound);

            if (item.Input != null)
                machine[Models.Internal.Machine.InputKey] = ConvertFromListxml(item.Input);

            if (item.DipSwitch != null && item.DipSwitch.Any())
            {
                var dipSwitches = new List<Models.Internal.DipSwitch>();
                foreach (var dipSwitch in item.DipSwitch)
                {
                    dipSwitches.Add(ConvertFromListxml(dipSwitch));
                }
                machine[Models.Internal.Machine.DipSwitchKey] = dipSwitches.ToArray();
            }

            if (item.Configuration != null && item.Configuration.Any())
            {
                var configurations = new List<Models.Internal.Configuration>();
                foreach (var configuration in item.Configuration)
                {
                    configurations.Add(ConvertFromListxml(configuration));
                }
                machine[Models.Internal.Machine.ConfigurationKey] = configurations.ToArray();
            }

            if (item.Port != null && item.Port.Any())
            {
                var ports = new List<Models.Internal.Port>();
                foreach (var port in item.Port)
                {
                    ports.Add(ConvertFromListxml(port));
                }
                machine[Models.Internal.Machine.PortKey] = ports.ToArray();
            }

            if (item.Adjuster != null && item.Adjuster.Any())
            {
                var adjusters = new List<Models.Internal.Adjuster>();
                foreach (var adjuster in item.Adjuster)
                {
                    adjusters.Add(ConvertFromListxml(adjuster));
                }
                machine[Models.Internal.Machine.AdjusterKey] = adjusters.ToArray();
            }

            if (item.Driver != null)
                machine[Models.Internal.Machine.DriverKey] = ConvertFromListxml(item.Driver);

            if (item.Feature != null && item.Feature.Any())
            {
                var features = new List<Models.Internal.Feature>();
                foreach (var feature in item.Feature)
                {
                    features.Add(ConvertFromListxml(feature));
                }
                machine[Models.Internal.Machine.FeatureKey] = features.ToArray();
            }

            if (item.Device != null && item.Device.Any())
            {
                var devices = new List<Models.Internal.Device>();
                foreach (var device in item.Device)
                {
                    devices.Add(ConvertFromListxml(device));
                }
                machine[Models.Internal.Machine.DeviceKey] = devices.ToArray();
            }

            if (item.Slot != null && item.Slot.Any())
            {
                var slots = new List<Models.Internal.Slot>();
                foreach (var slot in item.Slot)
                {
                    slots.Add(ConvertFromListxml(slot));
                }
                machine[Models.Internal.Machine.SlotKey] = slots.ToArray();
            }

            if (item.SoftwareList != null && item.SoftwareList.Any())
            {
                var softwareLists = new List<Models.Internal.SoftwareList>();
                foreach (var softwareList in item.SoftwareList)
                {
                    softwareLists.Add(ConvertFromListxml(softwareList));
                }
                machine[Models.Internal.Machine.SoftwareListKey] = softwareLists.ToArray();
            }

            if (item.RamOption != null && item.RamOption.Any())
            {
                var ramOptions = new List<Models.Internal.RamOption>();
                foreach (var ramOption in item.RamOption)
                {
                    ramOptions.Add(ConvertFromListxml(ramOption));
                }
                machine[Models.Internal.Machine.RamOptionKey] = ramOptions.ToArray();
            }

            return machine;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Adjuster"/> to <cref="Models.Internal.Adjuster"/>
        /// </summary>
        public static Models.Internal.Adjuster ConvertFromListxml(Models.Listxml.Adjuster item)
        {
            var adjuster = new Models.Internal.Adjuster
            {
                [Models.Internal.Adjuster.NameKey] = item.Name,
                [Models.Internal.Adjuster.DefaultKey] = item.Default,
            };

            if (item.Condition != null)
                adjuster[Models.Internal.Adjuster.ConditionKey] = ConvertFromListxml(item.Condition);

            return adjuster;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Analog"/> to <cref="Models.Internal.Analog"/>
        /// </summary>
        public static Models.Internal.Analog ConvertFromListxml(Models.Listxml.Analog item)
        {
            var analog = new Models.Internal.Analog
            {
                [Models.Internal.Analog.MaskKey] = item.Mask,
            };
            return analog;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.BiosSet"/> to <cref="Models.Internal.BiosSet"/>
        /// </summary>
        public static Models.Internal.BiosSet ConvertFromListxml(Models.Listxml.BiosSet item)
        {
            var biosset = new Models.Internal.BiosSet
            {
                [Models.Internal.BiosSet.NameKey] = item.Name,
                [Models.Internal.BiosSet.DescriptionKey] = item.Description,
                [Models.Internal.BiosSet.DefaultKey] = item.Default,
            };
            return biosset;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Chip"/> to <cref="Models.Internal.Chip"/>
        /// </summary>
        public static Models.Internal.Chip ConvertFromListxml(Models.Listxml.Chip item)
        {
            var chip = new Models.Internal.Chip
            {
                [Models.Internal.Chip.NameKey] = item.Name,
                [Models.Internal.Chip.TagKey] = item.Tag,
                [Models.Internal.Chip.TypeKey] = item.Type,
                [Models.Internal.Chip.SoundOnlyKey] = item.SoundOnly,
                [Models.Internal.Chip.ClockKey] = item.Clock,
            };
            return chip;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Condition"/> to <cref="Models.Internal.Condition"/>
        /// </summary>
        public static Models.Internal.Condition ConvertFromListxml(Models.Listxml.Condition item)
        {
            var condition = new Models.Internal.Condition
            {
                [Models.Internal.Condition.TagKey] = item.Tag,
                [Models.Internal.Condition.MaskKey] = item.Mask,
                [Models.Internal.Condition.RelationKey] = item.Relation,
                [Models.Internal.Condition.ValueKey] = item.Value,
            };
            return condition;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Configuration"/> to <cref="Models.Internal.Configuration"/>
        /// </summary>
        public static Models.Internal.Configuration ConvertFromListxml(Models.Listxml.Configuration item)
        {
            var configuration = new Models.Internal.Configuration
            {
                [Models.Internal.Configuration.NameKey] = item.Name,
                [Models.Internal.Configuration.TagKey] = item.Tag,
                [Models.Internal.Configuration.MaskKey] = item.Mask,
            };

            if (item.Condition != null)
                configuration[Models.Internal.Configuration.ConditionKey] = ConvertFromListxml(item.Condition);

            if (item.ConfLocation != null && item.ConfLocation.Any())
            {
                var confLocations = new List<Models.Internal.ConfLocation>();
                foreach (var confLocation in item.ConfLocation)
                {
                    confLocations.Add(ConvertFromListxml(confLocation));
                }
                configuration[Models.Internal.Configuration.ConfLocationKey] = confLocations.ToArray();
            }

            if (item.ConfSetting != null && item.ConfSetting.Any())
            {
                var confSettings = new List<Models.Internal.ConfSetting>();
                foreach (var confSetting in item.ConfSetting)
                {
                    confSettings.Add(ConvertFromListxml(confSetting));
                }
                configuration[Models.Internal.Configuration.ConfSettingKey] = confSettings.ToArray();
            }

            return configuration;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.ConfLocation"/> to <cref="Models.Internal.ConfLocation"/>
        /// </summary>
        public static Models.Internal.ConfLocation ConvertFromListxml(Models.Listxml.ConfLocation item)
        {
            var confLocation = new Models.Internal.ConfLocation
            {
                [Models.Internal.ConfLocation.NameKey] = item.Name,
                [Models.Internal.ConfLocation.NumberKey] = item.Number,
                [Models.Internal.ConfLocation.InvertedKey] = item.Inverted,
            };
            return confLocation;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.ConfSetting"/> to <cref="Models.Internal.ConfSetting"/>
        /// </summary>
        public static Models.Internal.ConfSetting ConvertFromListxml(Models.Listxml.ConfSetting item)
        {
            var confSetting = new Models.Internal.ConfSetting
            {
                [Models.Internal.ConfSetting.NameKey] = item.Name,
                [Models.Internal.ConfSetting.ValueKey] = item.Value,
                [Models.Internal.ConfSetting.DefaultKey] = item.Default,
            };

            if (item.Condition != null)
                confSetting[Models.Internal.ConfSetting.ConditionKey] = ConvertFromListxml(item.Condition);

            return confSetting;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Control"/> to <cref="Models.Internal.Control"/>
        /// </summary>
        public static Models.Internal.Control ConvertFromListxml(Models.Listxml.Control item)
        {
            var control = new Models.Internal.Control
            {
                [Models.Internal.Control.TypeKey] = item.Type,
                [Models.Internal.Control.PlayerKey] = item.Player,
                [Models.Internal.Control.ButtonsKey] = item.Buttons,
                [Models.Internal.Control.ReqButtonsKey] = item.ReqButtons,
                [Models.Internal.Control.MinimumKey] = item.Minimum,
                [Models.Internal.Control.MaximumKey] = item.Maximum,
                [Models.Internal.Control.SensitivityKey] = item.Sensitivity,
                [Models.Internal.Control.KeyDeltaKey] = item.KeyDelta,
                [Models.Internal.Control.ReverseKey] = item.Reverse,
                [Models.Internal.Control.WaysKey] = item.Ways,
                [Models.Internal.Control.Ways2Key] = item.Ways2,
                [Models.Internal.Control.Ways3Key] = item.Ways3,
            };
            return control;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Device"/> to <cref="Models.Internal.Device"/>
        /// </summary>
        public static Models.Internal.Device ConvertFromListxml(Models.Listxml.Device item)
        {
            var device = new Models.Internal.Device
            {
                [Models.Internal.Device.TypeKey] = item.Type,
                [Models.Internal.Device.TagKey] = item.Tag,
                [Models.Internal.Device.FixedImageKey] = item.FixedImage,
                [Models.Internal.Device.MandatoryKey] = item.Mandatory,
                [Models.Internal.Device.InterfaceKey] = item.Interface,
            };

            if (item.Instance != null)
                device[Models.Internal.Device.InstanceKey] = ConvertFromListxml(item.Instance);

            if (item.Extension != null && item.Extension.Any())
            {
                var extensions = new List<Models.Internal.Extension>();
                foreach (var extension in item.Extension)
                {
                    extensions.Add(ConvertFromListxml(extension));
                }
                device[Models.Internal.Device.ExtensionKey] = extensions.ToArray();
            }

            return device;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.DeviceRef"/> to <cref="Models.Internal.DeviceRef"/>
        /// </summary>
        public static Models.Internal.DeviceRef ConvertFromListxml(Models.Listxml.DeviceRef item)
        {
            var deviceRef = new Models.Internal.DeviceRef
            {
                [Models.Internal.DeviceRef.NameKey] = item.Name,
            };
            return deviceRef;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.DipLocation"/> to <cref="Models.Internal.DipLocation"/>
        /// </summary>
        public static Models.Internal.DipLocation ConvertFromListxml(Models.Listxml.DipLocation item)
        {
            var dipLocation = new Models.Internal.DipLocation
            {
                [Models.Internal.DipLocation.NameKey] = item.Name,
                [Models.Internal.DipLocation.NumberKey] = item.Number,
                [Models.Internal.DipLocation.InvertedKey] = item.Inverted,
            };
            return dipLocation;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.DipSwitch"/> to <cref="Models.Internal.DipSwitch"/>
        /// </summary>
        public static Models.Internal.DipSwitch ConvertFromListxml(Models.Listxml.DipSwitch item)
        {
            var dipSwitch = new Models.Internal.DipSwitch
            {
                [Models.Internal.DipSwitch.NameKey] = item.Name,
                [Models.Internal.DipSwitch.TagKey] = item.Tag,
                [Models.Internal.DipSwitch.MaskKey] = item.Mask,
            };

            if (item.Condition != null)
                dipSwitch[Models.Internal.DipSwitch.ConditionKey] = ConvertFromListxml(item.Condition);

            if (item.DipLocation != null && item.DipLocation.Any())
            {
                var dipLocations = new List<Models.Internal.DipLocation>();
                foreach (var dipLocation in item.DipLocation)
                {
                    dipLocations.Add(ConvertFromListxml(dipLocation));
                }
                dipSwitch[Models.Internal.DipSwitch.DipLocationKey] = dipLocations.ToArray();
            }

            if (item.DipValue != null && item.DipValue.Any())
            {
                var dipValues = new List<Models.Internal.DipValue>();
                foreach (var dipValue in item.DipValue)
                {
                    dipValues.Add(ConvertFromListxml(dipValue));
                }
                dipSwitch[Models.Internal.DipSwitch.DipValueKey] = dipValues.ToArray();
            }

            return dipSwitch;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.DipValue"/> to <cref="Models.Internal.DipValue"/>
        /// </summary>
        public static Models.Internal.DipValue ConvertFromListxml(Models.Listxml.DipValue item)
        {
            var dipValue = new Models.Internal.DipValue
            {
                [Models.Internal.DipValue.NameKey] = item.Name,
                [Models.Internal.DipValue.ValueKey] = item.Value,
                [Models.Internal.DipValue.DefaultKey] = item.Default,
            };

            if (item.Condition != null)
                dipValue[Models.Internal.DipValue.ConditionKey] = ConvertFromListxml(item.Condition);

            return dipValue;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Disk"/> to <cref="Models.Internal.Disk"/>
        /// </summary>
        public static Models.Internal.Disk ConvertFromListxml(Models.Listxml.Disk item)
        {
            var disk = new Models.Internal.Disk
            {
                [Models.Internal.Disk.NameKey] = item.Name,
                [Models.Internal.Disk.MD5Key] = item.MD5,
                [Models.Internal.Disk.SHA1Key] = item.SHA1,
                [Models.Internal.Disk.MergeKey] = item.Merge,
                [Models.Internal.Disk.RegionKey] = item.Region,
                [Models.Internal.Disk.IndexKey] = item.Index,
                [Models.Internal.Disk.WritableKey] = item.Writable,
                [Models.Internal.Disk.StatusKey] = item.Status,
                [Models.Internal.Disk.OptionalKey] = item.Optional,
            };
            return disk;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Display"/> to <cref="Models.Internal.Display"/>
        /// </summary>
        public static Models.Internal.Display ConvertFromListxml(Models.Listxml.Display item)
        {
            var display = new Models.Internal.Display
            {
                [Models.Internal.Display.TagKey] = item.Tag,
                [Models.Internal.Display.TypeKey] = item.Type,
                [Models.Internal.Display.RotateKey] = item.Rotate,
                [Models.Internal.Display.FlipXKey] = item.FlipX,
                [Models.Internal.Display.WidthKey] = item.Width,
                [Models.Internal.Display.HeightKey] = item.Height,
                [Models.Internal.Display.RefreshKey] = item.Refresh,
                [Models.Internal.Display.PixClockKey] = item.PixClock,
                [Models.Internal.Display.HTotalKey] = item.HTotal,
                [Models.Internal.Display.HBEndKey] = item.HBEnd,
                [Models.Internal.Display.HBStartKey] = item.HBStart,
                [Models.Internal.Display.VTotalKey] = item.VTotal,
                [Models.Internal.Display.VBEndKey] = item.VBEnd,
                [Models.Internal.Display.VBStartKey] = item.VBStart,
            };
            return display;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Driver"/> to <cref="Models.Internal.Driver"/>
        /// </summary>
        public static Models.Internal.Driver ConvertFromListxml(Models.Listxml.Driver item)
        {
            var driver = new Models.Internal.Driver
            {
                [Models.Internal.Driver.StatusKey] = item.Status,
                [Models.Internal.Driver.ColorKey] = item.Color,
                [Models.Internal.Driver.SoundKey] = item.Sound,
                [Models.Internal.Driver.PaletteSizeKey] = item.PaletteSize,
                [Models.Internal.Driver.EmulationKey] = item.Emulation,
                [Models.Internal.Driver.CocktailKey] = item.Cocktail,
                [Models.Internal.Driver.SaveStateKey] = item.SaveState,
                [Models.Internal.Driver.RequiresArtworkKey] = item.RequiresArtwork,
                [Models.Internal.Driver.UnofficialKey] = item.Unofficial,
                [Models.Internal.Driver.NoSoundHardwareKey] = item.NoSoundHardware,
                [Models.Internal.Driver.IncompleteKey] = item.Incomplete,
            };
            return driver;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Extension"/> to <cref="Models.Internal.Extension"/>
        /// </summary>
        public static Models.Internal.Extension ConvertFromListxml(Models.Listxml.Extension item)
        {
            var extension = new Models.Internal.Extension
            {
                [Models.Internal.Extension.NameKey] = item.Name,
            };
            return extension;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Feature"/> to <cref="Models.Internal.Feature"/>
        /// </summary>
        public static Models.Internal.Feature ConvertFromListxml(Models.Listxml.Feature item)
        {
            var feature = new Models.Internal.Feature
            {
                [Models.Internal.Feature.TypeKey] = item.Type,
                [Models.Internal.Feature.StatusKey] = item.Status,
                [Models.Internal.Feature.OverallKey] = item.Overall,
            };
            return feature;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Input"/> to <cref="Models.Internal.Input"/>
        /// </summary>
        public static Models.Internal.Input ConvertFromListxml(Models.Listxml.Input item)
        {
            var input = new Models.Internal.Input
            {
                [Models.Internal.Input.ServiceKey] = item.Service,
                [Models.Internal.Input.TiltKey] = item.Tilt,
                [Models.Internal.Input.PlayersKey] = item.Players,
                [Models.Internal.Input.ControlKey] = item.ControlAttr,
                [Models.Internal.Input.ButtonsKey] = item.Buttons,
                [Models.Internal.Input.CoinsKey] = item.Coins,
            };

            if (item.Control != null && item.Control.Any())
            {
                var controls = new List<Models.Internal.Control>();
                foreach (var control in item.Control)
                {
                    controls.Add(ConvertFromListxml(control));
                }
                input[Models.Internal.Input.ControlKey] = controls.ToArray();
            }

            return input;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Instance"/> to <cref="Models.Internal.Instance"/>
        /// </summary>
        public static Models.Internal.Instance ConvertFromListxml(Models.Listxml.Instance item)
        {
            var instance = new Models.Internal.Instance
            {
                [Models.Internal.Instance.NameKey] = item.Name,
                [Models.Internal.Instance.BriefNameKey] = item.BriefName,
            };
            return instance;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Port"/> to <cref="Models.Internal.Port"/>
        /// </summary>
        public static Models.Internal.Port ConvertFromListxml(Models.Listxml.Port item)
        {
            var port = new Models.Internal.Port
            {
                [Models.Internal.Port.TagKey] = item.Tag,
            };

            if (item.Analog != null && item.Analog.Any())
            {
                var analogs = new List<Models.Internal.Analog>();
                foreach (var analog in item.Analog)
                {
                    analogs.Add(ConvertFromListxml(analog));
                }
                port[Models.Internal.Port.AnalogKey] = analogs.ToArray();
            }

            return port;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.RamOption"/> to <cref="Models.Internal.RamOption"/>
        /// </summary>
        public static Models.Internal.RamOption ConvertFromListxml(Models.Listxml.RamOption item)
        {
            var ramOption = new Models.Internal.RamOption
            {
                [Models.Internal.RamOption.NameKey] = item.Name,
                [Models.Internal.RamOption.DefaultKey] = item.Default,
            };
            return ramOption;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Rom"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromListxml(Models.Listxml.Rom item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.NameKey] = item.Name,
                [Models.Internal.Rom.BiosKey] = item.Bios,
                [Models.Internal.Rom.SizeKey] = item.Size,
                [Models.Internal.Rom.CRCKey] = item.CRC,
                [Models.Internal.Rom.SHA1Key] = item.SHA1,
                [Models.Internal.Rom.MergeKey] = item.Merge,
                [Models.Internal.Rom.RegionKey] = item.Region,
                [Models.Internal.Rom.OffsetKey] = item.Offset,
                [Models.Internal.Rom.StatusKey] = item.Status,
                [Models.Internal.Rom.OptionalKey] = item.Optional,
                [Models.Internal.Rom.DisposeKey] = item.Dispose,
                [Models.Internal.Rom.SoundOnlyKey] = item.SoundOnly,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Sample"/> to <cref="Models.Internal.Sample"/>
        /// </summary>
        public static Models.Internal.Sample ConvertFromListxml(Models.Listxml.Sample item)
        {
            var sample = new Models.Internal.Sample
            {
                [Models.Internal.Sample.NameKey] = item.Name,
            };
            return sample;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Slot"/> to <cref="Models.Internal.Slot"/>
        /// </summary>
        public static Models.Internal.Slot ConvertFromListxml(Models.Listxml.Slot item)
        {
            var slot = new Models.Internal.Slot
            {
                [Models.Internal.Slot.NameKey] = item.Name,
            };

            if (item.SlotOption != null && item.SlotOption.Any())
            {
                var slotOptions = new List<Models.Internal.SlotOption>();
                foreach (var slotOption in item.SlotOption)
                {
                    slotOptions.Add(ConvertFromListxml(slotOption));
                }
                slot[Models.Internal.Slot.SlotOptionKey] = slotOptions.ToArray();
            }

            return slot;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.SlotOption"/> to <cref="Models.Internal.SlotOption"/>
        /// </summary>
        public static Models.Internal.SlotOption ConvertFromListxml(Models.Listxml.SlotOption item)
        {
            var slotOption = new Models.Internal.SlotOption
            {
                [Models.Internal.SlotOption.NameKey] = item.Name,
                [Models.Internal.SlotOption.DevNameKey] = item.DevName,
                [Models.Internal.SlotOption.DefaultKey] = item.Default,
            };
            return slotOption;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.SoftwareList"/> to <cref="Models.Internal.SoftwareList"/>
        /// </summary>
        public static Models.Internal.SoftwareList ConvertFromListxml(Models.Listxml.SoftwareList item)
        {
            var softwareList = new Models.Internal.SoftwareList
            {
                [Models.Internal.SoftwareList.TagKey] = item.Tag,
                [Models.Internal.SoftwareList.NameKey] = item.Name,
                [Models.Internal.SoftwareList.StatusKey] = item.Status,
                [Models.Internal.SoftwareList.FilterKey] = item.Filter,
            };
            return softwareList;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Sound"/> to <cref="Models.Internal.Sound"/>
        /// </summary>
        public static Models.Internal.Sound ConvertFromListxml(Models.Listxml.Sound item)
        {
            var sound = new Models.Internal.Sound
            {
                [Models.Internal.Sound.ChannelsKey] = item.Channels,
            };
            return sound;
        }

        /// <summary>
        /// Convert from <cref="Models.Listxml.Video"/> to <cref="Models.Internal.Video"/>
        /// </summary>
        public static Models.Internal.Video ConvertFromListxml(Models.Listxml.Video item)
        {
            var video = new Models.Internal.Video
            {
                [Models.Internal.Video.ScreenKey] = item.Screen,
                [Models.Internal.Video.OrientationKey] = item.Orientation,
                [Models.Internal.Video.WidthKey] = item.Width,
                [Models.Internal.Video.HeightKey] = item.Height,
                [Models.Internal.Video.AspectXKey] = item.AspectX,
                [Models.Internal.Video.AspectYKey] = item.AspectY,
                [Models.Internal.Video.RefreshKey] = item.Refresh,
            };
            return video;
        }

        #endregion

        #region Deserialize

        /// <summary>
        /// Convert from <cref="Models.Internal.Machine"/> to <cref="Models.Listxml.GameBase"/>
        /// </summary>
        public static Models.Listxml.GameBase ConvertMachineToListxml(Models.Internal.Machine item)
        {
            var machine = new Models.Listxml.Machine
            {
                Name = item.ReadString(Models.Internal.Machine.NameKey),
                SourceFile = item.ReadString(Models.Internal.Machine.SourceFileKey),
                IsBios = item.ReadString(Models.Internal.Machine.IsBiosKey),
                IsDevice = item.ReadString(Models.Internal.Machine.IsDeviceKey),
                IsMechanical = item.ReadString(Models.Internal.Machine.IsMechanicalKey),
                Runnable = item.ReadString(Models.Internal.Machine.RunnableKey),
                CloneOf = item.ReadString(Models.Internal.Machine.CloneOfKey),
                RomOf = item.ReadString(Models.Internal.Machine.RomOfKey),
                SampleOf = item.ReadString(Models.Internal.Machine.SampleOfKey),
                Description = item.ReadString(Models.Internal.Machine.DescriptionKey),
                Year = item.ReadString(Models.Internal.Machine.YearKey),
                Manufacturer = item.ReadString(Models.Internal.Machine.ManufacturerKey),
                History = item.ReadString(Models.Internal.Machine.HistoryKey),
            };

            if (item.ContainsKey(Models.Internal.Machine.BiosSetKey) && item[Models.Internal.Machine.BiosSetKey] is Models.Internal.BiosSet[] biosSets)
            {
                var biosSetItems = new List<Models.Listxml.BiosSet>();
                foreach (var biosSet in biosSets)
                {
                    biosSetItems.Add(ConvertToListxml(biosSet));
                }
                machine.BiosSet = biosSetItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.RomKey) && item[Models.Internal.Machine.RomKey] is Models.Internal.Rom[] roms)
            {
                var romItems = new List<Models.Listxml.Rom>();
                foreach (var rom in roms)
                {
                    romItems.Add(ConvertToListxml(rom));
                }
                machine.Rom = romItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.DiskKey) && item[Models.Internal.Machine.DiskKey] is Models.Internal.Disk[] disks)
            {
                var diskItems = new List<Models.Listxml.Disk>();
                foreach (var disk in disks)
                {
                    diskItems.Add(ConvertToListxml(disk));
                }
                machine.Disk = diskItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.DeviceRefKey) && item[Models.Internal.Machine.DeviceRefKey] is Models.Internal.DeviceRef[] deviceRefs)
            {
                var deviceRefItems = new List<Models.Listxml.DeviceRef>();
                foreach (var deviceRef in deviceRefs)
                {
                    deviceRefItems.Add(ConvertToListxml(deviceRef));
                }
                machine.DeviceRef = deviceRefItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.SampleKey) && item[Models.Internal.Machine.SampleKey] is Models.Internal.Sample[] samples)
            {
                var sampleItems = new List<Models.Listxml.Sample>();
                foreach (var sample in samples)
                {
                    sampleItems.Add(ConvertToListxml(sample));
                }
                machine.Sample = sampleItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.ChipKey) && item[Models.Internal.Machine.ChipKey] is Models.Internal.Chip[] chips)
            {
                var chipItems = new List<Models.Listxml.Chip>();
                foreach (var chip in chips)
                {
                    chipItems.Add(ConvertToListxml(chip));
                }
                machine.Chip = chipItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.DisplayKey) && item[Models.Internal.Machine.DisplayKey] is Models.Internal.Display[] displays)
            {
                var displayItems = new List<Models.Listxml.Display>();
                foreach (var display in displays)
                {
                    displayItems.Add(ConvertToListxml(display));
                }
                machine.Display = displayItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.VideoKey) && item[Models.Internal.Machine.VideoKey] is Models.Internal.Video[] videos)
            {
                var videoItems = new List<Models.Listxml.Video>();
                foreach (var video in videos)
                {
                    videoItems.Add(ConvertToListxml(video));
                }
                machine.Video = videoItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.SoundKey) && item[Models.Internal.Machine.SoundKey] is Models.Internal.Sound sound)
                machine.Sound = ConvertToListxml(sound);

            if (item.ContainsKey(Models.Internal.Machine.InputKey) && item[Models.Internal.Machine.InputKey] is Models.Internal.Input input)
                machine.Input = ConvertToListxml(input);

            if (item.ContainsKey(Models.Internal.Machine.DipSwitchKey) && item[Models.Internal.Machine.DipSwitchKey] is Models.Internal.DipSwitch[] dipSwitches)
            {
                var dipSwitchItems = new List<Models.Listxml.DipSwitch>();
                foreach (var dipSwitch in dipSwitches)
                {
                    dipSwitchItems.Add(ConvertToListxml(dipSwitch));
                }
                machine.DipSwitch = dipSwitchItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.ConfigurationKey) && item[Models.Internal.Machine.ConfigurationKey] is Models.Internal.Configuration[] configurations)
            {
                var configurationItems = new List<Models.Listxml.Configuration>();
                foreach (var configuration in configurations)
                {
                    configurationItems.Add(ConvertToListxml(configuration));
                }
                machine.Configuration = configurationItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.PortKey) && item[Models.Internal.Machine.PortKey] is Models.Internal.Port[] ports)
            {
                var portItems = new List<Models.Listxml.Port>();
                foreach (var port in ports)
                {
                    portItems.Add(ConvertToListxml(port));
                }
                machine.Port = portItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.AdjusterKey) && item[Models.Internal.Machine.AdjusterKey] is Models.Internal.Adjuster[] adjusters)
            {
                var adjusterItems = new List<Models.Listxml.Adjuster>();
                foreach (var adjuster in adjusters)
                {
                    adjusterItems.Add(ConvertToListxml(adjuster));
                }
                machine.Adjuster = adjusterItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.DriverKey) && item[Models.Internal.Machine.DriverKey] is Models.Internal.Driver driver)
                machine.Driver = ConvertToListxml(driver);

            if (item.ContainsKey(Models.Internal.Machine.FeatureKey) && item[Models.Internal.Machine.FeatureKey] is Models.Internal.Feature[] features)
            {
                var featureItems = new List<Models.Listxml.Feature>();
                foreach (var feature in features)
                {
                    featureItems.Add(ConvertToListxml(feature));
                }
                machine.Feature = featureItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.DeviceKey) && item[Models.Internal.Machine.DeviceKey] is Models.Internal.Device[] devices)
            {
                var deviceItems = new List<Models.Listxml.Device>();
                foreach (var device in devices)
                {
                    deviceItems.Add(ConvertToListxml(device));
                }
                machine.Device = deviceItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.SlotKey) && item[Models.Internal.Machine.SlotKey] is Models.Internal.Slot[] slots)
            {
                var slotItems = new List<Models.Listxml.Slot>();
                foreach (var slot in slots)
                {
                    slotItems.Add(ConvertToListxml(slot));
                }
                machine.Slot = slotItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.SoftwareListKey) && item[Models.Internal.Machine.SoftwareListKey] is Models.Internal.SoftwareList[] softwareLists)
            {
                var softwareListItems = new List<Models.Listxml.SoftwareList>();
                foreach (var softwareList in softwareLists)
                {
                    softwareListItems.Add(ConvertToListxml(softwareList));
                }
                machine.SoftwareList = softwareListItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Machine.RamOptionKey) && item[Models.Internal.Machine.RamOptionKey] is Models.Internal.RamOption[] ramOptions)
            {
                var ramOptionItems = new List<Models.Listxml.RamOption>();
                foreach (var ramOption in ramOptions)
                {
                    ramOptionItems.Add(ConvertToListxml(ramOption));
                }
                machine.RamOption = ramOptionItems.ToArray();
            }

            return machine;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Adjuster"/> to <cref="Models.Listxml.Adjuster"/>
        /// </summary>
        public static Models.Listxml.Adjuster ConvertToListxml(Models.Internal.Adjuster item)
        {
            var adjuster = new Models.Listxml.Adjuster
            {
                Name = item.ReadString(Models.Internal.Adjuster.NameKey),
                Default = item.ReadString(Models.Internal.Adjuster.DefaultKey),
            };

            if (item.ContainsKey(Models.Internal.Adjuster.ConditionKey) && item[Models.Internal.Adjuster.ConditionKey] is Models.Internal.Condition condition)
                adjuster.Condition = ConvertToListxml(condition);

            return adjuster;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Analog"/> to <cref="Models.Listxml.Analog"/>
        /// </summary>
        public static Models.Listxml.Analog ConvertToListxml(Models.Internal.Analog item)
        {
            var analog = new Models.Listxml.Analog
            {
                Mask = item.ReadString(Models.Internal.Analog.MaskKey),
            };
            return analog;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.BiosSet"/> to <cref="Models.Listxml.BiosSet"/>
        /// </summary>
        public static Models.Listxml.BiosSet ConvertToListxml(Models.Internal.BiosSet item)
        {
            var biosset = new Models.Listxml.BiosSet
            {
                Name = item.ReadString(Models.Internal.BiosSet.NameKey),
                Description = item.ReadString(Models.Internal.BiosSet.DescriptionKey),
                Default = item.ReadString(Models.Internal.BiosSet.DefaultKey),
            };
            return biosset;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Chip"/> to <cref="Models.Listxml.Chip"/>
        /// </summary>
        public static Models.Listxml.Chip ConvertToListxml(Models.Internal.Chip item)
        {
            var chip = new Models.Listxml.Chip
            {
                Name = item.ReadString(Models.Internal.Chip.NameKey),
                Tag = item.ReadString(Models.Internal.Chip.TagKey),
                Type = item.ReadString(Models.Internal.Chip.TypeKey),
                SoundOnly = item.ReadString(Models.Internal.Chip.SoundOnlyKey),
                Clock = item.ReadString(Models.Internal.Chip.ClockKey),
            };
            return chip;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Condition"/> to <cref="Models.Listxml.Condition"/>
        /// </summary>
        public static Models.Listxml.Condition ConvertToListxml(Models.Internal.Condition item)
        {
            var condition = new Models.Listxml.Condition
            {
                Tag = item.ReadString(Models.Internal.Condition.TagKey),
                Mask = item.ReadString(Models.Internal.Condition.MaskKey),
                Relation = item.ReadString(Models.Internal.Condition.RelationKey),
                Value = item.ReadString(Models.Internal.Condition.ValueKey),
            };
            return condition;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Configuration"/> to <cref="Models.Listxml.Configuration"/>
        /// </summary>
        public static Models.Listxml.Configuration ConvertToListxml(Models.Internal.Configuration item)
        {
            var configuration = new Models.Listxml.Configuration
            {
                Name = item.ReadString(Models.Internal.Configuration.NameKey),
                Tag = item.ReadString(Models.Internal.Configuration.TagKey),
                Mask = item.ReadString(Models.Internal.Configuration.MaskKey),
            };

            if (item.ContainsKey(Models.Internal.Configuration.ConditionKey) && item[Models.Internal.Configuration.ConditionKey] is Models.Internal.Condition condition)
                configuration.Condition = ConvertToListxml(condition);

            if (item.ContainsKey(Models.Internal.Configuration.ConfLocationKey) && item[Models.Internal.Configuration.ConfLocationKey] is Models.Internal.ConfLocation[] confLocations)
            {
                var confLocationItems = new List<Models.Listxml.ConfLocation>();
                foreach (var confLocation in confLocations)
                {
                    confLocationItems.Add(ConvertToListxml(confLocation));
                }
                configuration.ConfLocation = confLocationItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.Configuration.ConfSettingKey) && item[Models.Internal.Configuration.ConfSettingKey] is Models.Internal.ConfSetting[] confSettings)
            {
                var confSettingItems = new List<Models.Listxml.ConfSetting>();
                foreach (var confSetting in confSettings)
                {
                    confSettingItems.Add(ConvertToListxml(confSetting));
                }
                configuration.ConfSetting = confSettingItems.ToArray();
            }

            return configuration;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.ConfLocation"/> to <cref="Models.Listxml.ConfLocation"/>
        /// </summary>
        public static Models.Listxml.ConfLocation ConvertToListxml(Models.Internal.ConfLocation item)
        {
            var confLocation = new Models.Listxml.ConfLocation
            {
                Name = item.ReadString(Models.Internal.ConfLocation.NameKey),
                Number = item.ReadString(Models.Internal.ConfLocation.NumberKey),
                Inverted = item.ReadString(Models.Internal.ConfLocation.InvertedKey),
            };
            return confLocation;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.ConfSetting"/> to <cref="Models.Listxml.ConfSetting"/>
        /// </summary>
        public static Models.Listxml.ConfSetting ConvertToListxml(Models.Internal.ConfSetting item)
        {
            var confSetting = new Models.Listxml.ConfSetting
            {
                Name = item.ReadString(Models.Internal.ConfSetting.NameKey),
                Value = item.ReadString(Models.Internal.ConfSetting.ValueKey),
                Default = item.ReadString(Models.Internal.ConfSetting.DefaultKey),
            };

            if (item.ContainsKey(Models.Internal.ConfSetting.ConditionKey) && item[Models.Internal.ConfSetting.ConditionKey] is Models.Internal.Condition condition)
                confSetting.Condition = ConvertToListxml(condition);

            return confSetting;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Control"/> to <cref="Models.Listxml.Control"/>
        /// </summary>
        public static Models.Listxml.Control ConvertToListxml(Models.Internal.Control item)
        {
            var control = new Models.Listxml.Control
            {
                Type = item.ReadString(Models.Internal.Control.TypeKey),
                Player = item.ReadString(Models.Internal.Control.PlayerKey),
                Buttons = item.ReadString(Models.Internal.Control.ButtonsKey),
                ReqButtons = item.ReadString(Models.Internal.Control.ReqButtonsKey),
                Minimum = item.ReadString(Models.Internal.Control.MinimumKey),
                Maximum = item.ReadString(Models.Internal.Control.MaximumKey),
                Sensitivity = item.ReadString(Models.Internal.Control.SensitivityKey),
                KeyDelta = item.ReadString(Models.Internal.Control.KeyDeltaKey),
                Reverse = item.ReadString(Models.Internal.Control.ReverseKey),
                Ways = item.ReadString(Models.Internal.Control.WaysKey),
                Ways2 = item.ReadString(Models.Internal.Control.Ways2Key),
                Ways3 = item.ReadString(Models.Internal.Control.Ways3Key),
            };
            return control;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Device"/> to <cref="Models.Listxml.Device"/>
        /// </summary>
        public static Models.Listxml.Device ConvertToListxml(Models.Internal.Device item)
        {
            var device = new Models.Listxml.Device
            {
                Type = item.ReadString(Models.Internal.Device.TypeKey),
                Tag = item.ReadString(Models.Internal.Device.TagKey),
                FixedImage = item.ReadString(Models.Internal.Device.FixedImageKey),
                Mandatory = item.ReadString(Models.Internal.Device.MandatoryKey),
                Interface = item.ReadString(Models.Internal.Device.InterfaceKey),
            };

            if (item.ContainsKey(Models.Internal.Device.InstanceKey) && item[Models.Internal.Device.InstanceKey] is Models.Internal.Instance instance)
                device.Instance = ConvertToListxml(instance);

            if (item.ContainsKey(Models.Internal.Device.ExtensionKey) && item[Models.Internal.Device.ExtensionKey] is Models.Internal.Extension[] extensions)
            {
                var extensionItems = new List<Models.Listxml.Extension>();
                foreach (var extension in extensions)
                {
                    extensionItems.Add(ConvertToListxml(extension));
                }
                device.Extension = extensionItems.ToArray();
            }

            return device;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.DeviceRef"/> to <cref="Models.Listxml.DeviceRef"/>
        /// </summary>
        public static Models.Listxml.DeviceRef ConvertToListxml(Models.Internal.DeviceRef item)
        {
            var deviceRef = new Models.Listxml.DeviceRef
            {
                Name = item.ReadString(Models.Internal.DeviceRef.NameKey),
            };
            return deviceRef;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.DipLocation"/> to <cref="Models.Listxml.DipLocation"/>
        /// </summary>
        public static Models.Listxml.DipLocation ConvertToListxml(Models.Internal.DipLocation item)
        {
            var dipLocation = new Models.Listxml.DipLocation
            {
                Name = item.ReadString(Models.Internal.DipLocation.NameKey),
                Number = item.ReadString(Models.Internal.DipLocation.NumberKey),
                Inverted = item.ReadString(Models.Internal.DipLocation.InvertedKey),
            };
            return dipLocation;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.DipSwitch"/> to <cref="Models.Listxml.DipSwitch"/>
        /// </summary>
        public static Models.Listxml.DipSwitch ConvertToListxml(Models.Internal.DipSwitch item)
        {
            var dipSwitch = new Models.Listxml.DipSwitch
            {
                Name = item.ReadString(Models.Internal.DipSwitch.NameKey),
                Tag = item.ReadString(Models.Internal.DipSwitch.TagKey),
                Mask = item.ReadString(Models.Internal.DipSwitch.MaskKey),
            };

            if (item.ContainsKey(Models.Internal.DipSwitch.ConditionKey) && item[Models.Internal.DipSwitch.ConditionKey] is Models.Internal.Condition condition)
                dipSwitch.Condition = ConvertToListxml(condition);

            if (item.ContainsKey(Models.Internal.DipSwitch.DipLocationKey) && item[Models.Internal.DipSwitch.DipLocationKey] is Models.Internal.DipLocation[] dipLocations)
            {
                var dipLocationItems = new List<Models.Listxml.DipLocation>();
                foreach (var dipLocation in dipLocations)
                {
                    dipLocationItems.Add(ConvertToListxml(dipLocation));
                }
                dipSwitch.DipLocation = dipLocationItems.ToArray();
            }

            if (item.ContainsKey(Models.Internal.DipSwitch.DipValueKey) && item[Models.Internal.DipSwitch.DipValueKey] is Models.Internal.DipValue[] dipValues)
            {
                var dipValueItems = new List<Models.Listxml.DipValue>();
                foreach (var dipValue in dipValues)
                {
                    dipValueItems.Add(ConvertToListxml(dipValue));
                }
                dipSwitch.DipValue = dipValueItems.ToArray();
            }

            return dipSwitch;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.DipValue"/> to <cref="Models.Listxml.DipValue"/>
        /// </summary>
        public static Models.Listxml.DipValue ConvertToListxml(Models.Internal.DipValue item)
        {
            var dipValue = new Models.Listxml.DipValue
            {
                Name = item.ReadString(Models.Internal.DipValue.NameKey),
                Value = item.ReadString(Models.Internal.DipValue.ValueKey),
                Default = item.ReadString(Models.Internal.DipValue.DefaultKey),
            };

            if (item.ContainsKey(Models.Internal.DipValue.ConditionKey) && item[Models.Internal.DipValue.ConditionKey] is Models.Internal.Condition condition)
                dipValue.Condition = ConvertToListxml(condition);

            return dipValue;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Disk"/> to <cref="Models.Listxml.Disk"/>
        /// </summary>
        public static Models.Listxml.Disk ConvertToListxml(Models.Internal.Disk item)
        {
            var disk = new Models.Listxml.Disk
            {
                Name = item.ReadString(Models.Internal.Disk.NameKey),
                MD5 = item.ReadString(Models.Internal.Disk.MD5Key),
                SHA1 = item.ReadString(Models.Internal.Disk.SHA1Key),
                Merge = item.ReadString(Models.Internal.Disk.MergeKey),
                Region = item.ReadString(Models.Internal.Disk.RegionKey),
                Index = item.ReadString(Models.Internal.Disk.IndexKey),
                Writable = item.ReadString(Models.Internal.Disk.WritableKey),
                Status = item.ReadString(Models.Internal.Disk.StatusKey),
                Optional = item.ReadString(Models.Internal.Disk.OptionalKey),
            };
            return disk;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Display"/> to <cref="Models.Listxml.Display"/>
        /// </summary>
        public static Models.Listxml.Display ConvertToListxml(Models.Internal.Display item)
        {
            var display = new Models.Listxml.Display
            {
                Tag = item.ReadString(Models.Internal.Display.TagKey),
                Type = item.ReadString(Models.Internal.Display.TypeKey),
                Rotate = item.ReadString(Models.Internal.Display.RotateKey),
                FlipX = item.ReadString(Models.Internal.Display.FlipXKey),
                Width = item.ReadString(Models.Internal.Display.WidthKey),
                Height = item.ReadString(Models.Internal.Display.HeightKey),
                Refresh = item.ReadString(Models.Internal.Display.RefreshKey),
                PixClock = item.ReadString(Models.Internal.Display.PixClockKey),
                HTotal = item.ReadString(Models.Internal.Display.HTotalKey),
                HBEnd = item.ReadString(Models.Internal.Display.HBEndKey),
                HBStart = item.ReadString(Models.Internal.Display.HBStartKey),
                VTotal = item.ReadString(Models.Internal.Display.VTotalKey),
                VBEnd = item.ReadString(Models.Internal.Display.VBEndKey),
                VBStart = item.ReadString(Models.Internal.Display.VBStartKey),
            };
            return display;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Driver"/> to <cref="Models.Listxml.Driver"/>
        /// </summary>
        public static Models.Listxml.Driver ConvertToListxml(Models.Internal.Driver item)
        {
            var driver = new Models.Listxml.Driver
            {
                Status = item.ReadString(Models.Internal.Driver.StatusKey),
                Color = item.ReadString(Models.Internal.Driver.ColorKey),
                Sound = item.ReadString(Models.Internal.Driver.SoundKey),
                PaletteSize = item.ReadString(Models.Internal.Driver.PaletteSizeKey),
                Emulation = item.ReadString(Models.Internal.Driver.EmulationKey),
                Cocktail = item.ReadString(Models.Internal.Driver.CocktailKey),
                SaveState = item.ReadString(Models.Internal.Driver.SaveStateKey),
                RequiresArtwork = item.ReadString(Models.Internal.Driver.RequiresArtworkKey),
                Unofficial = item.ReadString(Models.Internal.Driver.UnofficialKey),
                NoSoundHardware = item.ReadString(Models.Internal.Driver.NoSoundHardwareKey),
                Incomplete = item.ReadString(Models.Internal.Driver.IncompleteKey),
            };
            return driver;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Extension"/> to <cref="Models.Listxml.Extension"/>
        /// </summary>
        public static Models.Listxml.Extension ConvertToListxml(Models.Internal.Extension item)
        {
            var extension = new Models.Listxml.Extension
            {
                Name = item.ReadString(Models.Internal.Extension.NameKey),
            };
            return extension;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Feature"/> to <cref="Models.Listxml.Feature"/>
        /// </summary>
        public static Models.Listxml.Feature ConvertToListxml(Models.Internal.Feature item)
        {
            var feature = new Models.Listxml.Feature
            {
                Type = item.ReadString(Models.Internal.Feature.TypeKey),
                Status = item.ReadString(Models.Internal.Feature.StatusKey),
                Overall = item.ReadString(Models.Internal.Feature.OverallKey),
            };
            return feature;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Input"/> to <cref="Models.Listxml.Input"/>
        /// </summary>
        public static Models.Listxml.Input ConvertToListxml(Models.Internal.Input item)
        {
            var input = new Models.Listxml.Input
            {
                Service = item.ReadString(Models.Internal.Input.ServiceKey),
                Tilt = item.ReadString(Models.Internal.Input.TiltKey),
                Players = item.ReadString(Models.Internal.Input.PlayersKey),
                ControlAttr = item.ReadString(Models.Internal.Input.ControlKey),
                Buttons = item.ReadString(Models.Internal.Input.ButtonsKey),
                Coins = item.ReadString(Models.Internal.Input.CoinsKey),
            };

            if (item.ContainsKey(Models.Internal.Input.ControlKey) && item[Models.Internal.Input.ControlKey] is Models.Internal.Control[] controls)
            {
                var controlItems = new List<Models.Listxml.Control>();
                foreach (var control in controls)
                {
                    controlItems.Add(ConvertToListxml(control));
                }
                input.Control = controlItems.ToArray();
            }

            return input;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Instance"/> to <cref="Models.Listxml.Instance"/>
        /// </summary>
        public static Models.Listxml.Instance ConvertToListxml(Models.Internal.Instance item)
        {
            var instance = new Models.Listxml.Instance
            {
                Name = item.ReadString(Models.Internal.Instance.NameKey),
                BriefName = item.ReadString(Models.Internal.Instance.BriefNameKey),
            };
            return instance;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Port"/> to <cref="Models.Listxml.Port"/>
        /// </summary>
        public static Models.Listxml.Port ConvertToListxml(Models.Internal.Port item)
        {
            var input = new Models.Listxml.Port
            {
                Tag = item.ReadString(Models.Internal.Port.TagKey),
            };

            if (item.ContainsKey(Models.Internal.Port.AnalogKey) && item[Models.Internal.Port.AnalogKey] is Models.Internal.Analog[] analogs)
            {
                var analogItems = new List<Models.Listxml.Analog>();
                foreach (var analog in analogs)
                {
                    analogItems.Add(ConvertToListxml(analog));
                }
                input.Analog = analogItems.ToArray();
            }

            return input;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.RamOption"/> to <cref="Models.Listxml.RamOption"/>
        /// </summary>
        public static Models.Listxml.RamOption ConvertToListxml(Models.Internal.RamOption item)
        {
            var ramOption = new Models.Listxml.RamOption
            {
                Name = item.ReadString(Models.Internal.RamOption.NameKey),
                Default = item.ReadString(Models.Internal.RamOption.DefaultKey),
            };
            return ramOption;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Listxml.Rom"/>
        /// </summary>
        public static Models.Listxml.Rom ConvertToListxml(Models.Internal.Rom item)
        {
            var rom = new Models.Listxml.Rom
            {
                Name = item.ReadString(Models.Internal.Rom.NameKey),
                Bios = item.ReadString(Models.Internal.Rom.BiosKey),
                Size = item.ReadString(Models.Internal.Rom.SizeKey),
                CRC = item.ReadString(Models.Internal.Rom.CRCKey),
                SHA1 = item.ReadString(Models.Internal.Rom.SHA1Key),
                Merge = item.ReadString(Models.Internal.Rom.MergeKey),
                Region = item.ReadString(Models.Internal.Rom.RegionKey),
                Offset = item.ReadString(Models.Internal.Rom.OffsetKey),
                Status = item.ReadString(Models.Internal.Rom.StatusKey),
                Optional = item.ReadString(Models.Internal.Rom.OptionalKey),
                Dispose = item.ReadString(Models.Internal.Rom.DisposeKey),
                SoundOnly = item.ReadString(Models.Internal.Rom.SoundOnlyKey),
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Sample"/> to <cref="Models.Listxml.Sample"/>
        /// </summary>
        public static Models.Listxml.Sample ConvertToListxml(Models.Internal.Sample item)
        {
            var sample = new Models.Listxml.Sample
            {
                Name = item.ReadString(Models.Internal.Sample.NameKey),
            };
            return sample;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Slot"/> to <cref="Models.Listxml.Slot"/>
        /// </summary>
        public static Models.Listxml.Slot ConvertToListxml(Models.Internal.Slot item)
        {
            var slot = new Models.Listxml.Slot
            {
                Name = item.ReadString(Models.Internal.Slot.NameKey),
            };

            if (item.ContainsKey(Models.Internal.Slot.SlotOptionKey) && item[Models.Internal.Slot.SlotOptionKey] is Models.Internal.SlotOption[] slotOptions)
            {
                var slotOptionItems = new List<Models.Listxml.SlotOption>();
                foreach (var slotOption in slotOptions)
                {
                    slotOptionItems.Add(ConvertToListxml(slotOption));
                }
                slot.SlotOption = slotOptionItems.ToArray();
            }

            return slot;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.SlotOption"/> to <cref="Models.Listxml.SlotOption"/>
        /// </summary>
        public static Models.Listxml.SlotOption ConvertToListxml(Models.Internal.SlotOption item)
        {
            var slotOption = new Models.Listxml.SlotOption
            {
                Name = item.ReadString(Models.Internal.SlotOption.NameKey),
                DevName = item.ReadString(Models.Internal.SlotOption.DevNameKey),
                Default = item.ReadString(Models.Internal.SlotOption.DefaultKey),
            };
            return slotOption;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.SoftwareList"/> to <cref="Models.Listxml.SoftwareList"/>
        /// </summary>
        public static Models.Listxml.SoftwareList ConvertToListxml(Models.Internal.SoftwareList item)
        {
            var softwareList = new Models.Listxml.SoftwareList
            {
                Tag = item.ReadString(Models.Internal.SoftwareList.TagKey),
                Name = item.ReadString(Models.Internal.SoftwareList.NameKey),
                Status = item.ReadString(Models.Internal.SoftwareList.StatusKey),
                Filter = item.ReadString(Models.Internal.SoftwareList.FilterKey),
            };
            return softwareList;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Sound"/> to <cref="Models.Listxml.Sound"/>
        /// </summary>
        public static Models.Listxml.Sound ConvertToListxml(Models.Internal.Sound item)
        {
            var sound = new Models.Listxml.Sound
            {
                Channels = item.ReadString(Models.Internal.Sound.ChannelsKey),
            };
            return sound;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Video"/> to <cref="Models.Listxml.Video"/>
        /// </summary>
        public static Models.Listxml.Video ConvertToListxml(Models.Internal.Video item)
        {
            var video = new Models.Listxml.Video
            {
                Screen = item.ReadString(Models.Internal.Video.ScreenKey),
                Orientation = item.ReadString(Models.Internal.Video.OrientationKey),
                Width = item.ReadString(Models.Internal.Video.WidthKey),
                Height = item.ReadString(Models.Internal.Video.HeightKey),
                AspectX = item.ReadString(Models.Internal.Video.AspectXKey),
                AspectY = item.ReadString(Models.Internal.Video.AspectYKey),
                Refresh = item.ReadString(Models.Internal.Video.RefreshKey),
            };
            return video;
        }

        #endregion
    }
}