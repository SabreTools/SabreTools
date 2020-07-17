using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Readers;
using SabreTools.Library.Tools;
using SabreTools.Library.Writers;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a value-separated DAT
    /// </summary>
    internal class SeparatedValue : DatFile
    {
        // Private instance variables specific to Separated Value DATs
        private readonly char _delim;

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="delim">Delimiter for parsing individual lines</param>
        public SeparatedValue(DatFile datFile, char delim)
            : base(datFile)
        {
            _delim = delim;
        }

        /// <summary>
        /// Parse a character-separated value DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        protected override void ParseFile(
            // Standard Dat parsing
            string filename,
            int indexId,

            // Miscellaneous
            bool keep)
        {
            // Open a file reader
            Encoding enc = FileExtensions.GetEncoding(filename);
            SeparatedValueReader svr = new SeparatedValueReader(FileExtensions.TryOpenRead(filename), enc)
            {
                Header = true,
                Quotes = true,
                Separator = _delim,
                VerifyFieldCount = true
            };

            // If we're somehow at the end of the stream already, we can't do anything
            if (svr.EndOfStream)
                return;

            // Read in the header
            svr.ReadHeader();

            // Loop through all of the data lines
            while (!svr.EndOfStream)
            {
                try
                {
                    // Get the current line, split and parse
                    svr.ReadNextLine();
                }
                catch (InvalidDataException)
                {
                    Globals.Logger.Warning($"Malformed line found in '{filename}' at line {svr.LineNumber}");
                    continue;
                }

                // Set the output item information
                Machine machine = new Machine();

                bool? def = null,
                    writable = null,
                    optional = null;
                string name = null,
                    partName = null,
                    partInterface = null,
                    areaName = null,
                    biosDescription = null,
                    crc = null,
                    md5 = null,
#if NET_FRAMEWORK
                    ripemd160 = null,
#endif
                    sha1 = null,
                    sha256 = null,
                    sha384 = null,
                    sha512 = null,
                    merge = null,
                    region = null,
                    index = null,
                    language = null,
                    date = null,
                    bios = null,
                    offset = null;
                long? areaSize = null;
                long size = -1;
                ItemType itemType = ItemType.Rom;
                ItemStatus status = ItemStatus.None;
                List<KeyValuePair<string, string>> features = null;

                // Now we loop through and get values for everything
                for (int i = 0; i < svr.HeaderValues.Count; i++)
                {
                    string value = svr.Line[i];
                    switch (GetNormalizedHeader(svr.HeaderValues[i]))
                    {
                        #region DatFile

                        case "DatFile.FileName":
                            DatHeader.FileName = (string.IsNullOrWhiteSpace(DatHeader.FileName) ? value : DatHeader.FileName);
                            break;

                        case "DatFile.Name":
                            DatHeader.Name = (string.IsNullOrWhiteSpace(DatHeader.Name) ? value : DatHeader.Name);
                            break;

                        case "DatFile.Description":
                            DatHeader.Description = (string.IsNullOrWhiteSpace(DatHeader.Description) ? value : DatHeader.Description);
                            break;

                        case "DatFile.RootDir":
                            DatHeader.RootDir = (string.IsNullOrWhiteSpace(DatHeader.RootDir) ? value : DatHeader.RootDir);
                            break;

                        case "DatFile.Category":
                            DatHeader.Category = (string.IsNullOrWhiteSpace(DatHeader.Category) ? value : DatHeader.Category);
                            break;

                        case "DatFile.Version":
                            DatHeader.Version = (string.IsNullOrWhiteSpace(DatHeader.Version) ? value : DatHeader.Version);
                            break;

                        case "DatFile.Date":
                            DatHeader.Date = (string.IsNullOrWhiteSpace(DatHeader.Date) ? value : DatHeader.Date);
                            break;

                        case "DatFile.Author":
                            DatHeader.Author = (string.IsNullOrWhiteSpace(DatHeader.Author) ? value : DatHeader.Author);
                            break;

                        case "DatFile.Email":
                            DatHeader.Email = (string.IsNullOrWhiteSpace(DatHeader.Email) ? value : DatHeader.Email);
                            break;

                        case "DatFile.Homepage":
                            DatHeader.Homepage = (string.IsNullOrWhiteSpace(DatHeader.Homepage) ? value : DatHeader.Homepage);
                            break;

                        case "DatFile.Url":
                            DatHeader.Url = (string.IsNullOrWhiteSpace(DatHeader.Url) ? value : DatHeader.Url);
                            break;

                        case "DatFile.Comment":
                            DatHeader.Comment = (string.IsNullOrWhiteSpace(DatHeader.Comment) ? value : DatHeader.Comment);
                            break;

                        case "DatFile.Header":
                            DatHeader.Header = (string.IsNullOrWhiteSpace(DatHeader.Header) ? value : DatHeader.Header);
                            break;

                        case "DatFile.Type":
                            DatHeader.Type = (string.IsNullOrWhiteSpace(DatHeader.Type) ? value : DatHeader.Type);
                            break;

                        case "DatFile.ForceMerging":
                            DatHeader.ForceMerging = (DatHeader.ForceMerging == ForceMerging.None ? value.AsForceMerging() : DatHeader.ForceMerging);
                            break;

                        case "DatFile.ForceNodump":
                            DatHeader.ForceNodump = (DatHeader.ForceNodump == ForceNodump.None ? value.AsForceNodump() : DatHeader.ForceNodump);
                            break;

                        case "DatFile.ForcePacking":
                            DatHeader.ForcePacking = (DatHeader.ForcePacking == ForcePacking.None ? value.AsForcePacking() : DatHeader.ForcePacking);
                            break;

                        #endregion

                        #region Machine

                        case "Machine.Name":
                            machine.Name = value;
                            break;

                        case "Machine.Comment":
                            machine.Comment = value;
                            break;

                        case "Machine.Description":
                            machine.Description = value;
                            break;

                        case "Machine.Year":
                            machine.Year = value;
                            break;

                        case "Machine.Manufacturer":
                            machine.Manufacturer = value;
                            break;

                        case "Machine.Publisher":
                            machine.Publisher = value;
                            break;

                        case "Machine.Category":
                            machine.Category = value;
                            break;

                        case "Machine.RomOf":
                            machine.RomOf = value;
                            break;

                        case "Machine.CloneOf":
                            machine.CloneOf = value;
                            break;

                        case "Machine.SampleOf":
                            machine.SampleOf = value;
                            break;

                        case "Machine.Supported":
                            machine.Supported = value.AsYesNo();
                            break;

                        case "Machine.SourceFile":
                            machine.SourceFile = value;
                            break;

                        case "Machine.Runnable":
                            machine.Runnable = value.AsYesNo();
                            break;

                        case "Machine.Board":
                            machine.Board = value;
                            break;

                        case "Machine.RebuildTo":
                            machine.RebuildTo = value;
                            break;

                        case "Machine.Devices":
                            machine.Devices = new List<string>();
                            var devices = value.Split(';');
                            foreach (var device in devices)
                            {
                                machine.Devices.Add(device);
                            }

                            break;

                        case "Machine.SlotOptions":
                            machine.SlotOptions = new List<string>();
                            var slotOptions = value.Split(';');
                            foreach (var slotOption in slotOptions)
                            {
                                machine.SlotOptions.Add(slotOption);
                            }

                            break;

                        case "Machine.Infos":
                            machine.Infos = new List<KeyValuePair<string, string>>();
                            var infos = value.Split(';');
                            foreach (var info in infos)
                            {
                                var infoPair = info.Split('=');
                                machine.Infos.Add(new KeyValuePair<string, string>(infoPair[0], infoPair[1]));
                            }

                            break;

                        case "Machine.MachineType":
                            machine.MachineType = value.AsMachineType();
                            break;

                        #endregion

                        #region DatItem

                        case "DatItem.Type":
                            itemType = value.AsItemType() ?? ItemType.Rom;
                            break;

                        case "DatItem.Name":
                            name = string.IsNullOrWhiteSpace(value) ? name : value;
                            break;

                        case "DatItem.PartName":
                            partName = string.IsNullOrWhiteSpace(value) ? partName : value;
                            break;

                        case "DatItem.PartInterface":
                            partInterface = string.IsNullOrWhiteSpace(value) ? partInterface : value;
                            break;

                        case "DatItem.Features":
                            features = new List<KeyValuePair<string, string>>();
                            var splitFeatures = value.Split(';');
                            foreach (var splitFeature in splitFeatures)
                            {
                                var featurePair = splitFeature.Split('=');
                                features.Add(new KeyValuePair<string, string>(featurePair[0], featurePair[1]));
                            }

                            break;

                        case "DatItem.AreaName":
                            areaName = string.IsNullOrWhiteSpace(value) ? areaName : value;
                            break;

                        case "DatItem.AreaSize":
                            if (Int64.TryParse(value, out long tempAreaSize))
                                areaSize = tempAreaSize;
                            else
                                areaSize = null;

                            break;

                        case "DatItem.Default":
                            def = value.AsYesNo();
                            break;

                        case "DatItem.Description":
                            biosDescription = string.IsNullOrWhiteSpace(value) ? biosDescription : value;
                            break;

                        case "DatItem.Size":
                            if (!Int64.TryParse(value, out size))
                                size = -1;

                            break;

                        case "DatItem.CRC":
                            crc = value;
                            break;

                        case "DatItem.MD5":
                            md5 = value;
                            break;

#if NET_FRAMEWORK
                        case "DatItem.RIPEMD160":
                            ripemd160 = value;
                            break;
#endif

                        case "DatItem.SHA1":
                            sha1 = value;
                            break;

                        case "DatItem.SHA256":
                            sha256 = value;
                            break;

                        case "DatItem.SHA384":
                            sha384 = value;
                            break;

                        case "DatItem.SHA512":
                            sha512 = value;
                            break;

                        case "DatItem.Merge":
                            merge = string.IsNullOrWhiteSpace(value) ? merge : value;
                            break;

                        case "DatItem.Region":
                            region = string.IsNullOrWhiteSpace(value) ? region : value;
                            break;

                        case "DatItem.Index":
                            index = string.IsNullOrWhiteSpace(value) ? index : value;
                            break;

                        case "DatItem.Writable":
                            writable = value.AsYesNo();
                            break;

                        case "DatItem.Optional":
                            optional = value.AsYesNo();
                            break;

                        case "DatItem.Status":
                            status = value.AsItemStatus();
                            break;

                        case "DatItem.Language":
                            language = string.IsNullOrWhiteSpace(value) ? language : value;
                            break;

                        case "DatItem.Date":
                            date = value;
                            break;

                        case "DatItem.Bios":
                            bios = string.IsNullOrWhiteSpace(value) ? bios : value;
                            break;

                        case "DatItem.Offset":
                            offset = string.IsNullOrWhiteSpace(value) ? offset : value;
                            break;

                        #endregion

                        case "INVALID":
                        default:
                            // No-op, we don't even care right now
                            break;
                    }
                }

                // And now we populate and add the new item
                switch (itemType)
                {
                    case ItemType.Archive:
                        Archive archive = new Archive()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        archive.CopyMachineInformation(machine);
                        ParseAddHelper(archive);
                        break;

                    case ItemType.BiosSet:
                        BiosSet biosset = new BiosSet()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            Description = biosDescription,
                            Default = def,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        biosset.CopyMachineInformation(machine);
                        ParseAddHelper(biosset);
                        break;

                    case ItemType.Disk:
                        Disk disk = new Disk()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            MD5 = md5,
#if NET_FRAMEWORK
                            RIPEMD160 = ripemd160,
#endif
                            SHA1 = sha1,
                            SHA256 = sha256,
                            SHA384 = sha384,
                            SHA512 = sha512,
                            MergeTag = merge,
                            Region = region,
                            Index = index,
                            Writable = writable,
                            ItemStatus = status,
                            Optional = optional,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        disk.CopyMachineInformation(machine);
                        ParseAddHelper(disk);
                        break;

                    case ItemType.Release:
                        Release release = new Release()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            Region = region,
                            Language = language,
                            Date = date,
                            Default = default,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        release.CopyMachineInformation(machine);
                        ParseAddHelper(release);
                        break;

                    case ItemType.Rom:
                        Rom rom = new Rom()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            Bios = bios,
                            Size = size,
                            CRC = crc,
                            MD5 = md5,
#if NET_FRAMEWORK
                            RIPEMD160 = ripemd160,
#endif
                            SHA1 = sha1,
                            SHA256 = sha256,
                            SHA384 = sha384,
                            SHA512 = sha512,
                            MergeTag = merge,
                            Region = region,
                            Offset = offset,
                            Date = date,
                            ItemStatus = status,
                            Optional = optional,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        rom.CopyMachineInformation(machine);
                        ParseAddHelper(rom);
                        break;

                    case ItemType.Sample:
                        Sample sample = new Sample()
                        {
                            Name = name,
                            PartName = partName,
                            PartInterface = partInterface,
                            Features = features,
                            AreaName = areaName,
                            AreaSize = areaSize,

                            IndexId = indexId,
                            IndexSource = filename,
                        };

                        sample.CopyMachineInformation(machine);
                        ParseAddHelper(sample);
                        break;
                }
            }
        }

        /// <summary>
        /// Get normalized header value for a given separated value header field
        /// </summary>
        private string GetNormalizedHeader(string header)
        {
            switch (header.ToLowerInvariant())
            {
                #region DatFile

                case "file":
                case "filename":
                case "file name":
                    return "DatFile.FileName";

                case "datname":
                case "dat name":
                case "internalname":
                case "internal name":
                    return "DatFile.Name";

                case "description":
                case "dat description":
                    return "DatFile.Description";

                case "rootdir":
                case "root dir":
                    return "DatFile.RootDir";

                case "category":
                    return "DatFile.Category";

                case "version":
                    return "DatFile.Version";

                case "datdate":
                case "dat date":
                    return "DatFile.Date";

                case "author":
                    return "DatFile.Author";

                case "email":
                    return "DatFile.Email";

                case "homepage":
                    return "DatFile.Homepage";

                case "url":
                    return "DatFile.Url";

                case "datcomment":
                case "dat comment":
                    return "DatFile.Comment";

                case "header":
                    return "DatFile.Header";

                case "dattype":
                case "dat type":
                    return "DatFile.Type";

                case "forcemerging":
                case "force merging":
                    return "DatFile.ForceMerging";

                case "forcenodump":
                case "force nodump":
                    return "DatFile.ForceNodump";

                case "forcepacking":
                case "force packing":
                    return "DatFile.ForcePacking";

                #endregion

                #region Machine

                case "game":
                case "gamename":
                case "game name":
                case "machine":
                case "machinename":
                case "machine name":
                    return "Machine.Name";

                case "comment":
                    return "Machine.Comment";

                case "gamedescription":
                case "game description":
                case "machinedescription":
                case "machine description":
                    return "Machine.Description";

                case "year":
                    return "Machine.Year";

                case "manufacturer":
                    return "Machine.Manufacturer";

                case "publisher":
                    return "Machine.Publisher";

                case "romof":
                    return "Machine.RomOf";

                case "cloneof":
                    return "Machine.CloneOf";

                case "sampleof":
                    return "Machine.SampleOf";

                case "supported":
                    return "Machine.Supported";

                case "sourcefile":
                case "source file":
                    return "Machine.SourceFile";

                case "runnable":
                    return "Machine.Runnable";

                case "board":
                    return "Machine.Board";

                case "rebuildto":
                case "rebuild to":
                    return "Machine.RebuildTo";

                case "devices":
                    return "Machine.Devices";

                case "slotoptions":
                case "slot options":
                    return "Machine.SlotOptions";

                case "infos":
                    return "Machine.Infos";

                case "gametype":
                case "game type":
                case "machinetype":
                case "machine type":
                    return "Machine.MachineType";

                #endregion

                #region DatItem

                case "itemtype":
                case "item type":
                case "type":
                    return "DatItem.Type";

                case "disk":
                case "diskname":
                case "disk name":
                case "item":
                case "itemname":
                case "item name":
                case "name":
                case "rom":
                case "romname":
                case "rom name":
                    return "DatItem.Name";

                case "partname":
                case "part name":
                    return "DatItem.PartName";

                case "partinterface":
                case "part interface":
                    return "DatItem.PartInterface";

                case "features":
                    return "DatItem.Features";

                case "areaname":
                case "area name":
                    return "DatItem.AreaName";

                case "areasize":
                case "area size":
                    return "DatItem.AreaSize";

                case "default":
                    return "DatItem.Default";

                case "biosdescription":
                case "bios description":
                    return "DatItem.Description";

                case "itemsize":
                case "item size":
                case "size":
                    return "DatItem.Size";

                case "crc":
                case "crc hash":
                    return "DatItem.CRC";

                case "md5":
                case "md5 hash":
                    return "DatItem.MD5";

                case "ripemd":
                case "ripemd160":
                case "ripemd hash":
                case "ripemd160 hash":
                    return "DatItem.RIPEMD160";

                case "sha1":
                case "sha-1":
                case "sha1 hash":
                case "sha-1 hash":
                    return "DatItem.SHA1";

                case "sha256":
                case "sha-256":
                case "sha256 hash":
                case "sha-256 hash":
                    return "DatItem.SHA256";

                case "sha384":
                case "sha-384":
                case "sha384 hash":
                case "sha-384 hash":
                    return "DatItem.SHA384";

                case "sha512":
                case "sha-512":
                case "sha512 hash":
                case "sha-512 hash":
                    return "DatItem.SHA512";

                case "merge":
                case "mergetag":
                case "merge tag":
                    return "DatItem.Merge";

                case "region":
                    return "DatItem.Region";

                case "index":
                    return "DatItem.Index";

                case "writable":
                    return "DatItem.Writable";

                case "optional":
                    return "DatItem.Optional";

                case "nodump":
                case "no dump":
                case "status":
                case "item status":
                    return "DatItem.Status";

                case "language":
                    return "DatItem.Language";

                case "date":
                    return "DatItem.Date";

                case "bios":
                    return "DatItem.Bios";

                case "offset":
                    return "DatItem.Offset";

                #endregion

                default:
                    return "INVALID";
            }
        }

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false)
        {
            try
            {
                Globals.Logger.User($"Opening file for writing: {outfile}");
                FileStream fs = FileExtensions.TryCreate(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    Globals.Logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                SeparatedValueWriter svw = new SeparatedValueWriter(fs, new UTF8Encoding(false))
                {
                    Quotes = true,
                    Separator = this._delim,
                    VerifyFieldCount = true
                };

                // Write out the header
                WriteHeader(svw);

                // Get a properly sorted set of keys
                List<string> keys = Keys;
                keys.Sort(new NaturalComparer());

                foreach (string key in keys)
                {
                    List<DatItem> roms = this[key];

                    // Resolve the names in the block
                    roms = DatItem.ResolveNames(roms);

                    for (int index = 0; index < roms.Count; index++)
                    {
                        DatItem rom = roms[index];

                        // There are apparently times when a null rom can skip by, skip them
                        if (rom.Name == null || rom.MachineName == null)
                        {
                            Globals.Logger.Warning("Null rom found!");
                            continue;
                        }

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");
                        }

                        // Now, output the rom data
                        WriteDatItem(svw, rom, ignoreblanks);
                    }
                }

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                svw.Dispose();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT header using the supplied StreamWriter
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(SeparatedValueWriter svw)
        {
            try
            {
                string[] headers = new string[]
                {
                    "File Name",
                    "Internal Name",
                    "Description",
                    "Game Name",
                    "Game Description",
                    "Type",
                    "Rom Name",
                    "Disk Name",
                    "Size",
                    "CRC",
                    "MD5",
                    //"RIPEMD160",
                    "SHA1",
                    "SHA256",
                    //"SHA384",
                    //"SHA512",
                    "Nodump",
                };

                svw.WriteHeader(headers);

                svw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(SeparatedValueWriter svw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            try
            {
                // Separated values should only output Rom and Disk
                if (datItem.ItemType != ItemType.Disk && datItem.ItemType != ItemType.Rom)
                    return true;

                // Build the state based on excluded fields
                // TODO: Can we have some way of saying what fields to write out? Support for read extends to all fields now
                string[] fields = new string[14]; // 17;
                fields[0] = DatHeader.FileName;
                fields[1] = DatHeader.Name;
                fields[2] = DatHeader.Description;
                fields[3] = datItem.GetField(Field.MachineName, DatHeader.ExcludeFields);
                fields[4] = datItem.GetField(Field.Description, DatHeader.ExcludeFields);

                switch (datItem.ItemType)
                {
                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        fields[5] = "disk";
                        fields[6] = string.Empty;
                        fields[7] = disk.GetField(Field.Name, DatHeader.ExcludeFields);
                        fields[8] = string.Empty;
                        fields[9] = string.Empty;
                        fields[10] = disk.GetField(Field.MD5, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[11] = disk.GetField(Field.RIPEMD160, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[11] = disk.GetField(Field.SHA1, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[12] = disk.GetField(Field.SHA256, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[13] = disk.GetField(Field.SHA384, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[14] = disk.GetField(Field.SHA512, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[13] = disk.GetField(Field.Status, DatHeader.ExcludeFields);
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        fields[5] = "rom";
                        fields[6] = rom.GetField(Field.Name, DatHeader.ExcludeFields);
                        fields[7] = string.Empty;
                        fields[8] = rom.GetField(Field.Size, DatHeader.ExcludeFields);
                        fields[9] = rom.GetField(Field.CRC, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[10] = rom.GetField(Field.MD5, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[11] = rom.GetField(Field.RIPEMD160, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[11] = rom.GetField(Field.SHA1, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[12] = rom.GetField(Field.SHA256, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[13] = rom.GetField(Field.SHA384, DatHeader.ExcludeFields).ToLowerInvariant();
                        //fields[14] = rom.GetField(Field.SHA512, DatHeader.ExcludeFields).ToLowerInvariant();
                        fields[13] = rom.GetField(Field.Status, DatHeader.ExcludeFields);
                        break;
                }

                svw.WriteString(CreatePrefixPostfix(datItem, true));
                svw.WriteValues(fields, false);
                svw.WriteString(CreatePrefixPostfix(datItem, false));
                svw.WriteLine();

                svw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
