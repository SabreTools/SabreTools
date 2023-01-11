using System.Collections.Generic;
using System.Linq;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.Filtering
{
    /// <summary>
    /// Represents the removal operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    public class DatItemRemover
    {
        #region Fields

        /// <summary>
        /// List of Machine fields to exclude from writing
        /// </summary>
        public List<MachineField> MachineFields { get; private set; } = new List<MachineField>();

        /// <summary>
        /// List of DatItem fields to exclude from writing
        /// </summary>
        public List<DatItemField> DatItemFields { get; private set; } = new List<DatItemField>();

        #endregion

        #region Population

        /// <summary>
        /// Set remover from a value
        /// </summary>
        /// <param name="field">Key for the remover to be set</param>
        public bool SetRemover(string field)
        {
            // If the key is null or empty, return false
            if (string.IsNullOrWhiteSpace(field))
                return false;

            // If we have a Machine field
            MachineField machineField = field.AsMachineField();
            if (machineField != MachineField.NULL)
            {
                MachineFields.Add(machineField);
                return true;
            }

            // If we have a DatItem field
            DatItemField datItemField = field.AsDatItemField();
            if (datItemField != DatItemField.NULL)
            {
                DatItemFields.Add(datItemField);
                return true;
            }

            return false;
        }

        #endregion

        #region Running

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to remove fields from</param>
        public void RemoveFields(DatItem datItem)
        {
            if (datItem == null)
                return;

            #region Common

            if (MachineFields != null && MachineFields.Any() && datItem.Machine != null)
                RemoveFields(datItem.Machine);

            if (DatItemFields == null || !DatItemFields.Any())
                return;

            if (DatItemFields.Contains(DatItemField.Name))
                datItem.SetName(null);

            #endregion

            #region Item-Specific

            if (datItem is Adjuster) RemoveFields(datItem as Adjuster);
            else if (datItem is Analog) RemoveFields(datItem as Analog);
            else if (datItem is BiosSet) RemoveFields(datItem as BiosSet);
            else if (datItem is Chip) RemoveFields(datItem as Chip);
            else if (datItem is Condition) RemoveFields(datItem as Condition);
            else if (datItem is Configuration) RemoveFields(datItem as Configuration);
            else if (datItem is Control) RemoveFields(datItem as Control);
            else if (datItem is DataArea) RemoveFields(datItem as DataArea);
            else if (datItem is Device) RemoveFields(datItem as Device);
            else if (datItem is DipSwitch) RemoveFields(datItem as DipSwitch);
            else if (datItem is Disk) RemoveFields(datItem as Disk);
            else if (datItem is DiskArea) RemoveFields(datItem as DiskArea);
            else if (datItem is Display) RemoveFields(datItem as Display);
            else if (datItem is Driver) RemoveFields(datItem as Driver);
            else if (datItem is Extension) RemoveFields(datItem as Extension);
            else if (datItem is Feature) RemoveFields(datItem as Feature);
            else if (datItem is Info) RemoveFields(datItem as Info);
            else if (datItem is Input) RemoveFields(datItem as Input);
            else if (datItem is Instance) RemoveFields(datItem as Instance);
            else if (datItem is Location) RemoveFields(datItem as Location);
            else if (datItem is Media) RemoveFields(datItem as Media);
            else if (datItem is Part) RemoveFields(datItem as Part);
            else if (datItem is PartFeature) RemoveFields(datItem as PartFeature);
            else if (datItem is Port) RemoveFields(datItem as Port);
            else if (datItem is RamOption) RemoveFields(datItem as RamOption);
            else if (datItem is Release) RemoveFields(datItem as Release);
            else if (datItem is Rom) RemoveFields(datItem as Rom);
            else if (datItem is Setting) RemoveFields(datItem as Setting);
            else if (datItem is SharedFeature) RemoveFields(datItem as SharedFeature);
            else if (datItem is Slot) RemoveFields(datItem as Slot);
            else if (datItem is SlotOption) RemoveFields(datItem as SlotOption);
            else if (datItem is SoftwareList) RemoveFields(datItem as SoftwareList);
            else if (datItem is Sound) RemoveFields(datItem as Sound);

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="machine">Machine to remove fields from</param>
        private void RemoveFields(Machine machine)
        {
            #region Common

            if (MachineFields.Contains(MachineField.Name))
                machine.Name = null;

            if (MachineFields.Contains(MachineField.Comment))
                machine.Comment = null;

            if (MachineFields.Contains(MachineField.Description))
                machine.Description = null;

            if (MachineFields.Contains(MachineField.Year))
                machine.Year = null;

            if (MachineFields.Contains(MachineField.Manufacturer))
                machine.Manufacturer = null;

            if (MachineFields.Contains(MachineField.Publisher))
                machine.Publisher = null;

            if (MachineFields.Contains(MachineField.Category))
                machine.Category = null;

            if (MachineFields.Contains(MachineField.RomOf))
                machine.RomOf = null;

            if (MachineFields.Contains(MachineField.CloneOf))
                machine.CloneOf = null;

            if (MachineFields.Contains(MachineField.SampleOf))
                machine.SampleOf = null;

            if (MachineFields.Contains(MachineField.Type))
                machine.MachineType = 0x0;

            #endregion

            #region AttractMode

            if (MachineFields.Contains(MachineField.Players))
                machine.Players = null;

            if (MachineFields.Contains(MachineField.Rotation))
                machine.Rotation = null;

            if (MachineFields.Contains(MachineField.Control))
                machine.Control = null;

            if (MachineFields.Contains(MachineField.Status))
                machine.Status = null;

            if (MachineFields.Contains(MachineField.DisplayCount))
                machine.DisplayCount = null;

            if (MachineFields.Contains(MachineField.DisplayType))
                machine.DisplayType = null;

            if (MachineFields.Contains(MachineField.Buttons))
                machine.Buttons = null;

            #endregion

            #region ListXML

            if (MachineFields.Contains(MachineField.History))
                machine.History = null;

            if (MachineFields.Contains(MachineField.SourceFile))
                machine.SourceFile = null;

            if (MachineFields.Contains(MachineField.Runnable))
                machine.Runnable = Runnable.NULL;

            #endregion

            #region Logiqx

            if (MachineFields.Contains(MachineField.Board))
                machine.Board = null;

            if (MachineFields.Contains(MachineField.RebuildTo))
                machine.RebuildTo = null;

            #endregion

            #region Logiqx EmuArc

            if (MachineFields.Contains(MachineField.TitleID))
                machine.TitleID = null;

            if (MachineFields.Contains(MachineField.Developer))
                machine.Developer = null;

            if (MachineFields.Contains(MachineField.Genre))
                machine.Genre = null;

            if (MachineFields.Contains(MachineField.Subgenre))
                machine.Subgenre = null;

            if (MachineFields.Contains(MachineField.Ratings))
                machine.Ratings = null;

            if (MachineFields.Contains(MachineField.Score))
                machine.Score = null;

            if (MachineFields.Contains(MachineField.Enabled))
                machine.Enabled = null;

            if (MachineFields.Contains(MachineField.CRC))
                machine.Crc = null;

            if (MachineFields.Contains(MachineField.RelatedTo))
                machine.RelatedTo = null;

            #endregion

            #region OpenMSX

            if (MachineFields.Contains(MachineField.GenMSXID))
                machine.GenMSXID = null;

            if (MachineFields.Contains(MachineField.System))
                machine.System = null;

            if (MachineFields.Contains(MachineField.Country))
                machine.Country = null;

            #endregion

            #region SoftwareList

            if (MachineFields.Contains(MachineField.Supported))
                machine.Supported = Supported.NULL;

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="adjuster">Adjuster to remove fields from</param>
        private void RemoveFields(Adjuster adjuster)
        {
            if (DatItemFields.Contains(DatItemField.Default))
                adjuster.Default = null;

            if (adjuster.ConditionsSpecified)
            {
                foreach (Condition subCondition in adjuster.Conditions)
                {
                    RemoveFields(subCondition, true);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="analog">Analog to remove fields from</param>
        private void RemoveFields(Analog analog)
        {
            if (DatItemFields.Contains(DatItemField.Analog_Mask))
                analog.Mask = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="biosSet">BiosSet to remove fields from</param>
        private void RemoveFields(BiosSet biosSet)
        {
            if (DatItemFields.Contains(DatItemField.Description))
                biosSet.Description = null;

            if (DatItemFields.Contains(DatItemField.Default))
                biosSet.Default = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="chip">Chip to remove fields from</param>
        private void RemoveFields(Chip chip)
        {
            if (DatItemFields.Contains(DatItemField.Tag))
                chip.Tag = null;

            if (DatItemFields.Contains(DatItemField.ChipType))
                chip.ChipType = ChipType.NULL;

            if (DatItemFields.Contains(DatItemField.Clock))
                chip.Clock = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="condition">Condition to remove fields from</param>
        /// <param name="sub">True if this is a subitem, false otherwise</param>
        private void RemoveFields(Condition condition, bool sub = false)
        {
            if (sub)
            {
                if (DatItemFields.Contains(DatItemField.Condition_Tag))
                    condition.Tag = null;

                if (DatItemFields.Contains(DatItemField.Condition_Mask))
                    condition.Mask = null;

                if (DatItemFields.Contains(DatItemField.Condition_Relation))
                    condition.Relation = Relation.NULL;

                if (DatItemFields.Contains(DatItemField.Condition_Value))
                    condition.Value = null;
            }
            else
            {
                if (DatItemFields.Contains(DatItemField.Tag))
                    condition.Tag = null;

                if (DatItemFields.Contains(DatItemField.Mask))
                    condition.Mask = null;

                if (DatItemFields.Contains(DatItemField.Relation))
                    condition.Relation = Relation.NULL;

                if (DatItemFields.Contains(DatItemField.Value))
                    condition.Value = null;
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="configuration">Configuration to remove fields from</param>
        private void RemoveFields(Configuration configuration)
        {
            if (DatItemFields.Contains(DatItemField.Tag))
                configuration.Tag = null;

            if (DatItemFields.Contains(DatItemField.Mask))
                configuration.Mask = null;

            if (configuration.ConditionsSpecified)
            {
                foreach (Condition subCondition in configuration.Conditions)
                {
                    RemoveFields(subCondition, true);
                }
            }

            if (configuration.LocationsSpecified)
            {
                foreach (Location subLocation in configuration.Locations)
                {
                    RemoveFields(subLocation);
                }
            }

            if (configuration.SettingsSpecified)
            {
                foreach (Setting subSetting in configuration.Settings)
                {
                    RemoveFields(subSetting);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="control">Control to remove fields from</param>
        private void RemoveFields(Control control)
        {
            if (DatItemFields.Contains(DatItemField.Control_Type))
                control.ControlType = ControlType.NULL;

            if (DatItemFields.Contains(DatItemField.Control_Player))
                control.Player = null;

            if (DatItemFields.Contains(DatItemField.Control_Buttons))
                control.Buttons = null;

            if (DatItemFields.Contains(DatItemField.Control_RequiredButtons))
                control.RequiredButtons = null;

            if (DatItemFields.Contains(DatItemField.Control_Minimum))
                control.Minimum = null;

            if (DatItemFields.Contains(DatItemField.Control_Maximum))
                control.Maximum = null;

            if (DatItemFields.Contains(DatItemField.Control_Sensitivity))
                control.Sensitivity = null;

            if (DatItemFields.Contains(DatItemField.Control_KeyDelta))
                control.KeyDelta = null;

            if (DatItemFields.Contains(DatItemField.Control_Reverse))
                control.Reverse = null;

            if (DatItemFields.Contains(DatItemField.Control_Ways))
                control.Ways = null;

            if (DatItemFields.Contains(DatItemField.Control_Ways2))
                control.Ways2 = null;

            if (DatItemFields.Contains(DatItemField.Control_Ways3))
                control.Ways3 = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="dataArea">DataArea to remove fields from</param>
        private void RemoveFields(DataArea dataArea)
        {
            if (DatItemFields.Contains(DatItemField.AreaName))
                dataArea.Name = null;

            if (DatItemFields.Contains(DatItemField.AreaSize))
                dataArea.Size = null;

            if (DatItemFields.Contains(DatItemField.AreaWidth))
                dataArea.Width = null;

            if (DatItemFields.Contains(DatItemField.AreaEndianness))
                dataArea.Endianness = Endianness.NULL;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="device">Device to remove fields from</param>
        private void RemoveFields(Device device)
        {
            if (DatItemFields.Contains(DatItemField.DeviceType))
                device.DeviceType = DeviceType.NULL;

            if (DatItemFields.Contains(DatItemField.Tag))
                device.Tag = null;

            if (DatItemFields.Contains(DatItemField.FixedImage))
                device.FixedImage = null;

            if (DatItemFields.Contains(DatItemField.Mandatory))
                device.Mandatory = null;

            if (DatItemFields.Contains(DatItemField.Interface))
                device.Interface = null;

            if (device.InstancesSpecified)
            {
                foreach (Instance subInstance in device.Instances)
                {
                    RemoveFields(subInstance);
                }
            }

            if (device.ExtensionsSpecified)
            {
                foreach (Extension subExtension in device.Extensions)
                {
                    RemoveFields(subExtension);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="dipSwitch">DipSwitch to remove fields from</param>
        private void RemoveFields(DipSwitch dipSwitch)
        {
            #region Common

            if (DatItemFields.Contains(DatItemField.Tag))
                dipSwitch.Tag = null;

            if (DatItemFields.Contains(DatItemField.Mask))
                dipSwitch.Mask = null;

            if (dipSwitch.ConditionsSpecified)
            {
                foreach (Condition subCondition in dipSwitch.Conditions)
                {
                    RemoveFields(subCondition, true);
                }
            }

            if (dipSwitch.LocationsSpecified)
            {
                foreach (Location subLocation in dipSwitch.Locations)
                {
                    RemoveFields(subLocation);
                }
            }

            if (dipSwitch.ValuesSpecified)
            {
                foreach (Setting subValue in dipSwitch.Values)
                {
                    RemoveFields(subValue);
                }
            }

            #endregion

            #region SoftwareList

            if (dipSwitch.PartSpecified)
                RemoveFields(dipSwitch.Part);

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="disk">Disk to remove fields from</param>
        private void RemoveFields(Disk disk)
        {
            #region Common

            if (DatItemFields.Contains(DatItemField.MD5))
                disk.MD5 = null;

            if (DatItemFields.Contains(DatItemField.SHA1))
                disk.SHA1 = null;

            if (DatItemFields.Contains(DatItemField.Merge))
                disk.MergeTag = null;

            if (DatItemFields.Contains(DatItemField.Region))
                disk.Region = null;

            if (DatItemFields.Contains(DatItemField.Index))
                disk.Index = null;

            if (DatItemFields.Contains(DatItemField.Writable))
                disk.Writable = null;

            if (DatItemFields.Contains(DatItemField.Status))
                disk.ItemStatus = ItemStatus.NULL;

            if (DatItemFields.Contains(DatItemField.Optional))
                disk.Optional = null;

            #endregion

            #region SoftwareList

            if (disk.DiskAreaSpecified)
                RemoveFields(disk.DiskArea);

            if (disk.PartSpecified)
                RemoveFields(disk.Part);

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="diskArea">DiskArea to remove fields from</param>
        private void RemoveFields(DiskArea diskArea)
        {
            if (DatItemFields.Contains(DatItemField.AreaName))
                diskArea.Name = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="display">Display to remove fields from</param>
        private void RemoveFields(Display display)
        {
            if (DatItemFields.Contains(DatItemField.Tag))
                display.Tag = null;

            if (DatItemFields.Contains(DatItemField.DisplayType))
                display.DisplayType = DisplayType.NULL;

            if (DatItemFields.Contains(DatItemField.Rotate))
                display.Rotate = null;

            if (DatItemFields.Contains(DatItemField.FlipX))
                display.FlipX = null;

            if (DatItemFields.Contains(DatItemField.Width))
                display.Width = null;

            if (DatItemFields.Contains(DatItemField.Height))
                display.Height = null;

            if (DatItemFields.Contains(DatItemField.Refresh))
                display.Refresh = null;

            if (DatItemFields.Contains(DatItemField.PixClock))
                display.PixClock = null;

            if (DatItemFields.Contains(DatItemField.HTotal))
                display.HTotal = null;

            if (DatItemFields.Contains(DatItemField.HBEnd))
                display.HBEnd = null;

            if (DatItemFields.Contains(DatItemField.HBStart))
                display.HBStart = null;

            if (DatItemFields.Contains(DatItemField.VTotal))
                display.VTotal = null;

            if (DatItemFields.Contains(DatItemField.VBEnd))
                display.VBEnd = null;

            if (DatItemFields.Contains(DatItemField.VBStart))
                display.VBStart = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="driver">Driver to remove fields from</param>
        private void RemoveFields(Driver driver)
        {
            if (DatItemFields.Contains(DatItemField.SupportStatus))
                driver.Status = SupportStatus.NULL;

            if (DatItemFields.Contains(DatItemField.EmulationStatus))
                driver.Emulation = SupportStatus.NULL;

            if (DatItemFields.Contains(DatItemField.CocktailStatus))
                driver.Cocktail = SupportStatus.NULL;

            if (DatItemFields.Contains(DatItemField.SaveStateStatus))
                driver.SaveState = Supported.NULL;

            if (DatItemFields.Contains(DatItemField.RequiresArtwork))
                driver.RequiresArtwork = null;

            if (DatItemFields.Contains(DatItemField.Unofficial))
                driver.Unofficial = null;

            if (DatItemFields.Contains(DatItemField.NoSoundHardware))
                driver.NoSoundHardware = null;

            if (DatItemFields.Contains(DatItemField.Incomplete))
                driver.Incomplete = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="extension">Extension to remove fields from</param>
        private void RemoveFields(Extension extension)
        {
            if (DatItemFields.Contains(DatItemField.Extension_Name))
                extension.Name = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="feature">Feature to remove fields from</param>
        private void RemoveFields(Feature feature)
        {
            if (DatItemFields.Contains(DatItemField.FeatureType))
                feature.Type = FeatureType.NULL;

            if (DatItemFields.Contains(DatItemField.FeatureStatus))
                feature.Status = FeatureStatus.NULL;

            if (DatItemFields.Contains(DatItemField.FeatureOverall))
                feature.Overall = FeatureStatus.NULL;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="info">Info to remove fields from</param>
        private void RemoveFields(Info info)
        {
            if (DatItemFields.Contains(DatItemField.Value))
                info.Value = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="input">Input to remove fields from</param>
        private void RemoveFields(Input input)
        {
            if (DatItemFields.Contains(DatItemField.Service))
                input.Service = null;

            if (DatItemFields.Contains(DatItemField.Tilt))
                input.Tilt = null;

            if (DatItemFields.Contains(DatItemField.Players))
                input.Players = 0;

            if (DatItemFields.Contains(DatItemField.Coins))
                input.Coins = null;

            if (input.ControlsSpecified)
            {
                foreach (Control subControl in input.Controls)
                {
                    RemoveFields(subControl);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="instance">Instance to remove fields from</param>
        private void RemoveFields(Instance instance)
        {
            if (DatItemFields.Contains(DatItemField.Instance_Name))
                instance.Name = null;

            if (DatItemFields.Contains(DatItemField.Instance_BriefName))
                instance.BriefName = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="location">Location to remove fields from</param>
        private void RemoveFields(Location location)
        {
            if (DatItemFields.Contains(DatItemField.Location_Name))
                location.Name = null;

            if (DatItemFields.Contains(DatItemField.Location_Number))
                location.Number = null;

            if (DatItemFields.Contains(DatItemField.Location_Inverted))
                location.Inverted = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="media">Media to remove fields from</param>
        private void RemoveFields(Media media)
        {
            if (DatItemFields.Contains(DatItemField.MD5))
                media.MD5 = null;

            if (DatItemFields.Contains(DatItemField.SHA1))
                media.SHA1 = null;

            if (DatItemFields.Contains(DatItemField.SHA256))
                media.SHA256 = null;

            if (DatItemFields.Contains(DatItemField.SpamSum))
                media.SpamSum = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="part">Part to remove fields from</param>
        private void RemoveFields(Part part)
        {
            if (DatItemFields.Contains(DatItemField.Part_Name))
                part.Name = null;

            if (DatItemFields.Contains(DatItemField.Part_Interface))
                part.Interface = null;

            if (part.FeaturesSpecified)
            {
                foreach (PartFeature subPartFeature in part.Features)
                {
                    RemoveFields(subPartFeature);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="partFeature">PartFeature to remove fields from</param>
        private void RemoveFields(PartFeature partFeature)
        {
            if (DatItemFields.Contains(DatItemField.Part_Feature_Name))
                partFeature.Name = null;

            if (DatItemFields.Contains(DatItemField.Part_Feature_Value))
                partFeature.Value = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="port">Port to remove fields from</param>
        private void RemoveFields(Port port)
        {
            if (DatItemFields.Contains(DatItemField.Tag))
                port.Tag = null;

            if (port.AnalogsSpecified)
            {
                foreach (Analog subAnalog in port.Analogs)
                {
                    RemoveFields(subAnalog);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="ramOption">RamOption to remove fields from</param>
        private void RemoveFields(RamOption ramOption)
        {
            if (DatItemFields.Contains(DatItemField.Default))
                ramOption.Default = null;

            if (DatItemFields.Contains(DatItemField.Content))
                ramOption.Content = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="release">Release to remove fields from</param>
        private void RemoveFields(Release release)
        {
            if (DatItemFields.Contains(DatItemField.Region))
                release.Region = null;

            if (DatItemFields.Contains(DatItemField.Language))
                release.Language = null;

            if (DatItemFields.Contains(DatItemField.Date))
                release.Date = null;

            if (DatItemFields.Contains(DatItemField.Default))
                release.Default = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="rom">Rom to remove fields from</param>
        private void RemoveFields(Rom rom)
        {
            #region Common

            if (DatItemFields.Contains(DatItemField.Bios))
                rom.Bios = null;

            if (DatItemFields.Contains(DatItemField.Size))
                rom.Size = 0;

            if (DatItemFields.Contains(DatItemField.CRC))
                rom.CRC = null;

            if (DatItemFields.Contains(DatItemField.MD5))
                rom.MD5 = null;

            if (DatItemFields.Contains(DatItemField.SHA1))
                rom.SHA1 = null;

            if (DatItemFields.Contains(DatItemField.SHA256))
                rom.SHA256 = null;

            if (DatItemFields.Contains(DatItemField.SHA384))
                rom.SHA384 = null;

            if (DatItemFields.Contains(DatItemField.SHA512))
                rom.SHA512 = null;

            if (DatItemFields.Contains(DatItemField.SpamSum))
                rom.SpamSum = null;

            if (DatItemFields.Contains(DatItemField.Merge))
                rom.MergeTag = null;

            if (DatItemFields.Contains(DatItemField.Region))
                rom.Region = null;

            if (DatItemFields.Contains(DatItemField.Offset))
                rom.Offset = null;

            if (DatItemFields.Contains(DatItemField.Date))
                rom.Date = null;

            if (DatItemFields.Contains(DatItemField.Status))
                rom.ItemStatus = ItemStatus.NULL;

            if (DatItemFields.Contains(DatItemField.Optional))
                rom.Optional = null;

            if (DatItemFields.Contains(DatItemField.Inverted))
                rom.Inverted = null;
            
            if (DatItemFields.Contains(DatItemField.MIA))
                rom.MIA = null;

            #endregion

            #region Archive.org

            if (DatItemFields.Contains(DatItemField.ArchiveDotOrgSource))
                rom.ArchiveDotOrgSource = null;

            if (DatItemFields.Contains(DatItemField.ArchiveDotOrgFormat))
                rom.ArchiveDotOrgFormat = null;

            if (DatItemFields.Contains(DatItemField.OriginalFilename))
                rom.OriginalFilename = null;

            if (DatItemFields.Contains(DatItemField.Rotation))
                rom.Rotation = null;

            if (DatItemFields.Contains(DatItemField.Summation))
                rom.Summation = null;

            #endregion

            #region AttractMode

            if (DatItemFields.Contains(DatItemField.AltName))
                rom.AltName = null;

            if (DatItemFields.Contains(DatItemField.AltTitle))
                rom.AltTitle = null;

            #endregion

            #region OpenMSX

            if (DatItemFields.Contains(DatItemField.Original))
                rom.Original = null;

            if (DatItemFields.Contains(DatItemField.OpenMSXSubType))
                rom.OpenMSXSubType = OpenMSXSubType.NULL;

            if (DatItemFields.Contains(DatItemField.OpenMSXType))
                rom.OpenMSXType = null;

            if (DatItemFields.Contains(DatItemField.Remark))
                rom.Remark = null;

            if (DatItemFields.Contains(DatItemField.Boot))
                rom.Boot = null;

            #endregion

            #region SoftwareList

            if (DatItemFields.Contains(DatItemField.LoadFlag))
                rom.LoadFlag = LoadFlag.NULL;

            if (DatItemFields.Contains(DatItemField.Value))
                rom.Value = null;

            if (rom.DataAreaSpecified)
                RemoveFields(rom.DataArea);

            if (rom.PartSpecified)
                RemoveFields(rom.Part);

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="setting">Setting to remove fields from</param>
        private void RemoveFields(Setting setting)
        {
            if (DatItemFields.Contains(DatItemField.Setting_Name))
                setting.Name = null;

            if (DatItemFields.Contains(DatItemField.Setting_Value))
                setting.Value = null;

            if (DatItemFields.Contains(DatItemField.Setting_Default))
                setting.Default = null;

            if (setting.ConditionsSpecified)
            {
                foreach (Condition subCondition in setting.Conditions)
                {
                    RemoveFields(subCondition, true);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="sharedFeature">SharedFeature to remove fields from</param>
        private void RemoveFields(SharedFeature sharedFeature)
        {
            if (DatItemFields.Contains(DatItemField.Value))
                sharedFeature.Value = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="slot">Slot to remove fields from</param>
        private void RemoveFields(Slot slot)
        {
            if (slot.SlotOptionsSpecified)
            {
                foreach (SlotOption subSlotOption in slot.SlotOptions)
                {
                    RemoveFields(subSlotOption);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="slotOption">SlotOption to remove fields from</param>
        private void RemoveFields(SlotOption slotOption)
        {
            if (DatItemFields.Contains(DatItemField.SlotOption_Name))
                slotOption.Name = null;

            if (DatItemFields.Contains(DatItemField.SlotOption_DeviceName))
                slotOption.DeviceName = null;

            if (DatItemFields.Contains(DatItemField.SlotOption_Default))
                slotOption.Default = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="softwareList">SoftwareList to remove fields from</param>
        private void RemoveFields(SoftwareList softwareList)
        {
            if (DatItemFields.Contains(DatItemField.Tag))
                softwareList.Tag = null;

            if (DatItemFields.Contains(DatItemField.SoftwareListStatus))
                softwareList.Status = SoftwareListStatus.NULL;

            if (DatItemFields.Contains(DatItemField.Filter))
                softwareList.Filter = null;
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="sound">Sound to remove fields from</param>
        private void RemoveFields(Sound sound)
        {
            if (DatItemFields.Contains(DatItemField.Channels))
                sound.Channels = null;
        }

        #endregion
    }
}