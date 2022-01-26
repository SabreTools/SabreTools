using System.Xml.Serialization;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.FileTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents Compressed Hunks of Data (CHD) formatted disks which use internal hashes
    /// </summary>
    [JsonObject("disk"), XmlRoot("disk")]
    public class Disk : DatItem
    {
        #region Private instance variables

        private byte[] _md5; // 16 bytes
        private byte[] _sha1; // 20 bytes

        #endregion

        #region Fields

        #region Common

        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonProperty("name")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Data MD5 hash
        /// </summary>
        [JsonProperty("md5", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("md5")]
        public string MD5
        {
            get { return _md5.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_md5); }
            set { _md5 = Utilities.StringToByteArray(CleanMD5(value)); }
        }

        /// <summary>
        /// Data SHA-1 hash
        /// </summary>
        [JsonProperty("sha1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("sha1")]
        public string SHA1
        {
            get { return _sha1.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_sha1); }
            set { _sha1 = Utilities.StringToByteArray(CleanSHA1(value)); }
        }

        /// <summary>
        /// Disk name to merge from parent
        /// </summary>
        [JsonProperty("merge", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("merge")]
        public string MergeTag { get; set; }

        /// <summary>
        /// Disk region
        /// </summary>
        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("region")]
        public string Region { get; set; }

        /// <summary>
        /// Disk index
        /// </summary>
        [JsonProperty("index", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("index")]
        public string Index { get; set; }

        /// <summary>
        /// Disk writable flag
        /// </summary>
        [JsonProperty("writable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("writable")]
        public bool? Writable { get; set; } = null;

        [JsonIgnore]
        public bool WritableSpecified { get { return Writable != null; } }

        /// <summary>
        /// Disk dump status
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        [XmlElement("status")]
        public ItemStatus ItemStatus { get; set; }

        [JsonIgnore]
        public bool ItemStatusSpecified { get { return ItemStatus != ItemStatus.NULL; } }

        /// <summary>
        /// Determine if the disk is optional in the set
        /// </summary>
        [JsonProperty("optional", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("optional")]
        public bool? Optional { get; set; } = null;

        [JsonIgnore]
        public bool OptionalSpecified { get { return Optional != null; } }

        #endregion

        #region SoftwareList

        /// <summary>
        /// Disk area information
        /// </summary>
        [JsonProperty("diskarea", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("diskarea")]
        public DiskArea DiskArea { get; set; } = null;

        [JsonIgnore]
        public bool DiskAreaSpecified
        {
            get
            {
                return DiskArea != null
                    && !string.IsNullOrEmpty(DiskArea.Name);
            }
        }

        /// <summary>
        /// Original hardware part associated with the item
        /// </summary>
        [JsonProperty("part", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("part")]
        public Part Part { get; set; } = null;

        [JsonIgnore]
        public bool PartSpecified
        {
            get
            {
                return Part != null
                    && (!string.IsNullOrEmpty(Part.Name)
                        || !string.IsNullOrEmpty(Part.Interface));
            }
        }

        #endregion

        #endregion // Fields

        #region Accessors

        /// <inheritdoc/>
        public override string GetName() => Name;

        /// <inheritdoc/>
        public override void SetName(string name) => Name = name;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Disk object
        /// </summary>
        public Disk()
        {
            Name = string.Empty;
            ItemType = ItemType.Disk;
            DupeType = 0x00;
            ItemStatus = ItemStatus.None;
        }

        /// <summary>
        /// Create a Disk object from a BaseFile
        /// </summary>
        /// <param name="baseFile"></param>
        public Disk(BaseFile baseFile)
        {
            Name = baseFile.Filename;
            _md5 = baseFile.MD5;
            _sha1 = baseFile.SHA1;

            ItemType = ItemType.Disk;
            DupeType = 0x00;
            ItemStatus = ItemStatus.None;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new Disk()
            {
                Name = this.Name,
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Machine = this.Machine.Clone() as Machine,
                Source = this.Source.Clone() as Source,
                Remove = this.Remove,

                _md5 = this._md5,
                _sha1 = this._sha1,
                MergeTag = this.MergeTag,
                Region = this.Region,
                Index = this.Index,
                Writable = this.Writable,
                ItemStatus = this.ItemStatus,
                Optional = this.Optional,

                DiskArea = this.DiskArea,
                Part = this.Part,
            };
        }

        /// <summary>
        /// Convert Disk object to a BaseFile
        /// </summary>
        public BaseFile ConvertToBaseFile()
        {
            return new BaseFile()
            {
                Filename = this.Name,
                Parent = this.Machine?.Name,
                MD5 = this._md5,
                SHA1 = this._sha1,
            };
        }

        /// <summary>
        /// Convert a disk to the closest Rom approximation
        /// </summary>
        /// <returns></returns>
        public Rom ConvertToRom()
        {
            var rom = new Rom()
            {
                Name = this.Name + ".chd",
                ItemType = ItemType.Rom,
                DupeType = this.DupeType,

                Machine = this.Machine.Clone() as Machine,
                Source = this.Source.Clone() as Source,
                Remove = this.Remove,

                MergeTag = this.MergeTag,
                Region = this.Region,
                ItemStatus = this.ItemStatus,
                Optional = this.Optional,

                MD5 = this.MD5,
                SHA1 = this.SHA1,

                Part = this.Part,
            };

            // for chd: ItemType Disk without DiskArea
            if (this.DiskArea == null) 
                rom.DataArea = new DataArea();
            else
                rom.DataArea = new DataArea { Name = this.DiskArea.Name };
            return rom;
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            bool dupefound = false;

            // If we don't have a rom, return false
            if (ItemType != other.ItemType)
                return dupefound;

            // Otherwise, treat it as a Disk
            Disk newOther = other as Disk;

            // If all hashes are empty but they're both nodump and the names match, then they're dupes
            if ((ItemStatus == ItemStatus.Nodump && newOther.ItemStatus == ItemStatus.Nodump)
                && Name == newOther.Name
                && !HasHashes() && !newOther.HasHashes())
            {
                dupefound = true;
            }

            // Otherwise if we get a partial match
            else if (HashMatch(newOther))
            {
                dupefound = true;
            }

            return dupefound;
        }

        /// <summary>
        /// Fill any missing size and hash information from another Disk
        /// </summary>
        /// <param name="other">Disk to fill information from</param>
        public void FillMissingInformation(Disk other)
        {
            if (_md5.IsNullOrEmpty() && !other._md5.IsNullOrEmpty())
                _md5 = other._md5;

            if (_sha1.IsNullOrEmpty() && !other._sha1.IsNullOrEmpty())
                _sha1 = other._sha1;
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        /// <returns>String representing the suffix</returns>
        public string GetDuplicateSuffix()
        {
            if (!_md5.IsNullOrEmpty())
                return $"_{MD5}";
            else if (!_sha1.IsNullOrEmpty())
                return $"_{SHA1}";
            else
                return "_1";
        }

        /// <summary>
        /// Returns if there are no, non-empty hashes in common with another Disk
        /// </summary>
        /// <param name="other">Disk to compare against</param>
        /// <returns>True if at least one hash is not mutually exclusive, false otherwise</returns>
        private bool HasCommonHash(Disk other)
        {
            return !(_md5.IsNullOrEmpty() ^ other._md5.IsNullOrEmpty())
                || !(_sha1.IsNullOrEmpty() ^ other._sha1.IsNullOrEmpty());
        }

        /// <summary>
        /// Returns if the Disk contains any hashes
        /// </summary>
        /// <returns>True if any hash exists, false otherwise</returns>
        private bool HasHashes()
        {
            return !_md5.IsNullOrEmpty()
                || !_sha1.IsNullOrEmpty();
        }

        /// <summary>
        /// Returns if any hashes are common with another Disk
        /// </summary>
        /// <param name="other">Disk to compare against</param>
        /// <returns>True if any hashes are in common, false otherwise</returns>
        private bool HashMatch(Disk other)
        {
            // If either have no hashes, we return false, otherwise this would be a false positive
            if (!HasHashes() || !other.HasHashes())
                return false;

            // If neither have hashes in common, we return false, otherwise this would be a false positive
            if (!HasCommonHash(other))
                return false;

            // Return if all hashes match according to merge rules
            return ConditionalHashEquals(_md5, other._md5)
                && ConditionalHashEquals(_sha1, other._sha1);
        }

        #endregion

        #region Sorting and Merging

        /// <inheritdoc/>
        public override string GetKey(ItemKey bucketedBy, bool lower = true, bool norename = true)
        {
            // Set the output key as the default blank string
            string key;

            // Now determine what the key should be based on the bucketedBy value
            switch (bucketedBy)
            {
                case ItemKey.MD5:
                    key = MD5;
                    break;

                case ItemKey.SHA1:
                    key = SHA1;
                    break;

                // Let the base handle generic stuff
                default:
                    return base.GetKey(bucketedBy, lower, norename);
            }

            // Double and triple check the key for corner cases
            if (key == null)
                key = string.Empty;

            return key;
        }

        #endregion
    }
}
