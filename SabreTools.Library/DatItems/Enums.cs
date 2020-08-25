﻿using System;

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
        Machine_SupportStatus,
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

        // Chips
        Machine_Chips, // TODO: Implement Chips usage
        Machine_Chip_Name,
        Machine_Chip_Tag,
        Machine_Chip_Type,
        Machine_Chip_Clock,

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
        Machine_InputService,
        Machine_InputTilt,
        Machine_InputPlayers,
        Machine_InputCoins,

        // Inputs.Controls
        Machine_InputControls,
        Machine_InputControl_Type,
        Machine_InputControl_Player,
        Machine_InputControl_Buttons,
        Machine_InputControl_RegButtons,
        Machine_InputControl_Minimum,
        Machine_InputControl_Maximum,
        Machine_InputControl_Sensitivity,
        Machine_InputControl_KeyDelta,
        Machine_InputControl_Reverse,
        Machine_InputControl_Ways,
        Machine_InputControl_Ways2,
        Machine_InputControl_Ways3,

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
        Machine_Ports_Tag,

        // Ports.Analogs
        Machine_Ports_Analogs,
        Machine_Ports_Analog_Mask,

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
        Machine_HasCrc,
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

        // TODO: Left off here on renaming
        #region SoftwareList

        PartName,
        PartInterface,
        Features,
        AreaName,
        AreaSize,
        AreaWidth,
        AreaEndianness,
        Value,
        LoadFlag,

        #endregion

        // BiosSet
        Default,
        BiosDescription,

        // Disk
        MD5,
#if NET_FRAMEWORK
        RIPEMD160,
#endif
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        Merge,
        Region,
        Index,
        Writable,
        Optional,
        Status,

        // Release
        Language,
        Date,

        // Rom
        Bios,
        Size,
        CRC,
        Offset,
        Inverted,

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
