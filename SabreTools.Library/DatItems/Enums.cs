using System;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Determines which type of duplicate a file is
    /// </summary>
    [Flags]
    public enum DupeType
    {
        // Type of match
        Hash = 1 << 0,
        All = 1 << 1,

        // Location of match
        Internal = 1 << 2,
        External = 1 << 3,
    }

    /// <summary>
    /// List of valid field types within a DatItem/Machine
    /// </summary>
    /// TODO: Move this to a more common location
    /// TODO: Should this be split into separate enums?
    public enum Field : int
    {
        NULL = 0,

        #region DatHeader

        #region Common

        DatHeader_FileName,
        DatHeader_Name,
        DatHeader_Description,
        DatHeader_RootDir,
        DatHeader_Category,
        DatHeader_Version,
        DatHeader_Date,
        DatHeader_Author,
        DatHeader_Email,
        DatHeader_Homepage,
        DatHeader_Url,
        DatHeader_Comment,
        DatHeader_HeaderSkipper,
        DatHeader_Type,
        DatHeader_ForceMerging,
        DatHeader_ForceNodump,
        DatHeader_ForcePacking,

        #endregion

        #region ListXML

        DatHeader_Debug,
        DatHeader_MameConfig,

        #endregion

        #region Logiqx

        DatHeader_Build,
        DatHeader_RomMode,
        DatHeader_BiosMode,
        DatHeader_SampleMode,
        DatHeader_LockRomMode,
        DatHeader_LockBiosMode,
        DatHeader_LockSampleMode,

        #endregion

        #region OfflineList

        DatHeader_System,
        DatHeader_ScreenshotsWidth,
        DatHeader_ScreenshotsHeight,
        DatHeader_CanOpen,
        DatHeader_RomTitle,

        // Infos
        DatHeader_Infos,
        DatHeader_Info_Name,
        DatHeader_Info_Visible,
        DatHeader_Info_IsNamingOption,
        DatHeader_Info_Default,

        #endregion

        #region RomCenter

        DatHeader_RomCenterVersion,

        #endregion

        #endregion // DatHeader

        #region Machine

        #region Common

        Machine_Name,
        Machine_Comment,
        Machine_Description,
        Machine_Year,
        Machine_Manufacturer,
        Machine_Publisher,
        Machine_Category,
        Machine_RomOf,
        Machine_CloneOf,
        Machine_SampleOf,
        Machine_Type,

        #endregion

        #region AttractMode

        Machine_Players,
        Machine_Rotation,
        Machine_Control,
        Machine_Status,
        Machine_DisplayCount,
        Machine_DisplayType,
        Machine_Buttons,

        #endregion

        #region ListXML

        Machine_SourceFile,
        Machine_Runnable,

        // DeviceReferences
        Machine_DeviceReferences, // TODO: Double-check DeviceReferences usage
        Machine_DeviceReference_Name,

        // Displays
        Machine_Displays, // TODO: Implement Displays usage
        Machine_Display_Tag,
        Machine_Display_Type,
        Machine_Display_Rotate,
        Machine_Display_FlipX,
        Machine_Display_Width,
        Machine_Display_Height,
        Machine_Display_Refresh,
        Machine_Display_PixClock,
        Machine_Display_HTotal,
        Machine_Display_HBEnd,
        Machine_Display_HBStart,
        Machine_Display_VTotal,
        Machine_Display_VBEnd,
        Machine_Display_VBStart,

        // Sounds
        Machine_Sounds, // TODO: Implement Sounds usage
        Machine_Sound_Channels,

        // Conditions
        Machine_Conditions, // TODO: Implement Conditions usage
        Machine_Condition_Tag,
        Machine_Condition_Mask,
        Machine_Condition_Relation,
        Machine_Condition_Value,

        // Inputs
        Machine_Inputs, // TODO: Implement Inputs usage
        Machine_Input_Service,
        Machine_Input_Tilt,
        Machine_Input_Players,
        Machine_Input_Coins,

        // Inputs.Controls
        Machine_Input_Controls,
        Machine_Input_Control_Type,
        Machine_Input_Control_Player,
        Machine_Input_Control_Buttons,
        Machine_Input_Control_RegButtons,
        Machine_Input_Control_Minimum,
        Machine_Input_Control_Maximum,
        Machine_Input_Control_Sensitivity,
        Machine_Input_Control_KeyDelta,
        Machine_Input_Control_Reverse,
        Machine_Input_Control_Ways,
        Machine_Input_Control_Ways2,
        Machine_Input_Control_Ways3,

        // DipSwitches
        Machine_DipSwitches, // TODO: Implement DipSwitches usage
        Machine_DipSwitch_Name,
        Machine_DipSwitch_Tag,
        Machine_DipSwitch_Mask,

        // DipSwitches.Locations
        Machine_DipSwitch_Locations,
        Machine_DipSwitch_Location_Name,
        Machine_DipSwitch_Location_Number,
        Machine_DipSwitch_Location_Inverted,

        // DipSwitches.Values
        Machine_DipSwitch_Values,
        Machine_DipSwitch_Value_Name,
        Machine_DipSwitch_Value_Value,
        Machine_DipSwitch_Value_Default,

        // Configurations
        Machine_Configurations, // TODO: Implement Configurations usage
        Machine_Configuration_Name,
        Machine_Configuration_Tag,
        Machine_Configuration_Mask,

        // Configurations.Locations
        Machine_Configuration_Locations,
        Machine_Configuration_Location_Name,
        Machine_Configuration_Location_Number,
        Machine_Configuration_Location_Inverted,

        // Configurations.Settings
        Machine_Configuration_Settings,
        Machine_Configuration_Setting_Name,
        Machine_Configuration_Setting_Value,
        Machine_Configuration_Setting_Default,

        // Ports
        Machine_Ports, // TODO: Implement Ports usage
        Machine_Port_Tag,

        // Ports.Analogs
        Machine_Port_Analogs,
        Machine_Port_Analog_Mask,

        // Adjusters
        Machine_Adjusters, // TODO: Implement Adjusters usage
        Machine_Adjuster_Name,
        Machine_Adjuster_Default,

        // Adjusters.Conditions
        Machine_Adjuster_Conditions,
        Machine_Adjuster_Condition_Tag,
        Machine_Adjuster_Condition_Mask,
        Machine_Adjuster_Condition_Relation,
        Machine_Adjuster_Condition_Value,

        // Drivers
        Machine_Drivers, // TODO: Implement Drivers usage
        Machine_Driver_Status,
        Machine_Driver_Emulation,
        Machine_Driver_Cocktail,
        Machine_Driver_SaveState,

        // Features
        Machine_Features, // TODO: Implement Features usage
        Machine_Feature_Type,
        Machine_Feature_Status,
        Machine_Feature_Overall,

        // Devices
        Machine_Devices, // TODO: Implement Devices usage
        Machine_Device_Type,
        Machine_Device_Tag,
        Machine_Device_FixedImage,
        Machine_Device_Mandatory,
        Machine_Device_Interface,

        // Devices.Instances
        Machine_Device_Instances,
        Machine_Device_Instance_Name,
        Machine_Device_Instance_BriefName,

        // Devices.Extensions
        Machine_Device_Extensions,
        Machine_Device_Extension_Name,

        // Slots
        Machine_Slots, // TODO: Fix Slots usage
        Machine_Slot_Name,

        // Slots.SlotOptions
        Machine_Slot_SlotOptions,
        Machine_Slot_SlotOption_Name,
        Machine_Slot_SlotOption_DeviceName,
        Machine_Slot_SlotOption_Default,

        // SoftwareLists
        Machine_SoftwareLists, // TODO: Implement SoftwareLists usage
        Machine_SoftwareList_Name,
        Machine_SoftwareList_Status,
        Machine_SoftwareList_Filter,

        // RamOptions
        Machine_RamOptions, // TODO: Implement RamOptions usage
        Machine_RamOption_Default,

        #endregion

        #region Logiqx

        Machine_Board,
        Machine_RebuildTo,

        #endregion

        #region Logiqx EmuArc

        Machine_TitleID,
        Machine_Developer,
        Machine_Genre,
        Machine_Subgenre,
        Machine_Ratings,
        Machine_Score,
        Machine_Enabled,
        Machine_CRC,
        Machine_RelatedTo,

        #endregion

        #region OpenMSX

        Machine_GenMSXID,
        Machine_System,
        Machine_Country,

        #endregion

        #region SoftwareList

        Machine_Supported,

        // Infos
        Machine_Infos, // TODO: Fix usage of Infos
        Machine_Info_Name,
        Machine_Info_Value,

        // SharedFeatures
        Machine_SharedFeatures, // TODO: Fix usage of SharedFeatures
        Machine_SharedFeature_Name,
        Machine_SharedFeature_Value,

        #endregion

        #endregion // Machine

        #region DatItem

        #region Common

        DatItem_Name,
        DatItem_Type,

        #endregion

        #region AttractMode

        DatItem_AltName,
        DatItem_AltTitle,

        #endregion

        #region OpenMSX

        DatItem_Original,
        DatItem_OpenMSXSubType,
        DatItem_OpenMSXType,
        DatItem_Remark,
        DatItem_Boot,

        #endregion

        #region SoftwareList

        // Part
        DatItem_Part, // TODO: Fully implement Part
        DatItem_Part_Name,
        DatItem_Part_Interface,

        // Feature
        DatItem_Features, // TODO: Fully implement Feature
        DatItem_Feature_Name,
        DatItem_Feature_Value,

        DatItem_AreaName,
        DatItem_AreaSize,
        DatItem_AreaWidth,
        DatItem_AreaEndianness,
        DatItem_Value,
        DatItem_LoadFlag,

        #endregion

        #region Item-Specific

        // BiosSet
        DatItem_Default,
        DatItem_Description,

        // Chip
        DatItem_Tag,
        DatItem_ChipType,
        DatItem_Clock,

        // Disk
        DatItem_MD5,
        DatItem_SHA1,
        DatItem_Merge,
        DatItem_Region,
        DatItem_Index,
        DatItem_Writable,
        DatItem_Status,
        DatItem_Optional,

        // Media
        DatItem_SHA256,

        // Release
        DatItem_Language,
        DatItem_Date,

        // Rom
        DatItem_Bios,
        DatItem_Size,
        DatItem_CRC,
#if NET_FRAMEWORK
        DatItem_RIPEMD160,
#endif
        DatItem_SHA384,
        DatItem_SHA512,
        DatItem_Offset,
        DatItem_Inverted,

        #endregion

        #endregion // DatItem
    }

    /// <summary>
    /// Determine the status of the item
    /// </summary>
    [Flags]
    public enum ItemStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0x00,

        None = 1 << 0,
        Good = 1 << 1,
        BadDump = 1 << 2,
        Nodump = 1 << 3,
        Verified = 1 << 4,
    }

    /// <summary>
    /// Determine what type of file an item is
    /// </summary>
    public enum ItemType
    {
        Rom = 0,
        Disk = 1,
        Sample = 2,
        Release = 3,
        BiosSet = 4,
        Archive = 5,
        Chip = 6,
        Media = 7,

        Blank = 99, // This is not a real type, only used internally
    }

    /// <summary>
    /// Determine which OpenMSX subtype an item is
    /// </summary>
    [Flags]
    public enum OpenMSXSubType
    {
        NULL = 0,
        Rom = 1,
        MegaRom = 2,
        SCCPlusCart = 3,
    }

    /// <summary>
    /// Determine what type of machine it is
    /// </summary>
    [Flags]
    public enum MachineType
    {
        NULL = 0x00,
        Bios = 1 << 0,
        Device = 1 << 1,
        Mechanical = 1 << 2,
    }

    /// <summary>
    /// Determine machine runnable status
    /// </summary>
    [Flags]
    public enum Runnable
    {
        NULL,
        No,
        Partial,
        Yes,
    }

    /// <summary>
    /// Determine machine support status
    /// </summary>
    [Flags]
    public enum Supported
    {
        NULL,
        No,
        Partial,
        Yes,
    }
}
