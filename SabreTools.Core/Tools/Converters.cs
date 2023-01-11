using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SabreTools.Core.Tools
{
    public static class Converters
    {
        #region Enum to Enum

        /// <summary>
        /// Get the DatItemFields associated with each hash type
        /// </summary>
        public static List<DatItemField> AsDatItemFields(this Hash hash)
        {
            List<DatItemField> fields = new List<DatItemField>();

            if (hash.HasFlag(Hash.CRC))
                fields.Add(DatItemField.CRC);
            if (hash.HasFlag(Hash.MD5))
                fields.Add(DatItemField.MD5);
            if (hash.HasFlag(Hash.SHA1))
                fields.Add(DatItemField.SHA1);
            if (hash.HasFlag(Hash.SHA256))
                fields.Add(DatItemField.SHA256);
            if (hash.HasFlag(Hash.SHA384))
                fields.Add(DatItemField.SHA384);
            if (hash.HasFlag(Hash.SHA512))
                fields.Add(DatItemField.SHA512);
            if (hash.HasFlag(Hash.SpamSum))
                fields.Add(DatItemField.SpamSum);

            return fields;
        }

        #endregion

        #region String to Enum

        /// <summary>
        /// Get ChipType value from input string
        /// </summary>
        /// <param name="chipType">String to get value from</param>
        /// <returns>ChipType value corresponding to the string</returns>
        public static ChipType AsChipType(this string chipType)
        {
            return chipType?.ToLowerInvariant() switch
            {
                "cpu" => ChipType.CPU,
                "audio" => ChipType.Audio,
                _ => ChipType.NULL,
            };
        }

        /// <summary>
        /// Get ControlType value from input string
        /// </summary>
        /// <param name="controlType">String to get value from</param>
        /// <returns>ControlType value corresponding to the string</returns>
        public static ControlType AsControlType(this string controlType)
        {
            return controlType?.ToLowerInvariant() switch
            {
                "joy" => ControlType.Joy,
                "stick" => ControlType.Stick,
                "paddle" => ControlType.Paddle,
                "pedal" => ControlType.Pedal,
                "lightgun" => ControlType.Lightgun,
                "positional" => ControlType.Positional,
                "dial" => ControlType.Dial,
                "trackball" => ControlType.Trackball,
                "mouse" => ControlType.Mouse,
                "only_buttons" => ControlType.OnlyButtons,
                "keypad" => ControlType.Keypad,
                "keyboard" => ControlType.Keyboard,
                "mahjong" => ControlType.Mahjong,
                "hanafuda" => ControlType.Hanafuda,
                "gambling" => ControlType.Gambling,
                _ => ControlType.NULL,
            };
        }

        /// <summary>
        /// Get DatHeaderField value from input string
        /// </summary>
        /// <param name="DatHeaderField">String to get value from</param>
        /// <returns>DatHeaderField value corresponding to the string</returns>
        public static DatHeaderField AsDatHeaderField(this string input)
        {
            // If the input is empty, we return null
            if (string.IsNullOrEmpty(input))
                return DatHeaderField.NULL;

            // Normalize the input
            input = input.ToLowerInvariant();

            // Create regex
            string headerRegex = @"^(dat|header|datheader)[.\-_\s]";

            // If we don't have a header field, skip
            if (!Regex.IsMatch(input, headerRegex))
                return DatHeaderField.NULL;

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
                    return DatHeaderField.FileName;

                case "dat":
                case "datname":
                case "dat_name":
                case "internalname":
                case "internal_name":
                    return DatHeaderField.Name;

                case "desc":
                case "description":
                    return DatHeaderField.Description;

                case "root":
                case "rootdir":
                case "root_dir":
                case "rootdirectory":
                case "root_directory":
                    return DatHeaderField.RootDir;

                case "category":
                    return DatHeaderField.Category;

                case "version":
                    return DatHeaderField.Version;

                case "date":
                case "timestamp":
                case "time_stamp":
                    return DatHeaderField.Date;

                case "author":
                    return DatHeaderField.Author;

                case "email":
                case "e_mail":
                    return DatHeaderField.Email;

                case "homepage":
                case "home_page":
                    return DatHeaderField.Homepage;

                case "url":
                    return DatHeaderField.Url;

                case "comment":
                    return DatHeaderField.Comment;

                case "header":
                case "headerskipper":
                case "header_skipper":
                case "skipper":
                    return DatHeaderField.HeaderSkipper;

                case "dattype":
                case "type":
                case "superdat":
                    return DatHeaderField.Type;

                case "forcemerging":
                case "force_merging":
                    return DatHeaderField.ForceMerging;

                case "forcenodump":
                case "force_nodump":
                    return DatHeaderField.ForceNodump;

                case "forcepacking":
                case "force_packing":
                    return DatHeaderField.ForcePacking;

                #endregion

                #region ListXML

                case "debug":
                    return DatHeaderField.Debug;

                case "mameconfig":
                case "mame_config":
                    return DatHeaderField.MameConfig;

                #endregion

                #region Logiqx

                case "build":
                    return DatHeaderField.Build;

                case "rommode":
                case "rom_mode":
                    return DatHeaderField.RomMode;

                case "biosmode":
                case "bios_mode":
                    return DatHeaderField.BiosMode;

                case "samplemode":
                case "sample_mode":
                    return DatHeaderField.SampleMode;

                case "lockrommode":
                case "lockrom_mode":
                case "lock_rommode":
                case "lock_rom_mode":
                    return DatHeaderField.LockRomMode;

                case "lockbiosmode":
                case "lockbios_mode":
                case "lock_biosmode":
                case "lock_bios_mode":
                    return DatHeaderField.LockBiosMode;

                case "locksamplemode":
                case "locksample_mode":
                case "lock_samplemode":
                case "lock_sample_mode":
                    return DatHeaderField.LockSampleMode;

                #endregion

                #region OfflineList

                case "system":
                case "plugin": // Used with RomCenter
                    return DatHeaderField.System;

                case "screenshotwidth":
                case "screenshotswidth":
                case "screenshot_width":
                case "screenshots_width":
                    return DatHeaderField.ScreenshotsWidth;

                case "screenshotheight":
                case "screenshotsheight":
                case "screenshot_height":
                case "screenshots_height":
                    return DatHeaderField.ScreenshotsHeight;

                case "info_name":
                case "infos_name":
                    return DatHeaderField.Info_Name;

                case "info_visible":
                case "infos_visible":
                    return DatHeaderField.Info_Visible;

                case "info_isnamingoption":
                case "info_is_naming_option":
                case "infos_isnamingoption":
                case "infos_is_naming_option":
                    return DatHeaderField.Info_IsNamingOption;

                case "info_default":
                case "infos_default":
                    return DatHeaderField.Info_Default;

                case "canopen":
                case "can_open":
                    return DatHeaderField.CanOpen;

                case "romtitle":
                case "rom_title":
                    return DatHeaderField.RomTitle;

                #endregion

                #region RomCenter

                case "rcversion":
                case "rc_version":
                case "romcenterversion":
                case "romcenter_version":
                case "rom_center_version":
                    return DatHeaderField.RomCenterVersion;

                #endregion

                default:
                    return DatHeaderField.NULL;
            }
        }

        /// <summary>
        /// Get DatItemField value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>DatItemField value corresponding to the string</returns>
        public static DatItemField AsDatItemField(this string input)
        {
            // If the input is empty, we return null
            if (string.IsNullOrEmpty(input))
                return DatItemField.NULL;

            // Normalize the input
            input = input.ToLowerInvariant();

            // Create regex
            string datItemRegex = @"^(item|datitem)[.\-_\s]";

            // If we don't have an item field, skip
            if (!Regex.IsMatch(input, datItemRegex))
                return DatItemField.NULL;

            // Replace the match and re-normalize
            string itemInput = Regex.Replace(input, datItemRegex, string.Empty)
                .Replace(' ', '_')
                .Replace('-', '_')
                .Replace('.', '_');

            switch (itemInput)
            {
                #region Common

                case "type":
                    return DatItemField.Type;

                #endregion

                #region Item-Specific

                #region Actionable

                // Rom
                case "name":
                    return DatItemField.Name;

                case "bios":
                    return DatItemField.Bios;

                case "size":
                    return DatItemField.Size;

                case "crc":
                case "crc32":
                    return DatItemField.CRC;

                case "md5":
                case "md5_hash":
                    return DatItemField.MD5;

                case "sha1":
                case "sha_1":
                case "sha1hash":
                case "sha1_hash":
                case "sha_1hash":
                case "sha_1_hash":
                    return DatItemField.SHA1;

                case "sha256":
                case "sha_256":
                case "sha256hash":
                case "sha256_hash":
                case "sha_256hash":
                case "sha_256_hash":
                    return DatItemField.SHA256;

                case "sha384":
                case "sha_384":
                case "sha384hash":
                case "sha384_hash":
                case "sha_384hash":
                case "sha_384_hash":
                    return DatItemField.SHA384;

                case "sha512":
                case "sha_512":
                case "sha512hash":
                case "sha512_hash":
                case "sha_512hash":
                case "sha_512_hash":
                    return DatItemField.SHA512;
                    
                case "spamsum":
                case "spam_sum":
                    return DatItemField.SpamSum;

                case "merge":
                case "mergetag":
                case "merge_tag":
                    return DatItemField.Merge;

                case "region":
                    return DatItemField.Region;

                case "offset":
                    return DatItemField.Offset;

                case "date":
                    return DatItemField.Date;

                case "status":
                    return DatItemField.Status;

                case "optional":
                    return DatItemField.Optional;

                case "inverted":
                    return DatItemField.Inverted;
                
                case "mia":
                    return DatItemField.MIA;

                // Rom (Archive.org)
                case "ado-source":
                case "ado source":
                    return DatItemField.ArchiveDotOrgSource;

                case "ado-format":
                case "ado format":
                    return DatItemField.ArchiveDotOrgFormat;

                case "original-filename":
                case "original filename":
                    return DatItemField.OriginalFilename;

                case "rotation":
                    return DatItemField.Rotation;

                case "summation":
                    return DatItemField.Summation;

                // Rom (AttractMode)
                case "altname":
                case "alt name":
                case "alt-name":
                case "altromname":
                case "alt romname":
                case "alt-romname":
                    return DatItemField.AltName;

                case "alttitle":
                case "alt title":
                case "alt-title":
                case "altromtitle":
                case "alt romtitle":
                case "alt-romtitle":
                    return DatItemField.AltTitle;

                // Rom (OpenMSX)
                case "original":
                    return DatItemField.Original;

                case "subtype":
                case "sub_type":
                case "openmsxsubtype":
                case "openmsx_subtype":
                    return DatItemField.OpenMSXSubType;

                case "openmsxtype":
                case "openmsx_type":
                    return DatItemField.OpenMSXType;

                case "remark":
                    return DatItemField.Remark;

                case "boot":
                    return DatItemField.Boot;

                // Rom (SoftwareList)
                case "areaname":
                case "area_name":
                    return DatItemField.AreaName;

                case "areasize":
                case "area_size":
                    return DatItemField.AreaSize;

                case "areawidth":
                case "area_width":
                    return DatItemField.AreaWidth;

                case "areaendinanness":
                case "area_endianness":
                    return DatItemField.AreaEndianness;

                case "loadflag":
                case "load_flag":
                    return DatItemField.LoadFlag;

                case "partname":
                case "part_name":
                    return DatItemField.Part_Name;

                case "partinterface":
                case "part_interface":
                    return DatItemField.Part_Interface;

                case "part_feature_name":
                    return DatItemField.Part_Feature_Name;

                case "part_feature_value":
                    return DatItemField.Part_Feature_Value;

                case "value":
                    return DatItemField.Value;

                // Disk
                case "index":
                    return DatItemField.Index;

                case "writable":
                    return DatItemField.Writable;

                #endregion

                #region Auxiliary

                // Adjuster
                case "default":
                    return DatItemField.Default;

                // Analog
                case "analog_mask":
                    return DatItemField.Analog_Mask;

                // BiosSet
                case "description":
                case "biosdescription":
                case "bios_description":
                    return DatItemField.Description;

                // Chip
                case "tag":
                    return DatItemField.Tag;

                case "chiptype":
                case "chip_type":
                    return DatItemField.ChipType;

                case "clock":
                    return DatItemField.Clock;

                // Condition
                case "mask":
                    return DatItemField.Mask;

                case "relation":
                    return DatItemField.Relation;

                case "condition_tag":
                    return DatItemField.Condition_Tag;

                case "condition_mask":
                    return DatItemField.Condition_Mask;

                case "condition_relation":
                    return DatItemField.Condition_Relation;

                case "condition_value":
                    return DatItemField.Condition_Value;

                // Control
                case "control_type":
                    return DatItemField.Control_Type;

                case "control_player":
                    return DatItemField.Control_Player;

                case "control_buttons":
                    return DatItemField.Control_Buttons;

                case "control_reqbuttons":
                    return DatItemField.Control_RequiredButtons;

                case "control_minimum":
                    return DatItemField.Control_Minimum;

                case "control_maximum":
                    return DatItemField.Control_Maximum;

                case "control_sensitivity":
                    return DatItemField.Control_Sensitivity;

                case "control_keydelta":
                    return DatItemField.Control_KeyDelta;

                case "control_reverse":
                    return DatItemField.Control_Reverse;

                case "control_ways":
                    return DatItemField.Control_Ways;

                case "control_ways2":
                    return DatItemField.Control_Ways2;

                case "control_ways3":
                    return DatItemField.Control_Ways3;

                // Device
                case "devicetype":
                    return DatItemField.DeviceType;

                case "fixedimage":
                    return DatItemField.FixedImage;

                case "mandatory":
                    return DatItemField.Mandatory;

                case "interface":
                    return DatItemField.Interface;

                // Display
                case "displaytype":
                    return DatItemField.DisplayType;

                case "rotate":
                    return DatItemField.Rotate;

                case "flipx":
                    return DatItemField.FlipX;

                case "width":
                    return DatItemField.Width;

                case "height":
                    return DatItemField.Height;

                case "refresh":
                    return DatItemField.Refresh;

                case "pixclock":
                    return DatItemField.PixClock;

                case "htotal":
                    return DatItemField.HTotal;

                case "hbend":
                    return DatItemField.HBEnd;

                case "hbstart":
                    return DatItemField.HBStart;

                case "vtotal":
                    return DatItemField.VTotal;

                case "vbend":
                    return DatItemField.VBEnd;

                case "vbstart":
                    return DatItemField.VBStart;

                // Driver
                case "supportstatus":
                    return DatItemField.SupportStatus;

                case "emulationstatus":
                    return DatItemField.EmulationStatus;

                case "cocktailstatus":
                    return DatItemField.CocktailStatus;

                case "savestatestatus":
                    return DatItemField.SaveStateStatus;

                case "requiresartwork":
                    return DatItemField.RequiresArtwork;

                case "unofficial":
                    return DatItemField.Unofficial;

                case "nosoundhardware":
                    return DatItemField.NoSoundHardware;

                case "incomplete":
                    return DatItemField.Incomplete;

                // Extension
                case "extension_name":
                    return DatItemField.Extension_Name;

                // Feature
                case "featuretype":
                    return DatItemField.FeatureType;

                case "featurestatus":
                    return DatItemField.FeatureStatus;

                case "featureoverall":
                    return DatItemField.FeatureOverall;

                // Input
                case "service":
                    return DatItemField.Service;

                case "tilt":
                    return DatItemField.Tilt;

                case "players":
                    return DatItemField.Players;

                case "coins":
                    return DatItemField.Coins;

                // Instance
                case "instance_name":
                    return DatItemField.Instance_Name;

                case "instance_briefname":
                    return DatItemField.Instance_BriefName;

                // Location
                case "location_name":
                    return DatItemField.Location_Name;

                case "location_number":
                    return DatItemField.Location_Number;

                case "location_inverted":
                    return DatItemField.Location_Inverted;

                // RamOption
                case "content":
                    return DatItemField.Content;

                // Release
                case "language":
                    return DatItemField.Language;

                // Setting
                case "setting_name":
                case "value_name":
                    return DatItemField.Setting_Name;

                case "setting_value":
                case "value_value":
                    return DatItemField.Setting_Value;

                case "setting_default":
                case "value_default":
                    return DatItemField.Setting_Default;

                // SlotOption
                case "slotoption_name":
                    return DatItemField.SlotOption_Name;

                case "slotoption_devicename":
                    return DatItemField.SlotOption_DeviceName;

                case "slotoption_default":
                    return DatItemField.SlotOption_Default;

                // SoftwareList
                case "softwareliststatus":
                case "softwarelist_status":
                    return DatItemField.SoftwareListStatus;

                case "filter":
                    return DatItemField.Filter;

                // Sound
                case "channels":
                    return DatItemField.Channels;

                #endregion

                #endregion // Item-Specific

                default:
                    return DatItemField.NULL;
            }
        }

        /// <summary>
        /// Get DeviceType value from input string
        /// </summary>
        /// <param name="deviceType">String to get value from</param>
        /// <returns>DeviceType value corresponding to the string</returns>
        public static DeviceType AsDeviceType(this string deviceType)
        {
            return deviceType?.ToLowerInvariant() switch
            {
                "unknown" => DeviceType.Unknown,
                "cartridge" => DeviceType.Cartridge,
                "floppydisk" => DeviceType.FloppyDisk,
                "harddisk" => DeviceType.HardDisk,
                "cylinder" => DeviceType.Cylinder,
                "cassette" => DeviceType.Cassette,
                "punchcard" => DeviceType.PunchCard,
                "punchtape" => DeviceType.PunchTape,
                "printout" => DeviceType.Printout,
                "serial" => DeviceType.Serial,
                "parallel" => DeviceType.Parallel,
                "snapshot" => DeviceType.Snapshot,
                "quickload" => DeviceType.QuickLoad,
                "memcard" => DeviceType.MemCard,
                "cdrom" => DeviceType.CDROM,
                "magtape" => DeviceType.MagTape,
                "romimage" => DeviceType.ROMImage,
                "midiin" => DeviceType.MIDIIn,
                "midiout" => DeviceType.MIDIOut,
                "picture" => DeviceType.Picture,
                "vidfile" => DeviceType.VidFile,
                _ => DeviceType.NULL,
            };
        }

        /// <summary>
        /// Get DisplayType value from input string
        /// </summary>
        /// <param name="displayType">String to get value from</param>
        /// <returns>DisplayType value corresponding to the string</returns>
        public static DisplayType AsDisplayType(this string displayType)
        {
            return displayType?.ToLowerInvariant() switch
            {
                "raster" => DisplayType.Raster,
                "vector" => DisplayType.Vector,
                "lcd" => DisplayType.LCD,
                "svg" => DisplayType.SVG,
                "unknown" => DisplayType.Unknown,
                _ => DisplayType.NULL,
            };
        }

        /// <summary>
        /// Get Endianness value from input string
        /// </summary>
        /// <param name="endianness">String to get value from</param>
        /// <returns>Endianness value corresponding to the string</returns>
        public static Endianness AsEndianness(this string endianness)
        {
            return endianness?.ToLowerInvariant() switch
            {
                "big" => Endianness.Big,
                "little" => Endianness.Little,
                _ => Endianness.NULL,
            };
        }

        /// <summary>
        /// Get FeatureStatus value from input string
        /// </summary>
        /// <param name="featureStatus">String to get value from</param>
        /// <returns>FeatureStatus value corresponding to the string</returns>
        public static FeatureStatus AsFeatureStatus(this string featureStatus)
        {
            return featureStatus?.ToLowerInvariant() switch
            {
                "unemulated" => FeatureStatus.Unemulated,
                "imperfect" => FeatureStatus.Imperfect,
                _ => FeatureStatus.NULL,
            };
        }

        /// <summary>
        /// Get FeatureType value from input string
        /// </summary>
        /// <param name="emulationStatus">String to get value from</param>
        /// <returns>FeatureType value corresponding to the string</returns>
        public static FeatureType AsFeatureType(this string featureType)
        {
            return featureType?.ToLowerInvariant() switch
            {
                "protection" => FeatureType.Protection,
                "palette" => FeatureType.Palette,
                "graphics" => FeatureType.Graphics,
                "sound" => FeatureType.Sound,
                "controls" => FeatureType.Controls,
                "keyboard" => FeatureType.Keyboard,
                "mouse" => FeatureType.Mouse,
                "microphone" => FeatureType.Microphone,
                "camera" => FeatureType.Camera,
                "disk" => FeatureType.Disk,
                "printer" => FeatureType.Printer,
                "lan" => FeatureType.Lan,
                "wan" => FeatureType.Wan,
                "timing" => FeatureType.Timing,
                _ => FeatureType.NULL,
            };
        }

        /// <summary>
        /// Get ItemStatus value from input string
        /// </summary>
        /// <param name="status">String to get value from</param>
        /// <returns>ItemStatus value corresponding to the string</returns>
        public static ItemStatus AsItemStatus(this string status)
        {
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
        }

        /// <summary>
        /// Get ItemType? value from input string
        /// </summary>
        /// <param name="itemType">String to get value from</param>
        /// <returns>ItemType? value corresponding to the string</returns>
        public static ItemType? AsItemType(this string itemType)
        {
            return itemType?.ToLowerInvariant() switch
            {
                "adjuster" => ItemType.Adjuster,
                "analog" => ItemType.Analog,
                "archive" => ItemType.Archive,
                "biosset" => ItemType.BiosSet,
                "blank" => ItemType.Blank,
                "chip" => ItemType.Chip,
                "condition" => ItemType.Condition,
                "configuration" => ItemType.Configuration,
                "control" => ItemType.Control,
                "dataarea" => ItemType.DataArea,
                "device" => ItemType.Device,
                "device_ref" => ItemType.DeviceReference,
                "dipswitch" => ItemType.DipSwitch,
                "disk" => ItemType.Disk,
                "diskarea" => ItemType.DiskArea,
                "display" => ItemType.Display,
                "driver" => ItemType.Driver,
                "extension" => ItemType.Extension,
                "feature" => ItemType.Feature,
                "info" => ItemType.Info,
                "input" => ItemType.Input,
                "instance" => ItemType.Instance,
                "location" => ItemType.Location,
                "media" => ItemType.Media,
                "part" => ItemType.Part,
                "partfeature" => ItemType.PartFeature,
                "part_feature" => ItemType.PartFeature,
                "port" => ItemType.Port,
                "ramoption" => ItemType.RamOption,
                "release" => ItemType.Release,
                "rom" => ItemType.Rom,
                "sample" => ItemType.Sample,
                "setting" => ItemType.Setting,
                "sharedfeat" => ItemType.SharedFeature,
                "slot" => ItemType.Slot,
                "slotoption" => ItemType.SlotOption,
                "softwarelist" => ItemType.SoftwareList,
                "sound" => ItemType.Sound,
                _ => null,
            };
        }

        /// <summary>
        /// Get LoadFlag value from input string
        /// </summary>
        /// <param name="loadFlag">String to get value from</param>
        /// <returns>LoadFlag value corresponding to the string</returns>
        public static LoadFlag AsLoadFlag(this string loadFlag)
        {
            return loadFlag?.ToLowerInvariant() switch
            {
                "load16_byte" => LoadFlag.Load16Byte,
                "load16_word" => LoadFlag.Load16Word,
                "load16_word_swap" => LoadFlag.Load16WordSwap,
                "load32_byte" => LoadFlag.Load32Byte,
                "load32_word" => LoadFlag.Load32Word,
                "load32_word_swap" => LoadFlag.Load32WordSwap,
                "load32_dword" => LoadFlag.Load32DWord,
                "load64_word" => LoadFlag.Load64Word,
                "load64_word_swap" => LoadFlag.Load64WordSwap,
                "reload" => LoadFlag.Reload,
                "fill" => LoadFlag.Fill,
                "continue" => LoadFlag.Continue,
                "reload_plain" => LoadFlag.ReloadPlain,
                "ignore" => LoadFlag.Ignore,
                _ => LoadFlag.NULL,
            };
        }

        /// <summary>
        /// Get LogLevel value from input string
        /// </summary>
        /// <param name="logLevel">String to get value from</param>
        /// <returns>LogLevel value corresponding to the string</returns>
        public static LogLevel AsLogLevel(this string logLevel)
        {
            return logLevel?.ToLowerInvariant() switch
            {
                "verbose" => LogLevel.VERBOSE,
                "user" => LogLevel.USER,
                "warning" => LogLevel.WARNING,
                "error" => LogLevel.ERROR,
                _ => LogLevel.VERBOSE,
            };
        }

        /// <summary>
        /// Get MachineField value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>MachineField value corresponding to the string</returns>
        public static MachineField AsMachineField(this string input)
        {
            // If the input is empty, we return null
            if (string.IsNullOrEmpty(input))
                return MachineField.NULL;

            // Normalize the input
            input = input.ToLowerInvariant();

            // Create regex
            string machineRegex = @"^(game|machine)[.\-_\s]";

            // If we don't have a machine field, skip
            if (!Regex.IsMatch(input, machineRegex))
                return MachineField.NULL;

            // Replace the match and re-normalize
            string machineInput = Regex.Replace(input, machineRegex, string.Empty)
                .Replace(' ', '_')
                .Replace('-', '_')
                .Replace('.', '_');

            switch (machineInput)
            {
                #region Common

                case "name":
                    return MachineField.Name;

                case "comment":
                case "extra": // Used with AttractMode
                    return MachineField.Comment;

                case "desc":
                case "description":
                    return MachineField.Description;

                case "year":
                    return MachineField.Year;

                case "manufacturer":
                    return MachineField.Manufacturer;

                case "publisher":
                    return MachineField.Publisher;

                case "category":
                    return MachineField.Category;

                case "romof":
                case "rom_of":
                    return MachineField.RomOf;

                case "cloneof":
                case "clone_of":
                    return MachineField.CloneOf;

                case "sampleof":
                case "sample_of":
                    return MachineField.SampleOf;

                case "type":
                    return MachineField.Type;

                #endregion

                #region AttractMode

                case "players":
                    return MachineField.Players;

                case "rotation":
                    return MachineField.Rotation;

                case "control":
                    return MachineField.Control;

                case "amstatus":
                case "am_status":
                case "gamestatus":
                case "supportstatus":
                case "support_status":
                    return MachineField.Status;

                case "displaycount":
                    return MachineField.DisplayCount;

                case "displaytype":
                    return MachineField.DisplayType;

                case "buttons":
                    return MachineField.Buttons;

                #endregion

                #region ListXML

                case "history":
                    return MachineField.History;

                case "sourcefile":
                case "source_file":
                    return MachineField.SourceFile;

                case "runnable":
                    return MachineField.Runnable;                    

                #endregion

                #region Logiqx

                case "board":
                    return MachineField.Board;

                case "rebuildto":
                case "rebuild_to":
                    return MachineField.RebuildTo;

                #endregion

                #region Logiqx EmuArc

                case "titleid":
                case "title_id":
                    return MachineField.TitleID;

                case "developer":
                    return MachineField.Developer;

                case "genre":
                    return MachineField.Genre;

                case "subgenre":
                case "sub_genre":
                    return MachineField.Subgenre;

                case "ratings":
                    return MachineField.Ratings;

                case "score":
                    return MachineField.Score;

                case "enabled":
                    return MachineField.Enabled;

                case "crc":
                case "hascrc":
                case "has_crc":
                    return MachineField.CRC;

                case "relatedto":
                case "related_to":
                    return MachineField.RelatedTo;

                #endregion

                #region OpenMSX

                case "genmsxid":
                case "genmsx_id":
                case "gen_msxid":
                case "gen_msx_id":
                    return MachineField.GenMSXID;

                case "system":
                case "msxsystem":
                case "msx_system":
                    return MachineField.System;

                case "country":
                    return MachineField.Country;

                #endregion

                #region SoftwareList

                case "supported":
                    return MachineField.Supported;

                #endregion

                default:
                    return MachineField.NULL;
            }
        }

        /// <summary>
        /// Get MachineType value from input string
        /// </summary>
        /// <param name="gametype">String to get value from</param>
        /// <returns>MachineType value corresponding to the string</returns>
        public static MachineType AsMachineType(this string gametype)
        {
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
        }

        /// <summary>
        /// Get MergingFlag value from input string
        /// </summary>
        /// <param name="merging">String to get value from</param>
        /// <returns>MergingFlag value corresponding to the string</returns>
        public static MergingFlag AsMergingFlag(this string merging)
        {
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
        }

        /// <summary>
        /// Get NodumpFlag value from input string
        /// </summary>
        /// <param name="nodump">String to get value from</param>
        /// <returns>NodumpFlag value corresponding to the string</returns>
        public static NodumpFlag AsNodumpFlag(this string nodump)
        {
            return nodump?.ToLowerInvariant() switch
            {
                "obsolete" => NodumpFlag.Obsolete,
                "required" => NodumpFlag.Required,
                "ignore" => NodumpFlag.Ignore,
                "none" => NodumpFlag.None,
                _ => NodumpFlag.None,
            };
        }

        /// <summary>
        /// Get OpenMSXSubType value from input string
        /// </summary>
        /// <param name="itemType">String to get value from</param>
        /// <returns>OpenMSXSubType value corresponding to the string</returns>
        public static OpenMSXSubType AsOpenMSXSubType(this string itemType)
        {
            return itemType?.ToLowerInvariant() switch
            {
                "rom" => OpenMSXSubType.Rom,
                "megarom" => OpenMSXSubType.MegaRom,
                "sccpluscart" => OpenMSXSubType.SCCPlusCart,
                _ => OpenMSXSubType.NULL,
            };
        }

        /// <summary>
        /// Get PackingFlag value from input string
        /// </summary>
        /// <param name="packing">String to get value from</param>
        /// <returns>PackingFlag value corresponding to the string</returns>
        public static PackingFlag AsPackingFlag(this string packing)
        {
            return packing?.ToLowerInvariant() switch
            {
                "yes" => PackingFlag.Zip,
                "zip" => PackingFlag.Zip,
                "no" => PackingFlag.Unzip,
                "unzip" => PackingFlag.Unzip,
                "partial" => PackingFlag.Partial,
                "flat" => PackingFlag.Flat,
                "none" => PackingFlag.None,
                _ => PackingFlag.None,
            };
        }

        /// <summary>
        /// Get Relation value from input string
        /// </summary>
        /// <param name="relation">String to get value from</param>
        /// <returns>Relation value corresponding to the string</returns>
        public static Relation AsRelation(this string relation)
        {
            return relation?.ToLowerInvariant() switch
            {
                "eq" => Relation.Equal,
                "ne" => Relation.NotEqual,
                "gt" => Relation.GreaterThan,
                "le" => Relation.LessThanOrEqual,
                "lt" => Relation.LessThan,
                "ge" => Relation.GreaterThanOrEqual,
                _ => Relation.NULL,
            };
        }

        /// <summary>
        /// Get Runnable value from input string
        /// </summary>
        /// <param name="runnable">String to get value from</param>
        /// <returns>Runnable value corresponding to the string</returns>
        public static Runnable AsRunnable(this string runnable)
        {
            return runnable?.ToLowerInvariant() switch
            {
                "no" => Runnable.No,
                "partial" => Runnable.Partial,
                "yes" => Runnable.Yes,
                _ => Runnable.NULL,
            };
        }

        /// <summary>
        /// Get SoftwareListStatus value from input string
        /// </summary>
        /// <param name="status">String to get value from</param>
        /// <returns>SoftwareListStatus value corresponding to the string</returns>
        public static SoftwareListStatus AsSoftwareListStatus(this string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "original" => SoftwareListStatus.Original,
                "compatible" => SoftwareListStatus.Compatible,
                "none" => SoftwareListStatus.NULL,
                _ => SoftwareListStatus.NULL,
            };
        }

        /// <summary>
        /// Get Supported value from input string
        /// </summary>
        /// <param name="supported">String to get value from</param>
        /// <returns>Supported value corresponding to the string</returns>
        public static Supported AsSupported(this string supported)
        {
            return supported?.ToLowerInvariant() switch
            {
                "no" => Supported.No,
                "unsupported" => Supported.No,
                "partial" => Supported.Partial,
                "yes" => Supported.Yes,
                "supported" => Supported.Yes,
                _ => Supported.NULL,
            };
        }

        /// <summary>
        /// Get SupportStatus value from input string
        /// </summary>
        /// <param name="supported">String to get value from</param>
        /// <returns>SupportStatus value corresponding to the string</returns>
        public static SupportStatus AsSupportStatus(this string supportStatus)
        {
            return supportStatus?.ToLowerInvariant() switch
            {
                "good" => SupportStatus.Good,
                "imperfect" => SupportStatus.Imperfect,
                "preliminary" => SupportStatus.Preliminary,
                _ => SupportStatus.NULL,
            };
        }

        /// <summary>
        /// Get bool? value from input string
        /// </summary>
        /// <param name="yesno">String to get value from</param>
        /// <returns>bool? corresponding to the string</returns>
        public static bool? AsYesNo(this string yesno)
        {
            return yesno?.ToLowerInvariant() switch
            {
                "yes" => true,
                "true" => true,
                "no" => false,
                "false" => false,
                _ => null,
            };
        }

        #endregion

        #region Enum to String

        /// <summary>
        /// Get string value from input ChipType
        /// </summary>
        /// <param name="chipType">ChipType to get value from</param>
        /// <returns>String value corresponding to the ChipType</returns>
        public static string FromChipType(this ChipType chipType)
        {
            return chipType switch
            {
                ChipType.CPU => "cpu",
                ChipType.Audio => "audio",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input ControlType
        /// </summary>
        /// <param name="controlType">ControlType to get value from</param>
        /// <returns>String value corresponding to the ControlType</returns>
        public static string FromControlType(this ControlType controlType)
        {
            return controlType switch
            {
                ControlType.Joy => "joy",
                ControlType.Stick => "stick",
                ControlType.Paddle => "paddle",
                ControlType.Pedal => "pedal",
                ControlType.Lightgun => "lightgun",
                ControlType.Positional => "positional",
                ControlType.Dial => "dial",
                ControlType.Trackball => "trackball",
                ControlType.Mouse => "mouse",
                ControlType.OnlyButtons => "only_buttons",
                ControlType.Keypad => "keypad",
                ControlType.Keyboard => "keyboard",
                ControlType.Mahjong => "mahjong",
                ControlType.Hanafuda => "hanafuda",
                ControlType.Gambling => "gambling",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input DeviceType
        /// </summary>
        /// <param name="deviceType">vDeviceType to get value from</param>
        /// <returns>String value corresponding to the DeviceType</returns>
        public static string FromDeviceType(this DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.Unknown => "unknown",
                DeviceType.Cartridge => "cartridge",
                DeviceType.FloppyDisk => "floppydisk",
                DeviceType.HardDisk => "harddisk",
                DeviceType.Cylinder => "cylinder",
                DeviceType.Cassette => "cassette",
                DeviceType.PunchCard => "punchcard",
                DeviceType.PunchTape => "punchtape",
                DeviceType.Printout => "printout",
                DeviceType.Serial => "serial",
                DeviceType.Parallel => "parallel",
                DeviceType.Snapshot => "snapshot",
                DeviceType.QuickLoad => "quickload",
                DeviceType.MemCard => "memcard",
                DeviceType.CDROM => "cdrom",
                DeviceType.MagTape => "magtape",
                DeviceType.ROMImage => "romimage",
                DeviceType.MIDIIn => "midiin",
                DeviceType.MIDIOut => "midiout",
                DeviceType.Picture => "picture",
                DeviceType.VidFile => "vidfile",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input DisplayType
        /// </summary>
        /// <param name="displayType">DisplayType to get value from</param>
        /// <returns>String value corresponding to the DisplayType</returns>
        public static string FromDisplayType(this DisplayType displayType)
        {
            return displayType switch
            {
                DisplayType.Raster => "raster",
                DisplayType.Vector => "vector",
                DisplayType.LCD => "lcd",
                DisplayType.SVG => "svg",
                DisplayType.Unknown => "unknown",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input Endianness
        /// </summary>
        /// <param name="endianness">Endianness to get value from</param>
        /// <returns>String value corresponding to the Endianness</returns>
        public static string FromEndianness(this Endianness endianness)
        {
            return endianness switch
            {
                Endianness.Big => "big",
                Endianness.Little => "little",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input FeatureStatus
        /// </summary>
        /// <param name="featureStatus">FeatureStatus to get value from</param>
        /// <returns>String value corresponding to the FeatureStatus</returns>
        public static string FromFeatureStatus(this FeatureStatus featureStatus)
        {
            return featureStatus switch
            {
                FeatureStatus.Unemulated => "unemulated",
                FeatureStatus.Imperfect => "imperfect",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input FeatureType
        /// </summary>
        /// <param name="featureType">FeatureType to get value from</param>
        /// <returns>String value corresponding to the FeatureType</returns>
        public static string FromFeatureType(this FeatureType featureType)
        {
            return featureType switch
            {
                FeatureType.Protection => "protection",
                FeatureType.Palette => "palette",
                FeatureType.Graphics => "graphics",
                FeatureType.Sound => "sound",
                FeatureType.Controls => "controls",
                FeatureType.Keyboard => "keyboard",
                FeatureType.Mouse => "mouse",
                FeatureType.Microphone => "microphone",
                FeatureType.Camera => "camera",
                FeatureType.Disk => "disk",
                FeatureType.Printer => "printer",
                FeatureType.Lan => "lan",
                FeatureType.Wan => "wan",
                FeatureType.Timing => "timing",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input ItemStatus
        /// </summary>
        /// <param name="status">ItemStatus to get value from</param>
        /// <param name="yesno">True to use Yes/No format instead</param>
        /// <returns>String value corresponding to the ItemStatus</returns>
        public static string FromItemStatus(this ItemStatus status, bool yesno)
        {
            return status switch
            {
                ItemStatus.Good => "good",
                ItemStatus.BadDump => "baddump",
                ItemStatus.Nodump => yesno ? "yes" : "nodump",
                ItemStatus.Verified => "verified",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input LoadFlag
        /// </summary>
        /// <param name="loadFlag">LoadFlag to get value from</param>
        /// <returns>String value corresponding to the LoadFlag</returns>
        public static string FromLoadFlag(this LoadFlag loadFlag)
        {
            return loadFlag switch
            {
                LoadFlag.Load16Byte => "load16_byte",
                LoadFlag.Load16Word => "load16_word",
                LoadFlag.Load16WordSwap => "load16_word_swap",
                LoadFlag.Load32Byte => "load32_byte",
                LoadFlag.Load32Word => "load32_word",
                LoadFlag.Load32WordSwap => "load32_word_swap",
                LoadFlag.Load32DWord => "load32_dword",
                LoadFlag.Load64Word => "load64_word",
                LoadFlag.Load64WordSwap => "load64_word_swap",
                LoadFlag.Reload => "reload",
                LoadFlag.Fill => "fill",
                LoadFlag.Continue => "continue",
                LoadFlag.ReloadPlain => "reload_plain",
                LoadFlag.Ignore => "ignore",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input ItemType?
        /// </summary>
        /// <param name="itemType">ItemType? to get value from</param>
        /// <returns>String value corresponding to the ItemType?</returns>
        public static string FromItemType(this ItemType? itemType)
        {
            return itemType switch
            {
                ItemType.Adjuster => "adjuster",
                ItemType.Analog => "analog",
                ItemType.Archive => "archive",
                ItemType.BiosSet => "biosset",
                ItemType.Blank => "blank",
                ItemType.Chip => "chip",
                ItemType.Condition => "condition",
                ItemType.Configuration => "configuration",
                ItemType.Control => "control",
                ItemType.DataArea => "dataarea",
                ItemType.Device => "device",
                ItemType.DeviceReference => "device_ref",
                ItemType.DipSwitch => "dipswitch",
                ItemType.Disk => "disk",
                ItemType.DiskArea => "diskarea",
                ItemType.Display => "display",
                ItemType.Driver => "driver",
                ItemType.Extension => "extension",
                ItemType.Feature => "feature",
                ItemType.Info => "info",
                ItemType.Input => "input",
                ItemType.Instance => "instance",
                ItemType.Location => "location",
                ItemType.Media => "media",
                ItemType.Part => "part",
                ItemType.PartFeature => "part_feature",
                ItemType.Port => "port",
                ItemType.RamOption => "ramoption",
                ItemType.Release => "release",
                ItemType.Rom => "rom",
                ItemType.Sample => "sample",
                ItemType.Setting => "setting",
                ItemType.SharedFeature => "sharedfeat",
                ItemType.Slot => "slot",
                ItemType.SlotOption => "slotoption",
                ItemType.SoftwareList => "softwarelist",
                ItemType.Sound => "sound",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input MachineType
        /// </summary>
        /// <param name="gametype">MachineType to get value from</param>
        /// <param name="romCenter">True to use old naming instead</param>
        /// <returns>String value corresponding to the MachineType</returns>
        public static string FromMachineType(this MachineType gametype, bool old)
        {
            return gametype switch
            {
                MachineType.Bios => "bios",
                MachineType.Device => old ? "dev" : "device",
                MachineType.Mechanical => old ? "mech" : "mechanical",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input MergingFlag
        /// </summary>
        /// <param name="merging">MergingFlag to get value from</param>
        /// <param name="romCenter">True to use RomCenter naming instead</param>
        /// <returns>String value corresponding to the MergingFlag</returns>
        public static string FromMergingFlag(this MergingFlag merging, bool romCenter)
        {
            return merging switch
            {
                MergingFlag.Split => "split",
                MergingFlag.Merged => "merged",
                MergingFlag.NonMerged => romCenter ? "unmerged" : "nonmerged",
                MergingFlag.Full => "full",
                MergingFlag.Device => "device",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input NodumpFlag
        /// </summary>
        /// <param name="nodump">NodumpFlag to get value from</param>
        /// <returns>String value corresponding to the NodumpFlag</returns>
        public static string FromNodumpFlag(this NodumpFlag nodump)
        {
            return nodump switch
            {
                NodumpFlag.Obsolete => "obsolete",
                NodumpFlag.Required => "required",
                NodumpFlag.Ignore => "ignore",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input OpenMSXSubType
        /// </summary>
        /// <param name="itemType">OpenMSXSubType to get value from</param>
        /// <returns>String value corresponding to the OpenMSXSubType</returns>
        public static string FromOpenMSXSubType(this OpenMSXSubType itemType)
        {
            return itemType switch
            {
                OpenMSXSubType.Rom => "rom",
                OpenMSXSubType.MegaRom => "megarom",
                OpenMSXSubType.SCCPlusCart => "sccpluscart",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input PackingFlag
        /// </summary>
        /// <param name="packing">PackingFlag to get value from</param>
        /// <param name="yesno">True to use Yes/No format instead</param>
        /// <returns>String value corresponding to the PackingFlag</returns>
        public static string FromPackingFlag(this PackingFlag packing, bool yesno)
        {
            return packing switch
            {
                PackingFlag.Zip => yesno ? "yes" : "zip",
                PackingFlag.Unzip => yesno ? "no" : "unzip",
                PackingFlag.Partial => "partial",
                PackingFlag.Flat => "flat",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input Relation
        /// </summary>
        /// <param name="relation">Relation to get value from</param>
        /// <returns>String value corresponding to the Relation</returns>
        public static string FromRelation(this Relation relation)
        {
            return relation switch
            {
                Relation.Equal => "eq",
                Relation.NotEqual => "ne",
                Relation.GreaterThan => "gt",
                Relation.LessThanOrEqual => "le",
                Relation.LessThan => "lt",
                Relation.GreaterThanOrEqual => "ge",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input Runnable
        /// </summary>
        /// <param name="runnable">Runnable to get value from</param>
        /// <returns>String value corresponding to the Runnable</returns>
        public static string FromRunnable(this Runnable runnable)
        {
            return runnable switch
            {
                Runnable.No => "no",
                Runnable.Partial => "partial",
                Runnable.Yes => "yes",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input SoftwareListStatus
        /// </summary>
        /// <param name="status">SoftwareListStatus to get value from</param>
        /// <returns>String value corresponding to the SoftwareListStatus</returns>
        public static string FromSoftwareListStatus(this SoftwareListStatus status)
        {
            return status switch
            {
                SoftwareListStatus.Original => "original",
                SoftwareListStatus.Compatible => "compatible",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input Supported
        /// </summary>
        /// <param name="supported">Supported to get value from</param>
        /// <param name="verbose">True to use verbose output, false otherwise</param>
        /// <returns>String value corresponding to the Supported</returns>
        public static string FromSupported(this Supported supported, bool verbose)
        {
            return supported switch
            {
                Supported.No => verbose ? "unsupported" : "no",
                Supported.Partial => "partial",
                Supported.Yes => verbose ? "supported" : "yes",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input SupportStatus
        /// </summary>
        /// <param name="supportStatus">SupportStatus to get value from</param>
        /// <returns>String value corresponding to the SupportStatus</returns>
        public static string FromSupportStatus(this SupportStatus supportStatus)
        {
            return supportStatus switch
            {
                SupportStatus.Good => "good",
                SupportStatus.Imperfect => "imperfect",
                SupportStatus.Preliminary => "preliminary",
                _ => null,
            };
        }

        /// <summary>
        /// Get string value from input bool?
        /// </summary>
        /// <param name="yesno">bool? to get value from</param>
        /// <returns>String corresponding to the bool?</returns>
        public static string FromYesNo(this bool? yesno)
        {
            return yesno switch
            {
                true => "yes",
                false => "no",
                _ => null,
            };
        }

        #endregion
    }
}
