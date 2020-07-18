using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    /// TODO: Can clever use of Filtering allow for easier external splitting methods?
    public class Filter
    {
        #region Private instance variables

        #region Machine Filters

        /// <summary>
        /// Include or exclude machine names
        /// </summary>
        private FilterItem<string> MachineName = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine comments
        /// </summary>
        private FilterItem<string> Comment = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine descriptions
        /// </summary>
        private FilterItem<string> MachineDescription = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine years
        /// </summary>
        private FilterItem<string> Year = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine manufacturers
        /// </summary>
        private FilterItem<string> Manufacturer = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine publishers
        /// </summary>
        private FilterItem<string> Publisher = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine categories
        /// </summary>
        private FilterItem<string> Category = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine romof
        /// </summary>
        private FilterItem<string> RomOf = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine cloneof
        /// </summary>
        private FilterItem<string> CloneOf = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine sampleof
        /// </summary>
        private FilterItem<string> SampleOf = new FilterItem<string>();

        /// <summary>
        /// Include or exclude items with the "Supported" tag
        /// </summary>
        private FilterItem<bool?> Supported = new FilterItem<bool?>() { Neutral = null };

        /// <summary>
        /// Include or exclude machine source file
        /// </summary>
        private FilterItem<string> SourceFile = new FilterItem<string>();

        /// <summary>
        /// Include or exclude items with the "Runnable" tag
        /// </summary>
        private FilterItem<bool?> Runnable = new FilterItem<bool?>() { Neutral = null };

        /// <summary>
        /// Include or exclude machine board
        /// </summary>
        private FilterItem<string> Board = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine rebuildto
        /// </summary>
        private FilterItem<string> RebuildTo = new FilterItem<string>();

        // TODO: Machine.Devices - List<string>
        // TODO: Machine.SlotOptions - List<string>
        // TODO: Machine.Infos - List<KeyValuePair<string, string>>

        /// <summary>
        /// Include or exclude machine types
        /// </summary>
        private FilterItem<MachineType> MachineTypes = new FilterItem<MachineType>() { Positive = MachineType.NULL, Negative = MachineType.NULL };

        #endregion

        #region DatItem Filters

        /// <summary>
        /// Include or exclude item types
        /// </summary>
        private FilterItem<string> ItemTypes = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item names
        /// </summary>
        private FilterItem<string> ItemName = new FilterItem<string>();

        // TODO: DatItem.Features - List<KeyValuePair<string, string>>

        /// <summary>
        /// Include or exclude part names
        /// </summary>
        private FilterItem<string> PartName = new FilterItem<string>();

        /// <summary>
        /// Include or exclude part interfaces
        /// </summary>
        private FilterItem<string> PartInterface = new FilterItem<string>();

        /// <summary>
        /// Include or exclude area names
        /// </summary>
        private FilterItem<string> AreaName = new FilterItem<string>();

        /// <summary>
        /// Include or exclude area sizes
        /// </summary>
        /// <remarks>Positive means "Greater than or equal", Negative means "Less than or equal", Neutral means "Equal"</remarks>
        private FilterItem<long?> AreaSize = new FilterItem<long?>() { Positive = null, Negative = null, Neutral = null };

        /// <summary>
        /// Include or exclude items with the "Default" tag
        /// </summary>
        private FilterItem<bool?> Default = new FilterItem<bool?>() { Neutral = null };

        /// <summary>
        /// Include or exclude descriptions
        /// </summary>
        private FilterItem<string> Description = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item sizes
        /// </summary>
        /// <remarks>Positive means "Greater than or equal", Negative means "Less than or equal", Neutral means "Equal"</remarks>
        private FilterItem<long> Size = new FilterItem<long>() { Positive = -1, Negative = -1, Neutral = -1 };

        /// <summary>
        /// Include or exclude CRC32 hashes
        /// </summary>
        private FilterItem<string> CRC = new FilterItem<string>();

        /// <summary>
        /// Include or exclude MD5 hashes
        /// </summary>
        private FilterItem<string> MD5 = new FilterItem<string>();

#if NET_FRAMEWORK
        /// <summary>
        /// Include or exclude RIPEMD160 hashes
        /// </summary>
        private FilterItem<string> RIPEMD160 = new FilterItem<string>();
#endif

        /// <summary>
        /// Include or exclude SHA-1 hashes
        /// </summary>
        private FilterItem<string> SHA1 = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-256 hashes
        /// </summary>
        private FilterItem<string> SHA256 = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-384 hashes
        /// </summary>
        private FilterItem<string> SHA384 = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-512 hashes
        /// </summary>
        private FilterItem<string> SHA512 = new FilterItem<string>();

        /// <summary>
        /// Include or exclude merge tags
        /// </summary>
        private FilterItem<string> MergeTag = new FilterItem<string>();

        /// <summary>
        /// Include or exclude regions
        /// </summary>
        private FilterItem<string> Region = new FilterItem<string>();

        /// <summary>
        /// Include or exclude indexes
        /// </summary>
        private FilterItem<string> Index = new FilterItem<string>();

        /// <summary>
        /// Include or exclude items with the "Writable" tag
        /// </summary>
        private FilterItem<bool?> Writable = new FilterItem<bool?>() { Neutral = null };

        /// <summary>
        /// Include or exclude items with the "Writable" tag
        /// </summary>
        private FilterItem<bool?> Optional = new FilterItem<bool?>() { Neutral = null };

        /// <summary>
        /// Include or exclude item statuses
        /// </summary>
        private FilterItem<ItemStatus> Status = new FilterItem<ItemStatus>() { Positive = ItemStatus.NULL, Negative = ItemStatus.NULL };

        /// <summary>
        /// Include or exclude languages
        /// </summary>
        private FilterItem<string> Language = new FilterItem<string>();

        /// <summary>
        /// Include or exclude dates
        /// </summary>
        private FilterItem<string> Date = new FilterItem<string>();

        /// <summary>
        /// Include or exclude bioses
        /// </summary>
        private FilterItem<string> Bios = new FilterItem<string>();

        /// <summary>
        /// Include or exclude offsets
        /// </summary>
        private FilterItem<string> Offset = new FilterItem<string>();

        #endregion

        #endregion // Private instance variables

        #region Pubically facing variables

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
        public SplitType InternalSplit { get; set; }

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

        #endregion // Pubically facing variables

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

                case Field.MachineName:
                    if (negate)
                        MachineName.NegativeSet.Add(value);
                    else
                        MachineName.PositiveSet.Add(value);
                    break;

                case Field.Comment:
                    if (negate)
                        Comment.NegativeSet.Add(value);
                    else
                        Comment.PositiveSet.Add(value);
                    break;

                case Field.Description:
                    if (negate)
                        MachineDescription.NegativeSet.Add(value);
                    else
                        MachineDescription.PositiveSet.Add(value);
                    break;

                case Field.Year:
                    if (negate)
                        Year.NegativeSet.Add(value);
                    else
                        Year.PositiveSet.Add(value);
                    break;

                case Field.Manufacturer:
                    if (negate)
                        Manufacturer.NegativeSet.Add(value);
                    else
                        Manufacturer.PositiveSet.Add(value);
                    break;

                case Field.Publisher:
                    if (negate)
                        Publisher.NegativeSet.Add(value);
                    else
                        Publisher.PositiveSet.Add(value);
                    break;

                case Field.Category:
                    if (negate)
                        Category.NegativeSet.Add(value);
                    else
                        Category.PositiveSet.Add(value);
                    break;

                case Field.RomOf:
                    if (negate)
                        RomOf.NegativeSet.Add(value);
                    else
                        RomOf.PositiveSet.Add(value);
                    break;

                case Field.CloneOf:
                    if (negate)
                        CloneOf.NegativeSet.Add(value);
                    else
                        CloneOf.PositiveSet.Add(value);
                    break;

                case Field.SampleOf:
                    if (negate)
                        SampleOf.NegativeSet.Add(value);
                    else
                        SampleOf.PositiveSet.Add(value);
                    break;

                case Field.Supported:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Supported.Neutral = false;
                    else
                        Supported.Neutral = true;
                    break;

                case Field.SourceFile:
                    if (negate)
                        SourceFile.NegativeSet.Add(value);
                    else
                        SourceFile.PositiveSet.Add(value);
                    break;

                case Field.Runnable:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Runnable.Neutral = false;
                    else
                        Runnable.Neutral = true;
                    break;

                case Field.Board:
                    if (negate)
                        Board.NegativeSet.Add(value);
                    else
                        Board.PositiveSet.Add(value);
                    break;

                case Field.RebuildTo:
                    if (negate)
                        RebuildTo.NegativeSet.Add(value);
                    else
                        RebuildTo.PositiveSet.Add(value);
                    break;

                case Field.MachineType:
                    if (negate)
                        MachineTypes.Negative |= value.AsMachineType();
                    else
                        MachineTypes.Positive |= value.AsMachineType();
                    break;

                #endregion

                #region DatItem Filters

                case Field.ItemType:
                    if (value.AsItemType() == null)
                        return;

                    if (negate)
                        ItemTypes.NegativeSet.Add(value);
                    else
                        ItemTypes.PositiveSet.Add(value);
                    break;

                case Field.Name:
                    if (negate)
                        ItemName.NegativeSet.Add(value);
                    else
                        ItemName.PositiveSet.Add(value);
                    break;

                case Field.PartName:
                    if (negate)
                        PartName.NegativeSet.Add(value);
                    else
                        PartName.PositiveSet.Add(value);
                    break;

                case Field.PartInterface:
                    if (negate)
                        PartInterface.NegativeSet.Add(value);
                    else
                        PartInterface.PositiveSet.Add(value);
                    break;

                case Field.AreaName:
                    if (negate)
                        AreaName.NegativeSet.Add(value);
                    else
                        AreaName.PositiveSet.Add(value);
                    break;

                case Field.AreaSize:
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
                        AreaSize.Neutral = areasize;
                    }

                    // Not Equal
                    else if (asOperation == null && negate)
                    {
                        AreaSize.Negative = areasize - 1;
                        AreaSize.Positive = areasize + 1;
                    }

                    // Greater Than or Equal
                    else if (asOperation == true && !negate)
                    {
                        AreaSize.Positive = areasize;
                    }

                    // Strictly Less Than
                    else if (asOperation == true && negate)
                    {
                        AreaSize.Negative = areasize - 1;
                    }

                    // Less Than or Equal
                    else if (asOperation == false && !negate)
                    {
                        AreaSize.Negative = areasize;
                    }

                    // Strictly Greater Than
                    else if (asOperation == false && negate)
                    {
                        AreaSize.Positive = areasize + 1;
                    }

                    break;

                case Field.Default:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Default.Neutral = false;
                    else
                        Default.Neutral = true;
                    break;

                case Field.BiosDescription:
                    if (negate)
                        Description.NegativeSet.Add(value);
                    else
                        Description.PositiveSet.Add(value);
                    break;

                case Field.Size:
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
                        Size.Neutral = size;
                    }

                    // Not Equal
                    else if (sOperation == null && negate)
                    {
                        Size.Negative = size - 1;
                        Size.Positive = size + 1;
                    }

                    // Greater Than or Equal
                    else if (sOperation == true && !negate)
                    {
                        Size.Positive = size;
                    }

                    // Strictly Less Than
                    else if (sOperation == true && negate)
                    {
                        Size.Negative = size - 1;
                    }

                    // Less Than or Equal
                    else if (sOperation == false && !negate)
                    {
                        Size.Negative = size;
                    }

                    // Strictly Greater Than
                    else if (sOperation == false && negate)
                    {
                        Size.Positive = size + 1;
                    }

                    break;

                case Field.CRC:
                    if (negate)
                        CRC.NegativeSet.Add(value);
                    else
                        CRC.PositiveSet.Add(value);
                    break;

                case Field.MD5:
                    if (negate)
                        MD5.NegativeSet.Add(value);
                    else
                        MD5.PositiveSet.Add(value);
                    break;

#if NET_FRAMEWORK
                case Field.RIPEMD160:
                    if (negate)
                        RIPEMD160.NegativeSet.Add(value);
                    else
                        RIPEMD160.PositiveSet.Add(value);
                    break;
#endif

                case Field.SHA1:
                    if (negate)
                        SHA1.NegativeSet.Add(value);
                    else
                        SHA1.PositiveSet.Add(value);
                    break;

                case Field.SHA256:
                    if (negate)
                        SHA256.NegativeSet.Add(value);
                    else
                        SHA256.PositiveSet.Add(value);
                    break;

                case Field.SHA384:
                    if (negate)
                        SHA384.NegativeSet.Add(value);
                    else
                        SHA384.PositiveSet.Add(value);
                    break;

                case Field.SHA512:
                    if (negate)
                        SHA512.NegativeSet.Add(value);
                    else
                        SHA512.PositiveSet.Add(value);
                    break;

                case Field.Merge:
                    if (negate)
                        MergeTag.NegativeSet.Add(value);
                    else
                        MergeTag.PositiveSet.Add(value);
                    break;

                case Field.Region:
                    if (negate)
                        Region.NegativeSet.Add(value);
                    else
                        Region.PositiveSet.Add(value);
                    break;

                case Field.Index:
                    if (negate)
                        Index.NegativeSet.Add(value);
                    else
                        Index.PositiveSet.Add(value);
                    break;

                case Field.Writable:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Writable.Neutral = false;
                    else
                        Writable.Neutral = true;
                    break;

                case Field.Optional:
                    if (negate || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        Optional.Neutral = false;
                    else
                        Optional.Neutral = true;
                    break;

                case Field.Status:
                    if (negate)
                        Status.Negative |= value.AsItemStatus();
                    else
                        Status.Positive |= value.AsItemStatus();
                    break;

                case Field.Language:
                    if (negate)
                        Language.NegativeSet.Add(value);
                    else
                        Language.PositiveSet.Add(value);
                    break;

                case Field.Date:
                    if (negate)
                        Date.NegativeSet.Add(value);
                    else
                        Date.PositiveSet.Add(value);
                    break;

                case Field.Bios:
                    if (negate)
                        Bios.NegativeSet.Add(value);
                    else
                        Bios.PositiveSet.Add(value);
                    break;

                case Field.Offset:
                    if (negate)
                        Offset.NegativeSet.Add(value);
                    else
                        Offset.PositiveSet.Add(value);
                    break;

                    #endregion
            }
        }

        #endregion

        /// <summary>
        /// Filter a DatFile using the inputs
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public bool FilterDatFile(DatFile datFile, bool useTags)
        {
            try
            {
                // Loop over every key in the dictionary
                List<string> keys = datFile.Keys;
                foreach (string key in keys)
                {
                    // For every item in the current key
                    List<DatItem> items = datFile[key];
                    List<DatItem> newitems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // If the rom passes the filter, include it
                        if (ItemPasses(item))
                        {
                            // If we're stripping unicode characters, do so from all relevant things
                            if (this.RemoveUnicode)
                            {
                                item.Name = Sanitizer.RemoveUnicodeCharacters(item.Name);
                                item.MachineName = Sanitizer.RemoveUnicodeCharacters(item.MachineName);
                                item.MachineDescription = Sanitizer.RemoveUnicodeCharacters(item.MachineDescription);
                            }

                            // If we're in cleaning mode, do so from all relevant things
                            if (this.Clean)
                            {
                                item.MachineName = Sanitizer.CleanGameName(item.MachineName);
                                item.MachineDescription = Sanitizer.CleanGameName(item.MachineDescription);
                            }

                            // If we are in single game mode, rename all games
                            if (this.Single)
                                item.MachineName = "!";

                            // If we are in NTFS trim mode, trim the game name
                            if (this.Trim)
                            {
                                // Windows max name length is 260
                                int usableLength = 260 - item.MachineName.Length - this.Root.Length;
                                if (item.Name.Length > usableLength)
                                {
                                    string ext = Path.GetExtension(item.Name);
                                    item.Name = item.Name.Substring(0, usableLength - ext.Length);
                                    item.Name += ext;
                                }
                            }

                            // Lock the list and add the item back
                            lock (newitems)
                            {
                                newitems.Add(item);
                            }
                        }
                    }

                    datFile.Remove(key);
                    datFile.AddRange(key, newitems);
                }

                // Process description to machine name
                if (this.DescriptionAsName)
                    MachineDescriptionToName(datFile);

                // If we are using tags from the DAT, set the proper input for split type unless overridden
                if (useTags && this.InternalSplit == SplitType.None)
                    this.InternalSplit = datFile.DatHeader.ForceMerging.AsSplitType();

                // Run internal splitting
                ProcessSplitType(datFile, this.InternalSplit);

                // We remove any blanks, if we aren't supposed to have any
                if (!datFile.DatHeader.KeepEmptyGames)
                {
                    foreach (string key in datFile.Keys)
                    {
                        List<DatItem> items = datFile[key];
                        List<DatItem> newitems = items.Where(i => i.ItemType != ItemType.Blank).ToList();

                        datFile.Remove(key);
                        datFile.AddRange(key, newitems);
                    }
                }

                // If we are removing scene dates, do that now
                if (datFile.DatHeader.SceneDateStrip)
                    StripSceneDatesFromItems(datFile);

                // Run the one rom per game logic, if required
                if (datFile.DatHeader.OneRom)
                    OneRomPerGame(datFile);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Filter a DatFile outputting to a new DatFile
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public DatFile FilterTo(DatFile datFile, bool useTags)
        {
            DatFile outDat = DatFile.Create(datFile.DatHeader);

            try
            {
                // Loop over every key in the dictionary
                List<string> keys = datFile.Keys;
                foreach (string key in keys)
                {
                    // For every item in the current key
                    List<DatItem> items = datFile[key];
                    List<DatItem> newitems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // If the rom passes the filter, include it
                        if (ItemPasses(item))
                        {
                            // If we're stripping unicode characters, do so from all relevant things
                            if (this.RemoveUnicode)
                            {
                                item.Name = Sanitizer.RemoveUnicodeCharacters(item.Name);
                                item.MachineName = Sanitizer.RemoveUnicodeCharacters(item.MachineName);
                                item.MachineDescription = Sanitizer.RemoveUnicodeCharacters(item.MachineDescription);
                            }

                            // If we're in cleaning mode, do so from all relevant things
                            if (this.Clean)
                            {
                                item.MachineName = Sanitizer.CleanGameName(item.MachineName);
                                item.MachineDescription = Sanitizer.CleanGameName(item.MachineDescription);
                            }

                            // If we are in single game mode, rename all games
                            if (this.Single)
                                item.MachineName = "!";

                            // If we are in NTFS trim mode, trim the game name
                            if (this.Trim)
                            {
                                // Windows max name length is 260
                                int usableLength = 260 - item.MachineName.Length - this.Root.Length;
                                if (item.Name.Length > usableLength)
                                {
                                    string ext = Path.GetExtension(item.Name);
                                    item.Name = item.Name.Substring(0, usableLength - ext.Length);
                                    item.Name += ext;
                                }
                            }

                            // Lock the list and add the item back
                            lock (newitems)
                            {
                                newitems.Add(item);
                            }
                        }
                    }

                    outDat.AddRange(key, newitems);
                }

                // Process description to machine name
                if (this.DescriptionAsName)
                    MachineDescriptionToName(outDat);

                // If we are using tags from the DAT, set the proper input for split type unless overridden
                if (useTags && this.InternalSplit == SplitType.None)
                    this.InternalSplit = outDat.DatHeader.ForceMerging.AsSplitType();

                // Run internal splitting
                ProcessSplitType(outDat, this.InternalSplit);

                // We remove any blanks, if we aren't supposed to have any
                if (!outDat.DatHeader.KeepEmptyGames)
                {
                    foreach (string key in outDat.Keys)
                    {
                        List<DatItem> items = outDat[key];
                        List<DatItem> newitems = items.Where(i => i.ItemType != ItemType.Blank).ToList();

                        outDat.Remove(key);
                        outDat.AddRange(key, newitems);
                    }
                }

                // If we are removing scene dates, do that now
                if (outDat.DatHeader.SceneDateStrip)
                    StripSceneDatesFromItems(outDat);

                // Run the one rom per game logic, if required
                if (outDat.DatHeader.OneRom)
                    OneRomPerGame(outDat);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return null;
            }

            return outDat;
        }

        /// <summary>
        /// Check to see if a DatItem passes the filter
        /// </summary>
        /// <param name="item">DatItem to check</param>
        /// <returns>True if the file passed the filter, false otherwise</returns>
        public bool ItemPasses(DatItem item)
        {
            // If the item is null, we automatically fail it
            if (item == null)
                return false;

            #region Machine Filters

            // Filter on machine name
            bool? machineNameFound = this.MachineName.MatchesPositiveSet(item.MachineName);
            if (this.IncludeOfInGame)
            {
                machineNameFound |= (this.MachineName.MatchesPositiveSet(item.CloneOf) == true);
                machineNameFound |= (this.MachineName.MatchesPositiveSet(item.RomOf) == true);
            }
            if (machineNameFound == false)
                return false;

            machineNameFound = this.MachineName.MatchesNegativeSet(item.MachineName);
            if (this.IncludeOfInGame)
            {
                machineNameFound |= (this.MachineName.MatchesNegativeSet(item.CloneOf) == true);
                machineNameFound |= (this.MachineName.MatchesNegativeSet(item.RomOf) == true);
            }
            if (machineNameFound == false)
                return false;

            // Filter on comment
            if (this.Comment.MatchesPositiveSet(item.Comment) == false)
                return false;
            if (this.Comment.MatchesNegativeSet(item.Comment) == true)
                return false;

            // Filter on machine description
            if (this.MachineDescription.MatchesPositiveSet(item.MachineDescription) == false)
                return false;
            if (this.MachineDescription.MatchesNegativeSet(item.MachineDescription) == true)
                return false;

            // Filter on year
            if (this.Year.MatchesPositiveSet(item.Year) == false)
                return false;
            if (this.Year.MatchesNegativeSet(item.Year) == true)
                return false;

            // Filter on manufacturer
            if (this.Manufacturer.MatchesPositiveSet(item.Manufacturer) == false)
                return false;
            if (this.Manufacturer.MatchesNegativeSet(item.Manufacturer) == true)
                return false;

            // Filter on publisher
            if (this.Publisher.MatchesPositiveSet(item.Publisher) == false)
                return false;
            if (this.Publisher.MatchesNegativeSet(item.Publisher) == true)
                return false;

            // Filter on category
            if (this.Category.MatchesPositiveSet(item.Category) == false)
                return false;
            if (this.Category.MatchesNegativeSet(item.Category) == true)
                return false;

            // Filter on romof
            if (this.RomOf.MatchesPositiveSet(item.RomOf) == false)
                return false;
            if (this.RomOf.MatchesNegativeSet(item.RomOf) == true)
                return false;

            // Filter on cloneof
            if (this.CloneOf.MatchesPositiveSet(item.CloneOf) == false)
                return false;
            if (this.CloneOf.MatchesNegativeSet(item.CloneOf) == true)
                return false;

            // Filter on sampleof
            if (this.SampleOf.MatchesPositiveSet(item.SampleOf) == false)
                return false;
            if (this.SampleOf.MatchesNegativeSet(item.SampleOf) == true)
                return false;

            // Filter on supported
            if (this.Supported.MatchesNeutral(null, item.Supported) == false)
                return false;

            // Filter on source file
            if (this.SourceFile.MatchesPositiveSet(item.SourceFile) == false)
                return false;
            if (this.SourceFile.MatchesNegativeSet(item.SourceFile) == true)
                return false;

            // Filter on runnable
            if (this.Runnable.MatchesNeutral(null, item.Runnable) == false)
                return false;

            // Filter on board
            if (this.Board.MatchesPositiveSet(item.Board) == false)
                return false;
            if (this.Board.MatchesNegativeSet(item.Board) == true)
                return false;

            // Filter on rebuildto
            if (this.RebuildTo.MatchesPositiveSet(item.RebuildTo) == false)
                return false;
            if (this.RebuildTo.MatchesNegativeSet(item.RebuildTo) == true)
                return false;

            // Filter on machine type
            if (this.MachineTypes.MatchesPositive(MachineType.NULL, item.MachineType) == false)
                return false;
            if (this.MachineTypes.MatchesNegative(MachineType.NULL, item.MachineType) == true)
                return false;

            #endregion

            #region DatItem Filters

            // Filter on item type
            // TODO: Remove default filtering at some point
            if (this.ItemTypes.PositiveSet.Count == 0 && this.ItemTypes.NegativeSet.Count == 0
                && item.ItemType != ItemType.Rom && item.ItemType != ItemType.Disk && item.ItemType != ItemType.Blank)
                return false;
            if (this.ItemTypes.MatchesPositiveSet(item.ItemType.ToString()) == false)
                return false;
            if (this.ItemTypes.MatchesNegativeSet(item.ItemType.ToString()) == true)
                return false;

            // Filter on item name
            if (this.ItemName.MatchesPositiveSet(item.Name) == false)
                return false;
            if (this.ItemName.MatchesNegativeSet(item.Name) == true)
                return false;

            // Filter on part name
            if (this.PartName.MatchesPositiveSet(item.PartName) == false)
                return false;
            if (this.PartName.MatchesNegativeSet(item.PartName) == true)
                return false;

            // Filter on part interface
            if (this.PartInterface.MatchesPositiveSet(item.PartInterface) == false)
                return false;
            if (this.PartInterface.MatchesNegativeSet(item.PartInterface) == true)
                return false;

            // Filter on area name
            if (this.AreaName.MatchesPositiveSet(item.AreaName) == false)
                return false;
            if (this.AreaName.MatchesNegativeSet(item.AreaName) == true)
                return false;

            // Filter on area size
            if (this.AreaSize.MatchesNeutral(null, item.AreaSize) == false)
                return false;
            else if (this.AreaSize.MatchesPositive(null, item.AreaSize) == false)
                return false;
            else if (this.AreaSize.MatchesNegative(null, item.AreaSize) == false)
                return false;

            // Take care of item-specific differences
            switch (item.ItemType)
            {
                case ItemType.Archive:
                    // Archive has no special fields
                    break;

                case ItemType.BiosSet:
                    BiosSet biosSet = (BiosSet)item;

                    // Filter on description
                    if (this.Description.MatchesNeutral(null, biosSet.Description) == false)
                        return false;

                    // Filter on default
                    if (this.Default.MatchesNeutral(null, biosSet.Default) == false)
                        return false;

                    break;

                case ItemType.Blank:
                    // Blank has no special fields
                    break;

                case ItemType.Disk:
                    Disk disk = (Disk)item;

                    // Filter on MD5
                    if (this.MD5.MatchesPositiveSet(disk.MD5) == false)
                        return false;
                    if (this.MD5.MatchesNegativeSet(disk.MD5) == true)
                        return false;

#if NET_FRAMEWORK
                    // Filter on RIPEMD160
                    if (this.RIPEMD160.MatchesPositiveSet(disk.RIPEMD160) == false)
                        return false;
                    if (this.RIPEMD160.MatchesNegativeSet(disk.RIPEMD160) == true)
                        return false;
#endif

                    // Filter on SHA-1
                    if (this.SHA1.MatchesPositiveSet(disk.SHA1) == false)
                        return false;
                    if (this.SHA1.MatchesNegativeSet(disk.SHA1) == true)
                        return false;

                    // Filter on SHA-256
                    if (this.SHA256.MatchesPositiveSet(disk.SHA256) == false)
                        return false;
                    if (this.SHA256.MatchesNegativeSet(disk.SHA256) == true)
                        return false;

                    // Filter on SHA-384
                    if (this.SHA384.MatchesPositiveSet(disk.SHA384) == false)
                        return false;
                    if (this.SHA384.MatchesNegativeSet(disk.SHA384) == true)
                        return false;

                    // Filter on SHA-512
                    if (this.SHA512.MatchesPositiveSet(disk.SHA512) == false)
                        return false;
                    if (this.SHA512.MatchesNegativeSet(disk.SHA512) == true)
                        return false;

                    // Filter on merge tag
                    if (this.MergeTag.MatchesPositiveSet(disk.MergeTag) == false)
                        return false;
                    if (this.MergeTag.MatchesNegativeSet(disk.MergeTag) == true)
                        return false;

                    // Filter on region
                    if (this.Region.MatchesPositiveSet(disk.Region) == false)
                        return false;
                    if (this.Region.MatchesNegativeSet(disk.Region) == true)
                        return false;

                    // Filter on index
                    if (this.Index.MatchesPositiveSet(disk.Index) == false)
                        return false;
                    if (this.Index.MatchesNegativeSet(disk.Index) == true)
                        return false;

                    // Filter on writable
                    if (this.Writable.MatchesNeutral(null, disk.Writable) == false)
                        return false;

                    // Filter on status
                    if (this.Status.MatchesPositive(ItemStatus.NULL, disk.ItemStatus) == false)
                        return false;
                    if (this.Status.MatchesNegative(ItemStatus.NULL, disk.ItemStatus) == true)
                        return false;

                    // Filter on optional
                    if (this.Optional.MatchesNeutral(null, disk.Optional) == false)
                        return false;

                    break;

                case ItemType.Release:
                    Release release = (Release)item;

                    // Filter on region
                    if (this.Region.MatchesPositiveSet(release.Region) == false)
                        return false;
                    if (this.Region.MatchesNegativeSet(release.Region) == true)
                        return false;

                    // Filter on language
                    if (this.Language.MatchesPositiveSet(release.Language) == false)
                        return false;
                    if (this.Language.MatchesNegativeSet(release.Language) == true)
                        return false;

                    // Filter on date
                    if (this.Date.MatchesPositiveSet(release.Date) == false)
                        return false;
                    if (this.Date.MatchesNegativeSet(release.Date) == true)
                        return false;

                    // Filter on default
                    if (this.Default.MatchesNeutral(null, release.Default) == false)
                        return false;

                    break;

                case ItemType.Rom:
                    Rom rom = (Rom)item;

                    // Filter on bios
                    if (this.Bios.MatchesPositiveSet(rom.Bios) == false)
                        return false;
                    if (this.Bios.MatchesNegativeSet(rom.Bios) == true)
                        return false;

                    // Filter on rom size
                    if (this.Size.MatchesNeutral(-1, rom.Size) == false)
                        return false;
                    else if (this.Size.MatchesPositive(-1, rom.Size) == false)
                        return false;
                    else if (this.Size.MatchesNegative(-1, rom.Size) == false)
                        return false;

                    // Filter on CRC
                    if (this.CRC.MatchesPositiveSet(rom.CRC) == false)
                        return false;
                    if (this.CRC.MatchesNegativeSet(rom.CRC) == true)
                        return false;

                    // Filter on MD5
                    if (this.MD5.MatchesPositiveSet(rom.MD5) == false)
                        return false;
                    if (this.MD5.MatchesNegativeSet(rom.MD5) == true)
                        return false;

#if NET_FRAMEWORK
                    // Filter on RIPEMD160
                    if (this.RIPEMD160.MatchesPositiveSet(rom.RIPEMD160) == false)
                        return false;
                    if (this.RIPEMD160.MatchesNegativeSet(rom.RIPEMD160) == true)
                        return false;
#endif

                    // Filter on SHA-1
                    if (this.SHA1.MatchesPositiveSet(rom.SHA1) == false)
                        return false;
                    if (this.SHA1.MatchesNegativeSet(rom.SHA1) == true)
                        return false;

                    // Filter on SHA-256
                    if (this.SHA256.MatchesPositiveSet(rom.SHA256) == false)
                        return false;
                    if (this.SHA256.MatchesNegativeSet(rom.SHA256) == true)
                        return false;

                    // Filter on SHA-384
                    if (this.SHA384.MatchesPositiveSet(rom.SHA384) == false)
                        return false;
                    if (this.SHA384.MatchesNegativeSet(rom.SHA384) == true)
                        return false;

                    // Filter on SHA-512
                    if (this.SHA512.MatchesPositiveSet(rom.SHA512) == false)
                        return false;
                    if (this.SHA512.MatchesNegativeSet(rom.SHA512) == true)
                        return false;

                    // Filter on merge tag
                    if (this.MergeTag.MatchesPositiveSet(rom.MergeTag) == false)
                        return false;
                    if (this.MergeTag.MatchesNegativeSet(rom.MergeTag) == true)
                        return false;

                    // Filter on region
                    if (this.Region.MatchesPositiveSet(rom.Region) == false)
                        return false;
                    if (this.Region.MatchesNegativeSet(rom.Region) == true)
                        return false;

                    // Filter on offset
                    if (this.Offset.MatchesPositiveSet(rom.Offset) == false)
                        return false;
                    if (this.Offset.MatchesNegativeSet(rom.Offset) == true)
                        return false;

                    // Filter on date
                    if (this.Date.MatchesPositiveSet(rom.Date) == false)
                        return false;
                    if (this.Date.MatchesNegativeSet(rom.Date) == true)
                        return false;

                    // Filter on status
                    if (this.Status.MatchesPositive(ItemStatus.NULL, rom.ItemStatus) == false)
                        return false;
                    if (this.Status.MatchesNegative(ItemStatus.NULL, rom.ItemStatus) == true)
                        return false;

                    // Filter on optional
                    if (this.Optional.MatchesNeutral(null, rom.Optional) == false)
                        return false;

                    break;

                case ItemType.Sample:
                    // Sample has no special fields
                    break;
            }

            #endregion

            return true;
        }

        #region Internal Splitting/Merging

        /// <summary>
        /// Process items according to SplitType
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="splitType">SplitType to implement</param>
        private void ProcessSplitType(DatFile datFile, SplitType splitType)
        {
            // Now we pre-process the DAT with the splitting/merging mode
            switch (splitType)
            {
                case SplitType.None:
                    // No-op
                    break;
                case SplitType.DeviceNonMerged:
                    CreateDeviceNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.FullNonMerged:
                    CreateFullyNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.NonMerged:
                    CreateNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.Merged:
                    CreateMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.Split:
                    CreateSplitSets(datFile, DedupeType.None);
                    break;
            }
        }

        /// <summary>
        /// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateDeviceNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating device non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(datFile, false, false)) ;
            while (AddRomsFromDevices(datFile, true, false)) ;

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateFullyNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating fully non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(datFile, true, true)) ;
            AddRomsFromDevices(datFile, false, true);
            AddRomsFromParent(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            AddRomsFromBios(datFile);

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromChildren(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromParent(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof and romof tags to create split sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateSplitSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating split sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            RemoveRomsFromChild(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use romof tags to add roms to the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromBios(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].RomOf))
                    parent = datFile[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = datFile[game][0];
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (datFile[game].Where(i => i.Name == datItem.Name).Count() == 0 && !datFile[game].Contains(datItem))
                        datFile.Add(game, datItem);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add roms to the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="dev">True if only child device sets are touched, false for non-device sets (default)</param>
        /// <param name="slotoptions">True if slotoptions tags are used as well, false otherwise</param>
        private bool AddRomsFromDevices(DatFile datFile, bool dev = false, bool slotoptions = false)
        {
            bool foundnew = false;
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game doesn't have items, we continue
                if (datFile[game] == null || datFile[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (dev ^ (datFile[game][0].MachineType.HasFlag(Data.MachineType.Device)))
                    continue;

                // If the game has no devices, we continue
                if (datFile[game][0].Devices == null
                    || datFile[game][0].Devices.Count == 0
                    || (slotoptions && datFile[game][0].SlotOptions == null)
                    || (slotoptions && datFile[game][0].SlotOptions.Count == 0))
                {
                    continue;
                }

                // Determine if the game has any devices or not
                List<string> devices = datFile[game][0].Devices;
                List<string> newdevs = new List<string>();
                foreach (string device in devices)
                {
                    // If the device doesn't exist then we continue
                    if (datFile[device].Count == 0)
                        continue;

                    // Otherwise, copy the items from the device to the current game
                    DatItem copyFrom = datFile[game][0];
                    List<DatItem> devItems = datFile[device];
                    foreach (DatItem item in devItems)
                    {
                        DatItem datItem = (DatItem)item.Clone();
                        newdevs.AddRange(datItem.Devices ?? new List<string>());
                        datItem.CopyMachineInformation(copyFrom);
                        if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                        {
                            foundnew = true;
                            datFile.Add(game, datItem);
                        }
                    }
                }

                // Now that every device is accounted for, add the new list of devices, if they don't already exist
                foreach (string device in newdevs)
                {
                    if (!datFile[game][0].Devices.Contains(device))
                        datFile[game][0].Devices.Add(device);
                }

                // If we're checking slotoptions too
                if (slotoptions)
                {
                    // Determine if the game has any slotoptions or not
                    List<string> slotopts = datFile[game][0].SlotOptions;
                    List<string> newslotopts = new List<string>();
                    foreach (string slotopt in slotopts)
                    {
                        // If the slotoption doesn't exist then we continue
                        if (datFile[slotopt].Count == 0)
                            continue;

                        // Otherwise, copy the items from the slotoption to the current game
                        DatItem copyFrom = datFile[game][0];
                        List<DatItem> slotItems = datFile[slotopt];
                        foreach (DatItem item in slotItems)
                        {
                            DatItem datItem = (DatItem)item.Clone();
                            newslotopts.AddRange(datItem.SlotOptions ?? new List<string>());
                            datItem.CopyMachineInformation(copyFrom);
                            if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                            {
                                foundnew = true;
                                datFile.Add(game, datItem);
                            }
                        }
                    }

                    // Now that every slotoption is accounted for, add the new list of slotoptions, if they don't already exist
                    foreach (string slotopt in newslotopts)
                    {
                        if (!datFile[game][0].SlotOptions.Contains(slotopt))
                            datFile[game][0].SlotOptions.Add(slotopt);
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use cloneof tags to add roms to the children, setting the new romof tag in the process
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromParent(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = datFile[game][0];
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0
                        && !datFile[game].Contains(datItem))
                    {
                        datFile.Add(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                List<DatItem> items = datFile[game];
                string romof = datFile[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add roms to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromChildren(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If there is no parent, then we continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom = datFile[parent].Count == 0 ? new Rom { MachineName = parent, MachineDescription = parent } : datFile[parent][0];
                List<DatItem> items = datFile[game];
                foreach (DatItem item in items)
                {
                    // If the disk doesn't have a valid merge tag OR the merged file doesn't exist in the parent, then add it
                    if (item.ItemType == Data.ItemType.Disk && (((Disk)item).MergeTag == null || !datFile[parent].Select(i => i.Name).Contains(((Disk)item).MergeTag)))
                    {
                        item.CopyMachineInformation(copyFrom);
                        datFile.Add(parent, item);
                    }

                    // Otherwise, if the parent doesn't already contain the non-disk (or a merge-equivalent), add it
                    else if (item.ItemType != Data.ItemType.Disk && !datFile[parent].Contains(item))
                    {
                        // Rename the child so it's in a subfolder
                        item.Name = $"{item.MachineName}\\{item.Name}";

                        // Update the machine to be the new parent
                        item.CopyMachineInformation(copyFrom);

                        // Add the rom to the parent set
                        datFile.Add(parent, item);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                datFile.Remove(game);
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveBiosAndDeviceSets(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                if (datFile[game].Count > 0
                    && (datFile[game][0].MachineType.HasFlag(Data.MachineType.Bios)
                        || datFile[game][0].MachineType.HasFlag(Data.MachineType.Device)))
                {
                    datFile.Remove(game);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios roms from children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets (default)</param>
        private void RemoveBiosRomsFromChild(DatFile datFile, bool bios = false)
        {
            // Loop through the romof tags
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (bios ^ datFile[game][0].MachineType.HasFlag(Data.MachineType.Bios))
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].RomOf))
                    parent = datFile[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (datFile[game].Contains(datItem))
                    {
                        datFile.Remove(game, datItem);
                    }
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove roms from the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveRomsFromChild(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (datFile[game].Contains(datItem))
                    {
                        datFile.Remove(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                List<DatItem> items = datFile[game];
                string romof = datFile[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all games
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveTagsFromChild(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                List<DatItem> items = datFile[game];
                foreach (DatItem item in items)
                {
                    item.CloneOf = null;
                    item.RomOf = null;
                }
            }
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void MachineDescriptionToName(DatFile datFile)
        {
            try
            {
                // First we want to get a mapping for all games to description
                ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
                List<string> keys = datFile.Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = datFile[key];
                    foreach (DatItem item in items)
                    {
                        // If the key mapping doesn't exist, add it
                        mapping.TryAdd(item.MachineName, item.MachineDescription.Replace('/', '_').Replace("\"", "''").Replace(":", " -"));
                    }
                });

                // Now we loop through every item and update accordingly
                keys = datFile.Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = datFile[key];
                    List<DatItem> newItems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // Update machine name
                        if (!string.IsNullOrWhiteSpace(item.MachineName) && mapping.ContainsKey(item.MachineName))
                            item.MachineName = mapping[item.MachineName];

                        // Update cloneof
                        if (!string.IsNullOrWhiteSpace(item.CloneOf) && mapping.ContainsKey(item.CloneOf))
                            item.CloneOf = mapping[item.CloneOf];

                        // Update romof
                        if (!string.IsNullOrWhiteSpace(item.RomOf) && mapping.ContainsKey(item.RomOf))
                            item.RomOf = mapping[item.RomOf];

                        // Update sampleof
                        if (!string.IsNullOrWhiteSpace(item.SampleOf) && mapping.ContainsKey(item.SampleOf))
                            item.SampleOf = mapping[item.SampleOf];

                        // Add the new item to the output list
                        newItems.Add(item);
                    }

                    // Replace the old list of roms with the new one
                    datFile.Remove(key);
                    datFile.AddRange(key, newItems);
                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning(ex.ToString());
            }
        }

        /// <summary>
        /// Ensure that all roms are in their own game (or at least try to ensure)
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// TODO: This is incorrect for the actual 1G1R logic... this is actually just silly
        private void OneRomPerGame(DatFile datFile)
        {
            // For each rom, we want to update the game to be "<game name>/<rom name>"
            Parallel.ForEach(datFile.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = datFile[key];
                for (int i = 0; i < items.Count; i++)
                {
                    string[] splitname = items[i].Name.Split('.');
                    items[i].MachineName += $"/{string.Join(".", splitname.Take(splitname.Length > 1 ? splitname.Length - 1 : 1))}";
                }
            });
        }

        /// <summary>
        /// Strip the dates from the beginning of scene-style set names
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void StripSceneDatesFromItems(DatFile datFile)
        {
            // Output the logging statement
            Globals.Logger.User("Stripping scene-style dates");

            // Set the regex pattern to use
            string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

            // Now process all of the roms
            List<string> keys = datFile.Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = datFile[key];
                for (int j = 0; j < items.Count; j++)
                {
                    DatItem item = items[j];
                    if (Regex.IsMatch(item.MachineName, pattern))
                        item.MachineName = Regex.Replace(item.MachineName, pattern, "$2");

                    if (Regex.IsMatch(item.MachineDescription, pattern))
                        item.MachineDescription = Regex.Replace(item.MachineDescription, pattern, "$2");

                    items[j] = item;
                }

                datFile.Remove(key);
                datFile.AddRange(key, items);
            });
        }

        #endregion

        #endregion // Instance Methods
    }
}
