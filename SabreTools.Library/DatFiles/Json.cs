using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using NaturalSort;
using Newtonsoft.Json;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents parsing and writing of a JSON DAT
    /// </summary>
    internal class Json : DatFile
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Json(DatFile datFile)
            : base(datFile, cloneHeader: false)
        {
        }

        /// <summary>
        /// Parse a Logiqx XML DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        public override void ParseFile(
            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode)
        {
            // Prepare all internal variables
            StreamReader sr = new StreamReader(FileExtensions.TryOpenRead(filename), new UTF8Encoding(false));
            JsonTextReader jtr = new JsonTextReader(sr);

            // If we got a null reader, just return
            if (jtr == null)
                return;

            // Otherwise, read the file to the end
            try
            {
                jtr.Read();
                while (!sr.EndOfStream)
                {
                    // Skip everything not a property name
                    if (jtr.TokenType != JsonToken.PropertyName)
                    {
                        jtr.Read();
                        continue;
                    }

                    switch (jtr.Value)
                    {
                        // Header value
                        case "header":
                            ReadHeader(sr, jtr, keep);
                            jtr.Read();
                            break;

                        // Machine array
                        case "machines":
                            ReadMachines(sr, jtr, clean, remUnicode);
                            jtr.Read();
                            break;

                        default:
                            jtr.Read();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning($"Exception found while parsing '{filename}': {ex}");
            }

            jtr.Close();
        }

        /// <summary>
        /// Read header information
        /// </summary>
        /// <param name="sr">StreamReader to use to parse the header</param>
        /// <param name="jtr">JsonTextReader to use to parse the header</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        private void ReadHeader(StreamReader sr, JsonTextReader jtr, bool keep)
        {
            bool superdat = false;

            // If the reader is invalid, skip
            if (jtr == null)
                return;

            jtr.Read();
            while (!sr.EndOfStream)
            {
                // If we hit the end of the header, return
                if (jtr.TokenType == JsonToken.EndObject)
                    return;

                // We don't care about anything except property names
                if (jtr.TokenType != JsonToken.PropertyName)
                {
                    jtr.Read();
                    continue;
                }

                // Get all header items (ONLY OVERWRITE IF THERE'S NO DATA)
                string content;
                switch (jtr.Value)
                {
                    case "name":
                        content = jtr.ReadAsString();
                        Name = (string.IsNullOrWhiteSpace(Name) ? content : Name);
                        superdat = superdat || content.Contains(" - SuperDAT");
                        if (keep && superdat)
                        {
                            Type = (string.IsNullOrWhiteSpace(Type) ? "SuperDAT" : Type);
                        }
                        break;

                    case "description":
                        content = jtr.ReadAsString();
                        Description = (string.IsNullOrWhiteSpace(Description) ? content : Description);
                        break;

                    case "rootdir": // This is exclusive to TruRip XML
                        content = jtr.ReadAsString();
                        RootDir = (string.IsNullOrWhiteSpace(RootDir) ? content : RootDir);
                        break;

                    case "category":
                        content = jtr.ReadAsString();
                        Category = (string.IsNullOrWhiteSpace(Category) ? content : Category);
                        break;

                    case "version":
                        content = jtr.ReadAsString();
                        Version = (string.IsNullOrWhiteSpace(Version) ? content : Version);
                        break;

                    case "date":
                        content = jtr.ReadAsString();
                        Date = (string.IsNullOrWhiteSpace(Date) ? content.Replace(".", "/") : Date);
                        break;

                    case "author":
                        content = jtr.ReadAsString();
                        Author = (string.IsNullOrWhiteSpace(Author) ? content : Author);
                        break;

                    case "email":
                        content = jtr.ReadAsString();
                        Email = (string.IsNullOrWhiteSpace(Email) ? content : Email);
                        break;

                    case "homepage":
                        content = jtr.ReadAsString();
                        Homepage = (string.IsNullOrWhiteSpace(Homepage) ? content : Homepage);
                        break;

                    case "url":
                        content = jtr.ReadAsString();
                        Url = (string.IsNullOrWhiteSpace(Url) ? content : Url);
                        break;

                    case "comment":
                        content = jtr.ReadAsString();
                        Comment = (string.IsNullOrWhiteSpace(Comment) ? content : Comment);
                        break;

                    case "type": // This is exclusive to TruRip XML
                        content = jtr.ReadAsString();
                        Type = (string.IsNullOrWhiteSpace(Type) ? content : Type);
                        superdat = superdat || content.Contains("SuperDAT");
                        break;

                    case "forcemerging":
                        if (ForceMerging == ForceMerging.None)
                            ForceMerging = jtr.ReadAsString().AsForceMerging();

                        break;

                    case "forcepacking":
                        if (ForcePacking == ForcePacking.None)
                            ForcePacking = jtr.ReadAsString().AsForcePacking();

                        break;

                    case "forcenodump":
                        if (ForceNodump == ForceNodump.None)
                            ForceNodump = jtr.ReadAsString().AsForceNodump();

                        break;

                    case "header":
                        content = jtr.ReadAsString();
                        Header = (string.IsNullOrWhiteSpace(Header) ? content : Header);
                        break;

                    default:
                        break;
                }

                jtr.Read();
            }
        }

        /// <summary>
        /// Read machine array information
        /// </summary>
        /// <param name="sr">StreamReader to use to parse the header</param>
        /// <param name="itr">JsonTextReader to use to parse the machine</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadMachines(
            StreamReader sr,
            JsonTextReader jtr,

            // Miscellaneous
            bool clean,
            bool remUnicode)
        {
            // If the reader is invalid, skip
            if (jtr == null)
                return;

            jtr.Read();
            while (!sr.EndOfStream)
            {
                // If we hit the end of an array, we want to return
                if (jtr.TokenType == JsonToken.EndArray)
                    return;

                // We don't care about anything except start object
                if (jtr.TokenType != JsonToken.StartObject)
                {
                    jtr.Read();
                    continue;
                }

                ReadMachine(sr, jtr, clean, remUnicode);
                jtr.Read();
            }
        }

        /// <summary>
        /// Read machine information
        /// </summary>
        /// <param name="sr">StreamReader to use to parse the header</param>
        /// <param name="itr">JsonTextReader to use to parse the machine</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadMachine(
            StreamReader sr,
            JsonTextReader jtr,

            // Miscellaneous
            bool clean,
            bool remUnicode)
        {
            // If we have an empty machine, skip it
            if (jtr == null)
                return;

            // Prepare internal variables
            Machine machine = new Machine();

            jtr.Read();
            while (!sr.EndOfStream)
            {
                // If we hit the end of the machine, return
                if (jtr.TokenType == JsonToken.EndObject)
                    return;

                // We don't care about anything except properties
                if (jtr.TokenType != JsonToken.PropertyName)
                {
                    jtr.Read();
                    continue;
                }

                switch (jtr.Value)
                {
                    case "name":
                        machine.Name = jtr.ReadAsString();
                        break;

                    case "comment":
                        machine.Comment = jtr.ReadAsString();
                        break;

                    case "description":
                        machine.Description = jtr.ReadAsString();
                        break;

                    case "year":
                        machine.Year = jtr.ReadAsString();
                        break;

                    case "manufacturer":
                        machine.Manufacturer = jtr.ReadAsString();
                        break;

                    case "publisher":
                        machine.Publisher = jtr.ReadAsString();
                        break;

                    case "romof":
                        machine.RomOf = jtr.ReadAsString();
                        break;

                    case "cloneof":
                        machine.CloneOf = jtr.ReadAsString();
                        break;

                    case "sampleof":
                        machine.SampleOf = jtr.ReadAsString();
                        break;

                    case "supported":
                        string supported = jtr.ReadAsString();
                        switch (supported)
                        {
                            case "yes":
                                machine.Supported = true;
                                break;
                            case "no":
                                machine.Supported = false;
                                break;
                            case "partial":
                                machine.Supported = null;
                                break;
                        }
                        break;

                    case "sourcefile":
                        machine.SourceFile = jtr.ReadAsString();
                        break;

                    case "runnable":
                        string runnable = jtr.ReadAsString();
                        switch (runnable)
                        {
                            case "yes":
                                machine.Runnable = true;
                                break;
                            case "no":
                                machine.Runnable = false;
                                break;
                        }
                        break;

                    case "board":
                        machine.Board = jtr.ReadAsString();
                        break;

                    case "rebuildto":
                        machine.RebuildTo = jtr.ReadAsString();
                        break;

                    case "devices":
                        machine.Devices = new List<string>();
                        jtr.Read(); // Start Array
                        while (!sr.EndOfStream && jtr.TokenType != JsonToken.EndArray)
                        {
                            machine.Devices.Add(jtr.ReadAsString());
                        }

                        break;

                    case "slotoptions":
                        machine.SlotOptions = new List<string>();
                        jtr.Read(); // Start Array
                        while (!sr.EndOfStream && jtr.TokenType != JsonToken.EndArray)
                        {
                            machine.SlotOptions.Add(jtr.ReadAsString());
                        }

                        break;

                    case "infos":
                        machine.Infos = new List<KeyValuePair<string, string>>();
                        jtr.Read(); // Start Array
                        while (!sr.EndOfStream)
                        {
                            jtr.Read(); // Start object (or end array)
                            if (jtr.TokenType == JsonToken.EndArray)
                                break;

                            jtr.Read(); // Key
                            string key = jtr.Value as string;
                            string value = jtr.ReadAsString();
                            jtr.Read(); // End object

                            machine.Infos.Add(new KeyValuePair<string, string>(key, value));
                        }

                        break;

                    case "isbios":
                        string isbios = jtr.ReadAsString();
                        if (string.Equals(isbios, "yes", StringComparison.OrdinalIgnoreCase))
                            machine.MachineType &= MachineType.Bios;

                        break;

                    case "isdevice":
                        string isdevice = jtr.ReadAsString();
                        if (string.Equals(isdevice, "yes", StringComparison.OrdinalIgnoreCase))
                            machine.MachineType &= MachineType.Device;

                        break;

                    case "ismechanical":
                        string ismechanical = jtr.ReadAsString();
                        if (string.Equals(ismechanical, "yes", StringComparison.OrdinalIgnoreCase))
                            machine.MachineType &= MachineType.Mechanical;

                        break;

                    case "items":
                        ReadItems(sr, jtr, clean, remUnicode, machine);
                        break;

                    default:
                        break;
                }

                jtr.Read();
            }
        }

        /// <summary>
        /// Read item array information
        /// </summary>
        /// <param name="sr">StreamReader to use to parse the header</param>
        /// <param name="itr">JsonTextReader to use to parse the machine</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadItems(
            StreamReader sr,
            JsonTextReader jtr,

            // Miscellaneous
            bool clean,
            bool remUnicode,
            Machine machine)
        {
            // If the reader is invalid, skip
            if (jtr == null)
                return;

            jtr.Read();
            while (!sr.EndOfStream)
            {
                // If we hit the end of an array, we want to return
                if (jtr.TokenType == JsonToken.EndArray)
                    return;

                // We don't care about anything except start object
                if (jtr.TokenType != JsonToken.StartObject)
                {
                    jtr.Read();
                    continue;
                }

                ReadItem(sr, jtr, clean, remUnicode, machine);
                jtr.Read();
            }
        }

        /// <summary>
        /// Read item information
        /// </summary>
        /// <param name="sr">StreamReader to use to parse the header</param>
        /// <param name="itr">JsonTextReader to use to parse the machine</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        private void ReadItem(
            StreamReader sr,
            JsonTextReader jtr,

            // Miscellaneous
            bool clean,
            bool remUnicode,
            Machine machine)
        {
            // If we have an empty machine, skip it
            if (jtr == null)
                return;

            // Prepare internal variables
            bool? def = null,
                writable = null,
                optional = null;
            long size = -1;
            long? areaSize = null;
            string name = null,
                partName = null,
                partInterface = null,
                areaName = null,
                biosDescription = null,
                region = null,
                language = null,
                date = null,
                crc = null,
                md5 = null,
                ripemd160 = null,
                sha1 = null,
                sha256 = null,
                sha384 = null,
                sha512 = null,
                merge = null,
                index = null,
                offset = null,
                bios = null;
            ItemStatus? itemStatus = null;
            ItemType? itemType = null;
            List<KeyValuePair<string, string>> features = null;

            jtr.Read();
            while (!sr.EndOfStream)
            {
                // If we hit the end of the machine - create, add, and return
                if (jtr.TokenType == JsonToken.EndObject)
                {
                    // If we didn't read something valid, just return
                    if (itemType == null)
                        return;

                    DatItem datItem = DatItem.Create(itemType.Value);
                    datItem.CopyMachineInformation(machine);

                    datItem.Name = name;
                    datItem.PartName = partName;
                    datItem.PartInterface = partInterface;
                    datItem.Features = features;
                    datItem.AreaName = areaName;
                    datItem.AreaSize = areaSize;

                    if (itemType == ItemType.BiosSet)
                    {
                        (datItem as BiosSet).Description = biosDescription;
                        (datItem as BiosSet).Default = def;
                    }
                    else if (itemType == ItemType.Disk)
                    {
                        (datItem as Disk).MD5 = md5;
#if NET_FRAMEWORK
                        (datItem as Disk).RIPEMD160 = ripemd160;
#endif
                        (datItem as Disk).SHA1 = sha1;
                        (datItem as Disk).SHA256 = sha256;
                        (datItem as Disk).SHA384 = sha384;
                        (datItem as Disk).SHA512 = sha512;
                        (datItem as Disk).MergeTag = merge;
                        (datItem as Disk).Region = region;
                        (datItem as Disk).Index = index;
                        (datItem as Disk).Writable = writable;
                        (datItem as Disk).ItemStatus = itemStatus ?? ItemStatus.None;
                        (datItem as Disk).Optional = optional;
                    }
                    else if (itemType == ItemType.Release)
                    {
                        (datItem as Release).Region = region;
                        (datItem as Release).Language = language;
                        (datItem as Release).Date = date;
                        (datItem as Release).Default = def;
                    }
                    else if (itemType == ItemType.Rom)
                    {
                        (datItem as Rom).Bios = bios;
                        (datItem as Rom).Size = size;
                        (datItem as Rom).CRC = crc;
                        (datItem as Rom).MD5 = md5;
#if NET_FRAMEWORK
                        (datItem as Rom).RIPEMD160 = ripemd160;
#endif
                        (datItem as Rom).SHA1 = sha1;
                        (datItem as Rom).SHA256 = sha256;
                        (datItem as Rom).SHA384 = sha384;
                        (datItem as Rom).SHA512 = sha512;
                        (datItem as Rom).MergeTag = merge;
                        (datItem as Rom).Region = region;
                        (datItem as Rom).Offset = offset;
                        (datItem as Rom).Date = date;
                        (datItem as Rom).ItemStatus = itemStatus ?? ItemStatus.None;
                        (datItem as Rom).Optional = optional;
                    }

                    ParseAddHelper(datItem, clean, remUnicode);

                    return;
                }

                // We don't care about anything except properties
                if (jtr.TokenType != JsonToken.PropertyName)
                {
                    jtr.Read();
                    continue;
                }

                switch (jtr.Value)
                {
                    case "type":
                        itemType = jtr.ReadAsString().AsItemType();
                        break;

                    case "name":
                        name = jtr.ReadAsString();
                        break;

                    case "partname":
                        partName = jtr.ReadAsString();
                        break;

                    case "partinterface":
                        partInterface = jtr.ReadAsString();
                        break;

                    case "features":
                        features = new List<KeyValuePair<string, string>>();
                        jtr.Read(); // Start Array
                        while (!sr.EndOfStream)
                        {
                            jtr.Read(); // Start object (or end array)
                            if (jtr.TokenType == JsonToken.EndArray)
                                break;

                            jtr.Read(); // Key
                            string key = jtr.Value as string;
                            string value = jtr.ReadAsString();
                            jtr.Read(); // End object

                            features.Add(new KeyValuePair<string, string>(key, value));
                        }

                        break;

                    case "areaname":
                        areaName = jtr.ReadAsString();
                        break;

                    case "areasize":
                        if (Int64.TryParse(jtr.ReadAsString(), out long tempAreaSize))
                            areaSize = tempAreaSize;
                        else
                            areaSize = null;

                        break;

                    case "description":
                        biosDescription = jtr.ReadAsString();
                        break;

                    case "default":
                        def = jtr.ReadAsBoolean();
                        break;

                    case "region":
                        region = jtr.ReadAsString();
                        break;

                    case "language":
                        language = jtr.ReadAsString();
                        break;

                    case "date":
                        date = jtr.ReadAsString();
                        break;

                    case "size":
                        if (!Int64.TryParse(jtr.ReadAsString(), out size))
                            size = -1;

                        break;

                    case "crc":
                        crc = jtr.ReadAsString();
                        break;

                    case "md5":
                        md5 = jtr.ReadAsString();
                        break;

                    case "ripemd160":
                        ripemd160 = jtr.ReadAsString();
                        break;

                    case "sha1":
                        sha1 = jtr.ReadAsString();
                        break;

                    case "sha256":
                        sha256 = jtr.ReadAsString();
                        break;

                    case "sha384":
                        sha384 = jtr.ReadAsString();
                        break;

                    case "sha512":
                        sha512 = jtr.ReadAsString();
                        break;

                    case "merge":
                        merge = jtr.ReadAsString();
                        break;

                    case "index":
                        index = jtr.ReadAsString();
                        break;

                    case "writable":
                        writable = jtr.ReadAsBoolean();
                        break;

                    case "status":
                        itemStatus = jtr.ReadAsString().AsItemStatus();
                        break;

                    case "optional":
                        optional = jtr.ReadAsBoolean();
                        break;

                    case "offset":
                        offset = jtr.ReadAsString();
                        break;

                    case "bios":
                        bios = jtr.ReadAsString();
                        break;

                    default:
                        break;
                }

                jtr.Read();
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

                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                JsonTextWriter jtw = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                // Write out the header
                WriteHeader(jtw);

                // Write out each of the machines and roms
                string lastgame = null;

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

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame != null && lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteEndGame(jtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame == null || lastgame.ToLowerInvariant() != rom.MachineName.ToLowerInvariant())
                            WriteStartGame(jtw, rom);

                        // If we have a "null" game (created by DATFromDir or something similar), log it to file
                        if (rom.ItemType == ItemType.Rom
                            && ((Rom)rom).Size == -1
                            && ((Rom)rom).CRC == "null")
                        {
                            Globals.Logger.Verbose($"Empty folder found: {rom.MachineName}");

                            rom.Name = (rom.Name == "null" ? "-" : rom.Name);
                            ((Rom)rom).Size = Constants.SizeZero;
                            ((Rom)rom).CRC = ((Rom)rom).CRC == "null" ? Constants.CRCZero : null;
                            ((Rom)rom).MD5 = ((Rom)rom).MD5 == "null" ? Constants.MD5Zero : null;
#if NET_FRAMEWORK
                            ((Rom)rom).RIPEMD160 = ((Rom)rom).RIPEMD160 == "null" ? Constants.RIPEMD160Zero : null;
#endif
                            ((Rom)rom).SHA1 = ((Rom)rom).SHA1 == "null" ? Constants.SHA1Zero : null;
                            ((Rom)rom).SHA256 = ((Rom)rom).SHA256 == "null" ? Constants.SHA256Zero : null;
                            ((Rom)rom).SHA384 = ((Rom)rom).SHA384 == "null" ? Constants.SHA384Zero : null;
                            ((Rom)rom).SHA512 = ((Rom)rom).SHA512 == "null" ? Constants.SHA512Zero : null;
                        }

                        // Now, output the rom data
                        WriteDatItem(jtw, rom, ignoreblanks);

                        // Set the new data to compare against
                        lastgame = rom.MachineName;
                    }
                }

                // Write the file footer out
                WriteFooter(jtw);

                Globals.Logger.Verbose("File written!" + Environment.NewLine);
                jtw.Close();
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
        /// <param name="jtw">JsonTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteHeader(JsonTextWriter jtw)
        {
            try
            {
                jtw.WriteStartObject();

                jtw.WritePropertyName("header");
                jtw.WriteStartObject();

                jtw.WritePropertyName("name");
                jtw.WriteValue(Name);
                jtw.WritePropertyName("description");
                jtw.WriteValue(Description);
                if (!string.IsNullOrWhiteSpace(RootDir))
                {
                    jtw.WritePropertyName("rootdir");
                    jtw.WriteValue(RootDir);
                }
                if (!string.IsNullOrWhiteSpace(Category))
                {
                    jtw.WritePropertyName("category");
                    jtw.WriteValue(Category);
                }
                jtw.WritePropertyName("version");
                jtw.WriteValue(Version);
                if (!string.IsNullOrWhiteSpace(Date))
                {
                    jtw.WritePropertyName("date");
                    jtw.WriteValue(Date);
                }
                jtw.WritePropertyName("author");
                jtw.WriteValue(Author);
                if (!string.IsNullOrWhiteSpace(Email))
                {
                    jtw.WritePropertyName("email");
                    jtw.WriteValue(Email);
                }
                if (!string.IsNullOrWhiteSpace(Homepage))
                {
                    jtw.WritePropertyName("homepage");
                    jtw.WriteValue(Homepage);
                }
                if (!string.IsNullOrWhiteSpace(Url))
                {
                    jtw.WritePropertyName("date");
                    jtw.WriteValue(Url);
                }
                if (!string.IsNullOrWhiteSpace(Comment))
                {
                    jtw.WritePropertyName("comment");
                    jtw.WriteValue(Comment);
                }
                if (!string.IsNullOrWhiteSpace(Type))
                {
                    jtw.WritePropertyName("type");
                    jtw.WriteValue(Type);
                }
                if (ForceMerging != ForceMerging.None)
                {
                    jtw.WritePropertyName("forcemerging");
                    switch (ForceMerging)
                    {
                        case ForceMerging.Full:
                            jtw.WriteValue("full");
                            break;
                        case ForceMerging.Split:
                            jtw.WriteValue("split");
                            break;
                        case ForceMerging.Merged:
                            jtw.WriteValue("merged");
                            break;
                        case ForceMerging.NonMerged:
                            jtw.WriteValue("nonmerged");
                            break;
                    }
                }
                if (ForcePacking != ForcePacking.None)
                {
                    jtw.WritePropertyName("forcepacking");
                    switch (ForcePacking)
                    {
                        case ForcePacking.Unzip:
                            jtw.WriteValue("unzip");
                            break;
                        case ForcePacking.Zip:
                            jtw.WriteValue("zip");
                            break;
                    }
                }
                if (ForceNodump != ForceNodump.None)
                {
                    jtw.WritePropertyName("forcenodump");
                    switch (ForceNodump)
                    {
                        case ForceNodump.Ignore:
                            jtw.WriteValue("ignore");
                            break;
                        case ForceNodump.Obsolete:
                            jtw.WriteValue("obsolete");
                            break;
                        case ForceNodump.Required:
                            jtw.WriteValue("required");
                            break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(Header))
                {
                    jtw.WritePropertyName("header");
                    jtw.WriteValue(Header);
                }

                // End header
                jtw.WriteEndObject();

                jtw.WritePropertyName("machines");
                jtw.WriteStartArray();

                jtw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="jtw">JsonTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteStartGame(JsonTextWriter jtw, DatItem datItem)
        {
            try
            {
                // No game should start with a path separator
                datItem.MachineName = datItem.MachineName.TrimStart(Path.DirectorySeparatorChar);

                // Build the state based on excluded fields
                jtw.WriteStartObject();

                jtw.WritePropertyName("name");
                jtw.WriteValue(datItem.GetField(Field.MachineName, ExcludeFields));

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Comment, ExcludeFields)))
                {
                    jtw.WritePropertyName("comment");
                    jtw.WriteValue(datItem.Comment);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Description, ExcludeFields)))
                {
                    jtw.WritePropertyName("description");
                    jtw.WriteValue(datItem.MachineDescription);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Year, ExcludeFields)))
                {
                    jtw.WritePropertyName("year");
                    jtw.WriteValue(datItem.Year);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Manufacturer, ExcludeFields)))
                {
                    jtw.WritePropertyName("manufacturer");
                    jtw.WriteValue(datItem.Manufacturer);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Publisher, ExcludeFields)))
                {
                    jtw.WritePropertyName("publisher");
                    jtw.WriteValue(datItem.Publisher);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RomOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.RomOf, StringComparison.OrdinalIgnoreCase))
                {
                    jtw.WritePropertyName("romof");
                    jtw.WriteValue(datItem.RomOf);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.CloneOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.CloneOf, StringComparison.OrdinalIgnoreCase))
                {
                    jtw.WritePropertyName("cloneof");
                    jtw.WriteValue(datItem.CloneOf);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SampleOf, ExcludeFields)) && !string.Equals(datItem.MachineName, datItem.SampleOf, StringComparison.OrdinalIgnoreCase))
                {
                    jtw.WritePropertyName("sampleof");
                    jtw.WriteValue(datItem.SampleOf);
                }
                if (!ExcludeFields[(int)Field.Supported] && datItem.Supported != null)
                {
                    if (datItem.Supported == true)
                    {
                        jtw.WritePropertyName("supported");
                        jtw.WriteValue("yes");
                    }
                    else if (datItem.Supported == false)
                    {
                        jtw.WritePropertyName("supported");
                        jtw.WriteValue("no");
                    }
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SourceFile, ExcludeFields)))
                {
                    jtw.WritePropertyName("sourcefile");
                    jtw.WriteValue(datItem.SourceFile);
                }
                if (!ExcludeFields[(int)Field.Runnable] && datItem.Runnable != null)
                {
                    if (datItem.Runnable == true)
                    {
                        jtw.WritePropertyName("runnable");
                        jtw.WriteValue("yes");
                    }
                    else if (datItem.Runnable == false)
                    {
                        jtw.WritePropertyName("runnable");
                        jtw.WriteValue("no");
                    }
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Board, ExcludeFields)))
                {
                    jtw.WritePropertyName("board");
                    jtw.WriteValue(datItem.Board);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.RebuildTo, ExcludeFields)))
                {
                    jtw.WritePropertyName("rebuildto");
                    jtw.WriteValue(datItem.RebuildTo);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Devices, ExcludeFields)))
                {
                    jtw.WritePropertyName("devices");
                    jtw.WriteStartArray();
                    foreach (string device in datItem.Devices)
                    {
                        jtw.WriteValue(device);
                    }

                    jtw.WriteEndArray();
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.SlotOptions, ExcludeFields)))
                {
                    jtw.WritePropertyName("slotoptions");
                    jtw.WriteStartArray();
                    foreach (string slotoption in datItem.SlotOptions)
                    {
                        jtw.WriteValue(slotoption);
                    }

                    jtw.WriteEndArray();
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Infos, ExcludeFields)))
                {
                    jtw.WritePropertyName("infos");
                    jtw.WriteStartArray();
                    foreach (var info in datItem.Infos)
                    {
                        jtw.WriteStartObject();
                        jtw.WritePropertyName(info.Key);
                        jtw.WriteValue(info.Value);
                        jtw.WriteEndObject();
                    }

                    jtw.WriteEndArray();
                }
                if (!ExcludeFields[(int)Field.MachineType])
                {
                    if ((datItem.MachineType & MachineType.Bios) != 0)
                    {
                        jtw.WritePropertyName("isbios");
                        jtw.WriteValue("yes");
                    }
                    if ((datItem.MachineType & MachineType.Device) != 0)
                    {
                        jtw.WritePropertyName("isdevice");
                        jtw.WriteValue("yes");
                    }
                    if ((datItem.MachineType & MachineType.Mechanical) != 0)
                    {
                        jtw.WritePropertyName("ismechanical");
                        jtw.WriteValue("yes");
                    }
                }

                jtw.WritePropertyName("items");
                jtw.WriteStartArray();

                jtw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out Game end using the supplied StreamWriter
        /// </summary>
        /// <param name="jtw">JsonTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteEndGame(JsonTextWriter jtw)
        {
            try
            {
                // End items
                jtw.WriteEndArray();

                // End machine
                jtw.WriteEndObject();

                jtw.Flush();
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
        /// <param name="jtw">JsonTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteDatItem(JsonTextWriter jtw, DatItem datItem, bool ignoreblanks = false)
        {
            // If we are in ignore blanks mode AND we have a blank (0-size) rom, skip
            if (ignoreblanks && (datItem.ItemType == ItemType.Rom && ((datItem as Rom).Size == 0 || (datItem as Rom).Size == -1)))
                return true;

            // If we have the blank item type somehow, skip
            if (datItem.ItemType == ItemType.Blank)
                return true;

            try
            {
                // Pre-process the item name
                ProcessItemName(datItem, true);

                // Build the state based on excluded fields
                jtw.WriteStartObject();
                jtw.WritePropertyName("type");

                switch (datItem.ItemType)
                {
                    case ItemType.Archive:
                        jtw.WriteValue("archive");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(datItem.GetField(Field.Name, ExcludeFields));
                        break;

                    case ItemType.BiosSet:
                        var biosSet = datItem as BiosSet;
                        jtw.WriteValue("biosset");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(biosSet.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.BiosDescription, ExcludeFields)))
                        {
                            jtw.WritePropertyName("description");
                            jtw.WriteValue(biosSet.Description);
                        }
                        if (!ExcludeFields[(int)Field.Default] && biosSet.Default != null)
                        {
                            jtw.WritePropertyName("default");
                            jtw.WriteValue(biosSet.Default);
                        }
                        break;

                    case ItemType.Disk:
                        var disk = datItem as Disk;
                        jtw.WriteValue("disk");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(disk.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.MD5, ExcludeFields)))
                        {
                            jtw.WritePropertyName("md5");
                            jtw.WriteValue(disk.MD5.ToLowerInvariant());
                        }
#if NET_FRAMEWORK
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.RIPEMD160, ExcludeFields)))
                        {
                            jtw.WritePropertyName("ripemd160");
                            jtw.WriteValue(disk.RIPEMD160.ToLowerInvariant());
                        }
#endif
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.SHA1, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha1");
                            jtw.WriteValue(disk.SHA1.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.SHA256, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha256");
                            jtw.WriteValue(disk.SHA256.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.SHA384, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha384");
                            jtw.WriteValue(disk.SHA384.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.SHA512, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha512");
                            jtw.WriteValue(disk.SHA512.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.Merge, ExcludeFields)))
                        {
                            jtw.WritePropertyName("merge");
                            jtw.WriteValue(disk.MergeTag);
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.Region, ExcludeFields)))
                        {
                            jtw.WritePropertyName("region");
                            jtw.WriteValue(disk.Region);
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.Index, ExcludeFields)))
                        {
                            jtw.WritePropertyName("index");
                            jtw.WriteValue(disk.Index);
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.Writable, ExcludeFields)))
                        {
                            jtw.WritePropertyName("writable");
                            jtw.WriteValue(disk.Writable);
                        }
                        if (!ExcludeFields[(int)Field.Status] && disk.ItemStatus != ItemStatus.None)
                        {
                            jtw.WritePropertyName("status");
                            jtw.WriteValue(disk.ItemStatus.ToString().ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(disk.GetField(Field.Optional, ExcludeFields)))
                        {
                            jtw.WritePropertyName("optional");
                            jtw.WriteValue(disk.Optional);
                        }
                        break;

                    case ItemType.Release:
                        var release = datItem as Release;
                        jtw.WriteValue("release");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(release.GetField(Field.Name, ExcludeFields));
                        if (!string.IsNullOrWhiteSpace(release.GetField(Field.Region, ExcludeFields)))
                        {
                            jtw.WritePropertyName("region");
                            jtw.WriteValue(release.Region);
                        }
                        if (!string.IsNullOrWhiteSpace(release.GetField(Field.Language, ExcludeFields)))
                        {
                            jtw.WritePropertyName("language");
                            jtw.WriteValue(release.Language);
                        }
                        if (!string.IsNullOrWhiteSpace(release.GetField(Field.Date, ExcludeFields)))
                        {
                            jtw.WritePropertyName("date");
                            jtw.WriteValue(release.Date);
                        }
                        if (!ExcludeFields[(int)Field.Default] && release.Default != null)
                        {
                            jtw.WritePropertyName("default");
                            jtw.WriteValue(release.Default);
                        }
                        break;

                    case ItemType.Rom:
                        var rom = datItem as Rom;
                        jtw.WriteValue("rom");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(rom.GetField(Field.Name, ExcludeFields));
                        if (!ExcludeFields[(int)Field.Size] && rom.Size != -1)
                        {
                            jtw.WritePropertyName("size");
                            jtw.WriteValue(rom.Size);
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Offset, ExcludeFields)))
                        {
                            jtw.WritePropertyName("offset");
                            jtw.WriteValue(rom.Offset);
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.CRC, ExcludeFields)))
                        {
                            jtw.WritePropertyName("crc");
                            jtw.WriteValue(rom.CRC.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.MD5, ExcludeFields)))
                        {
                            jtw.WritePropertyName("md5");
                            jtw.WriteValue(rom.MD5.ToLowerInvariant());
                        }
#if NET_FRAMEWORK
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.RIPEMD160, ExcludeFields)))
                        {
                            jtw.WritePropertyName("ripemd160");
                            jtw.WriteValue(rom.RIPEMD160.ToLowerInvariant());
                        }
#endif
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.SHA1, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha1");
                            jtw.WriteValue(rom.SHA1.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.SHA256, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha256");
                            jtw.WriteValue(rom.SHA256.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.SHA384, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha384");
                            jtw.WriteValue(rom.SHA384.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.SHA512, ExcludeFields)))
                        {
                            jtw.WritePropertyName("sha512");
                            jtw.WriteValue(rom.SHA512.ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Bios, ExcludeFields)))
                        {
                            jtw.WritePropertyName("bios");
                            jtw.WriteValue(rom.Bios);
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Merge, ExcludeFields)))
                        {
                            jtw.WritePropertyName("merge");
                            jtw.WriteValue(rom.MergeTag);
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Region, ExcludeFields)))
                        {
                            jtw.WritePropertyName("region");
                            jtw.WriteValue(rom.Region);
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Date, ExcludeFields)))
                        {
                            jtw.WritePropertyName("date");
                            jtw.WriteValue(rom.Date);
                        }
                        if (!ExcludeFields[(int)Field.Status] && rom.ItemStatus != ItemStatus.None)
                        {
                            jtw.WritePropertyName("status");
                            jtw.WriteValue(rom.ItemStatus.ToString().ToLowerInvariant());
                        }
                        if (!string.IsNullOrWhiteSpace(rom.GetField(Field.Optional, ExcludeFields)))
                        {
                            jtw.WritePropertyName("optional");
                            jtw.WriteValue(rom.Optional);
                        }
                        break;

                    case ItemType.Sample:
                        jtw.WriteValue("sample");
                        jtw.WritePropertyName("name");
                        jtw.WriteValue(datItem.GetField(Field.Name, ExcludeFields));
                        break;
                }

                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.PartName, ExcludeFields)))
                {
                    jtw.WritePropertyName("partname");
                    jtw.WriteValue(datItem.PartName);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.PartInterface, ExcludeFields)))
                {
                    jtw.WritePropertyName("partinterface");
                    jtw.WriteValue(datItem.PartInterface);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.Features, ExcludeFields)))
                {
                    jtw.WritePropertyName("features");
                    jtw.WriteStartArray();
                    foreach (var feature in datItem.Features)
                    {
                        jtw.WriteStartObject();
                        jtw.WritePropertyName(feature.Key);
                        jtw.WriteValue(feature.Value);
                        jtw.WriteEndObject();
                    }

                    jtw.WriteEndArray();
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.AreaName, ExcludeFields)))
                {
                    jtw.WritePropertyName("areaname");
                    jtw.WriteValue(datItem.AreaName);
                }
                if (!string.IsNullOrWhiteSpace(datItem.GetField(Field.AreaSize, ExcludeFields)))
                {
                    jtw.WritePropertyName("areasize");
                    jtw.WriteValue(datItem.AreaSize);
                }

                // End item
                jtw.WriteEndObject();

                jtw.Flush();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="jtw">JsonTextWriter to output to</param>
        /// <returns>True if the data was written, false on error</returns>
        private bool WriteFooter(JsonTextWriter jtw)
        {
            try
            {
                // End items
                jtw.WriteEndArray();

                // End machine
                jtw.WriteEndObject();

                // End machines
                jtw.WriteEndArray();

                // End file
                jtw.WriteEndObject();

                jtw.Flush();
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
