using System.Collections.Generic;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Logging;

namespace SabreTools.Filtering
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    public class DatItemFilter : Filter
    {
        #region Fields

        #region Common

        public FilterItem<string> Type { get; private set; } = new FilterItem<string>();

        #endregion

        #region Item-Specific

        #region Actionable

        // Rom
        public FilterItem<string> Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Bios { get; private set; } = new FilterItem<string>();
        public FilterItem<long?> Size { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<string> CRC { get; private set; } = new FilterItem<string>();
        public FilterItem<string> MD5 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SHA1 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SHA256 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SHA384 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SHA512 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SpamSum { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Merge { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Region { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Offset { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Date { get; private set; } = new FilterItem<string>();
        public FilterItem<ItemStatus> Status { get; private set; } = new FilterItem<ItemStatus>() { Positive = ItemStatus.NULL, Negative = ItemStatus.NULL };
        public FilterItem<bool?> Optional { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Inverted { get; private set; } = new FilterItem<bool?>();
        
        public FilterItem<bool?> MIA { get; private set; } = new FilterItem<bool?>();

        // Rom (Archive.org)
        public FilterItem<string> ArchiveDotOrgSource { get; private set; } = new FilterItem<string>();
        public FilterItem<string> ArchiveDotOrgFormat { get; private set; } = new FilterItem<string>();
        public FilterItem<string> OriginalFilename { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Rotation { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Summation { get; private set; } = new FilterItem<string>();


        // Rom (AttractMode)
        public FilterItem<string> AltName { get; private set; } = new FilterItem<string>();
        public FilterItem<string> AltTitle { get; private set; } = new FilterItem<string>();

        // Rom (OpenMSX)
        public FilterItem<string> Original { get; private set; } = new FilterItem<string>();
        public FilterItem<OpenMSXSubType> OpenMSXSubType { get; private set; } = new FilterItem<OpenMSXSubType>() { Positive = Core.OpenMSXSubType.NULL, Negative = Core.OpenMSXSubType.NULL };
        public FilterItem<string> OpenMSXType { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Remark { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Boot { get; private set; } = new FilterItem<string>();

        // Rom (SoftwareList)
        public FilterItem<LoadFlag> LoadFlag { get; private set; } = new FilterItem<LoadFlag>() { Positive = Core.LoadFlag.NULL, Negative = Core.LoadFlag.NULL };
        public FilterItem<string> Value { get; private set; } = new FilterItem<string>();

        // Disk
        public FilterItem<string> Index { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Writable { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        #endregion

        #region Auxiliary

        // Adjuster
        public FilterItem<bool?> Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Analog
        public FilterItem<string> Analog_Mask { get; private set; } = new FilterItem<string>();

        // BiosSet
        public FilterItem<string> Description { get; private set; } = new FilterItem<string>();

        // Chip
        public FilterItem<string> Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<ChipType> ChipType { get; private set; } = new FilterItem<ChipType>() { Positive = Core.ChipType.NULL, Negative = Core.ChipType.NULL };
        public FilterItem<long?> Clock { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };

        // Condition
        public FilterItem<string> Mask { get; private set; } = new FilterItem<string>();
        public FilterItem<Relation> Relation { get; private set; } = new FilterItem<Relation>() { Positive = Core.Relation.NULL, Negative = Core.Relation.NULL };
        public FilterItem<string> Condition_Tag { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Condition_Mask { get; private set; } = new FilterItem<string>();
        public FilterItem<Relation> Condition_Relation { get; private set; } = new FilterItem<Relation>() { Positive = Core.Relation.NULL, Negative = Core.Relation.NULL };
        public FilterItem<string> Condition_Value { get; private set; } = new FilterItem<string>();

        // Control
        public FilterItem<ControlType> Control_Type { get; private set; } = new FilterItem<ControlType>() { Positive = ControlType.NULL, Negative = ControlType.NULL };
        public FilterItem<long?> Control_Player { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_Buttons { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_ReqButtons { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_Minimum { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_Maximum { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_Sensitivity { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Control_KeyDelta { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<bool?> Control_Reverse { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<string> Control_Ways { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Control_Ways2 { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Control_Ways3 { get; private set; } = new FilterItem<string>();

        // DataArea
        public FilterItem<string> AreaName { get; private set; } = new FilterItem<string>();
        public FilterItem<long?> AreaSize { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> AreaWidth { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<Endianness> AreaEndianness { get; private set; } = new FilterItem<Endianness>() { Positive = Endianness.NULL, Negative = Endianness.NULL };

        // Device
        public FilterItem<DeviceType> DeviceType { get; private set; } = new FilterItem<DeviceType>() { Positive = Core.DeviceType.NULL, Negative = Core.DeviceType.NULL };
        public FilterItem<string> FixedImage { get; private set; } = new FilterItem<string>();
        public FilterItem<long?> Mandatory { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<string> Interface { get; private set; } = new FilterItem<string>();

        // Display
        public FilterItem<DisplayType> DisplayType { get; private set; } = new FilterItem<DisplayType>() { Positive = Core.DisplayType.NULL, Negative = Core.DisplayType.NULL };
        public FilterItem<long?> Rotate { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<bool?> FlipX { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<long?> Width { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Height { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<double?> Refresh { get; private set; } = new FilterItem<double?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> PixClock { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> HTotal { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> HBEnd { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> HBStart { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> VTotal { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> VBEnd { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> VBStart { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };

        // Driver
        public FilterItem<SupportStatus> SupportStatus { get; private set; } = new FilterItem<SupportStatus>() { Positive = Core.SupportStatus.NULL, Negative = Core.SupportStatus.NULL };
        public FilterItem<SupportStatus> EmulationStatus { get; private set; } = new FilterItem<SupportStatus>() { Positive = Core.SupportStatus.NULL, Negative = Core.SupportStatus.NULL };
        public FilterItem<SupportStatus> CocktailStatus { get; private set; } = new FilterItem<SupportStatus>() { Positive = Core.SupportStatus.NULL, Negative = Core.SupportStatus.NULL };
        public FilterItem<Supported> SaveStateStatus { get; private set; } = new FilterItem<Supported>() { Positive = Supported.NULL, Negative = Supported.NULL };
        public FilterItem<bool?> RequiresArtwork { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Unofficial { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> NoSoundHardware { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Incomplete { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // Extension
        public FilterItem<string> Extension_Name { get; private set; } = new FilterItem<string>();

        // Feature
        public FilterItem<FeatureType> FeatureType { get; private set; } = new FilterItem<FeatureType>() { Positive = Core.FeatureType.NULL, Negative = Core.FeatureType.NULL };
        public FilterItem<FeatureStatus> FeatureStatus { get; private set; } = new FilterItem<FeatureStatus>() { Positive = Core.FeatureStatus.NULL, Negative = Core.FeatureStatus.NULL };
        public FilterItem<FeatureStatus> FeatureOverall { get; private set; } = new FilterItem<FeatureStatus>() { Positive = Core.FeatureStatus.NULL, Negative = Core.FeatureStatus.NULL };

        // Input
        public FilterItem<bool?> Service { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<bool?> Tilt { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        public FilterItem<long?> Players { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<long?> Coins { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };

        // Instance
        public FilterItem<string> Instance_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Instance_BriefName { get; private set; } = new FilterItem<string>();

        // Location
        public FilterItem<string> Location_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<long?> Location_Number { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };
        public FilterItem<bool?> Location_Inverted { get; private set; } = new FilterItem<bool?>() { Neutral = null };
        
        // Part
        public FilterItem<string> Part_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Part_Interface { get; private set; } = new FilterItem<string>();
        
        // PartFeature
        public FilterItem<string> Part_Feature_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Part_Feature_Value { get; private set; } = new FilterItem<string>();

        // RamOption
        public FilterItem<string> Content { get; private set; } = new FilterItem<string>();

        // Release
        public FilterItem<string> Language { get; private set; } = new FilterItem<string>();

        // Setting
        public FilterItem<string> Setting_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> Setting_Value { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> Setting_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // SlotOption
        public FilterItem<string> SlotOption_Name { get; private set; } = new FilterItem<string>();
        public FilterItem<string> SlotOption_DeviceName { get; private set; } = new FilterItem<string>();
        public FilterItem<bool?> SlotOption_Default { get; private set; } = new FilterItem<bool?>() { Neutral = null };

        // SoftwareList
        public FilterItem<SoftwareListStatus> SoftwareListStatus { get; private set; } = new FilterItem<SoftwareListStatus>() { Positive = Core.SoftwareListStatus.NULL, Negative = Core.SoftwareListStatus.NULL };
        public FilterItem<string> Filter { get; private set; } = new FilterItem<string>();

        // Sound
        public FilterItem<long?> Channels { get; private set; } = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };

        #endregion

        #endregion // Item-Specific
        
        /// <summary>
        /// Determines if any filters have been set
        /// </summary>
        public bool HasFilters { get; set; } = false;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DatItemFilter()
        {
            logger = new Logger(this);
        }

        #endregion

        #region Population

        /// <summary>
        /// Set multiple filters from key
        /// </summary>
        /// <param name="key">Key for the filter to be set</param>
        /// <param name="values">List of values for the filter</param>
        /// <param name="negate">True if negative filter, false otherwise</param>
        public void SetFilter(DatItemField key, List<string> values, bool negate)
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
        public void SetFilter(DatItemField key, string value, bool negate)
        {
            switch (key)
            {
                #region Common

                case DatItemField.Type:
                    if (value.AsItemType() == null)
                        return;

                    SetStringFilter(Type, value, negate);
                    break;

                #endregion

                #region Item-Specific

                #region Actionable

                // Rom
                case DatItemField.Name:
                    SetStringFilter(Name, value, negate);
                    break;

                case DatItemField.Bios:
                    SetStringFilter(Bios, value, negate);
                    break;

                case DatItemField.Size:
                    SetLongFilter(Size, value, negate);
                    break;

                case DatItemField.CRC:
                    SetStringFilter(CRC, value, negate);
                    break;

                case DatItemField.MD5:
                    SetStringFilter(MD5, value, negate);
                    break;

                case DatItemField.SHA1:
                    SetStringFilter(SHA1, value, negate);
                    break;

                case DatItemField.SHA256:
                    SetStringFilter(SHA256, value, negate);
                    break;

                case DatItemField.SHA384:
                    SetStringFilter(SHA384, value, negate);
                    break;

                case DatItemField.SHA512:
                    SetStringFilter(SHA512, value, negate);
                    break;

                case DatItemField.SpamSum:
                    SetStringFilter(SpamSum, value, negate);
                    break;

                case DatItemField.Merge:
                    SetStringFilter(Merge, value, negate);
                    break;

                case DatItemField.Region:
                    SetStringFilter(Region, value, negate);
                    break;

                case DatItemField.Offset:
                    SetStringFilter(Offset, value, negate);
                    break;

                case DatItemField.Date:
                    SetStringFilter(Date, value, negate);
                    break;

                case DatItemField.Status:
                    if (negate)
                        Status.Negative |= value.AsItemStatus();
                    else
                        Status.Positive |= value.AsItemStatus();
                    break;

                case DatItemField.Optional:
                    SetBooleanFilter(Optional, value, negate);
                    break;

                case DatItemField.Inverted:
                    SetBooleanFilter(Inverted, value, negate);
                    break;

                // Rom (Archive.org)
                case DatItemField.ArchiveDotOrgSource:
                    SetStringFilter(ArchiveDotOrgSource, value, negate);
                    break;
                
                case DatItemField.ArchiveDotOrgFormat:
                    SetStringFilter(ArchiveDotOrgFormat, value, negate);
                    break;
                
                case DatItemField.OriginalFilename:
                    SetStringFilter(OriginalFilename, value, negate);
                    break;
                
                case DatItemField.Rotation:
                    SetStringFilter(Rotation, value, negate);
                    break;
                
                case DatItemField.Summation:
                    SetStringFilter(Summation, value, negate);
                    break;

                // Rom (AttractMode)
                case DatItemField.AltName:
                    SetStringFilter(AltName, value, negate);
                    break;

                case DatItemField.AltTitle:
                    SetStringFilter(AltTitle, value, negate);
                    break;

                // Rom (OpenMSX)
                case DatItemField.Original:
                    SetStringFilter(Original, value, negate);
                    break;

                case DatItemField.OpenMSXSubType:
                    if (negate)
                        OpenMSXSubType.Negative |= value.AsOpenMSXSubType();
                    else
                        OpenMSXSubType.Positive |= value.AsOpenMSXSubType();
                    break;

                case DatItemField.OpenMSXType:
                    SetStringFilter(OpenMSXType, value, negate);
                    break;

                case DatItemField.Remark:
                    SetStringFilter(Remark, value, negate);
                    break;

                case DatItemField.Boot:
                    SetStringFilter(Boot, value, negate);
                    break;

                // Rom (SoftwareList)
                case DatItemField.LoadFlag:
                    if (negate)
                        LoadFlag.Negative |= value.AsLoadFlag();
                    else
                        LoadFlag.Positive |= value.AsLoadFlag();
                    break;

                case DatItemField.Value:
                    SetStringFilter(Value, value, negate);
                    break;

                // Disk
                case DatItemField.Index:
                    SetStringFilter(Index, value, negate);
                    break;

                case DatItemField.Writable:
                    SetBooleanFilter(Writable, value, negate);
                    break;

                #endregion

                #region Auxiliary

                // Adjuster
                case DatItemField.Default:
                    SetBooleanFilter(Default, value, negate);
                    break;

                // Analog
                case DatItemField.Analog_Mask:
                    SetStringFilter(Analog_Mask, value, negate);
                    break;

                // BiosSet
                case DatItemField.Description:
                    SetStringFilter(Description, value, negate);
                    break;

                // Chip
                case DatItemField.Tag:
                    SetStringFilter(Tag, value, negate);
                    break;

                case DatItemField.ChipType:
                    if (negate)
                        ChipType.Negative |= value.AsChipType();
                    else
                        ChipType.Positive |= value.AsChipType();
                    break;

                case DatItemField.Clock:
                    SetLongFilter(Clock, value, negate);
                    break;

                // Condition
                case DatItemField.Mask:
                    SetStringFilter(Mask, value, negate);
                    break;

                case DatItemField.Relation:
                    if (negate)
                        Relation.Negative |= value.AsRelation();
                    else
                        Relation.Positive |= value.AsRelation();
                    break;

                case DatItemField.Condition_Tag:
                    SetStringFilter(Condition_Tag, value, negate);
                    break;

                case DatItemField.Condition_Mask:
                    SetStringFilter(Condition_Mask, value, negate);
                    break;

                case DatItemField.Condition_Relation:
                    if (negate)
                        Condition_Relation.Negative |= value.AsRelation();
                    else
                        Condition_Relation.Positive |= value.AsRelation();
                    break;

                case DatItemField.Condition_Value:
                    SetStringFilter(Condition_Value, value, negate);
                    break;

                // Control
                case DatItemField.Control_Type:

                    if (negate)
                        Control_Type.Negative |= value.AsControlType();
                    else
                        Control_Type.Positive |= value.AsControlType();
                    break;

                case DatItemField.Control_Player:
                    SetLongFilter(Control_Player, value, negate);
                    break;

                case DatItemField.Control_Buttons:
                    SetLongFilter(Control_Buttons, value, negate);
                    break;

                case DatItemField.Control_RequiredButtons:
                    SetLongFilter(Control_ReqButtons, value, negate);
                    break;

                case DatItemField.Control_Minimum:
                    SetLongFilter(Control_Minimum, value, negate);
                    break;

                case DatItemField.Control_Maximum:
                    SetLongFilter(Control_Maximum, value, negate);
                    break;

                case DatItemField.Control_Sensitivity:
                    SetLongFilter(Control_Sensitivity, value, negate);
                    break;

                case DatItemField.Control_KeyDelta:
                    SetLongFilter(Control_KeyDelta, value, negate);
                    break;

                case DatItemField.Control_Reverse:
                    SetBooleanFilter(Control_Reverse, value, negate);
                    break;

                case DatItemField.Control_Ways:
                    SetStringFilter(Control_Ways, value, negate);
                    break;

                case DatItemField.Control_Ways2:
                    SetStringFilter(Control_Ways2, value, negate);
                    break;

                case DatItemField.Control_Ways3:
                    SetStringFilter(Control_Ways3, value, negate);
                    break;

                // DataArea
                case DatItemField.AreaName:
                    SetStringFilter(AreaName, value, negate);
                    break;

                case DatItemField.AreaSize:
                    SetLongFilter(AreaSize, value, negate);
                    break;

                case DatItemField.AreaWidth:
                    SetLongFilter(AreaWidth, value, negate);
                    break;

                case DatItemField.AreaEndianness:
                    if (negate)
                        AreaEndianness.Negative |= value.AsEndianness();
                    else
                        AreaEndianness.Positive |= value.AsEndianness();
                    break;

                // Device
                case DatItemField.DeviceType:
                    if (negate)
                        DeviceType.Negative |= value.AsDeviceType();
                    else
                        DeviceType.Positive |= value.AsDeviceType();
                    break;

                case DatItemField.FixedImage:
                    SetStringFilter(FixedImage, value, negate);
                    break;

                case DatItemField.Mandatory:
                    SetLongFilter(Mandatory, value, negate);
                    break;

                case DatItemField.Interface:
                    SetStringFilter(Interface, value, negate);
                    break;

                // Display
                case DatItemField.DisplayType:
                    if (negate)
                        DisplayType.Negative |= value.AsDisplayType();
                    else
                        DisplayType.Positive |= value.AsDisplayType();
                    break;

                case DatItemField.Rotate:
                    SetLongFilter(Rotate, value, negate);
                    break;

                case DatItemField.FlipX:
                    SetBooleanFilter(FlipX, value, negate);
                    break;

                case DatItemField.Width:
                    SetLongFilter(Width, value, negate);
                    break;

                case DatItemField.Height:
                    SetLongFilter(Height, value, negate);
                    break;

                case DatItemField.Refresh:
                    SetDoubleFilter(Refresh, value, negate);
                    break;

                case DatItemField.PixClock:
                    SetLongFilter(PixClock, value, negate);
                    break;

                case DatItemField.HTotal:
                    SetLongFilter(HTotal, value, negate);
                    break;

                case DatItemField.HBEnd:
                    SetLongFilter(HBEnd, value, negate);
                    break;

                case DatItemField.HBStart:
                    SetLongFilter(HBStart, value, negate);
                    break;

                case DatItemField.VTotal:
                    SetLongFilter(VTotal, value, negate);
                    break;

                case DatItemField.VBEnd:
                    SetLongFilter(VBEnd, value, negate);
                    break;

                case DatItemField.VBStart:
                    SetLongFilter(VBStart, value, negate);
                    break;

                // Driver
                case DatItemField.SupportStatus:
                    if (negate)
                        SupportStatus.Negative |= value.AsSupportStatus();
                    else
                        SupportStatus.Positive |= value.AsSupportStatus();
                    break;

                case DatItemField.EmulationStatus:
                    if (negate)
                        EmulationStatus.Negative |= value.AsSupportStatus();
                    else
                        EmulationStatus.Positive |= value.AsSupportStatus();
                    break;

                case DatItemField.CocktailStatus:
                    if (negate)
                        CocktailStatus.Negative |= value.AsSupportStatus();
                    else
                        CocktailStatus.Positive |= value.AsSupportStatus();
                    break;

                case DatItemField.SaveStateStatus:
                    if (negate)
                        SaveStateStatus.Negative |= value.AsSupported();
                    else
                        SaveStateStatus.Positive |= value.AsSupported();
                    break;

                case DatItemField.RequiresArtwork:
                    SetBooleanFilter(RequiresArtwork, value, negate);
                    break;

                case DatItemField.Unofficial:
                    SetBooleanFilter(Unofficial, value, negate);
                    break;

                case DatItemField.NoSoundHardware:
                    SetBooleanFilter(NoSoundHardware, value, negate);
                    break;

                case DatItemField.Incomplete:
                    SetBooleanFilter(Incomplete, value, negate);
                    break;

                // Extension
                case DatItemField.Extension_Name:
                    SetStringFilter(Extension_Name, value, negate);
                    break;

                // Feature
                case DatItemField.FeatureType:
                    if (negate)
                        FeatureType.Negative |= value.AsFeatureType();
                    else
                        FeatureType.Positive |= value.AsFeatureType();
                    break;

                case DatItemField.FeatureStatus:
                    if (negate)
                        FeatureStatus.Negative |= value.AsFeatureStatus();
                    else
                        FeatureStatus.Positive |= value.AsFeatureStatus();
                    break;

                case DatItemField.FeatureOverall:
                    if (negate)
                        FeatureOverall.Negative |= value.AsFeatureStatus();
                    else
                        FeatureOverall.Positive |= value.AsFeatureStatus();
                    break;

                // Input
                case DatItemField.Service:
                    SetBooleanFilter(Service, value, negate);
                    break;

                case DatItemField.Tilt:
                    SetBooleanFilter(Tilt, value, negate);
                    break;

                case DatItemField.Players:
                    SetLongFilter(Players, value, negate);
                    break;

                case DatItemField.Coins:
                    SetLongFilter(Coins, value, negate);
                    break;

                // Instance
                case DatItemField.Instance_Name:
                    SetStringFilter(Instance_Name, value, negate);
                    break;

                case DatItemField.Instance_BriefName:
                    SetStringFilter(Instance_BriefName, value, negate);
                    break;

                // Location
                case DatItemField.Location_Name:
                    SetStringFilter(Location_Name, value, negate);
                    break;

                case DatItemField.Location_Number:
                    SetLongFilter(Location_Number, value, negate);
                    break;

                case DatItemField.Location_Inverted:
                    SetBooleanFilter(Location_Inverted, value, negate);
                    break;

                // Part
                case DatItemField.Part_Name:
                    SetStringFilter(Part_Name, value, negate);
                    break;

                case DatItemField.Part_Interface:
                    SetStringFilter(Part_Interface, value, negate);
                    break;

                // PartFeature
                case DatItemField.Part_Feature_Name:
                    SetStringFilter(Part_Feature_Name, value, negate);
                    break;

                case DatItemField.Part_Feature_Value:
                    SetStringFilter(Part_Feature_Value, value, negate);
                    break;

                // RamOption
                case DatItemField.Content:
                    SetStringFilter(Content, value, negate);
                    break;

                // Release
                case DatItemField.Language:
                    SetStringFilter(Language, value, negate);
                    break;

                // Setting
                case DatItemField.Setting_Name:
                    SetStringFilter(Setting_Name, value, negate);
                    break;

                case DatItemField.Setting_Value:
                    SetStringFilter(Setting_Value, value, negate);
                    break;

                case DatItemField.Setting_Default:
                    SetBooleanFilter(Setting_Default, value, negate);
                    break;

                // SlotOption
                case DatItemField.SlotOption_Name:
                    SetStringFilter(SlotOption_Name, value, negate);
                    break;

                case DatItemField.SlotOption_DeviceName:
                    SetStringFilter(SlotOption_DeviceName, value, negate);
                    break;

                case DatItemField.SlotOption_Default:
                    SetBooleanFilter(SlotOption_Default, value, negate);
                    break;

                // SoftwareList
                case DatItemField.SoftwareListStatus:
                    if (negate)
                        SoftwareListStatus.Negative |= value.AsSoftwareListStatus();
                    else
                        SoftwareListStatus.Positive |= value.AsSoftwareListStatus();
                    break;

                case DatItemField.Filter:
                    SetStringFilter(Filter, value, negate);
                    break;

                // Sound
                case DatItemField.Channels:
                    SetLongFilter(Channels, value, negate);
                    break;

                #endregion

                #endregion // Item-Specific
            }
        }
    
        #endregion
    
        #region Running

        /// <summary>
        /// Check to see if a DatItem passes the filters
        /// </summary>
        /// <param name="datItem">DatItem to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        public bool PassesFilters(DatItem datItem)
        {
            if (datItem == null)
                return false;

            #region Common

            // Filter on item type
            if (!PassStringFilter(Type, datItem.ItemType.ToString()))
                return false;

            // Name is common if the name is non-null
            if (datItem.GetName() != null)
            {
                // Filter on item name
                if (!PassStringFilter(Name, datItem.GetName()))
                    return false;
            }

            #endregion
        
            #region Item-Specific

            return datItem switch
            {
                Adjuster adjuster => PassesFilters(adjuster),
                Analog analog => PassesFilters(analog),
                BiosSet biosSet => PassesFilters(biosSet),
                Chip chip => PassesFilters(chip),
                Condition condition => PassesFilters(condition),
                Configuration configuration => PassesFilters(configuration),
                Control control => PassesFilters(control),
                DataArea dataArea => PassesFilters(dataArea),
                Device device => PassesFilters(device),
                DipSwitch dipSwitch => PassesFilters(dipSwitch),
                Disk disk => PassesFilters(disk),
                DiskArea diskArea => PassesFilters(diskArea),
                Display display => PassesFilters(display),
                Driver driver => PassesFilters(driver),
                Extension extension => PassesFilters(extension),
                Feature feature => PassesFilters(feature),
                Info info => PassesFilters(info),
                Input input => PassesFilters(input),
                Instance instance => PassesFilters(instance),
                Location location => PassesFilters(location),
                Media media => PassesFilters(media),
                Part part => PassesFilters(part),
                PartFeature partFeature => PassesFilters(partFeature),
                Port port => PassesFilters(port),
                RamOption ramOption => PassesFilters(ramOption),
                Release release => PassesFilters(release),
                Rom rom => PassesFilters(rom),
                Setting setting => PassesFilters(setting),
                SharedFeature sharedFeature => PassesFilters(sharedFeature),
                Slot slot => PassesFilters(slot),
                SlotOption slotOption => PassesFilters(slotOption),
                SoftwareList softwareList => PassesFilters(softwareList),
                Sound sound => PassesFilters(sound),
                _ => false,
            };

            #endregion
        }

        /// <summary>
        /// Check to see if an Adjuster passes the filters
        /// </summary>
        /// <param name="adjuster">Adjuster to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Adjuster adjuster)
        {
            // Filter on default
            if (!PassBoolFilter(Default, adjuster.Default))
                return false;

            // Filter on individual conditions
            if (adjuster.ConditionsSpecified)
            {
                foreach (Condition condition in adjuster.Conditions)
                {
                    if (!PassesFilters(condition, true))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if an Analog passes the filters
        /// </summary>
        /// <param name="analog">Analog to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Analog analog)
        {
            // Filter on mask
            if (!PassStringFilter(Analog_Mask, analog.Mask))
                return false;

            return true;
        }

        /// <summary>
        /// Check to see if a BiosSet passes the filters
        /// </summary>
        /// <param name="biosSet">BiosSet to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(BiosSet biosSet)
        {
            // Filter on description
            if (!PassStringFilter(Description, biosSet.Description))
                return false;

            // Filter on default
            if (!PassBoolFilter(Default, biosSet.Default))
                return false;

            return true;
        }

        /// <summary>
        /// Check to see if a Chip passes the filters
        /// </summary>
        /// <param name="chip">Chip to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Chip chip)
        {
            // Filter on tag
            if (!PassStringFilter(Tag, chip.Tag))
                return false;

            // DatItem_ChipType
            if (ChipType.MatchesPositive(Core.ChipType.NULL, chip.ChipType) == false)
                return false;
            if (ChipType.MatchesNegative(Core.ChipType.NULL, chip.ChipType) == true)
                return false;

            // DatItem_Clock
            if (!PassLongFilter(Clock, chip.Clock))
                return false;

            return true;
        }

        /// <summary>
        /// Check to see if a Condition passes the filters
        /// </summary>
        /// <param name="condition">Condition to check</param>
        /// <param name="sub">True if this is a subitem, false otherwise</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Condition condition, bool sub = false)
        {
            if (sub)
            {
                // Filter on tag
                if (!PassStringFilter(Condition_Tag, condition.Tag))
                    return false;

                // Filter on mask
                if (!PassStringFilter(Condition_Mask, condition.Mask))
                    return false;

                // Filter on relation
                if (Condition_Relation.MatchesPositive(Core.Relation.NULL, condition.Relation) == false)
                    return false;
                if (Condition_Relation.MatchesNegative(Core.Relation.NULL, condition.Relation) == true)
                    return false;

                // Filter on value
                if (!PassStringFilter(Condition_Value, condition.Value))
                    return false;
            }
            else
            {
                // Filter on tag
                if (!PassStringFilter(Tag, condition.Tag))
                    return false;

                // Filter on mask
                if (!PassStringFilter(Mask, condition.Mask))
                    return false;

                // Filter on relation
                if (Relation.MatchesPositive(Core.Relation.NULL, condition.Relation) == false)
                    return false;
                if (Relation.MatchesNegative(Core.Relation.NULL, condition.Relation) == true)
                    return false;

                // Filter on value
                if (!PassStringFilter(Value, condition.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check to see if a Configuration passes the filters
        /// </summary>
        /// <param name="configuration">Configuration to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Configuration configuration)
        {
            // Filter on tag
            if (!PassStringFilter(Tag, configuration.Tag))
                return false;

            // Filter on mask
            if (!PassStringFilter(Mask, configuration.Mask))
                return false;

            // Filter on individual conditions
            if (configuration.ConditionsSpecified)
            {
                foreach (Condition subCondition in configuration.Conditions)
                {
                    if (!PassesFilters(subCondition, true))
                        return false;
                }
            }

            // Filter on individual locations
            if (configuration.LocationsSpecified)
            {
                foreach (Location subLocation in configuration.Locations)
                {
                    if (!PassesFilters(subLocation))
                        return false;
                }
            }

            // Filter on individual conditions
            if (configuration.SettingsSpecified)
            {
                foreach (Setting subSetting in configuration.Settings)
                {
                    if (!PassesFilters(subSetting))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if a Control passes the filters
        /// </summary>
        /// <param name="control">Control to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Control control)
        {
            // Filter on control type
            if (Control_Type.MatchesPositive(ControlType.NULL, control.ControlType) == false)
                return false;
            if (Control_Type.MatchesNegative(ControlType.NULL, control.ControlType) == true)
                return false;

            // Filter on player
            if (!PassLongFilter(Control_Player, control.Player))
                return false;

            // Filter on buttons
            if (!PassLongFilter(Control_Buttons, control.Buttons))
                return false;

            // Filter on reqbuttons
            if (!PassLongFilter(Control_ReqButtons, control.RequiredButtons))
                return false;

            // Filter on minimum
            if (!PassLongFilter(Control_Minimum, control.Minimum))
                return false;

            // Filter on maximum
            if (!PassLongFilter(Control_Maximum, control.Maximum))
                return false;

            // Filter on sensitivity
            if (!PassLongFilter(Control_Sensitivity, control.Sensitivity))
                return false;

            // Filter on keydelta
            if (!PassLongFilter(Control_KeyDelta, control.KeyDelta))
                return false;

            // Filter on reverse
            if (!PassBoolFilter(Control_Reverse, control.Reverse))
                return false;

            // Filter on ways
            if (!PassStringFilter(Control_Ways, control.Ways))
                return false;

            // Filter on ways2
            if (!PassStringFilter(Control_Ways2, control.Ways2))
                return false;

            // Filter on ways3
            if (!PassStringFilter(Control_Ways3, control.Ways3))
                return false;

            return true;
        }

        /// <summary>
        /// Check to see if a DataArea passes the filters
        /// </summary>
        /// <param name="dataArea">DataArea to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(DataArea dataArea)
        {
            // Filter on area name
            if (!PassStringFilter(AreaName, dataArea.Name))
                return false;

            // Filter on area size
            if (!PassLongFilter(AreaSize, dataArea.Size))
                return false;

            // Filter on area width
            if (!PassLongFilter(AreaWidth, dataArea.Width))
                return false;

            // Filter on area endianness
            if (AreaEndianness.MatchesPositive(Endianness.NULL, dataArea.Endianness) == false)
                return false;
            if (AreaEndianness.MatchesNegative(Endianness.NULL, dataArea.Endianness) == true)
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Device passes the filters
        /// </summary>
        /// <param name="device">Device to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Device device)
        {
            // Filter on device type
            if (DeviceType.MatchesPositive(Core.DeviceType.NULL, device.DeviceType) == false)
                return false;
            if (DeviceType.MatchesNegative(Core.DeviceType.NULL, device.DeviceType) == true)
                return false;

            // Filter on tag
            if (!PassStringFilter(Tag, device.Tag))
                return false;

            // Filter on fixed image
            if (!PassStringFilter(FixedImage, device.FixedImage))
                return false;

            // Filter on mandatory
            if (!PassLongFilter(Mandatory, device.Mandatory))
                return false;

            // Filter on interface
            if (!PassStringFilter(Interface, device.Interface))
                return false;

            // Filter on individual instances
            if (device.InstancesSpecified)
            {
                foreach (Instance subInstance in device.Instances)
                {
                    if (!PassesFilters(subInstance))
                        return false;
                }
            }

            // Filter on individual extensions
            if (device.ExtensionsSpecified)
            {
                foreach (Extension subExtension in device.Extensions)
                {
                    if (!PassesFilters(subExtension))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a DipSwitch passes the filters
        /// </summary>
        /// <param name="dipSwitch">DipSwitch to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(DipSwitch dipSwitch)
        {
            #region Common

            // Filter on tag
            if (!PassStringFilter(Tag, dipSwitch.Tag))
                return false;

            // Filter on mask
            if (!PassStringFilter(Mask, dipSwitch.Mask))
                return false;

            // Filter on individual conditions
            if (dipSwitch.ConditionsSpecified)
            {
                foreach (Condition subCondition in dipSwitch.Conditions)
                {
                    if (!PassesFilters(subCondition, true))
                        return false;
                }
            }

            // Filter on individual locations
            if (dipSwitch.LocationsSpecified)
            {
                foreach (Location subLocation in dipSwitch.Locations)
                {
                    if (!PassesFilters(subLocation))
                        return false;
                }
            }

            // Filter on individual conditions
            if (dipSwitch.ValuesSpecified)
            {
                foreach (Setting subValue in dipSwitch.Values)
                {
                    if (!PassesFilters(subValue))
                        return false;
                }
            }

            #endregion

            #region SoftwareList

            // Filter on Part
            if (dipSwitch.PartSpecified)
            {
                if (!PassesFilters(dipSwitch.Part))
                    return false;
            }

            #endregion
            
            return true;
        }

        /// <summary>
        /// Check to see if a Disk passes the filters
        /// </summary>
        /// <param name="disk">Disk to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Disk disk)
        {
            #region Common

            // Filter on MD5
            if (!PassStringFilter(MD5, disk.MD5))
                return false;

            // Filter on SHA-1
            if (!PassStringFilter(SHA1, disk.SHA1))
                return false;

            // Filter on merge tag
            if (!PassStringFilter(Merge, disk.MergeTag))
                return false;

            // Filter on region
            if (!PassStringFilter(Region, disk.Region))
                return false;

            // Filter on index
            if (!PassStringFilter(Index, disk.Index))
                return false;

            // Filter on writable
            if (!PassBoolFilter(Writable, disk.Writable))
                return false;

            // Filter on status
            if (Status.MatchesPositive(ItemStatus.NULL, disk.ItemStatus) == false)
                return false;
            if (Status.MatchesNegative(ItemStatus.NULL, disk.ItemStatus) == true)
                return false;

            // Filter on optional
            if (!PassBoolFilter(Optional, disk.Optional))
                return false;

            #endregion

            #region SoftwareList

            // Filter on DiskArea
            if (disk.DiskAreaSpecified)
            {
                if (!PassesFilters(disk.DiskArea))
                    return false;
            }

            // Filter on Part
            if (disk.PartSpecified)
            {
                if (!PassesFilters(disk.Part))
                    return false;
            }

            #endregion
            
            return true;
        }

        /// <summary>
        /// Check to see if a DiskArea passes the filters
        /// </summary>
        /// <param name="diskArea">DiskArea to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(DiskArea diskArea)
        {
            // Filter on area name
            if (!PassStringFilter(AreaName, diskArea.Name))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Display passes the filters
        /// </summary>
        /// <param name="display">Display to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Display display)
        {
            // Filter on tag
            if (!PassStringFilter(Tag, display.Tag))
                return false;

            // Filter on display type
            if (DisplayType.MatchesPositive(Core.DisplayType.NULL, display.DisplayType) == false)
                return false;
            if (DisplayType.MatchesNegative(Core.DisplayType.NULL, display.DisplayType) == true)
                return false;

            // Filter on rotation
            if (!PassLongFilter(Rotate, display.Rotate))
                return false;

            // Filter on flipx
            if (!PassBoolFilter(FlipX, display.FlipX))
                return false;

            // Filter on width
            if (!PassLongFilter(Width, display.Width))
                return false;

            // Filter on height
            if (!PassLongFilter(Height, display.Height))
                return false;

            // Filter on refresh
            if (!PassDoubleFilter(Refresh, display.Refresh))
                return false;

            // Filter on pixclock
            if (!PassLongFilter(PixClock, display.PixClock))
                return false;

            // Filter on htotal
            if (!PassLongFilter(HTotal, display.HTotal))
                return false;

            // Filter on hbend
            if (!PassLongFilter(HBEnd, display.HBEnd))
                return false;

            // Filter on hbstart
            if (!PassLongFilter(HBStart, display.HBStart))
                return false;

            // Filter on vtotal
            if (!PassLongFilter(VTotal, display.VTotal))
                return false;

            // Filter on vbend
            if (!PassLongFilter(VBEnd, display.VBEnd))
                return false;

            // Filter on vbstart
            if (!PassLongFilter(VBStart, display.VBStart))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Driver passes the filters
        /// </summary>
        /// <param name="driver">Driver to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Driver driver)
        {
            // Filter on status
            if (SupportStatus.MatchesPositive(Core.SupportStatus.NULL, driver.Status) == false)
                return false;
            if (SupportStatus.MatchesNegative(Core.SupportStatus.NULL, driver.Status) == true)
                return false;

            // Filter on emulation
            if (EmulationStatus.MatchesPositive(Core.SupportStatus.NULL, driver.Emulation) == false)
                return false;
            if (EmulationStatus.MatchesNegative(Core.SupportStatus.NULL, driver.Emulation) == true)
                return false;

            // Filter on cocktail
            if (CocktailStatus.MatchesPositive(Core.SupportStatus.NULL, driver.Cocktail) == false)
                return false;
            if (CocktailStatus.MatchesNegative(Core.SupportStatus.NULL, driver.Cocktail) == true)
                return false;

            // Filter on savestate
            if (SaveStateStatus.MatchesPositive(Supported.NULL, driver.SaveState) == false)
                return false;
            if (SaveStateStatus.MatchesNegative(Supported.NULL, driver.SaveState) == true)
                return false;

            // Filter on requires artwork
            if (!PassBoolFilter(RequiresArtwork, driver.RequiresArtwork))
                return false;

            // Filter on unofficial
            if (!PassBoolFilter(Unofficial, driver.Unofficial))
                return false;

            // Filter on no sound hardware
            if (!PassBoolFilter(NoSoundHardware, driver.NoSoundHardware))
                return false;

            // Filter on incomplete
            if (!PassBoolFilter(Incomplete, driver.Incomplete))
                return false;

            return true;
        }

        /// <summary>
        /// Check to see if a Extension passes the filters
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Extension extension)
        {
            // Filter on item name
            if (!PassStringFilter(Extension_Name, extension.Name))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Feature passes the filters
        /// </summary>
        /// <param name="feature">Feature to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Feature feature)
        {
            // Filter on type
            if (FeatureType.MatchesPositive(Core.FeatureType.NULL, feature.Type) == false)
                return false;
            if (FeatureType.MatchesNegative(Core.FeatureType.NULL, feature.Type) == true)
                return false;

            // Filter on status
            if (FeatureStatus.MatchesPositive(Core.FeatureStatus.NULL, feature.Status) == false)
                return false;
            if (FeatureStatus.MatchesNegative(Core.FeatureStatus.NULL, feature.Status) == true)
                return false;

            // Filter on overall
            if (FeatureOverall.MatchesPositive(Core.FeatureStatus.NULL, feature.Overall) == false)
                return false;
            if (FeatureOverall.MatchesNegative(Core.FeatureStatus.NULL, feature.Overall) == true)
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Info passes the filters
        /// </summary>
        /// <param name="info">Info to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Info info)
        {
            // Filter on info value
            if (!PassStringFilter(Value, info.Value))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Input passes the filters
        /// </summary>
        /// <param name="input">Input to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Input input)
        {
            // Filter on service
            if (!PassBoolFilter(Service, input.Service))
                return false;

            // Filter on tilt
            if (!PassBoolFilter(Tilt, input.Tilt))
                return false;

            // Filter on players
            if (!PassLongFilter(Players, input.Players))
                return false;

            // Filter on coins
            if (!PassLongFilter(Coins, input.Coins))
                return false;

            // Filter on individual controls
            if (input.ControlsSpecified)
            {
                foreach (Control subControl in input.Controls)
                {
                    if (!PassesFilters(subControl))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a Instance passes the filters
        /// </summary>
        /// <param name="instance">Instance to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Instance instance)
        {
            // Filter on item name
            if (!PassStringFilter(Instance_Name, instance.Name))
                return false;

            // Filter on brief name
            if (!PassStringFilter(Instance_BriefName, instance.BriefName))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Location passes the filters
        /// </summary>
        /// <param name="location">Location to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Location location)
        {
            // Filter on item name
            if (!PassStringFilter(Location_Name, location.Name))
                return false;

            // Filter on number
            if (!PassLongFilter(Location_Number, location.Number))
                return false;

            // Filter on inverted
            if (!PassBoolFilter(Location_Inverted, location.Inverted))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Media passes the filters
        /// </summary>
        /// <param name="media">Media to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Media media)
        {
            // Filter on MD5
            if (!PassStringFilter(MD5, media.MD5))
                return false;

            // Filter on SHA-1
            if (!PassStringFilter(SHA1, media.SHA1))
                return false;

            // Filter on SHA-256
            if (!PassStringFilter(SHA256, media.SHA256))
                return false;

            // Filter on SpamSum
            if (!PassStringFilter(SpamSum, media.SpamSum))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Part passes the filters
        /// </summary>
        /// <param name="part">Part to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Part part)
        {
            // Filter on part name
            if (!PassStringFilter(Part_Name, part.Name))
                return false;

            // Filter on part interface
            if (!PassStringFilter(Part_Interface, part.Interface))
                return false;

            // Filter on features
            if (part.FeaturesSpecified)
            {
                foreach (PartFeature subPartFeature in part.Features)
                {
                    if (!PassesFilters(subPartFeature))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a PartFeature passes the filters
        /// </summary>
        /// <param name="partFeature">PartFeature to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(PartFeature partFeature)
        {
            // Filter on name
            if (!PassStringFilter(Part_Feature_Name, partFeature.Name))
                return false;

            // Filter on value
            if (!PassStringFilter(Part_Feature_Value, partFeature.Value))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Port passes the filters
        /// </summary>
        /// <param name="port">Port to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Port port)
        {
            // Filter on tag
            if (!PassStringFilter(Tag, port.Tag))
                return false;

            // Filter on individual analogs
            if (port.AnalogsSpecified)
            {
                foreach (Analog subAnalog in port.Analogs)
                {
                    if (!PassesFilters(subAnalog))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a RamOption passes the filters
        /// </summary>
        /// <param name="ramOption">RamOption to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(RamOption ramOption)
        {
            // Filter on default
            if (!PassBoolFilter(Default, ramOption.Default))
                return false;

            // Filter on content
            if (!PassStringFilter(Content, ramOption.Content))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Release passes the filters
        /// </summary>
        /// <param name="release">Release to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Release release)
        {
            // Filter on region
            if (!PassStringFilter(Region, release.Region))
                return false;

            // Filter on language
            if (!PassStringFilter(Language, release.Language))
                return false;

            // Filter on date
            if (!PassStringFilter(Date, release.Date))
                return false;

            // Filter on default
            if (!PassBoolFilter(Default, release.Default))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Rom passes the filters
        /// </summary>
        /// <param name="rom">Rom to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Rom rom)
        {
            #region Common

            // Filter on bios
            if (!PassStringFilter(Bios, rom.Bios))
                return false;

            // Filter on rom size
            if (!PassLongFilter(Size, rom.Size))
                return false;

            // Filter on CRC
            if (!PassStringFilter(CRC, rom.CRC))
                return false;

            // Filter on MD5
            if (!PassStringFilter(MD5, rom.MD5))
                return false;

            // Filter on SHA-1
            if (!PassStringFilter(SHA1, rom.SHA1))
                return false;

            // Filter on SHA-256
            if (!PassStringFilter(SHA256, rom.SHA256))
                return false;

            // Filter on SHA-384
            if (!PassStringFilter(SHA384, rom.SHA384))
                return false;

            // Filter on SHA-512
            if (!PassStringFilter(SHA512, rom.SHA512))
                return false;

            // Filter on SpamSum
            if (!PassStringFilter(SpamSum, rom.SpamSum))
                return false;

            // Filter on merge tag
            if (!PassStringFilter(Merge, rom.MergeTag))
                return false;

            // Filter on region
            if (!PassStringFilter(Region, rom.Region))
                return false;

            // Filter on offset
            if (!PassStringFilter(Offset, rom.Offset))
                return false;

            // Filter on date
            if (!PassStringFilter(Date, rom.Date))
                return false;

            // Filter on status
            if (Status.MatchesPositive(ItemStatus.NULL, rom.ItemStatus) == false)
                return false;
            if (Status.MatchesNegative(ItemStatus.NULL, rom.ItemStatus) == true)
                return false;

            // Filter on optional
            if (!PassBoolFilter(Optional, rom.Optional))
                return false;

            // Filter on inverted
            if (!PassBoolFilter(Inverted, rom.Inverted))
                return false;
            
            // Filter on MIA
            if (!PassBoolFilter(MIA, rom.MIA))
                return false;

            #endregion

            #region Archive.org

            // Filter on file source
            if (!PassStringFilter(ArchiveDotOrgSource, rom.ArchiveDotOrgSource))
                return false;

            // Filter on file format
            if (!PassStringFilter(ArchiveDotOrgFormat, rom.ArchiveDotOrgFormat))
                return false;

            // Filter on original filename
            if (!PassStringFilter(OriginalFilename, rom.OriginalFilename))
                return false;

            // Filter on rotation
            if (!PassStringFilter(Rotation, rom.Rotation))
                return false;

            // Filter on summation
            if (!PassStringFilter(Summation, rom.Summation))
                return false;

            #endregion

            #region AttractMode

            // Filter on alt name
            if (!PassStringFilter(AltName, rom.AltName))
                return false;

            // Filter on alt title
            if (!PassStringFilter(AltTitle, rom.AltTitle))
                return false;

            #endregion

            #region OpenMSX

            // Filter on original
            if (!PassStringFilter(Original, rom.Original?.Content))
                return false;

            // Filter on OpenMSX subtype
            if (OpenMSXSubType.MatchesPositive(Core.OpenMSXSubType.NULL, rom.OpenMSXSubType) == false)
                return false;
            if (OpenMSXSubType.MatchesNegative(Core.OpenMSXSubType.NULL, rom.OpenMSXSubType) == true)
                return false;

            // Filter on OpenMSX type
            if (!PassStringFilter(OpenMSXType, rom.OpenMSXType))
                return false;

            // Filter on remark
            if (!PassStringFilter(Remark, rom.Remark))
                return false;

            // Filter on boot
            if (!PassStringFilter(Boot, rom.Boot))
                return false;

            #endregion

            #region SoftwareList

            // Filter on load flag
            if (LoadFlag.MatchesPositive(Core.LoadFlag.NULL, rom.LoadFlag) == false)
                return false;
            if (LoadFlag.MatchesNegative(Core.LoadFlag.NULL, rom.LoadFlag) == true)
                return false;

            // Filter on value
            if (!PassStringFilter(Value, rom.Value))
                return false;

            // Filter on DataArea
            if (rom.DataAreaSpecified)
            {
                if (!PassesFilters(rom.DataArea))
                    return false;
            }

            // Filter on Part
            if (rom.PartSpecified)
            {
                if (!PassesFilters(rom.Part))
                    return false;
            }

            #endregion
            
            return true;
        }

        /// <summary>
        /// Check to see if a Setting passes the filters
        /// </summary>
        /// <param name="setting">Setting to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Setting setting)
        {
            // Filter on item name
            if (!PassStringFilter(Setting_Name, setting.Name))
                return false;

            // Filter on value
            if (!PassStringFilter(Setting_Value, setting.Value))
                return false;

            // Filter on default
            if (!PassBoolFilter(Setting_Default, setting.Default))
                return false;

            // Filter on individual conditions
            if (setting.ConditionsSpecified)
            {
                foreach (Condition subCondition in setting.Conditions)
                {
                    if (!PassesFilters(subCondition, true))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a SharedFeature passes the filters
        /// </summary>
        /// <param name="sharedFeature">SharedFeature to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(SharedFeature sharedFeature)
        {
            // Filter on value
            if (!PassStringFilter(Value, sharedFeature.Value))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Slot passes the filters
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Slot slot)
        {
            // Filter on individual slot options
            if (slot.SlotOptionsSpecified)
            {
                foreach (SlotOption subSlotOption in slot.SlotOptions)
                {
                    if (!PassesFilters(subSlotOption))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Check to see if a SlotOption passes the filters
        /// </summary>
        /// <param name="slotOption">SlotOption to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(SlotOption slotOption)
        {
            // Filter on item name
            if (!PassStringFilter(SlotOption_Name, slotOption.Name))
                return false;

            // Filter on device name
            if (!PassStringFilter(SlotOption_DeviceName, slotOption.DeviceName))
                return false;

            // Filter on default
            if (!PassBoolFilter(SlotOption_Default, slotOption.Default))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a SoftwareList passes the filters
        /// </summary>
        /// <param name="softwareList">SoftwareList to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(SoftwareList softwareList)
        {
            // Filter on tag
            if (!PassStringFilter(Tag, softwareList.Tag))
                return false;

            // Filter on status
            if (SoftwareListStatus.MatchesPositive(Core.SoftwareListStatus.NULL, softwareList.Status) == false)
                return false;
            if (SoftwareListStatus.MatchesNegative(Core.SoftwareListStatus.NULL, softwareList.Status) == true)
                return false;

            // Filter on filter
            if (!PassStringFilter(Filter, softwareList.Filter))
                return false;
            
            return true;
        }

        /// <summary>
        /// Check to see if a Sound passes the filters
        /// </summary>
        /// <param name="sound">Sound to check</param>
        /// <returns>True if the item passed the filter, false otherwise</returns>
        private bool PassesFilters(Sound sound)
        {
            // Filter on channels
            if (!PassLongFilter(Channels, sound.Channels))
                return false;
            
            return true;
        }

        #endregion
    }
}
