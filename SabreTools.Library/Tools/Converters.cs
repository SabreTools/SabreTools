using SabreTools.Library.Data;

namespace SabreTools.Library.Tools
{
    public static class Converters
    {
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
        /// Get the field associated with each hash type
        /// </summary>
        public static Field AsField(this Hash hash)
        {
            switch (hash)
            {
                case Hash.CRC:
                    return Field.CRC;
                case Hash.MD5:
                    return Field.MD5;
#if NET_FRAMEWORK
                case Hash.RIPEMD160:
                    return Field.RIPEMD160;
#endif
                case Hash.SHA1:
                    return Field.SHA1;
                case Hash.SHA256:
                    return Field.SHA256;
                case Hash.SHA384:
                    return Field.SHA384;
                case Hash.SHA512:
                    return Field.SHA512;

                default:
                    return Field.NULL;
            }
        }

        /// <summary>
        /// Get Field value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Field value corresponding to the string</returns>
        public static Field AsField(this string input)
        {
            switch (input?.ToLowerInvariant())
            {
                case "areaname":
                case "area-name":
                    return Field.AreaName;
                case "areasize":
                case "area-size":
                    return Field.AreaSize;
                case "bios":
                    return Field.Bios;
                case "biosdescription":
                case "bios-description":
                case "biossetdescription":
                case "biosset-description":
                case "bios-set-description":
                    return Field.BiosDescription;
                case "board":
                    return Field.Board;
                case "category":
                case "machinecategory":
                case "machine-category":
                    return Field.Category;
                case "cloneof":
                    return Field.CloneOf;
                case "comment":
                    return Field.Comment;
                case "crc":
                case "crc32":
                    return Field.CRC;
                case "default":
                    return Field.Default;
                case "date":
                    return Field.Date;
                case "desc":
                case "description":
                case "gamedesc":
                case "gamedescription":
                case "game-description":
                case "machinedesc":
                case "machinedescription":
                case "machine-description":
                    return Field.Description;
                case "devices":
                    return Field.Devices;
                case "equal":
                case "greater":
                case "less":
                case "size":
                    return Field.Size;
                case "features":
                    return Field.Features;
                case "gamename":
                case "machinename":
                    return Field.MachineName;
                case "gametype":
                case "game-type":
                case "machinetype":
                case "machine-type":
                    return Field.MachineType;
                case "index":
                    return Field.Index;
                case "infos":
                    return Field.Infos;
                case "itemname":
                case "item-name":
                case "name":
                    return Field.Name;
                case "itemtatus":
                case "item-status":
                case "status":
                    return Field.Status;
                case "itemtype":
                case "item-type":
                case "type":
                    return Field.ItemType;
                case "language":
                    return Field.Language;
                case "manufacturer":
                    return Field.Manufacturer;
                case "md5":
                    return Field.MD5;
                case "merge":
                case "mergetag":
                case "merge-tag":
                    return Field.Merge;
                case "offset":
                    return Field.Offset;
                case "optional":
                    return Field.Optional;
                case "partinterface":
                case "part-interface":
                    return Field.PartInterface;
                case "partname":
                case "part-name":
                    return Field.PartName;
                case "publisher":
                    return Field.Publisher;
                case "rebuildto":
                    return Field.RebuildTo;
                case "region":
                    return Field.Region;
#if NET_FRAMEWORK
                case "ripemd160":
                    return Field.RIPEMD160;
#endif
                case "romof":
                    return Field.RomOf;
                case "runnable":
                    return Field.Runnable;
                case "sampleof":
                    return Field.SampleOf;
                case "sha1":
                case "sha-1":
                    return Field.SHA1;
                case "sha256":
                case "sha-256":
                    return Field.SHA256;
                case "sha384":
                case "sha-384":
                    return Field.SHA384;
                case "sha512":
                case "sha-512":
                    return Field.SHA512;
                case "slotoptions":
                case "slot-options":
                    return Field.SlotOptions;
                case "sourcefile":
                case "source-file":
                    return Field.SourceFile;
                case "supported":
                    return Field.Supported;
                case "writable":
                    return Field.Writable;
                case "year":
                    return Field.Year;
                default:
                    return Field.NULL;
            }
        }

        /// <summary>
        /// Get ForceMerging value from input string
        /// </summary>
        /// <param name="forcemerge">String to get value from</param>
        /// <returns>ForceMerging value corresponding to the string</returns>
        public static ForceMerging AsForceMerging(this string forcemerge)
        {
#if NET_FRAMEWORK
            switch (forcemerge?.ToLowerInvariant())
            {
                case "split":
                    return ForceMerging.Split;
                case "merged":
                    return ForceMerging.Merged;
                case "nonmerged":
                    return ForceMerging.NonMerged;
                case "full":
                    return ForceMerging.Full;
                case "none":
                default:
                    return ForceMerging.None;
            }
#else
            return forcemerge?.ToLowerInvariant() switch
            {
                "split" => ForceMerging.Split,
                "merged" => ForceMerging.Merged,
                "nonmerged" => ForceMerging.NonMerged,
                "full" => ForceMerging.Full,
                "none" => ForceMerging.None,
                _ => ForceMerging.None,
            };
#endif
        }

        /// <summary>
        /// Get ForceNodump value from input string
        /// </summary>
        /// <param name="forcend">String to get value from</param>
        /// <returns>ForceNodump value corresponding to the string</returns>
        public static ForceNodump AsForceNodump(this string forcend)
        {
#if NET_FRAMEWORK
            switch (forcend?.ToLowerInvariant())
            {
                case "obsolete":
                    return ForceNodump.Obsolete;
                case "required":
                    return ForceNodump.Required;
                case "ignore":
                    return ForceNodump.Ignore;
                case "none":
                default:
                    return ForceNodump.None;
            }
#else
            return forcend?.ToLowerInvariant() switch
            {
                "obsolete" => ForceNodump.Obsolete,
                "required" => ForceNodump.Required,
                "ignore" => ForceNodump.Ignore,
                "none" => ForceNodump.None,
                _ => ForceNodump.None,
            };
#endif
        }

        /// <summary>
        /// Get ForcePacking value from input string
        /// </summary>
        /// <param name="forcepack">String to get value from</param>
        /// <returns>ForcePacking value corresponding to the string</returns>
        public static ForcePacking AsForcePacking(this string forcepack)
        {
#if NET_FRAMEWORK
            switch (forcepack?.ToLowerInvariant())
            {
                case "yes":
                case "zip":
                    return ForcePacking.Zip;
                case "no":
                case "unzip":
                    return ForcePacking.Unzip;
                case "none":
                default:
                    return ForcePacking.None;
            }
#else
            return forcepack?.ToLowerInvariant() switch
            {
                "yes" => ForcePacking.Zip,
                "zip" => ForcePacking.Zip,
                "no" => ForcePacking.Unzip,
                "unzip" => ForcePacking.Unzip,
                "none" => ForcePacking.None,
                _ => ForcePacking.None,
            };
#endif
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
                case "disk":
                    return ItemType.Disk;
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
                "disk" => ItemType.Disk,
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
                    return MachineType.None;
            }
#else
            return gametype?.ToLowerInvariant() switch
            {
                "bios" => MachineType.Bios,
                "dev" => MachineType.Device,
                "device" => MachineType.Device,
                "mech" => MachineType.Mechanical,
                "mechanical" => MachineType.Mechanical,
                "none" => MachineType.None,
                _ => MachineType.None,
            };
#endif
        }

        /// <summary>
        /// Get SplitType value from input ForceMerging
        /// </summary>
        /// <param name="forceMerging">ForceMerging to get value from</param>
        /// <returns>SplitType value corresponding to the string</returns>
        public static SplitType AsSplitType(this ForceMerging forceMerging)
        {
#if NET_FRAMEWORK
            switch (forceMerging)
            {
                case ForceMerging.Split:
                    return SplitType.Split;
                case ForceMerging.Merged:
                    return SplitType.Merged;
                case ForceMerging.NonMerged:
                    return SplitType.NonMerged;
                case ForceMerging.Full:
                    return SplitType.FullNonMerged;
                case ForceMerging.None:
                default:
                    return SplitType.None;
            }
#else
            return forceMerging switch
            {
                ForceMerging.Split => SplitType.Split,
                ForceMerging.Merged => SplitType.Merged,
                ForceMerging.NonMerged => SplitType.NonMerged,
                ForceMerging.Full => SplitType.FullNonMerged,
                ForceMerging.None => SplitType.None,
                _ => SplitType.None,
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
                    return true;
                case "no":
                    return false;
                case "partial":
                default:
                    return null;
            }
#else
            return yesno?.ToLowerInvariant() switch
            {
                "yes" => true,
                "no" => false,
                "partial" => null,
                _ => null,
            };
#endif
        }
    }
}
