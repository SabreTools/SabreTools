using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Reports;
using System.Text.RegularExpressions;

namespace SabreTools.Library.Tools
{
    public static class Converters
    {
        #region Enum to Enum

        /// <summary>
        /// Get the field associated with each hash type
        /// </summary>
        public static Field AsField(this Hash hash)
        {
            switch (hash)
            {
                case Hash.CRC:
                    return Field.DatItem_CRC;
                case Hash.MD5:
                    return Field.DatItem_MD5;
#if NET_FRAMEWORK
                case Hash.RIPEMD160:
                    return Field.DatItem_RIPEMD160;
#endif
                case Hash.SHA1:
                    return Field.DatItem_SHA1;
                case Hash.SHA256:
                    return Field.DatItem_SHA256;
                case Hash.SHA384:
                    return Field.DatItem_SHA384;
                case Hash.SHA512:
                    return Field.DatItem_SHA512;

                default:
                    return Field.NULL;
            }
        }

        #endregion

        #region String to Enum

        /// <summary>
        /// Get DatFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>DatFormat value corresponding to the string</returns>
        public static DatFormat AsDatFormat(this string input)
        {
            switch (input?.Trim().ToLowerInvariant())
            {
                case "all":
                    return DatFormat.ALL;
                case "am":
                case "attractmode":
                    return DatFormat.AttractMode;
                case "cmp":
                case "clrmamepro":
                    return DatFormat.ClrMamePro;
                case "csv":
                    return DatFormat.CSV;
                case "dc":
                case "doscenter":
                    return DatFormat.DOSCenter;
                case "json":
                    return DatFormat.Json;
                case "lr":
                case "listrom":
                    return DatFormat.Listrom;
                case "lx":
                case "listxml":
                    return DatFormat.Listxml;
                case "md5":
                    return DatFormat.RedumpMD5;
                case "miss":
                case "missfile":
                    return DatFormat.MissFile;
                case "msx":
                case "openmsx":
                    return DatFormat.OpenMSX;
                case "ol":
                case "offlinelist":
                    return DatFormat.OfflineList;
                case "rc":
                case "romcenter":
                    return DatFormat.RomCenter;
#if NET_FRAMEWORK
                case "ripemd160":
                    return DatFormat.RedumpRIPEMD160;
#endif
                case "sd":
                case "sabredat":
                    return DatFormat.SabreDat;
                case "sfv":
                    return DatFormat.RedumpSFV;
                case "sha1":
                    return DatFormat.RedumpSHA1;
                case "sha256":
                    return DatFormat.RedumpSHA256;
                case "sha384":
                    return DatFormat.RedumpSHA384;
                case "sha512":
                    return DatFormat.RedumpSHA512;
                case "sl":
                case "softwarelist":
                    return DatFormat.SoftwareList;
                case "smdb":
                case "everdrive":
                    return DatFormat.EverdriveSMDB;
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
                case "xml":
                case "logiqx":
                    return DatFormat.Logiqx;
                default:
                    return 0x0;
            }
        }

        /// <summary>
        /// Get Field value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Field value corresponding to the string</returns>
        /// TODO: Needs to be SEVERELY overhauled. Start using dot notation for fields... (where possible)
        public static Field AsField(this string input)
        {
            // If the input is null, we return null
            if (input == null)
                return Field.NULL;

            // Normalize the input
            input = input.ToLowerInvariant();

            // Create regex strings
            string headerRegex = @"^(dat|header|datheader)[.-_\s]";
            string machineRegex = @"^(game|machine)[.-_\s]";
            string datItemRegex = @"^(item|datitem|archive|biosset|blank|disk|release|rom|sample)[.-_\s]";

            // If we have a header field
            if (Regex.IsMatch(input, headerRegex))
            {
                // Replace the match and re-normalize
                string headerInput = Regex.Replace(input, headerRegex, string.Empty)
                    .Replace(' ', '_')
                    .Replace('-', '_')
                    .Replace('.', '_');

                switch (headerInput)
                {
                    #region Common

                    case "file":
                    case "filename":
                    case "file_name":
                        return Field.DatHeader_FileName;

                    case "dat":
                    case "datname":
                    case "dat_name":
                    case "internalname":
                    case "internal_name":
                        return Field.DatHeader_Name;

                    case "desc":
                    case "description":
                        return Field.DatHeader_Description;

                    case "root":
                    case "rootdir":
                    case "root_dir":
                    case "rootdirectory":
                    case "root_directory":
                        return Field.DatHeader_RootDir;

                    case "category":
                        return Field.DatHeader_Category;

                    case "version":
                        return Field.DatHeader_Version;

                    case "date":
                    case "timestamp":
                    case "time_stamp":
                        return Field.DatHeader_Date;

                    case "author":
                        return Field.DatHeader_Author;

                    case "email":
                    case "e_mail":
                        return Field.DatHeader_Email;

                    case "homepage":
                    case "home_page":
                        return Field.DatHeader_Homepage;

                    case "url":
                        return Field.DatHeader_Url;

                    case "comment":
                        return Field.DatHeader_Comment;

                    case "header":
                    case "headerskipper":
                    case "header_skipper":
                    case "skipper":
                        return Field.DatHeader_HeaderSkipper;

                    case "dattype":
                    case "type":
                    case "superdat":
                        return Field.DatHeader_Type;

                    case "forcemerging":
                    case "force_merging":
                        return Field.DatHeader_ForceMerging;

                    case "forcenodump":
                    case "force_nodump":
                        return Field.DatHeader_ForceNodump;

                    case "forcepacking":
                    case "force_packing":
                        return Field.DatHeader_ForcePacking;

                    #endregion

                    #region ListXML

                    case "debug":
                        return Field.DatHeader_Debug;

                    case "mameconfig":
                    case "mame_config":
                        return Field.DatHeader_MameConfig;

                    #endregion

                    #region Logiqx

                    case "build":
                        return Field.DatHeader_Build;

                    case "rommode":
                    case "rom_mode":
                        return Field.DatHeader_RomMode;

                    case "biosmode":
                    case "bios_mode":
                        return Field.DatHeader_BiosMode;

                    case "samplemode":
                    case "sample_mode":
                        return Field.DatHeader_SampleMode;

                    case "lockrommode":
                    case "lockrom_mode":
                    case "lock_rommode":
                    case "lock_rom_mode":
                        return Field.DatHeader_LockRomMode;

                    case "lockbiosmode":
                    case "lockbios_mode":
                    case "lock_biosmode":
                    case "lock_bios_mode":
                        return Field.DatHeader_LockBiosMode;

                    case "locksamplemode":
                    case "locksample_mode":
                    case "lock_samplemode":
                    case "lock_sample_mode":
                        return Field.DatHeader_LockSampleMode;

                    #endregion

                    #region OfflineList

                    case "system":
                    case "plugin": // Used with RomCenter
                        return Field.DatHeader_System;

                    case "screenshotwidth":
                    case "screenshotswidth":
                    case "screenshot_width":
                    case "screenshots_width":
                        return Field.DatHeader_ScreenshotsWidth;

                    case "screenshotheight":
                    case "screenshotsheight":
                    case "screenshot_height":
                    case "screenshots_height":
                        return Field.DatHeader_ScreenshotsHeight;

                    case "info":
                    case "infos":
                        return Field.DatHeader_Infos;

                    case "info_name":
                    case "infos_name":
                        return Field.DatHeader_Info_Name;

                    case "info_visible":
                    case "infos_visible":
                        return Field.DatHeader_Info_Visible;

                    case "info_isnamingoption":
                    case "info_is_naming_option":
                    case "infos_isnamingoption":
                    case "infos_is_naming_option":
                        return Field.DatHeader_Info_IsNamingOption;

                    case "info_default":
                    case "infos_default":
                        return Field.DatHeader_Info_Default;

                    case "canopen":
                    case "can_open":
                        return Field.DatHeader_CanOpen;

                    case "romtitle":
                    case "rom_title":
                        return Field.DatHeader_RomTitle;

                    #endregion

                    #region RomCenter

                    case "rcversion":
                    case "rc_version":
                    case "romcenterversion":
                    case "romcenter_version":
                    case "rom_center_version":
                        return Field.DatHeader_RomCenterVersion;

                    #endregion
                }
            }

            // If we have a machine field
            else if (Regex.IsMatch(input, machineRegex))
            {
                // Replace the match and re-normalize
                string machineInput = Regex.Replace(input, machineRegex, string.Empty)
                    .Replace(' ', '_')
                    .Replace('-', '_')
                    .Replace('.', '_');

                switch (machineInput)
                {
                    #region Common

                    case "name":
                        return Field.Machine_Name;

                    case "comment":
                    case "extra": // Used with AttractMode
                        return Field.Machine_Comment;

                    case "desc":
                    case "description":
                        return Field.Machine_Description;

                    case "year":
                        return Field.Machine_Year;

                    case "manufacturer":
                        return Field.Machine_Manufacturer;

                    case "publisher":
                        return Field.Machine_Publisher;

                    case "category":
                        return Field.Machine_Category;

                    case "romof":
                    case "rom_of":
                        return Field.Machine_RomOf;

                    case "cloneof":
                    case "clone_of":
                        return Field.Machine_CloneOf;

                    case "sampleof":
                    case "sample_of":
                        return Field.Machine_SampleOf;

                    case "type":
                        return Field.Machine_Type;

                    #endregion

                    #region AttractMode

                    case "players":
                        return Field.Machine_Players;

                    case "rotation":
                        return Field.Machine_Rotation;

                    case "control":
                        return Field.Machine_Control;

                    case "amstatus":
                    case "am_status":
                    case "gamestatus":
                    case "supportstatus":
                    case "support_status":
                        return Field.Machine_Status;

                    case "displaycount":
                        return Field.Machine_DisplayCount;

                    case "displaytype":
                        return Field.Machine_DisplayType;

                    case "buttons":
                        return Field.Machine_Buttons;

                    #endregion

                    #region ListXML

                    case "sourcefile":
                    case "source_file":
                        return Field.Machine_SourceFile;

                    case "runnable":
                        return Field.Machine_Runnable;

                    case "devreferences":
                    case "devicereferences":
                        return Field.Machine_DeviceReferences;

                    case "devreference_name":
                    case "devicereference_name":
                        return Field.Machine_DeviceReference_Name;

                    case "displays":
                        return Field.Machine_Displays;

                    case "display_tag":
                        return Field.Machine_Display_Tag;

                    case "display_type":
                        return Field.Machine_Display_Type;

                    case "display_rotate":
                        return Field.Machine_Display_Rotate;

                    case "display_flipx":
                        return Field.Machine_Display_FlipX;

                    case "display_width":
                        return Field.Machine_Display_Width;

                    case "display_height":
                        return Field.Machine_Display_Height;

                    case "display_refresh":
                        return Field.Machine_Display_Refresh;

                    case "display_pixclock":
                        return Field.Machine_Display_PixClock;

                    case "display_htotal":
                        return Field.Machine_Display_HTotal;

                    case "display_hbend":
                        return Field.Machine_Display_HBEnd;

                    case "display_hbstart":
                        return Field.Machine_Display_HBStart;

                    case "display_vtotal":
                        return Field.Machine_Display_VTotal;

                    case "display_vbend":
                        return Field.Machine_Display_VBEnd;

                    case "display_vbstart":
                        return Field.Machine_Display_VBStart;

                    case "sounds":
                        return Field.Machine_Sounds;

                    case "sound_channels":
                        return Field.Machine_Sound_Channels;

                    case "conditions":
                        return Field.Machine_Conditions;

                    case "condition_tag":
                        return Field.Machine_Condition_Tag;

                    case "condition_mask":
                        return Field.Machine_Condition_Mask;

                    case "condition_relation":
                        return Field.Machine_Condition_Relation;

                    case "condition_value":
                        return Field.Machine_Condition_Value;

                    case "inputs":
                        return Field.Machine_Inputs;

                    case "input_service":
                        return Field.Machine_Input_Service;

                    case "input_tilt":
                        return Field.Machine_Input_Tilt;

                    case "input_players":
                        return Field.Machine_Input_Players;

                    case "input_coins":
                        return Field.Machine_Input_Coins;

                    case "input_controls":
                        return Field.Machine_Input_Controls;

                    case "input_control_type":
                        return Field.Machine_Input_Control_Type;

                    case "input_control_player":
                        return Field.Machine_Input_Control_Player;

                    case "input_control_buttons":
                        return Field.Machine_Input_Control_Buttons;

                    case "input_control_regbuttons":
                        return Field.Machine_Input_Control_RegButtons;

                    case "input_control_minimum":
                        return Field.Machine_Input_Control_Minimum;

                    case "input_control_maximum":
                        return Field.Machine_Input_Control_Maximum;

                    case "input_control_sensitivity":
                        return Field.Machine_Input_Control_Sensitivity;

                    case "input_control_keydelta":
                        return Field.Machine_Input_Control_KeyDelta;

                    case "input_control_reverse":
                        return Field.Machine_Input_Control_Reverse;

                    case "input_control_ways":
                        return Field.Machine_Input_Control_Ways;

                    case "input_control_ways2":
                        return Field.Machine_Input_Control_Ways2;

                    case "input_control_ways3":
                        return Field.Machine_Input_Control_Ways3;

                    case "dipswitches":
                        return Field.Machine_DipSwitches;

                    case "dipswitch_name":
                        return Field.Machine_DipSwitch_Name;

                    case "dipswitch_tag":
                        return Field.Machine_DipSwitch_Tag;

                    case "dipswitch_mask":
                        return Field.Machine_DipSwitch_Mask;

                    case "dipswitch_locations":
                        return Field.Machine_DipSwitch_Locations;

                    case "dipswitch_location_name":
                        return Field.Machine_DipSwitch_Location_Name;

                    case "dipswitch_location_number":
                        return Field.Machine_DipSwitch_Location_Number;

                    case "dipswitch_location_inverted":
                        return Field.Machine_DipSwitch_Location_Inverted;

                    case "dipswitch_values":
                        return Field.Machine_DipSwitch_Values;

                    case "dipswitch_value_name":
                        return Field.Machine_DipSwitch_Value_Name;

                    case "dipswitch_value_value":
                        return Field.Machine_DipSwitch_Value_Value;

                    case "dipswitch_value_default":
                        return Field.Machine_DipSwitch_Value_Default;

                    case "configurations":
                        return Field.Machine_Configurations;

                    case "configuration_name":
                        return Field.Machine_Configuration_Name;

                    case "configuration_tag":
                        return Field.Machine_Configuration_Tag;

                    case "configuration_mask":
                        return Field.Machine_Configuration_Mask;

                    case "configuration_locations":
                        return Field.Machine_Configuration_Locations;

                    case "configuration_location_name":
                        return Field.Machine_Configuration_Location_Name;

                    case "configuration_location_number":
                        return Field.Machine_Configuration_Location_Number;

                    case "configuration_location_inverted":
                        return Field.Machine_Configuration_Location_Inverted;

                    case "configuration_settings":
                        return Field.Machine_Configuration_Settings;

                    case "configuration_setting_name":
                        return Field.Machine_Configuration_Setting_Name;

                    case "configuration_setting_value":
                        return Field.Machine_Configuration_Setting_Value;

                    case "configuration_setting_default":
                        return Field.Machine_Configuration_Setting_Default;

                    case "ports":
                        return Field.Machine_Ports;

                    case "port_tag":
                        return Field.Machine_Port_Tag;

                    case "port_analogs":
                        return Field.Machine_Port_Analogs;

                    case "port_analog_mask":
                        return Field.Machine_Port_Analog_Mask;

                    case "adjusters":
                        return Field.Machine_Adjusters;

                    case "adjuster_name":
                        return Field.Machine_Adjuster_Name;

                    case "adjuster_default":
                        return Field.Machine_Adjuster_Default;

                    case "adjuster_conditions":
                        return Field.Machine_Adjuster_Conditions;

                    case "adjuster_condition_tag":
                        return Field.Machine_Adjuster_Condition_Tag;

                    case "adjuster_condition_mask":
                        return Field.Machine_Adjuster_Condition_Mask;

                    case "adjuster_condition_relation":
                        return Field.Machine_Adjuster_Condition_Relation;

                    case "adjuster_condition_value":
                        return Field.Machine_Adjuster_Condition_Value;

                    case "drivers":
                        return Field.Machine_Drivers;

                    case "driver_status":
                        return Field.Machine_Driver_Status;

                    case "driver_emulation":
                        return Field.Machine_Driver_Emulation;

                    case "driver_cocktail":
                        return Field.Machine_Driver_Cocktail;

                    case "driver_savestate":
                        return Field.Machine_Driver_SaveState;

                    case "features":
                        return Field.Machine_Features;

                    case "feature_type":
                        return Field.Machine_Feature_Type;

                    case "feature_status":
                        return Field.Machine_Feature_Status;

                    case "feature_overall":
                        return Field.Machine_Feature_Overall;

                    case "devices":
                        return Field.Machine_Devices;

                    case "device_type":
                        return Field.Machine_Device_Type;

                    case "device_tag":
                        return Field.Machine_Device_Tag;

                    case "device_fixedimage":
                        return Field.Machine_Device_FixedImage;

                    case "device_mandatory":
                        return Field.Machine_Device_Mandatory;

                    case "device_interface":
                        return Field.Machine_Device_Interface;

                    case "device_instances":
                        return Field.Machine_Device_Instances;

                    case "device_instance_name":
                        return Field.Machine_Device_Instance_Name;

                    case "device_instance_briefname":
                        return Field.Machine_Device_Instance_BriefName;

                    case "device_extensions":
                        return Field.Machine_Device_Extensions;

                    case "device_extension_name":
                        return Field.Machine_Device_Extension_Name;

                    case "slots":
                        return Field.Machine_Slots;

                    case "slot_name":
                        return Field.Machine_Slot_Name;

                    case "slot_slotoptions":
                        return Field.Machine_Slot_SlotOptions;

                    case "slot_slotoption_name":
                        return Field.Machine_Slot_SlotOption_Name;

                    case "slot_slotoption_devicename":
                        return Field.Machine_Slot_SlotOption_DeviceName;

                    case "slot_slotoption_default":
                        return Field.Machine_Slot_SlotOption_Default;

                    case "softwarelists":
                        return Field.Machine_SoftwareLists;

                    case "softwarelist_name":
                        return Field.Machine_SoftwareList_Name;

                    case "softwarelist_status":
                        return Field.Machine_SoftwareList_Status;

                    case "softwarelist_filter":
                        return Field.Machine_SoftwareList_Filter;

                    case "ramoptions":
                        return Field.Machine_RamOptions;

                    case "ramoption_default":
                        return Field.Machine_RamOption_Default;

                    #endregion

                    #region Logiqx

                    case "board":
                        return Field.Machine_Board;

                    case "rebuildto":
                    case "rebuild_to":
                        return Field.Machine_RebuildTo;

                    #endregion

                    #region Logiqx EmuArc

                    case "titleid":
                    case "title_id":
                        return Field.Machine_TitleID;

                    case "developer":
                        return Field.Machine_Developer;

                    case "genre":
                        return Field.Machine_Genre;

                    case "subgenre":
                    case "sub_genre":
                        return Field.Machine_Subgenre;

                    case "ratings":
                        return Field.Machine_Ratings;

                    case "score":
                        return Field.Machine_Score;

                    case "enabled":
                        return Field.Machine_Enabled;

                    case "crc":
                    case "hascrc":
                    case "has_crc":
                        return Field.Machine_CRC;

                    case "relatedto":
                    case "related_to":
                        return Field.Machine_RelatedTo;

                    #endregion

                    #region OpenMSX

                    case "genmsxid":
                    case "genmsx_id":
                    case "gen_msxid":
                    case "gen_msx_id":
                        return Field.Machine_GenMSXID;

                    case "system":
                    case "msxsystem":
                    case "msx_system":
                        return Field.Machine_System;

                    case "country":
                        return Field.Machine_Country;

                    #endregion

                    #region SoftwareList

                    case "supported":
                        return Field.Machine_Supported;

                    case "infos":
                        return Field.Machine_Infos;

                    case "info_name":
                        return Field.Machine_Info_Name;

                    case "info_value":
                        return Field.Machine_Info_Value;

                    case "sharedfeatures":
                        return Field.Machine_SharedFeatures;

                    case "sharedfeature_name":
                        return Field.Machine_SharedFeature_Name;

                    case "sharedfeature_value":
                        return Field.Machine_SharedFeature_Value;

                    #endregion
                }
            }

            // If we have a datitem field
            else if (Regex.IsMatch(input, datItemRegex))
            {
                // Replace the match and re-normalize
                string itemInput = Regex.Replace(input, datItemRegex, string.Empty)
                    .Replace(' ', '_')
                    .Replace('-', '_')
                    .Replace('.', '_');

                switch (itemInput)
                {
                    #region Common

                    case "name":
                        return Field.DatItem_Name;

                    case "type":
                        return Field.DatItem_Type;

                    #endregion

                    #region AttractMode

                    case "altname":
                    case "alt name":
                    case "alt-name":
                    case "altromname":
                    case "alt romname":
                    case "alt-romname":
                        return Field.DatItem_AltName;

                    case "alttitle":
                    case "alt title":
                    case "alt-title":
                    case "altromtitle":
                    case "alt romtitle":
                    case "alt-romtitle":
                        return Field.DatItem_AltTitle;

                    #endregion

                    #region OpenMSX

                    case "original":
                        return Field.DatItem_Original;

                    case "subtype":
                    case "sub_type":
                    case "openmsxsubtype":
                    case "openmsx_subtype":
                        return Field.DatItem_OpenMSXSubType;

                    case "openmsxtype":
                    case "openmsx_type":
                        return Field.DatItem_OpenMSXType;

                    case "remark":
                        return Field.DatItem_Remark;

                    case "boot":
                        return Field.DatItem_Boot;

                    #endregion

                    #region SoftwareList

                    case "part":
                        return Field.DatItem_Part;

                    case "partname":
                    case "part_name":
                        return Field.DatItem_Part_Name;

                    case "partinterface":
                    case "part_interface":
                        return Field.DatItem_Part_Interface;

                    case "features":
                        return Field.DatItem_Features;

                    case "feature_name":
                        return Field.DatItem_Feature_Name;

                    case "feature_value":
                        return Field.DatItem_Feature_Value;

                    case "areaname":
                    case "area_name":
                        return Field.DatItem_AreaName;

                    case "areasize":
                    case "area_size":
                        return Field.DatItem_AreaSize;

                    case "areawidth":
                    case "area_width":
                        return Field.DatItem_AreaWidth;

                    case "areaendinanness":
                    case "area_endianness":
                        return Field.DatItem_AreaEndianness;

                    case "value":
                        return Field.DatItem_Value;

                    case "loadflag":
                    case "load_flag":
                        return Field.DatItem_LoadFlag;

                    #endregion

                    #region Item-Specific

                    // BiosSet
                    case "default":
                        return Field.DatItem_Default;

                    case "description":
                    case "biosdescription":
                    case "bios_description":
                        return Field.DatItem_Description;

                    // Chip
                    case "tag":
                        return Field.DatItem_Tag;

                    case "chiptype":
                    case "chip_type":
                        return Field.DatItem_ChipType;

                    case "clock":
                        return Field.DatItem_Clock;

                    // Disk
                    case "md5":
                    case "md5_hash":
                        return Field.DatItem_MD5;

#if NET_FRAMEWORK
                    case "ripemd160":
                    case "ripemd160_hash":
                        return Field.DatItem_RIPEMD160;
#endif

                    case "sha1":
                    case "sha_1":
                    case "sha1hash":
                    case "sha1_hash":
                    case "sha_1hash":
                    case "sha_1_hash":
                        return Field.DatItem_SHA1;

                    case "sha256":
                    case "sha_256":
                    case "sha256hash":
                    case "sha256_hash":
                    case "sha_256hash":
                    case "sha_256_hash":
                        return Field.DatItem_SHA256;

                    case "sha384":
                    case "sha_384":
                    case "sha384hash":
                    case "sha384_hash":
                    case "sha_384hash":
                    case "sha_384_hash":
                        return Field.DatItem_SHA384;

                    case "sha512":
                    case "sha_512":
                    case "sha512hash":
                    case "sha512_hash":
                    case "sha_512hash":
                    case "sha_512_hash":
                        return Field.DatItem_SHA512;

                    case "merge":
                    case "mergetag":
                    case "merge_tag":
                        return Field.DatItem_Merge;

                    case "region":
                        return Field.DatItem_Region;

                    case "index":
                        return Field.DatItem_Index;

                    case "writable":
                        return Field.DatItem_Writable;

                    case "status":
                        return Field.DatItem_Status;

                    case "optional":
                        return Field.DatItem_Optional;

                    // Release
                    case "language":
                        return Field.DatItem_Language;

                    case "date":
                        return Field.DatItem_Date;

                    // Rom
                    case "bios":
                        return Field.DatItem_Bios;

                    case "size":
                        return Field.DatItem_Size;

                    case "crc":
                        return Field.DatItem_CRC;

                    case "offset":
                        return Field.DatItem_Offset;

                    case "inverted":
                        return Field.DatItem_Inverted;

                    #endregion
                }
            }

            // Else, we fall back on the old matching
            // TODO: Remove this entirely
            switch (input)
            {
                #region Machine

                #region Common

                case "game":
                case "gamename":
                case "game-name":
                case "machine":
                case "machinename":
                case "machine-name":
                    return Field.Machine_Name;

                case "comment":
                case "extra":
                    return Field.Machine_Comment;

                case "desc":
                case "description":
                case "gamedesc":
                case "gamedescription":
                case "game-description":
                case "game description":
                case "machinedesc":
                case "machinedescription":
                case "machine-description":
                case "machine description":
                    return Field.Machine_Description;

                case "year":
                    return Field.Machine_Year;

                case "manufacturer":
                    return Field.Machine_Manufacturer;

                case "publisher":
                    return Field.Machine_Publisher;

                case "category":
                case "gamecategory":
                case "game-category":
                case "machinecategory":
                case "machine-category":
                    return Field.Machine_Category;

                case "romof":
                    return Field.Machine_RomOf;

                case "cloneof":
                    return Field.Machine_CloneOf;

                case "sampleof":
                    return Field.Machine_SampleOf;

                case "gametype":
                case "game type":
                case "game-type":
                case "machinetype":
                case "machine type":
                case "machine-type":
                    return Field.Machine_Type;

                #endregion

                #region AttractMode

                case "players":
                    return Field.Machine_Players;

                case "rotation":
                    return Field.Machine_Rotation;

                case "control":
                    return Field.Machine_Control;

                case "amstatus":
                case "am-status":
                case "gamestatus":
                case "game-status":
                case "machinestatus":
                case "machine-status":
                case "supportstatus":
                case "support-status":
                    return Field.Machine_Status;

                case "displaycount":
                case "display-count":
                case "displays":
                    return Field.Machine_DisplayCount;

                case "displaytype":
                case "display-type":
                    return Field.Machine_DisplayType;

                case "buttons":
                    return Field.Machine_Buttons;

                #endregion

                #region ListXML

                case "sourcefile":
                case "source file":
                case "source-file":
                    return Field.Machine_SourceFile;

                case "runnable":
                    return Field.Machine_Runnable;

                case "devices":
                    return Field.Machine_DeviceReference_Name;

                case "slotoptions":
                case "slot options":
                case "slot-options":
                    return Field.Machine_Slots;

                case "infos":
                    return Field.Machine_Infos;

                #endregion

                #region Logiqx

                case "board":
                    return Field.Machine_Board;

                case "rebuildto":
                case "rebuild to":
                case "rebuild-to":
                    return Field.Machine_RebuildTo;

                #endregion

                #region Logiqx EmuArc

                case "titleid":
                case "title id":
                case "title-id":
                    return Field.Machine_TitleID;

                case "developer":
                    return Field.Machine_Developer;

                case "genre":
                    return Field.Machine_Genre;

                case "subgenre":
                    return Field.Machine_Subgenre;

                case "ratings":
                    return Field.Machine_Ratings;

                case "score":
                    return Field.Machine_Score;

                case "enabled":
                    return Field.Machine_Enabled;

                case "hascrc":
                case "has crc":
                case "has-crc":
                    return Field.Machine_CRC;

                case "relatedto":
                case "related to":
                case "related-to":
                    return Field.Machine_RelatedTo;

                #endregion

                #region OpenMSX

                case "genmsxid":
                case "genmsx id":
                case "genmsx-id":
                case "gen msx id":
                case "gen-msx-id":
                    return Field.Machine_GenMSXID;

                case "system":
                case "msxsystem":
                case "msx system":
                case "msx-system":
                    return Field.Machine_System;

                case "country":
                    return Field.Machine_Country;

                #endregion

                #region SoftwareList

                case "supported":
                    return Field.Machine_Supported;
                case "sharedfeat":
                case "shared feat":
                case "shared-feat":
                case "sharedfeature":
                case "shared feature":
                case "shared-feature":
                case "sharedfeatures":
                case "shared features":
                case "shared-features":
                    return Field.Machine_SharedFeatures;
                case "dipswitch":
                case "dip switch":
                case "dip-switch":
                case "dipswitches":
                case "dip switches":
                case "dip-switches":
                    return Field.Machine_DipSwitches;

                #endregion

                #endregion // Machine

                #region DatItem

                #region Common

                case "itemname":
                case "item-name":
                case "name":
                    return Field.DatItem_Name;
                case "itemtype":
                case "item-type":
                case "type":
                    return Field.DatItem_Type;

                #endregion

                #region AttractMode

                case "altname":
                case "alt name":
                case "alt-name":
                case "altromname":
                case "alt romname":
                case "alt-romname":
                    return Field.DatItem_AltName;
                case "alttitle":
                case "alt title":
                case "alt-title":
                case "altromtitle":
                case "alt romtitle":
                case "alt-romtitle":
                    return Field.DatItem_AltTitle;

                #endregion

                #region OpenMSX

                case "original":
                    return Field.DatItem_Original;
                case "subtype":
                case "sub type":
                case "sub-type":
                case "openmsx_subtype":
                    return Field.DatItem_OpenMSXSubType;
                case "openmsx_type":
                    return Field.DatItem_OpenMSXType;
                case "remark":
                    return Field.DatItem_Remark;
                case "boot":
                    return Field.DatItem_Boot;

                #endregion

                #region SoftwareList

                case "partname":
                case "part name":
                case "part-name":
                    return Field.DatItem_Part_Name;
                case "partinterface":
                case "part interface":
                case "part-interface":
                    return Field.DatItem_Part_Interface;
                case "features":
                    return Field.DatItem_Features;
                case "areaname":
                case "area name":
                case "area-name":
                    return Field.DatItem_AreaName;
                case "areasize":
                case "area size":
                case "area-size":
                    return Field.DatItem_AreaSize;
                case "areawidth":
                case "area width":
                case "area-width":
                    return Field.DatItem_AreaWidth;
                case "areaendinanness":
                case "area endianness":
                case "area-endianness":
                    return Field.DatItem_AreaEndianness;
                case "value":
                    return Field.DatItem_Value;
                case "loadflag":
                case "load flag":
                case "load-flag":
                    return Field.DatItem_LoadFlag;

                #endregion

                case "bios":
                    return Field.DatItem_Bios;
                case "biosdescription":
                case "bios-description":
                case "biossetdescription":
                case "biosset-description":
                case "bios-set-description":
                    return Field.DatItem_Description;

                case "tag":
                    return Field.DatItem_Tag;

                case "chiptype":
                case "chip_type":
                    return Field.DatItem_ChipType;

                case "clock":
                    return Field.DatItem_Clock;
                
                case "crc":
                case "crc32":
                    return Field.DatItem_CRC;
                case "default":
                    return Field.DatItem_Default;
                case "date":
                    return Field.DatItem_Date;
                case "equal":
                case "greater":
                case "less":
                case "size":
                    return Field.DatItem_Size;
                case "index":
                    return Field.DatItem_Index;
                case "inverted":
                    return Field.DatItem_Inverted;
                case "itemtatus":
                case "item-status":
                case "status":
                    return Field.DatItem_Status;
                case "language":
                    return Field.DatItem_Language;
                case "md5":
                    return Field.DatItem_MD5;
                case "merge":
                case "mergetag":
                case "merge-tag":
                    return Field.DatItem_Merge;
                case "offset":
                    return Field.DatItem_Offset;
                case "optional":
                    return Field.DatItem_Optional;
                case "region":
                    return Field.DatItem_Region;
#if NET_FRAMEWORK
                case "ripemd160":
                    return Field.DatItem_RIPEMD160;
#endif
                case "sha1":
                case "sha-1":
                    return Field.DatItem_SHA1;
                case "sha256":
                case "sha-256":
                    return Field.DatItem_SHA256;
                case "sha384":
                case "sha-384":
                    return Field.DatItem_SHA384;
                case "sha512":
                case "sha-512":
                    return Field.DatItem_SHA512;
                case "writable":
                    return Field.DatItem_Writable;

                #endregion

                default:
                    return Field.NULL;
            }
        }

        /// <summary>
        /// Get ItemStatus value from input string
        /// </summary>
        /// <param name="status">String to get value from</param>
        /// <returns>ItemStatus value corresponding to the string</returns>
        public static ItemStatus AsItemStatus(this string status)
        {
#if NET_FRAMEWORK
            switch (status?.ToLowerInvariant())
            {
                case "good":
                    return ItemStatus.Good;
                case "baddump":
                    return ItemStatus.BadDump;
                case "nodump":
                case "yes":
                    return ItemStatus.Nodump;
                case "verified":
                    return ItemStatus.Verified;
                case "none":
                case "no":
                default:
                    return ItemStatus.None;
            }
#else
            return status?.ToLowerInvariant() switch
            {
                "good" => ItemStatus.Good,
                "baddump" => ItemStatus.BadDump,
                "nodump" => ItemStatus.Nodump,
                "yes" => ItemStatus.Nodump,
                "verified" => ItemStatus.Verified,
                "none" => ItemStatus.None,
                "no" => ItemStatus.None,
                _ => ItemStatus.None,
            };
#endif
        }

        /// <summary>
        /// Get ItemType? value from input string
        /// </summary>
        /// <param name="itemType">String to get value from</param>
        /// <returns>ItemType? value corresponding to the string</returns>
        public static ItemType? AsItemType(this string itemType)
        {
#if NET_FRAMEWORK
            switch (itemType?.ToLowerInvariant())
            {
                case "archive":
                    return ItemType.Archive;
                case "biosset":
                    return ItemType.BiosSet;
                case "blank":
                    return ItemType.Blank;
                case "chip":
                    return ItemType.Chip;
                case "disk":
                    return ItemType.Disk;
                case "media":
                    return ItemType.Media;
                case "release":
                    return ItemType.Release;
                case "rom":
                    return ItemType.Rom;
                case "sample":
                    return ItemType.Sample;
                default:
                    return null;
            }
#else
            return itemType?.ToLowerInvariant() switch
            {
                "archive" => ItemType.Archive,
                "biosset" => ItemType.BiosSet,
                "blank" => ItemType.Blank,
                "chip" => ItemType.Chip,
                "disk" => ItemType.Disk,
                "media" => ItemType.Media,
                "release" => ItemType.Release,
                "rom" => ItemType.Rom,
                "sample" => ItemType.Sample,
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get MachineType value from input string
        /// </summary>
        /// <param name="gametype">String to get value from</param>
        /// <returns>MachineType value corresponding to the string</returns>
        public static MachineType AsMachineType(this string gametype)
        {
#if NET_FRAMEWORK
            switch (gametype?.ToLowerInvariant())
            {
                case "bios":
                    return MachineType.Bios;
                case "dev":
                case "device":
                    return MachineType.Device;
                case "mech":
                case "mechanical":
                    return MachineType.Mechanical;
                case "none":
                default:
                    return MachineType.NULL;
            }
#else
            return gametype?.ToLowerInvariant() switch
            {
                "bios" => MachineType.Bios,
                "dev" => MachineType.Device,
                "device" => MachineType.Device,
                "mech" => MachineType.Mechanical,
                "mechanical" => MachineType.Mechanical,
                "none" => MachineType.NULL,
                _ => MachineType.NULL,
            };
#endif
        }

        /// <summary>
        /// Get MergingFlag value from input string
        /// </summary>
        /// <param name="merging">String to get value from</param>
        /// <returns>MergingFlag value corresponding to the string</returns>
        public static MergingFlag AsMergingFlag(this string merging)
        {
#if NET_FRAMEWORK
            switch (merging?.ToLowerInvariant())
            {
                case "split":
                    return MergingFlag.Split;
                case "merged":
                    return MergingFlag.Merged;
                case "nonmerged":
                case "unmerged":
                    return MergingFlag.NonMerged;
                case "full":
                    return MergingFlag.Full;
                case "device":
                    return MergingFlag.Device;
                case "none":
                default:
                    return MergingFlag.None;
            }
#else
            return merging?.ToLowerInvariant() switch
            {
                "split" => MergingFlag.Split,
                "merged" => MergingFlag.Merged,
                "nonmerged" => MergingFlag.NonMerged,
                "unmerged" => MergingFlag.NonMerged,
                "full" => MergingFlag.Full,
                "none" => MergingFlag.None,
                _ => MergingFlag.None,
            };
#endif
        }

        /// <summary>
        /// Get NodumpFlag value from input string
        /// </summary>
        /// <param name="nodump">String to get value from</param>
        /// <returns>NodumpFlag value corresponding to the string</returns>
        public static NodumpFlag AsNodumpFlag(this string nodump)
        {
#if NET_FRAMEWORK
            switch (nodump?.ToLowerInvariant())
            {
                case "obsolete":
                    return NodumpFlag.Obsolete;
                case "required":
                    return NodumpFlag.Required;
                case "ignore":
                    return NodumpFlag.Ignore;
                case "none":
                default:
                    return NodumpFlag.None;
            }
#else
            return nodump?.ToLowerInvariant() switch
            {
                "obsolete" => NodumpFlag.Obsolete,
                "required" => NodumpFlag.Required,
                "ignore" => NodumpFlag.Ignore,
                "none" => NodumpFlag.None,
                _ => NodumpFlag.None,
            };
#endif
        }

        /// <summary>
        /// Get OpenMSXSubType value from input string
        /// </summary>
        /// <param name="itemType">String to get value from</param>
        /// <returns>OpenMSXSubType value corresponding to the string</returns>
        public static OpenMSXSubType AsOpenMSXSubType(this string itemType)
        {
#if NET_FRAMEWORK
            switch (itemType?.ToLowerInvariant())
            {
                case "rom":
                    return OpenMSXSubType.Rom;
                case "megarom":
                    return OpenMSXSubType.MegaRom;
                case "sccpluscart":
                    return OpenMSXSubType.SCCPlusCart;
                default:
                    return OpenMSXSubType.NULL;
            }
#else
            return itemType?.ToLowerInvariant() switch
            {
                "rom" => OpenMSXSubType.Rom,
                "megarom" => OpenMSXSubType.MegaRom,
                "sccpluscart" => OpenMSXSubType.SCCPlusCart,
                _ => OpenMSXSubType.NULL,
            };
#endif
        }

        /// <summary>
        /// Get PackingFlag value from input string
        /// </summary>
        /// <param name="packing">String to get value from</param>
        /// <returns>PackingFlag value corresponding to the string</returns>
        public static PackingFlag AsPackingFlag(this string packing)
        {
#if NET_FRAMEWORK
            switch (packing?.ToLowerInvariant())
            {
                case "yes":
                case "zip":
                    return PackingFlag.Zip;
                case "no":
                case "unzip":
                    return PackingFlag.Unzip;
                case "none":
                default:
                    return PackingFlag.None;
            }
#else
            return packing?.ToLowerInvariant() switch
            {
                "yes" => PackingFlag.Zip,
                "zip" => PackingFlag.Zip,
                "no" => PackingFlag.Unzip,
                "unzip" => PackingFlag.Unzip,
                "none" => PackingFlag.None,
                _ => PackingFlag.None,
            };
#endif
        }

        /// <summary>
        /// Get Runnable value from input string
        /// </summary>
        /// <param name="runnable">String to get value from</param>
        /// <returns>Runnable value corresponding to the string</returns>
        public static Runnable AsRunnable(this string runnable)
        {
#if NET_FRAMEWORK
            switch (runnable?.ToLowerInvariant())
            {
                case "no":
                    return Runnable.No;
                case "partial":
                    return Runnable.Partial;
                case "yes":
                    return Runnable.Yes;
                default:
                    return Runnable.NULL;
            }
#else
            return runnable?.ToLowerInvariant() switch
            {
                "no" => Runnable.No,
                "partial" => Runnable.Partial,
                "yes" => Runnable.Yes,
                _ => Runnable.NULL,
            };
#endif
        }

        /// <summary>
        /// Get StatReportFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>StatReportFormat value corresponding to the string</returns>
        public static StatReportFormat AsStatReportFormat(this string input)
        {
#if NET_FRAMEWORK
            switch (input?.Trim().ToLowerInvariant())
            {
                case "all":
                    return StatReportFormat.All;
                case "csv":
                    return StatReportFormat.CSV;
                case "html":
                    return StatReportFormat.HTML;
                case "ssv":
                    return StatReportFormat.SSV;
                case "text":
                    return StatReportFormat.Textfile;
                case "tsv":
                    return StatReportFormat.TSV;
                default:
                    return 0x0;
            }
#else
            return input?.Trim().ToLowerInvariant() switch
            {
                "all" => StatReportFormat.All,
                "csv" => StatReportFormat.CSV,
                "html" => StatReportFormat.HTML,
                "ssv" => StatReportFormat.SSV,
                "text" => StatReportFormat.Textfile,
                "tsv" => StatReportFormat.TSV,
                _ => 0x0,
            };
#endif
        }

        /// <summary>
        /// Get Supported value from input string
        /// </summary>
        /// <param name="supported">String to get value from</param>
        /// <returns>Supported value corresponding to the string</returns>
        public static Supported AsSupported(this string supported)
        {
#if NET_FRAMEWORK
            switch (supported?.ToLowerInvariant())
            {
                case "no":
                    return Supported.No;
                case "partial":
                    return Supported.Partial;
                case "yes":
                    return Supported.Yes;
                default:
                    return Supported.NULL;
            }
#else
            return supported?.ToLowerInvariant() switch
            {
                "no" => Supported.No,
                "partial" => Supported.Partial,
                "yes" => Supported.Yes,
                _ => Supported.NULL,
            };
#endif
        }

        /// <summary>
        /// Get bool? value from input string
        /// </summary>
        /// <param name="yesno">String to get value from</param>
        /// <returns>bool? corresponding to the string</returns>
        public static bool? AsYesNo(this string yesno)
        {
#if NET_FRAMEWORK
            switch (yesno?.ToLowerInvariant())
            {
                case "yes":
                case "true":
                    return true;
                case "no":
                case "false":
                    return false;
                default:
                    return null;
            }
#else
            return yesno?.ToLowerInvariant() switch
            {
                "yes" => true,
                "true" => true,
                "no" => false,
                "false" => false,
                _ => null,
            };
#endif
        }

        #endregion

        #region Enum to String

        // TODO: DatFormat -> string
        // TODO: Field -> string

        /// <summary>
        /// Get string value from input ItemStatus
        /// </summary>
        /// <param name="status">ItemStatus to get value from</param>
        /// <param name="yesno">True to use Yes/No format instead</param>
        /// <returns>String value corresponding to the ItemStatus</returns>
        public static string FromItemStatus(this ItemStatus status, bool yesno)
        {
#if NET_FRAMEWORK
            switch (status)
            {
                case ItemStatus.Good:
                    return "good";
                case ItemStatus.BadDump:
                    return "baddump";
                case ItemStatus.Nodump:
                    return yesno ? "yes" : "nodump";
                case ItemStatus.Verified:
                    return "verified";
                default:
                    return null;
            }
#else
            return status switch
            {
                ItemStatus.Good => "good",
                ItemStatus.BadDump => "baddump",
                ItemStatus.Nodump => yesno ? "yes" : "nodump",
                ItemStatus.Verified => "verified",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input ItemType?
        /// </summary>
        /// <param name="itemType">ItemType? to get value from</param>
        /// <returns>String value corresponding to the ItemType?</returns>
        public static string FromItemType(this ItemType? itemType)
        {
#if NET_FRAMEWORK
            switch (itemType)
            {
                case ItemType.Archive:
                    return "archive";
                case ItemType.BiosSet:
                    return "biosset";
                case ItemType.Blank:
                    return "blank";
                case ItemType.Chip:
                    return "chip";
                case ItemType.Disk:
                    return "disk";
                case ItemType.Media:
                    return "media";
                case ItemType.Release:
                    return "release";
                case ItemType.Rom:
                    return "rom";
                case ItemType.Sample:
                    return "sample";
                default:
                    return null;
            }
#else
            return itemType switch
            {
                ItemType.Archive => "archive",
                ItemType.BiosSet => "biosset",
                ItemType.Blank => "blank",
                ItemType.Chip => "chip",
                ItemType.Disk => "disk",
                ItemType.Media => "media",
                ItemType.Release => "release",
                ItemType.Rom => "rom",
                ItemType.Sample => "sample",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input MachineType
        /// </summary>
        /// <param name="gametype">MachineType to get value from</param>
        /// <param name="romCenter">True to use old naming instead</param>
        /// <returns>String value corresponding to the MachineType</returns>
        public static string FromMachineType(this MachineType gametype, bool old)
        {
#if NET_FRAMEWORK
            switch (gametype)
            {
                case MachineType.Bios:
                    return "bios";
                case MachineType.Device:
                    return old ? "dev" : "device";
                case MachineType.Mechanical:
                    return old ? "mech" : "mechanical";
                default:
                    return null;
            }
#else
            return gametype switch
            {
                MachineType.Bios => "bios",
                MachineType.Device => old ? "dev" : "device",
                MachineType.Mechanical => old ? "mech" : "mechanical",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input MergingFlag
        /// </summary>
        /// <param name="merging">MergingFlag to get value from</param>
        /// <param name="romCenter">True to use RomCenter naming instead</param>
        /// <returns>String value corresponding to the MergingFlag</returns>
        public static string FromMergingFlag(this MergingFlag merging, bool romCenter)
        {
#if NET_FRAMEWORK
            switch (merging)
            {
                case MergingFlag.Split:
                    return "split";
                case MergingFlag.Merged:
                    return "merged";
                case MergingFlag.NonMerged:
                    return romCenter ? "unmerged" : "nonmerged";
                case MergingFlag.Full:
                    return "full";
                case MergingFlag.Device:
                    return "device";
                default:
                    return null;
            }
#else
            return merging switch
            {
                MergingFlag.Split => "split",
                MergingFlag.Merged => "merged",
                MergingFlag.NonMerged => romCenter ? "unmerged" : "nonmerged",
                MergingFlag.Full => "full",
                MergingFlag.Device => "device",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input NodumpFlag
        /// </summary>
        /// <param name="nodump">NodumpFlag to get value from</param>
        /// <returns>String value corresponding to the NodumpFlag</returns>
        public static string FromNodumpFlag(this NodumpFlag nodump)
        {
#if NET_FRAMEWORK
            switch (nodump)
            {
                case NodumpFlag.Obsolete:
                    return "obsolete";
                case NodumpFlag.Required:
                    return "required";
                case NodumpFlag.Ignore:
                    return "ignore";
                default:
                    return null;
            }
#else
            return nodump switch
            {
                NodumpFlag.Obsolete => "obsolete",
                NodumpFlag.Required => "required",
                NodumpFlag.Ignore => "ignore",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input OpenMSXSubType
        /// </summary>
        /// <param name="itemType">OpenMSXSubType to get value from</param>
        /// <returns>String value corresponding to the OpenMSXSubType</returns>
        public static string FromOpenMSXSubType(this OpenMSXSubType itemType)
        {
#if NET_FRAMEWORK
            switch (itemType)
            {
                case OpenMSXSubType.Rom:
                    return "rom";
                case OpenMSXSubType.MegaRom:
                    return "megarom";
                case OpenMSXSubType.SCCPlusCart:
                    return "sccpluscart";
                default:
                    return null;
            }
#else
            return itemType switch
            {
                OpenMSXSubType.Rom => "rom",
                OpenMSXSubType.MegaRom => "megarom",
                OpenMSXSubType.SCCPlusCart => "sccpluscart",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input PackingFlag
        /// </summary>
        /// <param name="packing">PackingFlag to get value from</param>
        /// <param name="yesno">True to use Yes/No format instead</param>
        /// <returns>String value corresponding to the PackingFlag</returns>
        public static string FromPackingFlag(this PackingFlag packing, bool yesno)
        {
#if NET_FRAMEWORK
            switch (packing)
            {
                case PackingFlag.Zip:
                    return yesno ? "yes" : "zip";
                case PackingFlag.Unzip:
                    return yesno ? "no" : "unzip";
                default:
                    return null;
            }
#else
            return packing switch
            {
                PackingFlag.Zip => yesno ? "yes" : "zip",
                PackingFlag.Unzip => yesno ? "yes" : "zip",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input Runnable
        /// </summary>
        /// <param name="runnable">Runnable to get value from</param>
        /// <returns>String value corresponding to the Runnable</returns>
        public static string FromRunnable(this Runnable runnable)
        {
#if NET_FRAMEWORK
            switch (runnable)
            {
                case Runnable.No:
                    return "no";
                case Runnable.Partial:
                    return "partial";
                case Runnable.Yes:
                    return "yes";
                default:
                    return null;
            }
#else
            return runnable switch
            {
                Runnable.No => "no",
                Runnable.Partial => "partial",
                Runnable.Yes => "yes",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input StatReportFormat
        /// </summary>
        /// <param name="input">StatReportFormat to get value from</param>
        /// <returns>String value corresponding to the StatReportFormat</returns>
        public static string FromStatReportFormat(this StatReportFormat input)
        {
#if NET_FRAMEWORK
            switch (input)
            {
                case StatReportFormat.All:
                    return "all";
                case StatReportFormat.CSV:
                    return "csv";
                case StatReportFormat.HTML:
                    return "html";
                case StatReportFormat.SSV:
                    return "ssv";
                case StatReportFormat.Textfile:
                    return "text";
                case StatReportFormat.TSV:
                    return "tsv";
                default:
                    return null;
            }
#else
            return input switch
            {
                StatReportFormat.All => "all",
                StatReportFormat.CSV => "csv",
                StatReportFormat.HTML => "html",
                StatReportFormat.SSV => "ssv",
                StatReportFormat.Textfile => "text",
                StatReportFormat.TSV => "tsv",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input Supported
        /// </summary>
        /// <param name="supported">Supported to get value from</param>
        /// <returns>String value corresponding to the Supported</returns>
        public static string FromSupported(this Supported supported)
        {
#if NET_FRAMEWORK
            switch (supported)
            {
                case Supported.No:
                    return "no";
                case Supported.Partial:
                    return "partial";
                case Supported.Yes:
                    return "yes";
                default:
                    return null;
            }
#else
            return supported switch
            {
                Supported.No => "no",
                Supported.Partial => "partial",
                Supported.Yes => "yes",
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Get string value from input bool?
        /// </summary>
        /// <param name="yesno">bool? to get value from</param>
        /// <returns>String corresponding to the bool?</returns>
        public static string FromYesNo(this bool? yesno)
        {
#if NET_FRAMEWORK
            switch (yesno)
            {
                case true:
                    return "yes";
                case false:
                    return "no";
                default:
                    return null;
            }
#else
            return yesno switch
            {
                true => "yes",
                false => "no",
                _ => null,
            };
#endif
        }

        #endregion
    }
}
