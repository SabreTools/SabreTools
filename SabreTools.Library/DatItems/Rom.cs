using System.Linq;
using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Tools;

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
        public string Bios { get; set; }

        /// <summary>
        /// Byte size of the rom
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// File CRC32 hash
        /// </summary>
        public string CRC
        {
            get { return _crc.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_crc); }
            set { _crc = (value == "null" ? Constants.CRCZeroBytes : Utilities.StringToByteArray(value)); }
        }

        /// <summary>
        /// File MD5 hash
        /// </summary>
        public string MD5
        {
            get { return _md5.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_md5); }
            set { _md5 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// File RIPEMD160 hash
        /// </summary>
        public string RIPEMD160
        {
            get { return _ripemd160.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_ripemd160); }
            set { _ripemd160 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// File SHA-1 hash
        /// </summary>
        public string SHA1
        {
            get { return _sha1.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha1); }
            set { _sha1 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// File SHA-256 hash
        /// </summary>
        public string SHA256
        {
            get { return _sha256.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha256); }
            set { _sha256 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// File SHA-384 hash
        /// </summary>
        public string SHA384
        {
            get { return _sha384.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha384); }
            set { _sha384 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// File SHA-512 hash
        /// </summary>
        public string SHA512
        {
            get { return _sha512.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha512); }
            set { _sha512 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Rom name to merge from parent
        /// </summary>
        public string MergeTag { get; set; }

        /// <summary>
        /// Rom region
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Data offset within rom
        /// </summary>
        public string Offset { get; set; }

        /// <summary>
        /// File created date
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Rom dump status
        /// </summary>
        public ItemStatus ItemStatus { get; set; }

        /// <summary>
        /// Determine if the rom is optional in the set
        /// </summary>
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
        /// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
        public Rom(string name, string machineName, Hash omitFromScan = Hash.DeepHashes)
        {
            this.Name = name;
            this.ItemType = ItemType.Rom;
            this.Size = -1;

            if ((omitFromScan & Hash.CRC) == 0)
                _crc = null;

            if ((omitFromScan & Hash.MD5) == 0)
                _md5 = null;

            if ((omitFromScan & Hash.RIPEMD160) == 0)
                _ripemd160 = null;

            if ((omitFromScan & Hash.SHA1) == 0)
                _sha1 = null;

            if ((omitFromScan & Hash.SHA256) == 0)
                _sha256 = null;

            if ((omitFromScan & Hash.SHA384) == 0)
                _sha384 = null;

            if ((omitFromScan & Hash.SHA512) == 0)
                _sha512 = null;

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
            _ripemd160 = baseFile.RIPEMD160;
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

                SystemID = this.SystemID,
                System = this.System,
                SourceID = this.SourceID,
                Source = this.Source,

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
                && (this._crc.IsNullOrWhiteSpace() && newOther._crc.IsNullOrWhiteSpace())
                && (this._md5.IsNullOrWhiteSpace() && newOther._md5.IsNullOrWhiteSpace())
                && (this._ripemd160.IsNullOrWhiteSpace() && newOther._ripemd160.IsNullOrWhiteSpace())
                && (this._sha1.IsNullOrWhiteSpace() && newOther._sha1.IsNullOrWhiteSpace())
                && (this._sha256.IsNullOrWhiteSpace() && newOther._sha256.IsNullOrWhiteSpace())
                && (this._sha384.IsNullOrWhiteSpace() && newOther._sha384.IsNullOrWhiteSpace())
                && (this._sha512.IsNullOrWhiteSpace() && newOther._sha512.IsNullOrWhiteSpace()))
            {
                dupefound = true;
            }

            // If we can determine that the roms have no non-empty hashes in common, we return false
            else if ((this._crc.IsNullOrWhiteSpace() || newOther._crc.IsNullOrWhiteSpace())
                && (this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace())
                && (this._ripemd160.IsNullOrWhiteSpace() || newOther._ripemd160.IsNullOrWhiteSpace())
                && (this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace())
                && (this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace())
                && (this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace())
                && (this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()))
            {
                dupefound = false;
            }

            // If we have a file that has no known size, rely on the hashes only
            else if ((this.Size == -1)
                && ((this._crc.IsNullOrWhiteSpace() || newOther._crc.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._crc, newOther._crc))
                && ((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
                && ((this._ripemd160.IsNullOrWhiteSpace() || newOther._ripemd160.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._ripemd160, newOther._ripemd160))
                && ((this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
                && ((this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
                && ((this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
                && ((this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
            {
                dupefound = true;
            }

            // Otherwise if we get a partial match
            else if ((this.Size == newOther.Size)
                && ((this._crc.IsNullOrWhiteSpace() || newOther._crc.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._crc, newOther._crc))
                && ((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
                && ((this._ripemd160.IsNullOrWhiteSpace() || newOther._ripemd160.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._ripemd160, newOther._ripemd160))
                && ((this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
                && ((this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
                && ((this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
                && ((this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
            {
                dupefound = true;
            }

            return dupefound;
        }

        #endregion
    }
}
