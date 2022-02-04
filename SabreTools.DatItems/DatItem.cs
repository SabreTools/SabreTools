using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.Logging;
using NaturalSort;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Base class for all items included in a set
    /// </summary>
    [JsonObject("datitem"), XmlRoot("datitem")]
    [XmlInclude(typeof(Adjuster))]
    [XmlInclude(typeof(Analog))]
    [XmlInclude(typeof(Archive))]
    [XmlInclude(typeof(BiosSet))]
    [XmlInclude(typeof(Blank))]
    [XmlInclude(typeof(Chip))]
    [XmlInclude(typeof(Condition))]
    [XmlInclude(typeof(Configuration))]
    [XmlInclude(typeof(Control))]
    [XmlInclude(typeof(DataArea))]
    [XmlInclude(typeof(Device))]
    [XmlInclude(typeof(DeviceReference))]
    [XmlInclude(typeof(DipSwitch))]
    [XmlInclude(typeof(Disk))]
    [XmlInclude(typeof(DiskArea))]
    [XmlInclude(typeof(Display))]
    [XmlInclude(typeof(Driver))]
    [XmlInclude(typeof(Extension))]
    [XmlInclude(typeof(Feature))]
    [XmlInclude(typeof(Info))]
    [XmlInclude(typeof(Input))]
    [XmlInclude(typeof(Instance))]
    [XmlInclude(typeof(Location))]
    [XmlInclude(typeof(Media))]
    [XmlInclude(typeof(Part))]
    [XmlInclude(typeof(PartFeature))]
    [XmlInclude(typeof(Port))]
    [XmlInclude(typeof(RamOption))]
    [XmlInclude(typeof(Release))]
    [XmlInclude(typeof(Rom))]
    [XmlInclude(typeof(Sample))]
    [XmlInclude(typeof(Setting))]
    [XmlInclude(typeof(SharedFeature))]
    [XmlInclude(typeof(Slot))]
    [XmlInclude(typeof(SlotOption))]
    [XmlInclude(typeof(SoftwareList))]
    [XmlInclude(typeof(Sound))]
    public abstract class DatItem : IEquatable<DatItem>, IComparable<DatItem>, ICloneable
    {
        #region Fields

        #region Common Fields

        /// <summary>
        /// Item type for outputting
        /// </summary>
        [JsonProperty("itemtype")]
        [JsonConverter(typeof(StringEnumConverter))]
        [XmlElement("itemtype")]
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Duplicate type when compared to another item
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public DupeType DupeType { get; set; }

        #endregion

        #region Machine Fields

        /// <summary>
        /// Machine values
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public Machine Machine { get; set; } = new Machine();

        #endregion

        #region Metadata information

        /// <summary>
        /// Source information
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public Source Source { get; set; } = new Source();

        /// <summary>
        /// Flag if item should be removed
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public bool Remove { get; set; }

        #endregion // Metadata information

        #endregion
        
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        [JsonIgnore, XmlIgnore]
        protected Logger logger;

        /// <summary>
        /// Static logger for static methods
        /// </summary>
        [JsonIgnore, XmlIgnore]
        protected static Logger staticLogger = new Logger();

        #endregion

        #region Instance Methods

        #region Accessors

        /// <summary>
        /// Gets the name to use for a DatItem
        /// </summary>
        /// <returns>Name if available, null otherwise</returns>
        public virtual string GetName() => null;

        /// <summary>
        /// Sets the name to use for a DatItem
        /// </summary>
        /// <param name="name">Name to set for the item</param>
        public virtual void SetName(string name) { }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DatItem()
        {
            logger = new Logger(this);
        }

        /// <summary>
        /// Create a specific type of DatItem to be used based on an ItemType
        /// </summary>
        /// <param name="itemType">Type of the DatItem to be created</param>
        /// <returns>DatItem of the specific internal type that corresponds to the inputs</returns>
        public static DatItem Create(ItemType? itemType)
        {
            return itemType switch
            {
                ItemType.Adjuster => new Adjuster(),
                ItemType.Analog => new Analog(),
                ItemType.Archive => new Archive(),
                ItemType.BiosSet => new BiosSet(),
                ItemType.Blank => new Blank(),
                ItemType.Chip => new Chip(),
                ItemType.Condition => new Condition(),
                ItemType.Configuration => new Configuration(),
                ItemType.Device => new Device(),
                ItemType.DeviceReference => new DeviceReference(),
                ItemType.DipSwitch => new DipSwitch(),
                ItemType.Disk => new Disk(),
                ItemType.Display => new Display(),
                ItemType.Driver => new Driver(),
                ItemType.Extension => new Extension(),
                ItemType.Feature => new Feature(),
                ItemType.Info => new Info(),
                ItemType.Instance => new Instance(),
                ItemType.Location => new Location(),
                ItemType.Media => new Media(),
                ItemType.PartFeature => new PartFeature(),
                ItemType.Port => new Port(),
                ItemType.RamOption => new RamOption(),
                ItemType.Release => new Release(),
                ItemType.Rom => new Rom(),
                ItemType.Sample => new Sample(),
                ItemType.SharedFeature => new SharedFeature(),
                ItemType.Slot => new Slot(),
                ItemType.SlotOption => new SlotOption(),
                ItemType.SoftwareList => new SoftwareList(),
                ItemType.Sound => new Sound(),
                _ => new Rom(),
            };
        }

        /// <summary>
        /// Create a specific type of DatItem to be used based on a BaseFile
        /// </summary>
        /// <param name="baseFile">BaseFile containing information to be created</param>
        /// <returns>DatItem of the specific internal type that corresponds to the inputs</returns>
        public static DatItem Create(BaseFile baseFile)
        {
            switch (baseFile.Type)
            {
                case FileType.AaruFormat:
                    return new Media(baseFile);

                case FileType.CHD:
                case FileType.ISOArchive:
                    return new Disk(baseFile);

                case FileType.GZipArchive:
                case FileType.LRZipArchive:
                case FileType.LZ4Archive:
                case FileType.None:
                case FileType.RarArchive:
                case FileType.SevenZipArchive:
                case FileType.TapeArchive:
                case FileType.XZArchive:
                case FileType.ZipArchive:
                case FileType.ZPAQArchive:
                case FileType.ZstdArchive:
                    return new Rom(baseFile);

                case FileType.Folder:
                default:
                    return null;
            }
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Clone the DatItem
        /// </summary>
        /// <returns>Clone of the DatItem</returns>
        public abstract object Clone();

        /// <summary>
        /// Copy all machine information over in one shot
        /// </summary>
        /// <param name="item">Existing item to copy information from</param>
        public void CopyMachineInformation(DatItem item)
        {
            Machine = (Machine)item.Machine.Clone();
        }

        /// <summary>
        /// Copy all machine information over in one shot
        /// </summary>
        /// <param name="machine">Existing machine to copy information from</param>
        public void CopyMachineInformation(Machine machine)
        {
            Machine = (Machine)machine.Clone();
        }

        #endregion

        #region Comparision Methods

        public int CompareTo(DatItem other)
        {
            try
            {
                if (GetName() == other.GetName())
                    return Equals(other) ? 0 : 1;

                return string.Compare(GetName(), other.GetName());
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Determine if an item is a duplicate using partial matching logic
        /// </summary>
        /// <param name="other">DatItem to use as a baseline</param>
        /// <returns>True if the roms are duplicates, false otherwise</returns>
        public abstract bool Equals(DatItem other);

        /// <summary>
        /// Return the duplicate status of two items
        /// </summary>
        /// <param name="lastItem">DatItem to check against</param>
        /// <returns>The DupeType corresponding to the relationship between the two</returns>
        public DupeType GetDuplicateStatus(DatItem lastItem)
        {
            DupeType output = 0x00;

            // If we don't have a duplicate at all, return none
            if (!Equals(lastItem))
                return output;

            // If the duplicate is external already or should be, set it
            if (lastItem.DupeType.HasFlag(DupeType.External) || lastItem.Source.Index != Source.Index)
            {
                if (lastItem.Machine.Name == Machine.Name && lastItem.GetName() == GetName())
                    output = DupeType.External | DupeType.All;
                else
                    output = DupeType.External | DupeType.Hash;
            }

            // Otherwise, it's considered an internal dupe
            else
            {
                if (lastItem.Machine.Name == Machine.Name && lastItem.GetName() == GetName())
                    output = DupeType.Internal | DupeType.All;
                else
                    output = DupeType.Internal | DupeType.Hash;
            }

            return output;
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Clean a CRC32 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanCRC32(string hash)
        {
            return CleanHashData(hash, Constants.CRCLength);
        }

        /// <summary>
        /// Clean a MD5 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanMD5(string hash)
        {
            return CleanHashData(hash, Constants.MD5Length);
        }

        /// <summary>
        /// Clean a SHA1 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanSHA1(string hash)
        {
            return CleanHashData(hash, Constants.SHA1Length);
        }

        /// <summary>
        /// Clean a SHA256 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanSHA256(string hash)
        {
            return CleanHashData(hash, Constants.SHA256Length);
        }

        /// <summary>
        /// Clean a SHA384 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanSHA384(string hash)
        {
            return CleanHashData(hash, Constants.SHA384Length);
        }

        /// <summary>
        /// Clean a SHA512 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        protected string CleanSHA512(string hash)
        {
            return CleanHashData(hash, Constants.SHA512Length);
        }

        /// <summary>
        /// Clean a hash string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <param name="padding">Amount of characters to pad to</param>
        /// <returns>Cleaned string</returns>
        private string CleanHashData(string hash, int padding)
        {
            // If we have a known blank hash, return blank
            if (string.IsNullOrWhiteSpace(hash) || hash == "-" || hash == "_")
                return string.Empty;

            // Check to see if it's a "hex" hash
            hash = hash.Trim().Replace("0x", string.Empty);

            // If we have a blank hash now, return blank
            if (string.IsNullOrWhiteSpace(hash))
                return string.Empty;

            // If the hash shorter than the required length, pad it
            if (hash.Length < padding)
                hash = hash.PadLeft(padding, '0');

            // If the hash is longer than the required length, it's invalid
            else if (hash.Length > padding)
                return string.Empty;

            // Now normalize the hash
            hash = hash.ToLowerInvariant();

            // Otherwise, make sure that every character is a proper match
            for (int i = 0; i < hash.Length; i++)
            {
                if ((hash[i] < '0' || hash[i] > '9') && (hash[i] < 'a' || hash[i] > 'f'))
                {
                    hash = string.Empty;
                    break;
                }
            }

            return hash;
        }

        #endregion

        #region Sorting and Merging

        /// <summary>
        /// Get the dictionary key that should be used for a given item and bucketing type
        /// </summary>
        /// <param name="bucketedBy">ItemKey value representing what key to get</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        /// <returns>String representing the key to be used for the DatItem</returns>
        public virtual string GetKey(ItemKey bucketedBy, bool lower = true, bool norename = true)
        {
            // Set the output key as the default blank string
            string key = string.Empty;

            // Now determine what the key should be based on the bucketedBy value
            switch (bucketedBy)
            {
                case ItemKey.CRC:
                    key = Constants.CRCZero;
                    break;

                case ItemKey.Machine:
                    key = (norename ? string.Empty
                        : Source.Index.ToString().PadLeft(10, '0')
                            + "-")
                    + (string.IsNullOrWhiteSpace(Machine.Name)
                            ? "Default"
                            : Machine.Name);
                    if (lower)
                        key = key.ToLowerInvariant();

                    if (key == null)
                        key = "null";

                    break;

                case ItemKey.MD5:
                    key = Constants.MD5Zero;
                    break;

                case ItemKey.SHA1:
                    key = Constants.SHA1Zero;
                    break;

                case ItemKey.SHA256:
                    key = Constants.SHA256Zero;
                    break;

                case ItemKey.SHA384:
                    key = Constants.SHA384Zero;
                    break;

                case ItemKey.SHA512:
                    key = Constants.SHA512Zero;
                    break;

                case ItemKey.SpamSum:
                    key = Constants.SpamSumZero;
                    break;
            }

            // Double and triple check the key for corner cases
            if (key == null)
                key = string.Empty;

            return key;
        }

        #endregion

        #endregion // Instance Methods

        #region Static Methods

        #region Sorting and Merging

        /// <summary>
        /// Determine if two hashes are equal for the purposes of merging
        /// </summary>
        /// <param name="firstHash">First hash to compare</param>
        /// <param name="secondHash">Second hash to compare</param>
        /// <returns>True if either is empty OR the hashes exactly match, false otherwise</returns>
        public static bool ConditionalHashEquals(byte[] firstHash, byte[] secondHash)
        {
            // If either hash is empty, we say they're equal for merging
            if (firstHash.IsNullOrEmpty() || secondHash.IsNullOrEmpty())
                return true;

            // If they're different sizes, they can't match
            if (firstHash.Length != secondHash.Length)
                return false;

            // Otherwise, they need to match exactly
            return Enumerable.SequenceEqual(firstHash, secondHash);
        }

        /// <summary>
        /// Merge an arbitrary set of ROMs based on the supplied information
        /// </summary>
        /// <param name="infiles">List of File objects representing the roms to be merged</param>
        /// <returns>A List of DatItem objects representing the merged roms</returns>
        public static ConcurrentList<DatItem> Merge(ConcurrentList<DatItem> infiles)
        {
            // Check for null or blank roms first
            if (infiles == null || infiles.Count == 0)
                return new ConcurrentList<DatItem>();

            // Create output list
            ConcurrentList<DatItem> outfiles = new ConcurrentList<DatItem>();

            // Then deduplicate them by checking to see if data matches previous saved roms
            int nodumpCount = 0;
            for (int f = 0; f < infiles.Count; f++)
            {
                DatItem file = infiles[f];

                // If we somehow have a null item, skip
                if (file == null)
                    continue;

                // If we don't have a Disk, Media, or Rom, we skip checking for duplicates
                if (file.ItemType != ItemType.Disk && file.ItemType != ItemType.Media && file.ItemType != ItemType.Rom)
                    continue;

                // If it's a nodump, add and skip
                if (file.ItemType == ItemType.Rom && (file as Rom).ItemStatus == ItemStatus.Nodump)
                {
                    outfiles.Add(file);
                    nodumpCount++;
                    continue;
                }
                else if (file.ItemType == ItemType.Disk && (file as Disk).ItemStatus == ItemStatus.Nodump)
                {
                    outfiles.Add(file);
                    nodumpCount++;
                    continue;
                }
                // If it's the first non-nodump rom in the list, don't touch it
                else if (outfiles.Count == 0 || outfiles.Count == nodumpCount)
                {
                    outfiles.Add(file);
                    continue;
                }

                // Check if the rom is a duplicate
                DupeType dupetype = 0x00;
                DatItem saveditem = new Blank();
                int pos = -1;
                for (int i = 0; i < outfiles.Count; i++)
                {
                    DatItem lastrom = outfiles[i];

                    // Get the duplicate status
                    dupetype = file.GetDuplicateStatus(lastrom);

                    // If it's a duplicate, skip adding it to the output but add any missing information
                    if (dupetype != 0x00)
                    {
                        saveditem = lastrom;
                        pos = i;

                        // Disks, Media, and Roms have more information to fill
                        if (file.ItemType == ItemType.Disk)
                            (saveditem as Disk).FillMissingInformation(file as Disk);
                        else if (file.ItemType == ItemType.Media)
                            (saveditem as Media).FillMissingInformation(file as Media);
                        else if (file.ItemType == ItemType.Rom)
                            (saveditem as Rom).FillMissingInformation(file as Rom);

                        saveditem.DupeType = dupetype;

                        // If the current system has a lower ID than the previous, set the system accordingly
                        if (file.Source.Index < saveditem.Source.Index)
                        {
                            saveditem.Source = file.Source.Clone() as Source;
                            saveditem.CopyMachineInformation(file);
                            saveditem.SetName(file.GetName());
                        }

                        // If the current machine is a child of the new machine, use the new machine instead
                        if (saveditem.Machine.CloneOf == file.Machine.Name || saveditem.Machine.RomOf == file.Machine.Name)
                        {
                            saveditem.CopyMachineInformation(file);
                            saveditem.SetName(file.GetName());
                        }
                        
                        // add cloneof to parent
                        // only saveditem add to list (cause dupetype != 0x00)
                        // used to create parent/child/grand-child chd
                        // name = parent, cloneof = child, grand-child, more grand-childs (using parent/child)
                        if (saveditem.Machine.CloneOf == null || !saveditem.Machine.CloneOf.Contains(file.Machine.Name)
                            || saveditem.Machine.Name != file.Machine.Name)
                            saveditem.Machine.CloneOf += file.Machine.Name + ",";


                        break;
                    }
                }

                // If no duplicate is found, add it to the list
                if (dupetype == 0x00)
                {
                    outfiles.Add(file);
                }
                // Otherwise, if a new rom information is found, add that
                else
                {
                    outfiles.RemoveAt(pos);
                    outfiles.Insert(pos, saveditem);
                }
            }

            // Then return the result
            return outfiles;
        }

        /// <summary>
        /// Resolve name duplicates in an arbitrary set of ROMs based on the supplied information
        /// </summary>
        /// <param name="infiles">List of File objects representing the roms to be merged</param>
        /// <returns>A List of DatItem objects representing the renamed roms</returns>
        public static ConcurrentList<DatItem> ResolveNames(ConcurrentList<DatItem> infiles)
        {
            // Create the output list
            ConcurrentList<DatItem> output = new ConcurrentList<DatItem>();

            // First we want to make sure the list is in alphabetical order
            Sort(ref infiles, true);

            // Now we want to loop through and check names
            DatItem lastItem = null;
            string lastrenamed = null;
            int lastid = 0;
            for (int i = 0; i < infiles.Count; i++)
            {
                DatItem datItem = infiles[i];

                // If we have the first item, we automatically add it
                if (lastItem == null)
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    continue;
                }

                // Get the last item name, if applicable
                string lastItemName = lastItem.GetName() ?? lastItem.ItemType.ToString();

                // Get the current item name, if applicable
                string datItemName = datItem.GetName() ?? datItem.ItemType.ToString();

                // If the current item exactly matches the last item, then we don't add it
                if (datItem.GetDuplicateStatus(lastItem).HasFlag(DupeType.All))
                {
                    staticLogger.Verbose($"Exact duplicate found for '{datItemName}'");
                    continue;
                }

                // If the current name matches the previous name, rename the current item
                else if (datItemName == lastItemName)
                {
                    staticLogger.Verbose($"Name duplicate found for '{datItemName}'");

                    if (datItem.ItemType == ItemType.Disk || datItem.ItemType == ItemType.Media || datItem.ItemType == ItemType.Rom)
                    {
                        datItemName += GetDuplicateSuffix(datItem);
                        lastrenamed ??= datItemName;
                    }

                    // If we have a conflict with the last renamed item, do the right thing
                    if (datItemName == lastrenamed)
                    {
                        lastrenamed = datItemName;
                        datItemName += (lastid == 0 ? string.Empty : "_" + lastid);
                        lastid++;
                    }
                    // If we have no conflict, then we want to reset the lastrenamed and id
                    else
                    {
                        lastrenamed = null;
                        lastid = 0;
                    }

                    // Set the item name back to the datItem
                    datItem.SetName(datItemName);

                    output.Add(datItem);
                }

                // Otherwise, we say that we have a valid named file
                else
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    lastrenamed = null;
                    lastid = 0;
                }
            }

            // One last sort to make sure this is ordered
            Sort(ref output, true);

            return output;
        }

        /// <summary>
        /// Get duplicate suffix based on the item type
        /// </summary>
        private static string GetDuplicateSuffix(DatItem datItem)
        {
            if (datItem.ItemType == ItemType.Disk)
                return (datItem as Disk).GetDuplicateSuffix();
            else if (datItem.ItemType == ItemType.Media)
                return (datItem as Media).GetDuplicateSuffix();
            else if (datItem.ItemType == ItemType.Rom)
                return (datItem as Rom).GetDuplicateSuffix();

            return "_1";
        }

        /// <summary>
        /// Sort a list of File objects by SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="roms">List of File objects representing the roms to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        public static bool Sort(ref ConcurrentList<DatItem> roms, bool norename)
        {
            roms.Sort(delegate (DatItem x, DatItem y)
            {
                try
                {
                    NaturalComparer nc = new NaturalComparer();
                    if (x.Source.Index == y.Source.Index)
                    {
                        if (x.Machine.Name == y.Machine.Name)
                        {
                            if (x.ItemType == y.ItemType)
                            {
                                if (Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.GetName() ?? string.Empty)) == Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.GetName() ?? string.Empty)))
                                    return nc.Compare(Path.GetFileName(Utilities.RemovePathUnsafeCharacters(x.GetName() ?? string.Empty)), Path.GetFileName(Utilities.RemovePathUnsafeCharacters(y.GetName() ?? string.Empty)));

                                return nc.Compare(Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(x.GetName() ?? string.Empty)), Path.GetDirectoryName(Utilities.RemovePathUnsafeCharacters(y.GetName() ?? string.Empty)));
                            }

                            return x.ItemType - y.ItemType;
                        }

                        return nc.Compare(x.Machine.Name, y.Machine.Name);
                    }

                    return (norename ? nc.Compare(x.Machine.Name, y.Machine.Name) : x.Source.Index - y.Source.Index);
                }
                catch
                {
                    // Absorb the error
                    return 0;
                }
            });

            return true;
        }

        #endregion

        #endregion // Static Methods
    }
}
