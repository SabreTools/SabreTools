using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Tools;
using Newtonsoft.Json;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents a generic file within a set
    /// </summary>
    public class Rom : DatItem
    {
        #region Private instance variables

        private byte[] _crc; // 8 bytes
        private byte[] _md5; // 16 bytes
        private byte[] _ripemd160; // 20 bytes
        private byte[] _sha1; // 20 bytes
        private byte[] _sha256; // 32 bytes
        private byte[] _sha384; // 48 bytes
        private byte[] _sha512; // 64 bytes

        #endregion

        #region Publicly facing variables

        /// <summary>
        /// What BIOS is required for this rom
        /// </summary>
        [JsonProperty("bios")]
        public string Bios { get; set; }

        /// <summary>
        /// Byte size of the rom
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>
        /// File CRC32 hash
        /// </summary>
        [JsonProperty("crc")]
        public string CRC
        {
            get { return _crc.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_crc); }
            set { _crc = (value == "null" ? Constants.CRCZeroBytes : Utilities.StringToByteArray(Sanitizer.CleanCRC32(value))); }
        }

        /// <summary>
        /// File MD5 hash
        /// </summary>
        [JsonProperty("md5")]
        public string MD5
        {
            get { return _md5.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_md5); }
            set { _md5 = Utilities.StringToByteArray(Sanitizer.CleanMD5(value)); }
        }

#if NET_FRAMEWORK
        /// <summary>
        /// File RIPEMD160 hash
        /// </summary>
        [JsonProperty("ripemd160")]
        public string RIPEMD160
        {
            get { return _ripemd160.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_ripemd160); }
            set { _ripemd160 = Utilities.StringToByteArray(Sanitizer.CleanRIPEMD160(value)); }
        }
#endif

        /// <summary>
        /// File SHA-1 hash
        /// </summary>
        [JsonProperty("sha1")]
        public string SHA1
        {
            get { return _sha1.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_sha1); }
            set { _sha1 = Utilities.StringToByteArray(Sanitizer.CleanSHA1(value)); }
        }

        /// <summary>
        /// File SHA-256 hash
        /// </summary>
        [JsonProperty("sha256")]
        public string SHA256
        {
            get { return _sha256.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_sha256); }
            set { _sha256 = Utilities.StringToByteArray(Sanitizer.CleanSHA256(value)); }
        }

        /// <summary>
        /// File SHA-384 hash
        /// </summary>
        [JsonProperty("sha384")]
        public string SHA384
        {
            get { return _sha384.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_sha384); }
            set { _sha384 = Utilities.StringToByteArray(Sanitizer.CleanSHA384(value)); }
        }

        /// <summary>
        /// File SHA-512 hash
        /// </summary>
        [JsonProperty("sha512")]
        public string SHA512
        {
            get { return _sha512.IsNullOrEmpty() ? null : Utilities.ByteArrayToString(_sha512); }
            set { _sha512 = Utilities.StringToByteArray(Sanitizer.CleanSHA512(value)); }
        }

        /// <summary>
        /// Rom name to merge from parent
        /// </summary>
        [JsonProperty("merge")]
        public string MergeTag { get; set; }

        /// <summary>
        /// Rom region
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// Data offset within rom
        /// </summary>
        [JsonProperty("offset")]
        public string Offset { get; set; }

        /// <summary>
        /// File created date
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }

        /// <summary>
        /// Rom dump status
        /// </summary>
        [JsonProperty("status")]
        public ItemStatus ItemStatus { get; set; }

        /// <summary>
        /// Determine if the rom is optional in the set
        /// </summary>
        [JsonProperty("optional")]
        public bool? Optional { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Rom object
        /// </summary>
        public Rom()
        {
            this.Name = string.Empty;
            this.ItemType = ItemType.Rom;
            this.DupeType = 0x00;
            this.ItemStatus = ItemStatus.None;
            this.Date = string.Empty;
        }

        /// <summary>
        /// Create a "blank" Rom object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="machineName"></param>
        /// <param name="omitFromScan"></param>
        public Rom(string name, string machineName)
        {
            this.Name = name;
            this.ItemType = ItemType.Rom;
            this.Size = -1;
            this.ItemStatus = ItemStatus.None;

            _machine = new Machine
            {
                Name = machineName,
                Description = machineName,
            };
        }

        /// <summary>
        /// Create a Rom object from a BaseFile
        /// </summary>
        /// <param name="baseFile"></param>
        public Rom(BaseFile baseFile)
        {
            this.Name = baseFile.Filename;
            this.Size = baseFile.Size ?? -1;
            _crc = baseFile.CRC;
            _md5 = baseFile.MD5;
#if NET_FRAMEWORK
            _ripemd160 = baseFile.RIPEMD160;
#endif
            _sha1 = baseFile.SHA1;
            _sha256 = baseFile.SHA256;
            _sha384 = baseFile.SHA384;
            _sha512 = baseFile.SHA512;

            this.ItemType = ItemType.Rom;
            this.DupeType = 0x00;
            this.ItemStatus = ItemStatus.None;
            this.Date = baseFile.Date;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new Rom()
            {
                Name = this.Name,
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Supported = this.Supported,
                Publisher = this.Publisher,
                Category = this.Category,
                Infos = this.Infos,
                PartName = this.PartName,
                PartInterface = this.PartInterface,
                Features = this.Features,
                AreaName = this.AreaName,
                AreaSize = this.AreaSize,

                MachineName = this.MachineName,
                Comment = this.Comment,
                MachineDescription = this.MachineDescription,
                Year = this.Year,
                Manufacturer = this.Manufacturer,
                RomOf = this.RomOf,
                CloneOf = this.CloneOf,
                SampleOf = this.SampleOf,
                SourceFile = this.SourceFile,
                Runnable = this.Runnable,
                Board = this.Board,
                RebuildTo = this.RebuildTo,
                Devices = this.Devices,
                MachineType = this.MachineType,

                IndexId = this.IndexId,
                IndexSource = this.IndexSource,

                Size = this.Size,
                _crc = this._crc,
                _md5 = this._md5,
                _ripemd160 = this._ripemd160,
                _sha1 = this._sha1,
                _sha256 = this._sha256,
                _sha384 = this._sha384,
                _sha512 = this._sha512,
                ItemStatus = this.ItemStatus,
                Date = this.Date,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            bool dupefound = false;

            // If we don't have a rom, return false
            if (this.ItemType != other.ItemType)
                return dupefound;

            // Otherwise, treat it as a Rom
            Rom newOther = other as Rom;

            // If all hashes are empty but they're both nodump and the names match, then they're dupes
            if ((this.ItemStatus == ItemStatus.Nodump && newOther.ItemStatus == ItemStatus.Nodump)
                && (this.Name == newOther.Name)
                && (this._crc.IsNullOrEmpty() && newOther._crc.IsNullOrEmpty())
                && (this._md5.IsNullOrEmpty() && newOther._md5.IsNullOrEmpty())
                && (this._ripemd160.IsNullOrEmpty() && newOther._ripemd160.IsNullOrEmpty())
                && (this._sha1.IsNullOrEmpty() && newOther._sha1.IsNullOrEmpty())
                && (this._sha256.IsNullOrEmpty() && newOther._sha256.IsNullOrEmpty())
                && (this._sha384.IsNullOrEmpty() && newOther._sha384.IsNullOrEmpty())
                && (this._sha512.IsNullOrEmpty() && newOther._sha512.IsNullOrEmpty()))
            {
                dupefound = true;
            }

            // If we can determine that the roms have no non-empty hashes in common, we return false
            else if ((this._crc.IsNullOrEmpty() || newOther._crc.IsNullOrEmpty())
                && (this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty())
                && (this._ripemd160.IsNullOrEmpty() || newOther._ripemd160.IsNullOrEmpty())
                && (this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty())
                && (this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty())
                && (this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty())
                && (this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()))
            {
                dupefound = false;
            }

            // If we have a file that has no known size, rely on the hashes only
            else if ((this.Size == -1)
                && ((this._crc.IsNullOrEmpty() || newOther._crc.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._crc, newOther._crc))
                && ((this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
                && ((this._ripemd160.IsNullOrEmpty() || newOther._ripemd160.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._ripemd160, newOther._ripemd160))
                && ((this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
                && ((this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
                && ((this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
                && ((this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
            {
                dupefound = true;
            }

            // Otherwise if we get a partial match
            else if ((this.Size == newOther.Size)
                && ((this._crc.IsNullOrEmpty() || newOther._crc.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._crc, newOther._crc))
                && ((this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
                && ((this._ripemd160.IsNullOrEmpty() || newOther._ripemd160.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._ripemd160, newOther._ripemd160))
                && ((this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
                && ((this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
                && ((this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
                && ((this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
            {
                dupefound = true;
            }

            return dupefound;
        }

        #endregion
    }
}
