using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Filtering
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    /// TODO: Can clever use of Filtering allow for easier external splitting methods?
    /// TODO: Field name for filter population needs to be overhauled
    public class Filter
    {
        #region Fields

        #region Machine Filters

        #region Common

        public FilterItem<string> Machine_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Comment { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Description { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Year { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Manufacturer { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Publisher { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Category { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_RomOf { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_CloneOf { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_SampleOf { get; private set; } = new FilterItem<string>();
        public FilterItem<MachineType> Machine_Type { get; private set; } = new FilterItem<MachineType>() { Positive = MachineType.NULL, Negative = MachineType.NULL };

        #endregion

        #region AttractMode

        public FilterItem<string> Machine_Players { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Rotation { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Control { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Status { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DisplayCount { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DisplayType { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Buttons { get; private set; } = new FilterItem<string>();

        #endregion

        #region ListXML

        public FilterItem<string> Machine_SourceFile { get; private set; } = new FilterItem<string>();
        public FilterItem<Runnable> Machine_Runnable { get; private set; } = new FilterItem<Runnable>() { Positive = Runnable.NULL, Negative = Runnable.NULL };
        
        // DeviceReferences
        public FilterItem<bool?> Machine_DeviceReferences { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_DeviceReference_Name { get; private set; } = new FilterItem<string>();
        
        // Displays
        public FilterItem<bool?> Machine_Displays { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Display_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_Type { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_Rotate { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Display_FlipX { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Display_Width { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_Height { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_Refresh { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_PixClock { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_HTotal { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_HBEnd { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_HBStart { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_VTotal { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_VBEnd { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Display_VBStart { get; private set; } = new FilterItem<string>();

        // Sounds
        public FilterItem<bool?> Machine_Sounds { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Sound_Channels { get; private set; } = new FilterItem<string>();

        // Conditions
        public FilterItem<bool?> Machine_Conditions { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Condition_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Condition_Mask { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Condition_Relation { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Condition_Value { get; private set; } = new FilterItem<string>();

        // Inputs
        public FilterItem<bool?> Machine_Inputs { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Machine_Input_Service { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Machine_Input_Tilt { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Input_Players { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Coins { get; private set; } = new FilterItem<string>();

        // Inputs.Controls
        public FilterItem<bool?> Machine_Input_Controls { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Input_Control_Type { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Player { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Buttons { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_RegButtons { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Minimum { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Maximum { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Sensitivity { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_KeyDelta { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Input_Control_Reverse { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Input_Control_Ways { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Ways2 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Input_Control_Ways3 { get; private set; } = new FilterItem<string>();

        // DipSwitches
        public FilterItem<bool?> Machine_DipSwitches { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_DipSwitch_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DipSwitch_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DipSwitch_Mask { get; private set; } = new FilterItem<string>();

        // DipSwitches.Locations
        public FilterItem<bool?> Machine_DipSwitch_Locations { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_DipSwitch_Location_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DipSwitch_Location_Number { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_DipSwitch_Location_Inverted { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // DipSwitches.Values
        public FilterItem<bool?> Machine_DipSwitch_Values { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_DipSwitch_Value_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_DipSwitch_Value_Value { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_DipSwitch_Value_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Configurations
        public FilterItem<bool?> Machine_Configurations { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Configuration_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Configuration_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Configuration_Mask { get; private set; } = new FilterItem<string>();

        // Configurations.Locations
        public FilterItem<bool?> Machine_Configuration_Locations { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Configuration_Location_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Configuration_Location_Number { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Configuration_Location_Inverted { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Configurations.Settings
        public FilterItem<bool?> Machine_Configuration_Settings { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Configuration_Setting_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Configuration_Setting_Value { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Configuration_Setting_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Ports
        public FilterItem<bool?> Machine_Ports { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Port_Tag { get; private set; } = new FilterItem<string>();

        // Ports.Analogs
        public FilterItem<bool?> Machine_Port_Analogs { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Port_Analog_Mask { get; private set; } = new FilterItem<string>();

        // Adjusters
        public FilterItem<bool?> Machine_Adjusters { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Adjuster_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Adjuster_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Adjusters.Conditions
        public FilterItem<bool?> Machine_Adjuster_Conditions { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Adjuster_Condition_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Adjuster_Condition_Mask { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Adjuster_Condition_Relation { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Adjuster_Condition_Value { get; private set; } = new FilterItem<string>();

        // Drivers
        public FilterItem<bool?> Machine_Drivers { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Driver_Status { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Driver_Emulation { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Driver_Cocktail { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Driver_SaveState { get; private set; } = new FilterItem<string>();

        // Features
        public FilterItem<bool?> Machine_Features { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Feature_Type { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Feature_Status { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Feature_Overall { get; private set; } = new FilterItem<string>();

        // Devices
        public FilterItem<bool?> Machine_Devices { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Device_Type { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Device_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Device_FixedImage { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Device_Mandatory { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Device_Interface { get; private set; } = new FilterItem<string>();

        // Devices.Instances
        public FilterItem<bool?> Machine_Device_Instances { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Device_Instance_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Device_Instance_BriefName { get; private set; } = new FilterItem<string>();

        // Devices.Extensions
        public FilterItem<bool?> Machine_Device_Extensions { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Device_Extension_Name { get; private set; } = new FilterItem<string>();

        // Slots
        public FilterItem<bool?> Machine_Slots { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Slot_Name { get; private set; } = new FilterItem<string>();

        // Slots.SlotOptions
        public FilterItem<bool?> Machine_Slot_SlotOptions { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Slot_SlotOption_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Slot_SlotOption_DeviceName { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_Slot_SlotOption_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // SoftwareLists
        public FilterItem<bool?> Machine_SoftwareLists { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_SoftwareList_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_SoftwareList_Status { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_SoftwareList_Filter { get; private set; } = new FilterItem<string>();

        // RamOptions
        public FilterItem<bool?> Machine_RamOptions { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Machine_RamOption_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        #endregion

        #region Logiqx

        public FilterItem<string> Machine_Board { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_RebuildTo { get; private set; } = new FilterItem<string>();

        #endregion

        #region Logiqx EmuArc

        public FilterItem<string> Machine_TitleID { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Developer { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Genre { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Subgenre { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Ratings { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Score { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Enabled { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Machine_CRC { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_RelatedTo { get; private set; } = new FilterItem<string>();

        #endregion

        #region OpenMSX

        public FilterItem<string> Machine_GenMSXID { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_System { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Country { get; private set; } = new FilterItem<string>();

        #endregion

        #region SoftwareList

        public FilterItem<Supported> Machine_Supported { get; private set; } = new FilterItem<Supported>() { Positive = Supported.NULL, Negative = Supported.NULL };

        // Infos
        public FilterItem<bool?> Machine_Infos { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_Info_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_Info_Value { get; private set; } = new FilterItem<string>();

        // SharedFeatures
        public FilterItem<bool?> Machine_SharedFeatures { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Machine_SharedFeature_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Machine_SharedFeature_Value { get; private set; } = new FilterItem<string>();

        #endregion

        #endregion // Machine Filters

        #region DatItem Filters

        #region Common

        public FilterItem<string> DatItem_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Type { get; private set; } = new FilterItem<string>();

        #endregion

        #region AttractMode

        public FilterItem<string> DatItem_AltName { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_AltTitle { get; private set; } = new FilterItem<string>();

        #endregion

        #region OpenMSX

        public FilterItem<string> DatItem_Original { get; private set; } = new FilterItem<string>();
        public FilterItem<OpenMSXSubType> DatItem_OpenMSXSubType { get; private set; } = new FilterItem<OpenMSXSubType>() { Positive = OpenMSXSubType.NULL, Negative = OpenMSXSubType.NULL };
        public FilterItem<string> DatItem_OpenMSXType { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Remark { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Boot { get; private set; } = new FilterItem<string>();

        #endregion

        #region SoftwareList

        // Part
        public FilterItem<bool?> DatItem_Part { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> DatItem_Part_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Part_Interface { get; private set; } = new FilterItem<string>();

        // Feature
        public FilterItem<bool?> DatItem_Features { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> DatItem_Feature_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Feature_Value { get; private set; } = new FilterItem<string>();

        public FilterItem<string> DatItem_AreaName { get; private set; } = new FilterItem<string>();
        public FilterItem<long?> DatItem_AreaSize { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<string> DatItem_AreaWidth { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_AreaEndianness { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Value { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_LoadFlag { get; private set; } = new FilterItem<string>();

        #endregion

        #region Item-Specific

        // BiosSet
        public FilterItem<bool?> DatItem_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> DatItem_Description { get; private set; } = new FilterItem<string>();

        // Chip
        public FilterItem<string> DatItem_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_ChipType { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Clock { get; private set; } = new FilterItem<string>();

        // Disk
        public FilterItem<string> DatItem_MD5 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_SHA1 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Merge { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Region { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Index { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> DatItem_Writable { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> DatItem_Optional { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<ItemStatus> DatItem_Status { get; private set; } = new FilterItem<ItemStatus>() { Positive = ItemStatus.NULL, Negative = ItemStatus.NULL };

        // Media
        public FilterItem<string> DatItem_SHA256 { get; private set; } = new FilterItem<string>();

        // Release
        public FilterItem<string> DatItem_Language { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Date { get; private set; } = new FilterItem<string>();

        // Rom
        public FilterItem<string> DatItem_Bios { get; private set; } = new FilterItem<string>();
        public FilterItem<long> DatItem_Size { get; private set; } = new FilterItem<long>() { Positive = -1, Negative = -1, Neutral = -1 };
        public FilterItem<string> DatItem_CRC { get; private set; } = new FilterItem<string>();
#if NET_FRAMEWORK
        public FilterItem<string> DatItem_RIPEMD160 { get; private set; } = new FilterItem<string>();
#endif
        public FilterItem<string> DatItem_SHA384 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_SHA512 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> DatItem_Offset { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> DatItem_Inverted { get; private set; } = new FilterItem<bool?>();

        #endregion

        #endregion // DatItem Filters

        #region Manipulation Flags

        /// <summary>
        /// Clean all names to WoD standards
        /// </summary>
        public bool Clean { get; set; }

        /// <summary>
        /// Set Machine Description from Machine Name
        /// </summary>
        public bool DescriptionAsName { get; set; }

        /// <summary>
        /// Include romof and cloneof when filtering machine names
        /// </summary>
        public bool IncludeOfInGame { get; set; }

        /// <summary>
        /// Internally split a DatFile
        /// </summary>
        public MergingFlag InternalSplit { get; set; }

        /// <summary>
        /// Remove all unicode characters
        /// </summary>
        public bool RemoveUnicode { get; set; }

        /// <summary>
        /// Include root directory when determing trim sizes
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        /// Change all machine names to "!"
        /// </summary>
        public bool Single { get; set; }

        /// <summary>
        /// Trim total machine and item name to not exceed NTFS limits
        /// </summary>
        public bool Trim { get; set; }

        #endregion

        #endregion // Fields

        #region Instance methods

        #region Filter Population

        /// <summary>
        /// Populate the filters object using a set of key:value filters
        /// </summary>
        /// <param name="filters">List of key:value where ~key/!key is negated</param>
        public void PopulateFromList(List<string> filters)
        {
            foreach (string filterPair in filters)
            {
                // If we don't even have a possible filter pair
                if (!filterPair.Contains(":"))
                {
                    Globals.Logger.Warning($"'{filterPair}` is not a valid filter string. Valid filter strings are of the form 'key:value'. Please refer to README.1ST or the help feature for more details.");
                    continue;
                }

                string filterPairTrimmed = filterPair.Trim('"', ' ', '\t');
                bool negate = filterPairTrimmed.StartsWith("!")
                    || filterPairTrimmed.StartsWith("~")
                    || filterPairTrimmed.StartsWith("not-");
                filterPairTrimmed = filterPairTrimmed.TrimStart('!', '~');
                filterPairTrimmed = filterPairTrimmed.StartsWith("not-") ? filterPairTrimmed.Substring(4) : filterPairTrimmed;

                string filterFieldString = filterPairTrimmed.Split(':')[0].ToLowerInvariant().Trim('"', ' ', '\t');
                string filterValue = filterPairTrimmed.Substring(filterFieldString.Length + 1).Trim('"', ' ', '\t');

                Field filterField = filterFieldString.AsField();
                SetFilter(filterField, filterValue, negate);
            }
        }

        /// <summary>
        /// Set multiple filters from key
        /// </summary>
        /// <param name="key">Key for the filter to be set</param>
        /// <param name="values">List of values for the filter</param>
        /// <param name="negate">True if negative filter, false otherwise</param>
        public void SetFilter(Field key, List<string> values, bool negate)
        {
            foreach (string value in values)
            {
                SetFilter(key, value, negate);
            }
        }

        /// <summary>
        /// Set a single filter from key
        /// </summary>
        /// <param name="key">Key for the filter to be set</param>
        /// <param name="value">Value of the filter</param>
        /// <param name="negate">True if negative filter, false otherwise</param>
        public void SetFilter(Field key, string value, bool negate)
        {
            switch (key)
            {
                #region Machine Filters

                #region Common

                case Field.Machine_Name:
                    if (negate)
                        Machine_Name.NegativeSet.Add(value);
                    else
                        Machine_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Comment:
                    if (negate)
                        Machine_Comment.NegativeSet.Add(value);
                    else
                        Machine_Comment.PositiveSet.Add(value);
                    break;

                case Field.Machine_Description:
                    if (negate)
                        Machine_Description.NegativeSet.Add(value);
                    else
                        Machine_Description.PositiveSet.Add(value);
                    break;

                case Field.Machine_Year:
                    if (negate)
                        Machine_Year.NegativeSet.Add(value);
                    else
                        Machine_Year.PositiveSet.Add(value);
                    break;

                case Field.Machine_Manufacturer:
                    if (negate)
                        Machine_Manufacturer.NegativeSet.Add(value);
                    else
                        Machine_Manufacturer.PositiveSet.Add(value);
                    break;

                case Field.Machine_Publisher:
                    if (negate)
                        Machine_Publisher.NegativeSet.Add(value);
                    else
                        Machine_Publisher.PositiveSet.Add(value);
                    break;

                case Field.Machine_Category:
                    if (negate)
                        Machine_Category.NegativeSet.Add(value);
                    else
                        Machine_Category.PositiveSet.Add(value);
                    break;

                case Field.Machine_RomOf:
                    if (negate)
                        Machine_RomOf.NegativeSet.Add(value);
                    else
                        Machine_RomOf.PositiveSet.Add(value);
                    break;

                case Field.Machine_CloneOf:
                    if (negate)
                        Machine_CloneOf.NegativeSet.Add(value);
                    else
                        Machine_CloneOf.PositiveSet.Add(value);
                    break;

                case Field.Machine_SampleOf:
                    if (negate)
                        Machine_SampleOf.NegativeSet.Add(value);
                    else
                        Machine_SampleOf.PositiveSet.Add(value);
                    break;

                case Field.Machine_Type:
                    if (negate)
                        Machine_Type.Negative |= value.AsMachineType();
                    else
                        Machine_Type.Positive |= value.AsMachineType();
                    break;

                #endregion

                #region AttractMode

                case Field.Machine_Players:
                    if (negate)
                        Machine_Players.NegativeSet.Add(value);
                    else
                        Machine_Players.PositiveSet.Add(value);
                    break;

                case Field.Machine_Rotation:
                    if (negate)
                        Machine_Rotation.NegativeSet.Add(value);
                    else
                        Machine_Rotation.PositiveSet.Add(value);
                    break;

                case Field.Machine_Control:
                    if (negate)
                        Machine_Control.NegativeSet.Add(value);
                    else
                        Machine_Control.PositiveSet.Add(value);
                    break;

                case Field.Machine_Status:
                    if (negate)
                        Machine_Status.NegativeSet.Add(value);
                    else
                        Machine_Status.PositiveSet.Add(value);
                    break;

                case Field.Machine_DisplayCount:
                    if (negate)
                        Machine_DisplayCount.NegativeSet.Add(value);
                    else
                        Machine_DisplayCount.PositiveSet.Add(value);
                    break;

                case Field.Machine_DisplayType:
                    if (negate)
                        Machine_DisplayType.NegativeSet.Add(value);
                    else
                        Machine_DisplayType.PositiveSet.Add(value);
                    break;

                case Field.Machine_Buttons:
                    if (negate)
                        Machine_Buttons.NegativeSet.Add(value);
                    else
                        Machine_Buttons.PositiveSet.Add(value);
                    break;

                #endregion

                #region ListXML

                case Field.Machine_SourceFile:
                    if (negate)
                        Machine_SourceFile.NegativeSet.Add(value);
                    else
                        Machine_SourceFile.PositiveSet.Add(value);
                    break;

                case Field.Machine_Runnable:
                    if (negate)
                        Machine_Runnable.Negative |= value.AsRunnable();
                    else
                        Machine_Runnable.Positive |= value.AsRunnable();
                    break;

                // DeviceReferences
                case Field.Machine_DeviceReferences:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DeviceReferences.Neutral = false;
                    else
                        Machine_DeviceReferences.Neutral = true;
                    break;

                case Field.Machine_DeviceReference_Name:
                    if (negate)
                        Machine_DeviceReference_Name.NegativeSet.Add(value);
                    else
                        Machine_DeviceReference_Name.PositiveSet.Add(value);
                    break;

                // Displays
                case Field.Machine_Displays:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Displays.Neutral = false;
                    else
                        Machine_Displays.Neutral = true;
                    break;

                case Field.Machine_Display_Tag:
                    if (negate)
                        Machine_Display_Tag.NegativeSet.Add(value);
                    else
                        Machine_Display_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_Type:
                    if (negate)
                        Machine_Display_Type.NegativeSet.Add(value);
                    else
                        Machine_Display_Type.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_Rotate:
                    if (negate)
                        Machine_Display_Rotate.NegativeSet.Add(value);
                    else
                        Machine_Display_Rotate.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_FlipX:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Display_FlipX.Neutral = false;
                    else
                        Machine_Display_FlipX.Neutral = true;
                    break;

                case Field.Machine_Display_Width:
                    if (negate)
                        Machine_Display_Width.NegativeSet.Add(value);
                    else
                        Machine_Display_Width.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_Height:
                    if (negate)
                        Machine_Display_Height.NegativeSet.Add(value);
                    else
                        Machine_Display_Height.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_Refresh:
                    if (negate)
                        Machine_Display_Refresh.NegativeSet.Add(value);
                    else
                        Machine_Display_Refresh.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_PixClock:
                    if (negate)
                        Machine_Display_PixClock.NegativeSet.Add(value);
                    else
                        Machine_Display_PixClock.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_HTotal:
                    if (negate)
                        Machine_Display_HTotal.NegativeSet.Add(value);
                    else
                        Machine_Display_HTotal.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_HBEnd:
                    if (negate)
                        Machine_Display_HBEnd.NegativeSet.Add(value);
                    else
                        Machine_Display_HBEnd.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_HBStart:
                    if (negate)
                        Machine_Display_HBStart.NegativeSet.Add(value);
                    else
                        Machine_Display_HBStart.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_VTotal:
                    if (negate)
                        Machine_Display_VTotal.NegativeSet.Add(value);
                    else
                        Machine_Display_VTotal.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_VBEnd:
                    if (negate)
                        Machine_Display_VBEnd.NegativeSet.Add(value);
                    else
                        Machine_Display_VBEnd.PositiveSet.Add(value);
                    break;

                case Field.Machine_Display_VBStart:
                    if (negate)
                        Machine_Display_VBStart.NegativeSet.Add(value);
                    else
                        Machine_Display_VBStart.PositiveSet.Add(value);
                    break;

                // Sounds
                case Field.Machine_Sounds:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Sounds.Neutral = false;
                    else
                        Machine_Sounds.Neutral = true;
                    break;

                case Field.Machine_Sound_Channels:
                    if (negate)
                        Machine_Sound_Channels.NegativeSet.Add(value);
                    else
                        Machine_Sound_Channels.PositiveSet.Add(value);
                    break;

                // Conditions
                case Field.Machine_Conditions:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Conditions.Neutral = false;
                    else
                        Machine_Conditions.Neutral = true;
                    break;

                case Field.Machine_Condition_Tag:
                    if (negate)
                        Machine_Condition_Tag.NegativeSet.Add(value);
                    else
                        Machine_Condition_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_Condition_Mask:
                    if (negate)
                        Machine_Condition_Mask.NegativeSet.Add(value);
                    else
                        Machine_Condition_Mask.PositiveSet.Add(value);
                    break;

                case Field.Machine_Condition_Relation:
                    if (negate)
                        Machine_Condition_Relation.NegativeSet.Add(value);
                    else
                        Machine_Condition_Relation.PositiveSet.Add(value);
                    break;

                case Field.Machine_Condition_Value:
                    if (negate)
                        Machine_Condition_Value.NegativeSet.Add(value);
                    else
                        Machine_Condition_Value.PositiveSet.Add(value);
                    break;

                // Inputs
                case Field.Machine_Inputs:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Inputs.Neutral = false;
                    else
                        Machine_Inputs.Neutral = true;
                    break;

                case Field.Machine_Input_Service:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Input_Service.Neutral = false;
                    else
                        Machine_Input_Service.Neutral = true;
                    break;

                case Field.Machine_Input_Tilt:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Input_Tilt.Neutral = false;
                    else
                        Machine_Input_Tilt.Neutral = true;
                    break;

                case Field.Machine_Input_Players:
                    if (negate)
                        Machine_Input_Players.NegativeSet.Add(value);
                    else
                        Machine_Input_Players.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Coins:
                    if (negate)
                        Machine_Input_Coins.NegativeSet.Add(value);
                    else
                        Machine_Input_Coins.PositiveSet.Add(value);
                    break;

                // Inputs.Controls
                case Field.Machine_Input_Controls:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Input_Controls.Neutral = false;
                    else
                        Machine_Input_Controls.Neutral = true;
                    break;

                case Field.Machine_Input_Control_Type:
                    if (negate)
                        Machine_Input_Control_Type.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Type.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Player:
                    if (negate)
                        Machine_Input_Control_Player.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Player.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Buttons:
                    if (negate)
                        Machine_Input_Control_Buttons.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Buttons.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_RegButtons:
                    if (negate)
                        Machine_Input_Control_RegButtons.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_RegButtons.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Minimum:
                    if (negate)
                        Machine_Input_Control_Minimum.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Minimum.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Maximum:
                    if (negate)
                        Machine_Input_Control_Maximum.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Maximum.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Sensitivity:
                    if (negate)
                        Machine_Input_Control_Sensitivity.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Sensitivity.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_KeyDelta:
                    if (negate)
                        Machine_Input_Control_KeyDelta.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_KeyDelta.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Reverse:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Input_Control_Reverse.Neutral = false;
                    else
                        Machine_Input_Control_Reverse.Neutral = true;
                    break;

                case Field.Machine_Input_Control_Ways:
                    if (negate)
                        Machine_Input_Control_Ways.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Ways.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Ways2:
                    if (negate)
                        Machine_Input_Control_Ways2.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Ways2.PositiveSet.Add(value);
                    break;

                case Field.Machine_Input_Control_Ways3:
                    if (negate)
                        Machine_Input_Control_Ways3.NegativeSet.Add(value);
                    else
                        Machine_Input_Control_Ways3.PositiveSet.Add(value);
                    break;

                // DipSwitches
                case Field.Machine_DipSwitches:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DipSwitches.Neutral = false;
                    else
                        Machine_DipSwitches.Neutral = true;
                    break;

                case Field.Machine_DipSwitch_Name:
                    if (negate)
                        Machine_DipSwitch_Name.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Tag:
                    if (negate)
                        Machine_DipSwitch_Tag.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Mask:
                    if (negate)
                        Machine_DipSwitch_Mask.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Mask.PositiveSet.Add(value);
                    break;

                // DipSwitches.Locations
                case Field.Machine_DipSwitch_Locations:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DipSwitch_Locations.Neutral = false;
                    else
                        Machine_DipSwitch_Locations.Neutral = true;
                    break;

                case Field.Machine_DipSwitch_Location_Name:
                    if (negate)
                        Machine_DipSwitch_Location_Name.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Location_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Location_Number:
                    if (negate)
                        Machine_DipSwitch_Location_Number.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Location_Number.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Location_Inverted:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DipSwitch_Location_Inverted.Neutral = false;
                    else
                        Machine_DipSwitch_Location_Inverted.Neutral = true;
                    break;

                // DipSwitches.Values
                case Field.Machine_DipSwitch_Values:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DipSwitch_Values.Neutral = false;
                    else
                        Machine_DipSwitch_Values.Neutral = true;
                    break;

                case Field.Machine_DipSwitch_Value_Name:
                    if (negate)
                        Machine_DipSwitch_Value_Name.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Value_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Value_Value:
                    if (negate)
                        Machine_DipSwitch_Value_Value.NegativeSet.Add(value);
                    else
                        Machine_DipSwitch_Value_Value.PositiveSet.Add(value);
                    break;

                case Field.Machine_DipSwitch_Value_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_DipSwitch_Value_Default.Neutral = false;
                    else
                        Machine_DipSwitch_Value_Default.Neutral = true;
                    break;

                // Configurations
                case Field.Machine_Configurations:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Configurations.Neutral = false;
                    else
                        Machine_Configurations.Neutral = true;
                    break;

                case Field.Machine_Configuration_Name:
                    if (negate)
                        Machine_Configuration_Name.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Tag:
                    if (negate)
                        Machine_Configuration_Tag.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Mask:
                    if (negate)
                        Machine_Configuration_Mask.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Mask.PositiveSet.Add(value);
                    break;

                // Configurations.Locations
                case Field.Machine_Configuration_Locations:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Configuration_Locations.Neutral = false;
                    else
                        Machine_Configuration_Locations.Neutral = true;
                    break;

                case Field.Machine_Configuration_Location_Name:
                    if (negate)
                        Machine_Configuration_Location_Name.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Location_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Location_Number:
                    if (negate)
                        Machine_Configuration_Location_Number.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Location_Number.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Location_Inverted:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Configuration_Location_Inverted.Neutral = false;
                    else
                        Machine_Configuration_Location_Inverted.Neutral = true;
                    break;

                // Configurations.Settings
                case Field.Machine_Configuration_Settings:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Configuration_Settings.Neutral = false;
                    else
                        Machine_Configuration_Settings.Neutral = true;
                    break;

                case Field.Machine_Configuration_Setting_Name:
                    if (negate)
                        Machine_Configuration_Setting_Name.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Setting_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Setting_Value:
                    if (negate)
                        Machine_Configuration_Setting_Value.NegativeSet.Add(value);
                    else
                        Machine_Configuration_Setting_Value.PositiveSet.Add(value);
                    break;

                case Field.Machine_Configuration_Setting_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Configuration_Setting_Default.Neutral = false;
                    else
                        Machine_Configuration_Setting_Default.Neutral = true;
                    break;

                // Ports
                case Field.Machine_Ports:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Ports.Neutral = false;
                    else
                        Machine_Ports.Neutral = true;
                    break;

                case Field.Machine_Port_Tag:
                    if (negate)
                        Machine_Port_Tag.NegativeSet.Add(value);
                    else
                        Machine_Port_Tag.PositiveSet.Add(value);
                    break;

                // Ports.Analogs
                case Field.Machine_Port_Analogs:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Port_Analogs.Neutral = false;
                    else
                        Machine_Port_Analogs.Neutral = true;
                    break;

                case Field.Machine_Port_Analog_Mask:
                    if (negate)
                        Machine_Port_Analog_Mask.NegativeSet.Add(value);
                    else
                        Machine_Port_Analog_Mask.PositiveSet.Add(value);
                    break;

                // Adjusters
                case Field.Machine_Adjusters:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Adjusters.Neutral = false;
                    else
                        Machine_Adjusters.Neutral = true;
                    break;

                case Field.Machine_Adjuster_Name:
                    if (negate)
                        Machine_Adjuster_Name.NegativeSet.Add(value);
                    else
                        Machine_Adjuster_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Adjuster_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Adjuster_Default.Neutral = false;
                    else
                        Machine_Adjuster_Default.Neutral = true;
                    break;

                // Adjusters.Conditions
                case Field.Machine_Adjuster_Conditions:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Adjuster_Conditions.Neutral = false;
                    else
                        Machine_Adjuster_Conditions.Neutral = true;
                    break;

                case Field.Machine_Adjuster_Condition_Tag:
                    if (negate)
                        Machine_Adjuster_Condition_Tag.NegativeSet.Add(value);
                    else
                        Machine_Adjuster_Condition_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_Adjuster_Condition_Mask:
                    if (negate)
                        Machine_Adjuster_Condition_Mask.NegativeSet.Add(value);
                    else
                        Machine_Adjuster_Condition_Mask.PositiveSet.Add(value);
                    break;

                case Field.Machine_Adjuster_Condition_Relation:
                    if (negate)
                        Machine_Adjuster_Condition_Relation.NegativeSet.Add(value);
                    else
                        Machine_Adjuster_Condition_Relation.PositiveSet.Add(value);
                    break;

                case Field.Machine_Adjuster_Condition_Value:
                    if (negate)
                        Machine_Adjuster_Condition_Value.NegativeSet.Add(value);
                    else
                        Machine_Adjuster_Condition_Value.PositiveSet.Add(value);
                    break;

                // Drivers
                case Field.Machine_Drivers:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Drivers.Neutral = false;
                    else
                        Machine_Drivers.Neutral = true;
                    break;

                case Field.Machine_Driver_Status:
                    if (negate)
                        Machine_Driver_Status.NegativeSet.Add(value);
                    else
                        Machine_Driver_Status.PositiveSet.Add(value);
                    break;

                case Field.Machine_Driver_Emulation:
                    if (negate)
                        Machine_Driver_Emulation.NegativeSet.Add(value);
                    else
                        Machine_Driver_Emulation.PositiveSet.Add(value);
                    break;

                case Field.Machine_Driver_Cocktail:
                    if (negate)
                        Machine_Driver_Cocktail.NegativeSet.Add(value);
                    else
                        Machine_Driver_Cocktail.PositiveSet.Add(value);
                    break;

                case Field.Machine_Driver_SaveState:
                    if (negate)
                        Machine_Driver_SaveState.NegativeSet.Add(value);
                    else
                        Machine_Driver_SaveState.PositiveSet.Add(value);
                    break;

                // Features
                case Field.Machine_Features:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Features.Neutral = false;
                    else
                        Machine_Features.Neutral = true;
                    break;

                case Field.Machine_Feature_Type:
                    if (negate)
                        Machine_Feature_Type.NegativeSet.Add(value);
                    else
                        Machine_Feature_Type.PositiveSet.Add(value);
                    break;

                case Field.Machine_Feature_Status:
                    if (negate)
                        Machine_Feature_Status.NegativeSet.Add(value);
                    else
                        Machine_Feature_Status.PositiveSet.Add(value);
                    break;

                case Field.Machine_Feature_Overall:
                    if (negate)
                        Machine_Feature_Overall.NegativeSet.Add(value);
                    else
                        Machine_Feature_Overall.PositiveSet.Add(value);
                    break;

                // Devices
                case Field.Machine_Devices:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Devices.Neutral = false;
                    else
                        Machine_Devices.Neutral = true;
                    break;

                case Field.Machine_Device_Type:
                    if (negate)
                        Machine_Device_Type.NegativeSet.Add(value);
                    else
                        Machine_Device_Type.PositiveSet.Add(value);
                    break;

                case Field.Machine_Device_Tag:
                    if (negate)
                        Machine_Device_Tag.NegativeSet.Add(value);
                    else
                        Machine_Device_Tag.PositiveSet.Add(value);
                    break;

                case Field.Machine_Device_FixedImage:
                    if (negate)
                        Machine_Device_FixedImage.NegativeSet.Add(value);
                    else
                        Machine_Device_FixedImage.PositiveSet.Add(value);
                    break;

                case Field.Machine_Device_Mandatory:
                    if (negate)
                        Machine_Device_Mandatory.NegativeSet.Add(value);
                    else
                        Machine_Device_Mandatory.PositiveSet.Add(value);
                    break;

                case Field.Machine_Device_Interface:
                    if (negate)
                        Machine_Device_Interface.NegativeSet.Add(value);
                    else
                        Machine_Device_Interface.PositiveSet.Add(value);
                    break;

                // Devices.Instances
                case Field.Machine_Device_Instances:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Device_Instances.Neutral = false;
                    else
                        Machine_Device_Instances.Neutral = true;
                    break;

                case Field.Machine_Device_Instance_Name:
                    if (negate)
                        Machine_Device_Instance_Name.NegativeSet.Add(value);
                    else
                        Machine_Device_Instance_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Device_Instance_BriefName:
                    if (negate)
                        Machine_Device_Instance_BriefName.NegativeSet.Add(value);
                    else
                        Machine_Device_Instance_BriefName.PositiveSet.Add(value);
                    break;

                // Devices.Extensions
                case Field.Machine_Device_Extensions:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Device_Extensions.Neutral = false;
                    else
                        Machine_Device_Extensions.Neutral = true;
                    break;

                case Field.Machine_Device_Extension_Name:
                    if (negate)
                        Machine_Device_Extension_Name.NegativeSet.Add(value);
                    else
                        Machine_Device_Extension_Name.PositiveSet.Add(value);
                    break;

                // Slots
                case Field.Machine_Slots:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Slots.Neutral = false;
                    else
                        Machine_Slots.Neutral = true;
                    break;

                case Field.Machine_Slot_Name:
                    if (negate)
                        Machine_Slot_Name.NegativeSet.Add(value);
                    else
                        Machine_Slot_Name.PositiveSet.Add(value);
                    break;

                // Slots.SlotOptions
                case Field.Machine_Slot_SlotOptions:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Slot_SlotOptions.Neutral = false;
                    else
                        Machine_Slot_SlotOptions.Neutral = true;
                    break;

                case Field.Machine_Slot_SlotOption_Name:
                    if (negate)
                        Machine_Slot_SlotOption_Name.NegativeSet.Add(value);
                    else
                        Machine_Slot_SlotOption_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Slot_SlotOption_DeviceName:
                    if (negate)
                        Machine_Slot_SlotOption_DeviceName.NegativeSet.Add(value);
                    else
                        Machine_Slot_SlotOption_DeviceName.PositiveSet.Add(value);
                    break;

                case Field.Machine_Slot_SlotOption_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Slot_SlotOption_Default.Neutral = false;
                    else
                        Machine_Slot_SlotOption_Default.Neutral = true;
                    break;

                // SoftwareLists
                case Field.Machine_SoftwareLists:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_SoftwareLists.Neutral = false;
                    else
                        Machine_SoftwareLists.Neutral = true;
                    break;

                case Field.Machine_SoftwareList_Name:
                    if (negate)
                        Machine_SoftwareList_Name.NegativeSet.Add(value);
                    else
                        Machine_SoftwareList_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_SoftwareList_Status:
                    if (negate)
                        Machine_SoftwareList_Status.NegativeSet.Add(value);
                    else
                        Machine_SoftwareList_Status.PositiveSet.Add(value);
                    break;

                case Field.Machine_SoftwareList_Filter:
                    if (negate)
                        Machine_SoftwareList_Filter.NegativeSet.Add(value);
                    else
                        Machine_SoftwareList_Filter.PositiveSet.Add(value);
                    break;

                // RamOptions
                case Field.Machine_RamOptions:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_RamOptions.Neutral = false;
                    else
                        Machine_RamOptions.Neutral = true;
                    break;

                case Field.Machine_RamOption_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_RamOption_Default.Neutral = false;
                    else
                        Machine_RamOption_Default.Neutral = true;
                    break;

                #endregion

                #region Logiqx

                case Field.Machine_Board:
                    if (negate)
                        Machine_Board.NegativeSet.Add(value);
                    else
                        Machine_Board.PositiveSet.Add(value);
                    break;

                case Field.Machine_RebuildTo:
                    if (negate)
                        Machine_RebuildTo.NegativeSet.Add(value);
                    else
                        Machine_RebuildTo.PositiveSet.Add(value);
                    break;

                #endregion

                #region Logiqx EmuArc

                case Field.Machine_TitleID:
                    if (negate)
                        Machine_TitleID.NegativeSet.Add(value);
                    else
                        Machine_TitleID.PositiveSet.Add(value);
                    break;

                case Field.Machine_Developer:
                    if (negate)
                        Machine_Developer.NegativeSet.Add(value);
                    else
                        Machine_Developer.PositiveSet.Add(value);
                    break;

                case Field.Machine_Genre:
                    if (negate)
                        Machine_Genre.NegativeSet.Add(value);
                    else
                        Machine_Genre.PositiveSet.Add(value);
                    break;

                case Field.Machine_Subgenre:
                    if (negate)
                        Machine_Subgenre.NegativeSet.Add(value);
                    else
                        Machine_Subgenre.PositiveSet.Add(value);
                    break;

                case Field.Machine_Ratings:
                    if (negate)
                        Machine_Ratings.NegativeSet.Add(value);
                    else
                        Machine_Ratings.PositiveSet.Add(value);
                    break;

                case Field.Machine_Score:
                    if (negate)
                        Machine_Score.NegativeSet.Add(value);
                    else
                        Machine_Score.PositiveSet.Add(value);
                    break;

                case Field.Machine_Enabled:
                    if (negate)
                        Machine_Enabled.NegativeSet.Add(value);
                    else
                        Machine_Enabled.PositiveSet.Add(value);
                    break;

                case Field.Machine_CRC:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_CRC.Neutral = false;
                    else
                        Machine_CRC.Neutral = true;
                    break;

                case Field.Machine_RelatedTo:
                    if (negate)
                        Machine_RelatedTo.NegativeSet.Add(value);
                    else
                        Machine_RelatedTo.PositiveSet.Add(value);
                    break;

                #endregion

                #region OpenMSX

                case Field.Machine_GenMSXID:
                    if (negate)
                        Machine_GenMSXID.NegativeSet.Add(value);
                    else
                        Machine_GenMSXID.PositiveSet.Add(value);
                    break;

                case Field.Machine_System:
                    if (negate)
                        Machine_System.NegativeSet.Add(value);
                    else
                        Machine_System.PositiveSet.Add(value);
                    break;

                case Field.Machine_Country:
                    if (negate)
                        Machine_Country.NegativeSet.Add(value);
                    else
                        Machine_Country.PositiveSet.Add(value);
                    break;

                #endregion

                #region SoftwareList

                case Field.Machine_Supported:
                    if (negate)
                        Machine_Supported.Negative |= value.AsSupported();
                    else
                        Machine_Supported.Positive |= value.AsSupported();
                    break;

                // Infos
                case Field.Machine_Infos:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_Infos.Neutral = false;
                    else
                        Machine_Infos.Neutral = true;
                    break;

                case Field.Machine_Info_Name:
                    if (negate)
                        Machine_Info_Name.NegativeSet.Add(value);
                    else
                        Machine_Info_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_Info_Value:
                    if (negate)
                        Machine_Info_Value.NegativeSet.Add(value);
                    else
                        Machine_Info_Value.PositiveSet.Add(value);
                    break;

                // SharedFeatures
                case Field.Machine_SharedFeatures:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Machine_SharedFeatures.Neutral = false;
                    else
                        Machine_SharedFeatures.Neutral = true;
                    break;

                case Field.Machine_SharedFeature_Name:
                    if (negate)
                        Machine_SharedFeature_Name.NegativeSet.Add(value);
                    else
                        Machine_SharedFeature_Name.PositiveSet.Add(value);
                    break;

                case Field.Machine_SharedFeature_Value:
                    if (negate)
                        Machine_SharedFeature_Value.NegativeSet.Add(value);
                    else
                        Machine_SharedFeature_Value.PositiveSet.Add(value);
                    break;

                #endregion

                #endregion // Machine Filters

                #region DatItem Filters

                #region Common

                case Field.DatItem_Name:
                    if (negate)
                        DatItem_Name.NegativeSet.Add(value);
                    else
                        DatItem_Name.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Type:
                    if (value.AsItemType() == null)
                        return;

                    if (negate)
                        DatItem_Type.NegativeSet.Add(value);
                    else
                        DatItem_Type.PositiveSet.Add(value);
                    break;

                #endregion

                #region AttractMode

                case Field.DatItem_AltName:
                    if (negate)
                        DatItem_AltName.NegativeSet.Add(value);
                    else
                        DatItem_AltName.PositiveSet.Add(value);
                    break;

                case Field.DatItem_AltTitle:
                    if (negate)
                        DatItem_AltTitle.NegativeSet.Add(value);
                    else
                        DatItem_AltTitle.PositiveSet.Add(value);
                    break;

                #endregion

                #region OpenMSX

                case Field.DatItem_Original:
                    if (negate)
                        DatItem_Original.NegativeSet.Add(value);
                    else
                        DatItem_Original.PositiveSet.Add(value);
                    break;

                case Field.DatItem_OpenMSXSubType:
                    if (negate)
                        DatItem_OpenMSXSubType.Negative |= value.AsOpenMSXSubType();
                    else
                        DatItem_OpenMSXSubType.Positive |= value.AsOpenMSXSubType();
                    break;

                case Field.DatItem_OpenMSXType:
                    if (negate)
                        DatItem_OpenMSXType.NegativeSet.Add(value);
                    else
                        DatItem_OpenMSXType.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Remark:
                    if (negate)
                        DatItem_Remark.NegativeSet.Add(value);
                    else
                        DatItem_Remark.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Boot:
                    if (negate)
                        DatItem_Boot.NegativeSet.Add(value);
                    else
                        DatItem_Boot.PositiveSet.Add(value);
                    break;

                #endregion

                #region SoftwareList

                // Part
                case Field.DatItem_Part:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Part.Neutral = false;
                    else
                        DatItem_Part.Neutral = true;
                    break;

                case Field.DatItem_Part_Name:
                    if (negate)
                        DatItem_Part_Name.NegativeSet.Add(value);
                    else
                        DatItem_Part_Name.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Part_Interface:
                    if (negate)
                        DatItem_Part_Interface.NegativeSet.Add(value);
                    else
                        DatItem_Part_Interface.PositiveSet.Add(value);
                    break;

                // Feature
                case Field.DatItem_Features:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Features.Neutral = false;
                    else
                        DatItem_Features.Neutral = true;
                    break;

                case Field.DatItem_Feature_Name:
                    if (negate)
                        DatItem_Feature_Name.NegativeSet.Add(value);
                    else
                        DatItem_Feature_Name.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Feature_Value:
                    if (negate)
                        DatItem_Feature_Value.NegativeSet.Add(value);
                    else
                        DatItem_Feature_Value.PositiveSet.Add(value);
                    break;

                case Field.DatItem_AreaName:
                    if (negate)
                        DatItem_AreaName.NegativeSet.Add(value);
                    else
                        DatItem_AreaName.PositiveSet.Add(value);
                    break;

                case Field.DatItem_AreaSize:
                    bool? asOperation = null;
                    if (value.StartsWith(">"))
                        asOperation = true;
                    else if (value.StartsWith("<"))
                        asOperation = false;
                    else if (value.StartsWith("="))
                        asOperation = null;

                    string areasizeString = value.TrimStart('>', '<', '=');
                    if (!Int64.TryParse(areasizeString, out long areasize))
                        return;

                    // Equal
                    if (asOperation == null && !negate)
                    {
                        DatItem_AreaSize.Neutral = areasize;
                    }

                    // Not Equal
                    else if (asOperation == null && negate)
                    {
                        DatItem_AreaSize.Negative = areasize - 1;
                        DatItem_AreaSize.Positive = areasize + 1;
                    }

                    // Greater Than or Equal
                    else if (asOperation == true && !negate)
                    {
                        DatItem_AreaSize.Positive = areasize;
                    }

                    // Strictly Less Than
                    else if (asOperation == true && negate)
                    {
                        DatItem_AreaSize.Negative = areasize - 1;
                    }

                    // Less Than or Equal
                    else if (asOperation == false && !negate)
                    {
                        DatItem_AreaSize.Negative = areasize;
                    }

                    // Strictly Greater Than
                    else if (asOperation == false && negate)
                    {
                        DatItem_AreaSize.Positive = areasize + 1;
                    }

                    break;

                case Field.DatItem_AreaWidth:
                    if (negate)
                        DatItem_AreaWidth.NegativeSet.Add(value);
                    else
                        DatItem_AreaWidth.PositiveSet.Add(value);
                    break;

                case Field.DatItem_AreaEndianness:
                    if (negate)
                        DatItem_AreaEndianness.NegativeSet.Add(value);
                    else
                        DatItem_AreaEndianness.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Value:
                    if (negate)
                        DatItem_Value.NegativeSet.Add(value);
                    else
                        DatItem_Value.PositiveSet.Add(value);
                    break;

                case Field.DatItem_LoadFlag:
                    if (negate)
                        DatItem_LoadFlag.NegativeSet.Add(value);
                    else
                        DatItem_LoadFlag.PositiveSet.Add(value);
                    break;

                #endregion

                #region Item-Specific

                // BiosSet
                case Field.DatItem_Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Default.Neutral = false;
                    else
                        DatItem_Default.Neutral = true;
                    break;

                case Field.DatItem_Description:
                    if (negate)
                        DatItem_Description.NegativeSet.Add(value);
                    else
                        DatItem_Description.PositiveSet.Add(value);
                    break;

                // Chip
                case Field.DatItem_Tag:
                    if (negate)
                        DatItem_Tag.NegativeSet.Add(value);
                    else
                        DatItem_Tag.PositiveSet.Add(value);
                    break;

                case Field.DatItem_ChipType:
                    if (negate)
                        DatItem_ChipType.NegativeSet.Add(value);
                    else
                        DatItem_ChipType.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Clock:
                    if (negate)
                        DatItem_Clock.NegativeSet.Add(value);
                    else
                        DatItem_Clock.PositiveSet.Add(value);
                    break;

                // Disk
                case Field.DatItem_MD5:
                    if (negate)
                        DatItem_MD5.NegativeSet.Add(value);
                    else
                        DatItem_MD5.PositiveSet.Add(value);
                    break;

                case Field.DatItem_SHA1:
                    if (negate)
                        DatItem_SHA1.NegativeSet.Add(value);
                    else
                        DatItem_SHA1.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Merge:
                    if (negate)
                        DatItem_Merge.NegativeSet.Add(value);
                    else
                        DatItem_Merge.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Region:
                    if (negate)
                        DatItem_Region.NegativeSet.Add(value);
                    else
                        DatItem_Region.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Index:
                    if (negate)
                        DatItem_Index.NegativeSet.Add(value);
                    else
                        DatItem_Index.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Writable:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Writable.Neutral = false;
                    else
                        DatItem_Writable.Neutral = true;
                    break;

                case Field.DatItem_Optional:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Optional.Neutral = false;
                    else
                        DatItem_Optional.Neutral = true;
                    break;

                case Field.DatItem_Status:
                    if (negate)
                        DatItem_Status.Negative |= value.AsItemStatus();
                    else
                        DatItem_Status.Positive |= value.AsItemStatus();
                    break;

                // Media
                case Field.DatItem_SHA256:
                    if (negate)
                        DatItem_SHA256.NegativeSet.Add(value);
                    else
                        DatItem_SHA256.PositiveSet.Add(value);
                    break;

                // Release
                case Field.DatItem_Language:
                    if (negate)
                        DatItem_Language.NegativeSet.Add(value);
                    else
                        DatItem_Language.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Date:
                    if (negate)
                        DatItem_Date.NegativeSet.Add(value);
                    else
                        DatItem_Date.PositiveSet.Add(value);
                    break;

                // Rom
                case Field.DatItem_Bios:
                    if (negate)
                        DatItem_Bios.NegativeSet.Add(value);
                    else
                        DatItem_Bios.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Size:
                    bool? sOperation = null;
                    if (value.StartsWith(">"))
                        sOperation = true;
                    else if (value.StartsWith("<"))
                        sOperation = false;
                    else if (value.StartsWith("="))
                        sOperation = null;

                    string sizeString = value.TrimStart('>', '<', '=');
                    if (!Int64.TryParse(sizeString, out long size))
                        return;

                    // Equal
                    if (sOperation == null && !negate)
                    {
                        DatItem_Size.Neutral = size;
                    }

                    // Not Equal
                    else if (sOperation == null && negate)
                    {
                        DatItem_Size.Negative = size - 1;
                        DatItem_Size.Positive = size + 1;
                    }

                    // Greater Than or Equal
                    else if (sOperation == true && !negate)
                    {
                        DatItem_Size.Positive = size;
                    }

                    // Strictly Less Than
                    else if (sOperation == true && negate)
                    {
                        DatItem_Size.Negative = size - 1;
                    }

                    // Less Than or Equal
                    else if (sOperation == false && !negate)
                    {
                        DatItem_Size.Negative = size;
                    }

                    // Strictly Greater Than
                    else if (sOperation == false && negate)
                    {
                        DatItem_Size.Positive = size + 1;
                    }

                    break;

                case Field.DatItem_CRC:
                    if (negate)
                        DatItem_CRC.NegativeSet.Add(value);
                    else
                        DatItem_CRC.PositiveSet.Add(value);
                    break;

#if NET_FRAMEWORK
                case Field.DatItem_RIPEMD160:
                    if (negate)
                        DatItem_RIPEMD160.NegativeSet.Add(value);
                    else
                        DatItem_RIPEMD160.PositiveSet.Add(value);
                    break;
#endif

                case Field.DatItem_SHA384:
                    if (negate)
                        DatItem_SHA384.NegativeSet.Add(value);
                    else
                        DatItem_SHA384.PositiveSet.Add(value);
                    break;

                case Field.DatItem_SHA512:
                    if (negate)
                        DatItem_SHA512.NegativeSet.Add(value);
                    else
                        DatItem_SHA512.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Offset:
                    if (negate)
                        DatItem_Offset.NegativeSet.Add(value);
                    else
                        DatItem_Offset.PositiveSet.Add(value);
                    break;

                case Field.DatItem_Inverted:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        DatItem_Inverted.Neutral = false;
                    else
                        DatItem_Inverted.Neutral = true;
                    break;

                #endregion

                #endregion // DatItem Filters
            }
        }

        #endregion

        #endregion // Instance Methods
    }
}
