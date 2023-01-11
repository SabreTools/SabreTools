using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Logging;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Set fields on DatItems
    /// </summary>
    /// TODO: Figure out how to get this into the Filtering namespace
    public class Setter
    {
        #region Fields

        /// <summary>
        /// Mappings to set DatHeader fields
        /// </summary>
        public Dictionary<DatHeaderField, string> DatHeaderMappings { get; set; }

        /// <summary>
        /// Mappings to set Machine fields
        /// </summary>
        public Dictionary<MachineField, string> MachineMappings { get; set; }

        /// <summary>
        /// Mappings to set DatItem fields
        /// </summary>
        public Dictionary<DatItemField, string> DatItemMappings { get; set; }

        #endregion
        
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly Logger logger = new Logger();

        #endregion

        #region Population

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="headers">List of header names</param>
        /// <param name="fields">List of field names</param>
        public void PopulateSettersFromList(List<string> headers, List<string> fields)
        {
            // Instantiate the setters, if necessary
            DatHeaderMappings ??= new Dictionary<DatHeaderField, string>();
            MachineMappings ??= new Dictionary<MachineField, string>();
            DatItemMappings ??= new Dictionary<DatItemField, string>();

            // If the list is null or empty, just return
            if (fields == null || fields.Count == 0)
                return;

            // Now we loop through and get values for everything
            for (int i = 0; i < headers.Count; i++)
            {
                string field = fields[i];
                DatHeaderField dhf = headers[i].AsDatHeaderField();
                if (dhf != DatHeaderField.NULL)
                {
                    DatHeaderMappings[dhf] = field;
                    continue;
                }

                MachineField mf = headers[i].AsMachineField();
                if (mf != MachineField.NULL)
                {
                    MachineMappings[mf] = field;
                    continue;
                }

                DatItemField dif = headers[i].AsDatItemField();
                if (dif != DatItemField.NULL)
                {
                    DatItemMappings[dif] = field;
                    continue;
                }

                // If we didn't match anything, log an error
                logger.Warning($"The value {field} did not match any known field names. Please check the wiki for more details on supported field names.");
            }
        }

        #endregion

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="datHeader">DatHeader to set fields on</param>
        public void SetFields(DatHeader datHeader)
        {
            if (datHeader == null)
                return;

            #region Common

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.FileName))
                datHeader.FileName = DatHeaderMappings[DatHeaderField.FileName];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Name))
                datHeader.Name = DatHeaderMappings[DatHeaderField.Name];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Description))
                datHeader.Description = DatHeaderMappings[DatHeaderField.Description];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.RootDir))
                datHeader.RootDir = DatHeaderMappings[DatHeaderField.RootDir];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Category))
                datHeader.Category = DatHeaderMappings[DatHeaderField.Category];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Version))
                datHeader.Version = DatHeaderMappings[DatHeaderField.Version];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Date))
                datHeader.Date = DatHeaderMappings[DatHeaderField.Date];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Author))
                datHeader.Author = DatHeaderMappings[DatHeaderField.Author];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Email))
                datHeader.Email = DatHeaderMappings[DatHeaderField.Email];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Homepage))
                datHeader.Homepage = DatHeaderMappings[DatHeaderField.Homepage];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Url))
                datHeader.Url = DatHeaderMappings[DatHeaderField.Url];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Comment))
                datHeader.Comment = DatHeaderMappings[DatHeaderField.Comment];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.HeaderSkipper))
                datHeader.HeaderSkipper = DatHeaderMappings[DatHeaderField.HeaderSkipper];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Type))
                datHeader.Type = DatHeaderMappings[DatHeaderField.Type];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.ForceMerging))
                datHeader.ForceMerging = DatHeaderMappings[DatHeaderField.ForceMerging].AsMergingFlag();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.ForceNodump))
                datHeader.ForceNodump = DatHeaderMappings[DatHeaderField.ForceNodump].AsNodumpFlag();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.ForcePacking))
                datHeader.ForcePacking = DatHeaderMappings[DatHeaderField.ForcePacking].AsPackingFlag();

            #endregion

            #region ListXML

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Debug))
                datHeader.Debug = DatHeaderMappings[DatHeaderField.Debug].AsYesNo();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.MameConfig))
                datHeader.MameConfig = DatHeaderMappings[DatHeaderField.MameConfig];

            #endregion

            #region Logiqx

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.Build))
                datHeader.Build = DatHeaderMappings[DatHeaderField.Build];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.RomMode))
                datHeader.RomMode = DatHeaderMappings[DatHeaderField.RomMode].AsMergingFlag();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.BiosMode))
                datHeader.BiosMode = DatHeaderMappings[DatHeaderField.BiosMode].AsMergingFlag();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.SampleMode))
                datHeader.SampleMode = DatHeaderMappings[DatHeaderField.SampleMode].AsMergingFlag();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.LockRomMode))
                datHeader.LockRomMode = DatHeaderMappings[DatHeaderField.LockRomMode].AsYesNo();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.LockBiosMode))
                datHeader.LockBiosMode = DatHeaderMappings[DatHeaderField.LockBiosMode].AsYesNo();

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.LockSampleMode))
                datHeader.LockSampleMode = DatHeaderMappings[DatHeaderField.LockSampleMode].AsYesNo();

            #endregion

            #region OfflineList

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.System))
                datHeader.System = DatHeaderMappings[DatHeaderField.System];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.ScreenshotsWidth))
                datHeader.ScreenshotsWidth = DatHeaderMappings[DatHeaderField.ScreenshotsWidth];

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.ScreenshotsHeight))
                datHeader.ScreenshotsHeight = DatHeaderMappings[DatHeaderField.ScreenshotsHeight];

            // TODO: Add DatHeader_Info*
            // TDOO: Add DatHeader_CanOpen*

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.RomTitle))
                datHeader.RomTitle = DatHeaderMappings[DatHeaderField.RomTitle];

            #endregion

            #region RomCenter

            if (DatHeaderMappings.Keys.Contains(DatHeaderField.RomCenterVersion))
                datHeader.RomCenterVersion = DatHeaderMappings[DatHeaderField.RomCenterVersion];

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to set fields on</param>
        public void SetFields(DatItem datItem)
        {
            if (datItem == null || DatItemMappings == null)
                return;

            #region Common            

            if (DatItemMappings.Keys.Contains(DatItemField.Name))
                datItem.SetName(DatItemMappings[DatItemField.Name]);

            #endregion

            #region Item-Specific

            if (datItem is Adjuster) SetFields(datItem as Adjuster);
            else if (datItem is Analog) SetFields(datItem as Analog);
            else if (datItem is BiosSet) SetFields(datItem as BiosSet);
            else if (datItem is Chip) SetFields(datItem as Chip);
            else if (datItem is Condition) SetFields(datItem as Condition);
            else if (datItem is Configuration) SetFields(datItem as Configuration);
            else if (datItem is Control) SetFields(datItem as Control);
            else if (datItem is DataArea) SetFields(datItem as DataArea);
            else if (datItem is Device) SetFields(datItem as Device);
            else if (datItem is DipSwitch) SetFields(datItem as DipSwitch);
            else if (datItem is Disk) SetFields(datItem as Disk);
            else if (datItem is DiskArea) SetFields(datItem as DiskArea);
            else if (datItem is Display) SetFields(datItem as Display);
            else if (datItem is Driver) SetFields(datItem as Driver);
            else if (datItem is Extension) SetFields(datItem as Extension);
            else if (datItem is Feature) SetFields(datItem as Feature);
            else if (datItem is Info) SetFields(datItem as Info);
            else if (datItem is Input) SetFields(datItem as Input);
            else if (datItem is Instance) SetFields(datItem as Instance);
            else if (datItem is Location) SetFields(datItem as Location);
            else if (datItem is Media) SetFields(datItem as Media);
            else if (datItem is Part) SetFields(datItem as Part);
            else if (datItem is PartFeature) SetFields(datItem as PartFeature);
            else if (datItem is Port) SetFields(datItem as Port);
            else if (datItem is RamOption) SetFields(datItem as RamOption);
            else if (datItem is Release) SetFields(datItem as Release);
            else if (datItem is Rom) SetFields(datItem as Rom);
            else if (datItem is Setting) SetFields(datItem as Setting);
            else if (datItem is SharedFeature) SetFields(datItem as SharedFeature);
            else if (datItem is Slot) SetFields(datItem as Slot);
            else if (datItem is SlotOption) SetFields(datItem as SlotOption);
            else if (datItem is SoftwareList) SetFields(datItem as SoftwareList);
            else if (datItem is Sound) SetFields(datItem as Sound);

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="machine">Machine to set fields on</param>
        public void SetFields(Machine machine)
        {
            if (machine == null || MachineMappings == null)
                return;

            #region Common

            if (MachineMappings.Keys.Contains(MachineField.Name))
                machine.Name = MachineMappings[MachineField.Name];

            if (MachineMappings.Keys.Contains(MachineField.Comment))
                machine.Comment = MachineMappings[MachineField.Comment];

            if (MachineMappings.Keys.Contains(MachineField.Description))
                machine.Description = MachineMappings[MachineField.Description];

            if (MachineMappings.Keys.Contains(MachineField.Year))
                machine.Year = MachineMappings[MachineField.Year];

            if (MachineMappings.Keys.Contains(MachineField.Manufacturer))
                machine.Manufacturer = MachineMappings[MachineField.Manufacturer];

            if (MachineMappings.Keys.Contains(MachineField.Publisher))
                machine.Publisher = MachineMappings[MachineField.Publisher];

            if (MachineMappings.Keys.Contains(MachineField.Category))
                machine.Category = MachineMappings[MachineField.Category];

            if (MachineMappings.Keys.Contains(MachineField.RomOf))
                machine.RomOf = MachineMappings[MachineField.RomOf];

            if (MachineMappings.Keys.Contains(MachineField.CloneOf))
                machine.CloneOf = MachineMappings[MachineField.CloneOf];

            if (MachineMappings.Keys.Contains(MachineField.SampleOf))
                machine.SampleOf = MachineMappings[MachineField.SampleOf];

            if (MachineMappings.Keys.Contains(MachineField.Type))
                machine.MachineType = MachineMappings[MachineField.Type].AsMachineType();

            #endregion

            #region AttractMode

            if (MachineMappings.Keys.Contains(MachineField.Players))
                machine.Players = MachineMappings[MachineField.Players];

            if (MachineMappings.Keys.Contains(MachineField.Rotation))
                machine.Rotation = MachineMappings[MachineField.Rotation];

            if (MachineMappings.Keys.Contains(MachineField.Control))
                machine.Control = MachineMappings[MachineField.Control];

            if (MachineMappings.Keys.Contains(MachineField.Status))
                machine.Status = MachineMappings[MachineField.Status];

            if (MachineMappings.Keys.Contains(MachineField.DisplayCount))
                machine.DisplayCount = MachineMappings[MachineField.DisplayCount];

            if (MachineMappings.Keys.Contains(MachineField.DisplayType))
                machine.DisplayType = MachineMappings[MachineField.DisplayType];

            if (MachineMappings.Keys.Contains(MachineField.Buttons))
                machine.Buttons = MachineMappings[MachineField.Buttons];

            #endregion

            #region ListXML

            if (MachineMappings.Keys.Contains(MachineField.History))
                machine.History = MachineMappings[MachineField.History];

            if (MachineMappings.Keys.Contains(MachineField.SourceFile))
                machine.SourceFile = MachineMappings[MachineField.SourceFile];

            if (MachineMappings.Keys.Contains(MachineField.Runnable))
                machine.Runnable = MachineMappings[MachineField.Runnable].AsRunnable();

            #endregion

            #region Logiqx

            if (MachineMappings.Keys.Contains(MachineField.Board))
                machine.Board = MachineMappings[MachineField.Board];

            if (MachineMappings.Keys.Contains(MachineField.RebuildTo))
                machine.RebuildTo = MachineMappings[MachineField.RebuildTo];

            #endregion

            #region Logiqx EmuArc

            if (MachineMappings.Keys.Contains(MachineField.TitleID))
                machine.TitleID = MachineMappings[MachineField.TitleID];

            if (MachineMappings.Keys.Contains(MachineField.Developer))
                machine.Developer = MachineMappings[MachineField.Developer];

            if (MachineMappings.Keys.Contains(MachineField.Genre))
                machine.Genre = MachineMappings[MachineField.Genre];

            if (MachineMappings.Keys.Contains(MachineField.Subgenre))
                machine.Subgenre = MachineMappings[MachineField.Subgenre];

            if (MachineMappings.Keys.Contains(MachineField.Ratings))
                machine.Ratings = MachineMappings[MachineField.Ratings];

            if (MachineMappings.Keys.Contains(MachineField.Score))
                machine.Score = MachineMappings[MachineField.Score];

            if (MachineMappings.Keys.Contains(MachineField.Enabled))
                machine.Enabled = MachineMappings[MachineField.Enabled];

            if (MachineMappings.Keys.Contains(MachineField.CRC))
                machine.Crc = MachineMappings[MachineField.CRC].AsYesNo();

            if (MachineMappings.Keys.Contains(MachineField.RelatedTo))
                machine.RelatedTo = MachineMappings[MachineField.RelatedTo];

            #endregion

            #region OpenMSX

            if (MachineMappings.Keys.Contains(MachineField.GenMSXID))
                machine.GenMSXID = MachineMappings[MachineField.GenMSXID];

            if (MachineMappings.Keys.Contains(MachineField.System))
                machine.System = MachineMappings[MachineField.System];

            if (MachineMappings.Keys.Contains(MachineField.Country))
                machine.Country = MachineMappings[MachineField.Country];

            #endregion

            #region SoftwareList

            if (MachineMappings.Keys.Contains(MachineField.Supported))
                machine.Supported = MachineMappings[MachineField.Supported].AsSupported();

            #endregion
        }
    
        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="adjuster">Adjuster to remove replace fields in</param>
        private void SetFields(Adjuster adjuster)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Default))
                adjuster.Default = DatItemMappings[DatItemField.Default].AsYesNo();

            // Field.DatItem_Conditions does not apply here
            if (adjuster.ConditionsSpecified)
            {
                foreach (Condition subCondition in adjuster.Conditions)
                {
                    SetFields(subCondition, true);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="analog">Analog to remove replace fields in</param>
        private void SetFields(Analog analog)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Analog_Mask))
                analog.Mask = DatItemMappings[DatItemField.Analog_Mask];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="biosSet">BiosSet to remove replace fields in</param>
        private void SetFields(BiosSet biosSet)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Description))
                biosSet.Description = DatItemMappings[DatItemField.Description];

            if (DatItemMappings.Keys.Contains(DatItemField.Default))
                biosSet.Default = DatItemMappings[DatItemField.Default].AsYesNo();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="chip">Chip to remove replace fields in</param>
        private void SetFields(Chip chip)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                chip.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.ChipType))
                chip.ChipType = DatItemMappings[DatItemField.ChipType].AsChipType();

            if (DatItemMappings.Keys.Contains(DatItemField.Clock))
                chip.Clock = Utilities.CleanLong(DatItemMappings[DatItemField.Clock]);
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="condition">Condition to remove replace fields in</param>
        /// <param name="sub">True if this is a subitem, false otherwise</param>
        private void SetFields(Condition condition, bool sub = false)
        {
            if (sub)
            {
                if (DatItemMappings.Keys.Contains(DatItemField.Condition_Tag))
                    condition.Tag = DatItemMappings[DatItemField.Condition_Tag];

                if (DatItemMappings.Keys.Contains(DatItemField.Condition_Mask))
                    condition.Mask = DatItemMappings[DatItemField.Condition_Mask];

                if (DatItemMappings.Keys.Contains(DatItemField.Condition_Relation))
                    condition.Relation = DatItemMappings[DatItemField.Condition_Relation].AsRelation();

                if (DatItemMappings.Keys.Contains(DatItemField.Condition_Value))
                    condition.Value = DatItemMappings[DatItemField.Condition_Value];
            }
            else
            {
                if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                    condition.Tag = DatItemMappings[DatItemField.Tag];

                if (DatItemMappings.Keys.Contains(DatItemField.Mask))
                    condition.Mask = DatItemMappings[DatItemField.Mask];

                if (DatItemMappings.Keys.Contains(DatItemField.Relation))
                    condition.Relation = DatItemMappings[DatItemField.Relation].AsRelation();

                if (DatItemMappings.Keys.Contains(DatItemField.Value))
                    condition.Value = DatItemMappings[DatItemField.Value];
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="configuration">Configuration to remove replace fields in</param>
        private void SetFields(Configuration configuration)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                configuration.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.Mask))
                configuration.Mask = DatItemMappings[DatItemField.Mask];

            if (configuration.ConditionsSpecified)
            {
                foreach (Condition subCondition in configuration.Conditions)
                {
                    SetFields(subCondition, true);
                }
            }

            if (configuration.LocationsSpecified)
            {
                foreach (Location subLocation in configuration.Locations)
                {
                    SetFields(subLocation);
                }
            }

            if (configuration.SettingsSpecified)
            {
                foreach (Setting subSetting in configuration.Settings)
                {
                    SetFields(subSetting);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="control">Control to remove replace fields in</param>
        private void SetFields(Control control)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Control_Type))
                control.ControlType = DatItemMappings[DatItemField.Control_Type].AsControlType();

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Player))
                control.Player = Utilities.CleanLong(DatItemMappings[DatItemField.Control_Player]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Buttons))
                control.Buttons = Utilities.CleanLong(DatItemMappings[DatItemField.Control_Buttons]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_RequiredButtons))
                control.RequiredButtons = Utilities.CleanLong(DatItemMappings[DatItemField.Control_RequiredButtons]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Minimum))
                control.Minimum = Utilities.CleanLong(DatItemMappings[DatItemField.Control_Minimum]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Maximum))
                control.Maximum = Utilities.CleanLong(DatItemMappings[DatItemField.Control_Maximum]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Sensitivity))
                control.Sensitivity = Utilities.CleanLong(DatItemMappings[DatItemField.Control_Sensitivity]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_KeyDelta))
                control.KeyDelta = Utilities.CleanLong(DatItemMappings[DatItemField.Control_KeyDelta]);

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Reverse))
                control.Reverse = DatItemMappings[DatItemField.Control_Reverse].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Ways))
                control.Ways = DatItemMappings[DatItemField.Control_Ways];

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Ways2))
                control.Ways2 = DatItemMappings[DatItemField.Control_Ways2];

            if (DatItemMappings.Keys.Contains(DatItemField.Control_Ways3))
                control.Ways3 = DatItemMappings[DatItemField.Control_Ways3];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="dataArea">DataArea to remove replace fields in</param>
        private void SetFields(DataArea dataArea)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.AreaSize))
                dataArea.Size = Utilities.CleanLong(DatItemMappings[DatItemField.AreaSize]);

            if (DatItemMappings.Keys.Contains(DatItemField.AreaWidth))
                dataArea.Width = Utilities.CleanLong(DatItemMappings[DatItemField.AreaWidth]);

            if (DatItemMappings.Keys.Contains(DatItemField.AreaEndianness))
                dataArea.Endianness = DatItemMappings[DatItemField.AreaEndianness].AsEndianness();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="device">Device to remove replace fields in</param>
        private void SetFields(Device device)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.DeviceType))
                device.DeviceType = DatItemMappings[DatItemField.DeviceType].AsDeviceType();

            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                device.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.FixedImage))
                device.FixedImage = DatItemMappings[DatItemField.FixedImage];

            if (DatItemMappings.Keys.Contains(DatItemField.Mandatory))
                device.Mandatory = Utilities.CleanLong(DatItemMappings[DatItemField.Mandatory]);

            if (DatItemMappings.Keys.Contains(DatItemField.Interface))
                device.Interface = DatItemMappings[DatItemField.Interface];

            if (device.InstancesSpecified)
            {
                foreach (Instance subInstance in device.Instances)
                {
                    SetFields(subInstance);
                }
            }

            if (device.ExtensionsSpecified)
            {
                foreach (Extension subExtension in device.Extensions)
                {
                    SetFields(subExtension);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="dipSwitch">DipSwitch to remove replace fields in</param>
        private void SetFields(DipSwitch dipSwitch)
        {
            #region Common

            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                dipSwitch.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.Mask))
                dipSwitch.Mask = DatItemMappings[DatItemField.Mask];

            if (dipSwitch.ConditionsSpecified)
            {
                foreach (Condition subCondition in dipSwitch.Conditions)
                {
                    SetFields(subCondition, true);
                }
            }

            if (dipSwitch.LocationsSpecified)
            {
                foreach (Location subLocation in dipSwitch.Locations)
                {
                    SetFields(subLocation);
                }
            }

            if (dipSwitch.ValuesSpecified)
            {
                foreach (Setting subValue in dipSwitch.Values)
                {
                    SetFields(subValue);
                }
            }

            #endregion

            #region SoftwareList

            // Handle Part-specific fields
            if (dipSwitch.Part == null)
                dipSwitch.Part = new Part();

            SetFields(dipSwitch.Part);

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="disk">Disk to remove replace fields in</param>
        private void SetFields(Disk disk)
        {
            #region Common
                
            if (DatItemMappings.Keys.Contains(DatItemField.MD5))
                disk.MD5 = DatItemMappings[DatItemField.MD5];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA1))
                disk.SHA1 = DatItemMappings[DatItemField.SHA1];

            if (DatItemMappings.Keys.Contains(DatItemField.Merge))
                disk.MergeTag = DatItemMappings[DatItemField.Merge];

            if (DatItemMappings.Keys.Contains(DatItemField.Region))
                disk.Region = DatItemMappings[DatItemField.Region];

            if (DatItemMappings.Keys.Contains(DatItemField.Index))
                disk.Index = DatItemMappings[DatItemField.Index];

            if (DatItemMappings.Keys.Contains(DatItemField.Writable))
                disk.Writable = DatItemMappings[DatItemField.Writable].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Status))
                disk.ItemStatus = DatItemMappings[DatItemField.Status].AsItemStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.Optional))
                disk.Optional = DatItemMappings[DatItemField.Optional].AsYesNo();

            #endregion

            #region SoftwareList

            if (disk.DiskArea == null)
                disk.DiskArea = new DiskArea();

            SetFields(disk.DiskArea);

            if (disk.Part == null)
                disk.Part = new Part();

            SetFields(disk.Part);

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="diskArea">DiskArea to remove replace fields in</param>
        private void SetFields(DiskArea diskArea)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.AreaName))
                diskArea.Name = DatItemMappings[DatItemField.AreaName];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="display">Display to remove replace fields in</param>
        private void SetFields(Display display)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                display.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.DisplayType))
                display.DisplayType = DatItemMappings[DatItemField.DisplayType].AsDisplayType();

            if (DatItemMappings.Keys.Contains(DatItemField.Rotate))
                display.Rotate = Utilities.CleanLong(DatItemMappings[DatItemField.Rotate]);

            if (DatItemMappings.Keys.Contains(DatItemField.FlipX))
                display.FlipX = DatItemMappings[DatItemField.FlipX].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Width))
                display.Width = Utilities.CleanLong(DatItemMappings[DatItemField.Width]);

            if (DatItemMappings.Keys.Contains(DatItemField.Height))
                display.Height = Utilities.CleanLong(DatItemMappings[DatItemField.Height]);

            if (DatItemMappings.Keys.Contains(DatItemField.Refresh))
                display.Refresh = Utilities.CleanDouble(DatItemMappings[DatItemField.Refresh]);

            if (DatItemMappings.Keys.Contains(DatItemField.PixClock))
                display.PixClock = Utilities.CleanLong(DatItemMappings[DatItemField.PixClock]);

            if (DatItemMappings.Keys.Contains(DatItemField.HTotal))
                display.HTotal = Utilities.CleanLong(DatItemMappings[DatItemField.HTotal]);

            if (DatItemMappings.Keys.Contains(DatItemField.HBEnd))
                display.HBEnd = Utilities.CleanLong(DatItemMappings[DatItemField.HBEnd]);

            if (DatItemMappings.Keys.Contains(DatItemField.HBStart))
                display.HBStart = Utilities.CleanLong(DatItemMappings[DatItemField.HBStart]);

            if (DatItemMappings.Keys.Contains(DatItemField.VTotal))
                display.VTotal = Utilities.CleanLong(DatItemMappings[DatItemField.VTotal]);

            if (DatItemMappings.Keys.Contains(DatItemField.VBEnd))
                display.VBEnd = Utilities.CleanLong(DatItemMappings[DatItemField.VBEnd]);

            if (DatItemMappings.Keys.Contains(DatItemField.VBStart))
                display.VBStart = Utilities.CleanLong(DatItemMappings[DatItemField.VBStart]);
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="driver">Driver to remove replace fields in</param>
        private void SetFields(Driver driver)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.SupportStatus))
                driver.Status = DatItemMappings[DatItemField.SupportStatus].AsSupportStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.EmulationStatus))
                driver.Emulation = DatItemMappings[DatItemField.EmulationStatus].AsSupportStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.CocktailStatus))
                driver.Cocktail = DatItemMappings[DatItemField.CocktailStatus].AsSupportStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.SaveStateStatus))
                driver.SaveState = DatItemMappings[DatItemField.SaveStateStatus].AsSupported();

            if (DatItemMappings.Keys.Contains(DatItemField.RequiresArtwork))
                driver.RequiresArtwork = DatItemMappings[DatItemField.RequiresArtwork].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Unofficial))
                driver.Unofficial = DatItemMappings[DatItemField.Unofficial].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.NoSoundHardware))
                driver.NoSoundHardware = DatItemMappings[DatItemField.NoSoundHardware].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Incomplete))
                driver.Incomplete = DatItemMappings[DatItemField.Incomplete].AsYesNo();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="extension">Extension to remove replace fields in</param>
        private void SetFields(Extension extension)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Extension_Name))
                extension.Name = DatItemMappings[DatItemField.Extension_Name];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="feature">Feature to remove replace fields in</param>
        private void SetFields(Feature feature)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.FeatureType))
                feature.Type = DatItemMappings[DatItemField.FeatureType].AsFeatureType();

            if (DatItemMappings.Keys.Contains(DatItemField.FeatureStatus))
                feature.Status = DatItemMappings[DatItemField.FeatureStatus].AsFeatureStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.FeatureOverall))
                feature.Overall = DatItemMappings[DatItemField.FeatureOverall].AsFeatureStatus();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="info">Info to remove replace fields in</param>
        private void SetFields(Info info)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Value))
                info.Value = DatItemMappings[DatItemField.Value];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="input">Input to remove replace fields in</param>
        private void SetFields(Input input)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Service))
                input.Service = DatItemMappings[DatItemField.Service].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Tilt))
                input.Tilt = DatItemMappings[DatItemField.Tilt].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Players))
                input.Players = Utilities.CleanLong(DatItemMappings[DatItemField.Players]);

            if (DatItemMappings.Keys.Contains(DatItemField.Coins))
                input.Coins = Utilities.CleanLong(DatItemMappings[DatItemField.Coins]);

            if (input.ControlsSpecified)
            {
                foreach (Control subControl in input.Controls)
                {
                    SetFields(subControl);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="instance">Instance to remove replace fields in</param>
        private void SetFields(Instance instance)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Instance_Name))
                instance.BriefName = DatItemMappings[DatItemField.Instance_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.Instance_BriefName))
                instance.BriefName = DatItemMappings[DatItemField.Instance_BriefName];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="location">Location to remove replace fields in</param>
        private void SetFields(Location location)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Location_Name))
                location.Name = DatItemMappings[DatItemField.Location_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.Location_Number))
                location.Number = Utilities.CleanLong(DatItemMappings[DatItemField.Location_Number]);

            if (DatItemMappings.Keys.Contains(DatItemField.Location_Inverted))
                location.Inverted = DatItemMappings[DatItemField.Location_Inverted].AsYesNo();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="media">Media to remove replace fields in</param>
        private void SetFields(Media media)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.MD5))
                media.MD5 = DatItemMappings[DatItemField.MD5];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA1))
                media.SHA1 = DatItemMappings[DatItemField.SHA1];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA256))
                media.SHA256 = DatItemMappings[DatItemField.SHA256];

            if (DatItemMappings.Keys.Contains(DatItemField.SpamSum))
                media.SpamSum = DatItemMappings[DatItemField.SpamSum];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="part">Part to remove replace fields in</param>
        private void SetFields(Part part)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Part_Name))
                part.Name = DatItemMappings[DatItemField.Part_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.Part_Interface))
                part.Interface = DatItemMappings[DatItemField.Part_Interface];

            if (part.FeaturesSpecified)
            {
                foreach (PartFeature subPartFeature in part.Features)
                {
                    SetFields(subPartFeature);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="partFeature">PartFeature to remove replace fields in</param>
        private void SetFields(PartFeature partFeature)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Part_Feature_Name))
                partFeature.Name = DatItemMappings[DatItemField.Part_Feature_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.Part_Feature_Value))
                partFeature.Value = DatItemMappings[DatItemField.Part_Feature_Value];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="port">Port to remove replace fields in</param>
        private void SetFields(Port port)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                port.Tag = DatItemMappings[DatItemField.Tag];

            if (port.AnalogsSpecified)
            {
                foreach (Analog subAnalog in port.Analogs)
                {
                    SetFields(subAnalog);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="ramOption">RamOption to remove replace fields in</param>
        private void SetFields(RamOption ramOption)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Default))
                ramOption.Default = DatItemMappings[DatItemField.Default].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Content))
                ramOption.Content = DatItemMappings[DatItemField.Content];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="release">Release to remove replace fields in</param>
        private void SetFields(Release release)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Region))
                release.Region = DatItemMappings[DatItemField.Region];

            if (DatItemMappings.Keys.Contains(DatItemField.Language))
                release.Language = DatItemMappings[DatItemField.Language];

            if (DatItemMappings.Keys.Contains(DatItemField.Date))
                release.Date = DatItemMappings[DatItemField.Date];

            if (DatItemMappings.Keys.Contains(DatItemField.Default))
                release.Default = DatItemMappings[DatItemField.Default].AsYesNo();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="rom">Rom to remove replace fields in</param>
        private void SetFields(Rom rom)
        {
            #region Common

            if (DatItemMappings.Keys.Contains(DatItemField.Bios))
                rom.Bios = DatItemMappings[DatItemField.Bios];

            if (DatItemMappings.Keys.Contains(DatItemField.Size))
                rom.Size = Utilities.CleanLong(DatItemMappings[DatItemField.Size]);

            if (DatItemMappings.Keys.Contains(DatItemField.CRC))
                rom.CRC = DatItemMappings[DatItemField.CRC];

            if (DatItemMappings.Keys.Contains(DatItemField.MD5))
                rom.MD5 = DatItemMappings[DatItemField.MD5];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA1))
                rom.SHA1 = DatItemMappings[DatItemField.SHA1];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA256))
                rom.SHA256 = DatItemMappings[DatItemField.SHA256];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA384))
                rom.SHA384 = DatItemMappings[DatItemField.SHA384];

            if (DatItemMappings.Keys.Contains(DatItemField.SHA512))
                rom.SHA512 = DatItemMappings[DatItemField.SHA512];

            if (DatItemMappings.Keys.Contains(DatItemField.SpamSum))
                rom.SpamSum = DatItemMappings[DatItemField.SpamSum];

            if (DatItemMappings.Keys.Contains(DatItemField.Merge))
                rom.MergeTag = DatItemMappings[DatItemField.Merge];

            if (DatItemMappings.Keys.Contains(DatItemField.Region))
                rom.Region = DatItemMappings[DatItemField.Region];

            if (DatItemMappings.Keys.Contains(DatItemField.Offset))
                rom.Offset = DatItemMappings[DatItemField.Offset];

            if (DatItemMappings.Keys.Contains(DatItemField.Date))
                rom.Date = DatItemMappings[DatItemField.Date];
            
            if (DatItemMappings.Keys.Contains(DatItemField.Status))
                rom.ItemStatus = DatItemMappings[DatItemField.Status].AsItemStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.Optional))
                rom.Optional = DatItemMappings[DatItemField.Optional].AsYesNo();

            if (DatItemMappings.Keys.Contains(DatItemField.Inverted))
                rom.Inverted = DatItemMappings[DatItemField.Optional].AsYesNo();
            
            if (DatItemMappings.Keys.Contains(DatItemField.MIA))
                rom.MIA = DatItemMappings[DatItemField.MIA].AsYesNo();

            #endregion

            #region Archive.org

            if (DatItemMappings.Keys.Contains(DatItemField.ArchiveDotOrgSource))
                rom.ArchiveDotOrgSource = DatItemMappings[DatItemField.ArchiveDotOrgSource];

            if (DatItemMappings.Keys.Contains(DatItemField.ArchiveDotOrgFormat))
                rom.ArchiveDotOrgFormat = DatItemMappings[DatItemField.ArchiveDotOrgFormat];

            if (DatItemMappings.Keys.Contains(DatItemField.OriginalFilename))
                rom.OriginalFilename = DatItemMappings[DatItemField.OriginalFilename];

            if (DatItemMappings.Keys.Contains(DatItemField.Rotation))
                rom.Rotation = DatItemMappings[DatItemField.Rotation];

            if (DatItemMappings.Keys.Contains(DatItemField.Summation))
                rom.Summation = DatItemMappings[DatItemField.Summation];

            #endregion

            #region AttractMode

            if (DatItemMappings.Keys.Contains(DatItemField.AltName))
                rom.AltName = DatItemMappings[DatItemField.AltName];

            if (DatItemMappings.Keys.Contains(DatItemField.AltTitle))
                rom.AltTitle = DatItemMappings[DatItemField.AltTitle];

            #endregion

            #region OpenMSX

            if (DatItemMappings.Keys.Contains(DatItemField.Original))
                rom.Original = new Original() { Content = DatItemMappings[DatItemField.Original] };

            if (DatItemMappings.Keys.Contains(DatItemField.OpenMSXSubType))
                rom.OpenMSXSubType = DatItemMappings[DatItemField.OpenMSXSubType].AsOpenMSXSubType();

            if (DatItemMappings.Keys.Contains(DatItemField.OpenMSXType))
                rom.OpenMSXType = DatItemMappings[DatItemField.OpenMSXType];

            if (DatItemMappings.Keys.Contains(DatItemField.Remark))
                rom.Remark = DatItemMappings[DatItemField.Remark];

            if (DatItemMappings.Keys.Contains(DatItemField.Boot))
                rom.Boot = DatItemMappings[DatItemField.Boot];

            #endregion

            #region SoftwareList

            if (DatItemMappings.Keys.Contains(DatItemField.LoadFlag))
                rom.LoadFlag = DatItemMappings[DatItemField.LoadFlag].AsLoadFlag();

            if (DatItemMappings.Keys.Contains(DatItemField.Value))
                rom.Value = DatItemMappings[DatItemField.Value];

            if (rom.DataArea == null)
                rom.DataArea = new DataArea();

            SetFields(rom.DataArea);

            if (rom.Part == null)
                rom.Part = new Part();

            SetFields(rom.Part);

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="setting">Setting to remove replace fields in</param>
        private void SetFields(Setting setting)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Setting_Name))
                setting.Name = DatItemMappings[DatItemField.Setting_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.Setting_Value))
                setting.Value = DatItemMappings[DatItemField.Setting_Value];

            if (DatItemMappings.Keys.Contains(DatItemField.Setting_Default))
                setting.Default = DatItemMappings[DatItemField.Setting_Default].AsYesNo();

            if (setting.ConditionsSpecified)
            {
                foreach (Condition subCondition in setting.Conditions)
                {
                    SetFields(subCondition, true);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="sharedFeature">SharedFeature to remove replace fields in</param>
        private void SetFields(SharedFeature sharedFeature)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Value))
                sharedFeature.Value = DatItemMappings[DatItemField.Value];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="slot">Slot to remove replace fields in</param>
        private void SetFields(Slot slot)
        {
            if (slot.SlotOptionsSpecified)
            {
                foreach (SlotOption subSlotOption in slot.SlotOptions)
                {
                    SetFields(subSlotOption);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="slotOption">SlotOption to remove replace fields in</param>
        private void SetFields(SlotOption slotOption)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.SlotOption_Name))
                slotOption.Name = DatItemMappings[DatItemField.SlotOption_Name];

            if (DatItemMappings.Keys.Contains(DatItemField.SlotOption_DeviceName))
                slotOption.DeviceName = DatItemMappings[DatItemField.SlotOption_DeviceName];

            if (DatItemMappings.Keys.Contains(DatItemField.SlotOption_Default))
                slotOption.Default = DatItemMappings[DatItemField.SlotOption_Default].AsYesNo();
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="softwareList">SoftwareList to remove replace fields in</param>
        private void SetFields(SoftwareList softwareList)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Tag))
                softwareList.Tag = DatItemMappings[DatItemField.Tag];

            if (DatItemMappings.Keys.Contains(DatItemField.SoftwareListStatus))
                softwareList.Status = DatItemMappings[DatItemField.SoftwareListStatus].AsSoftwareListStatus();

            if (DatItemMappings.Keys.Contains(DatItemField.Filter))
                softwareList.Filter = DatItemMappings[DatItemField.Filter];
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="sound">Sound to remove replace fields in</param>
        private void SetFields(Sound sound)
        {
            if (DatItemMappings.Keys.Contains(DatItemField.Channels))
                sound.Channels = Utilities.CleanLong(DatItemMappings[DatItemField.Channels]);
        }
    }
}