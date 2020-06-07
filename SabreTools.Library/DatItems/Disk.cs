using System.Linq;
using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents Compressed Hunks of Data (CHD) formatted disks which use internal hashes
    /// </summary>
    public class Disk : DatItem
    {
        #region Private instance variables

        private byte[] _md5; // 16 bytes
        private byte[] _ripemd160; // 20 bytes
        private byte[] _sha1; // 20 bytes
        private byte[] _sha256; // 32 bytes
        private byte[] _sha384; // 48 bytes
        private byte[] _sha512; // 64 bytes

        #endregion

        #region Publicly facing variables

        /// <summary>
        /// Data MD5 hash
        /// </summary>
        public string MD5
        {
            get { return _md5.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_md5); }
            set { _md5 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Data RIPEMD160 hash
        /// </summary>
        public string RIPEMD160
        {
            get { return _ripemd160.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_ripemd160); }
            set { _ripemd160 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Data SHA-1 hash
        /// </summary>
        public string SHA1
        {
            get { return _sha1.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha1); }
            set { _sha1 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Data SHA-256 hash
        /// </summary>
        public string SHA256
        {
            get { return _sha256.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha256); }
            set { _sha256 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Data SHA-384 hash
        /// </summary>
        public string SHA384
        {
            get { return _sha384.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha384); }
            set { _sha384 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Data SHA-512 hash
        /// </summary>
        public string SHA512
        {
            get { return _sha512.IsNullOrWhiteSpace() ? null : Utilities.ByteArrayToString(_sha512); }
            set { _sha512 = Utilities.StringToByteArray(value); }
        }

        /// <summary>
        /// Disk name to merge from parent
        /// </summary>
        public string MergeTag { get; set; }

        /// <summary>
        /// Disk region
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Disk index
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Disk writable flag
        /// </summary>
        public bool? Writable { get; set; }

        /// <summary>
        /// Disk dump status
        /// </summary>
        public ItemStatus ItemStatus { get; set; }

        /// <summary>
        /// Determine if the disk is optional in the set
        /// </summary>
        public bool? Optional { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Disk object
        /// </summary>
        public Disk()
        {
            this.Name = string.Empty;
            this.ItemType = ItemType.Disk;
            this.DupeType = 0x00;
            this.ItemStatus = ItemStatus.None;
        }

        /// <summary>
        /// Create a Rom object from a BaseFile
        /// </summary>
        /// <param name="baseFile"></param>
        public Disk(BaseFile baseFile)
        {
            this.Name = baseFile.Filename;
            _md5 = baseFile.MD5;
            _ripemd160 = baseFile.RIPEMD160;
            _sha1 = baseFile.SHA1;
            _sha256 = baseFile.SHA256;
            _sha384 = baseFile.SHA384;
            _sha512 = baseFile.SHA512;

            this.ItemType = ItemType.Disk;
            this.DupeType = 0x00;
            this.ItemStatus = ItemStatus.None;
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

                _md5 = this._md5,
                _ripemd160 = this._ripemd160,
                _sha1 = this._sha1,
                _sha256 = this._sha256,
                _sha384 = this._sha384,
                _sha512 = this._sha512,
                ItemStatus = this.ItemStatus,
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
                Name = this.Name,
                ItemType = ItemType.Rom,
                DupeType = this.DupeType,

                CRC = null,
                MD5 = this.MD5,
                RIPEMD160 = this.RIPEMD160,
                SHA1 = this.SHA1,
                SHA256 = this.SHA256,
                SHA384 = this.SHA384,
                SHA512 = this.SHA512,

                MergeTag = this.MergeTag,
                Region = this.Region,
                ItemStatus = this.ItemStatus,
                Optional = this.Optional,

                MachineName = this.MachineName,
                Comment = this.Comment,
                MachineDescription = this.MachineDescription,
                Year = this.Year,
                Manufacturer = this.Manufacturer,
                Publisher = this.Publisher,
                RomOf = this.RomOf,
                CloneOf = this.CloneOf,
                SampleOf = this.SampleOf,
                Supported = this.Supported,
                SourceFile = this.SourceFile,
                Runnable = this.Runnable,
                Board = this.Board,
                RebuildTo = this.RebuildTo,
                Devices = this.Devices,
                SlotOptions = this.SlotOptions,
                Infos = this.Infos,
                MachineType = this.MachineType,

                PartName = this.PartName,
                PartInterface = this.PartInterface,
                Features = this.Features,
                AreaName = this.AreaName,
                AreaSize = this.AreaSize,

                SystemID = this.SystemID,
                System = this.System,
                SourceID = this.SourceID,
                Source = this.Source,
                Remove = this.Remove,
            };

            return rom;
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            bool dupefound = false;

            // If we don't have a disk, return false
            if (this.ItemType != other.ItemType)
                return dupefound;

            // Otherwise, treat it as a Disk
            Disk newOther = other as Disk;

            // If all hashes are empty but they're both nodump and the names match, then they're dupes
            if ((this.ItemStatus == ItemStatus.Nodump && newOther.ItemStatus == ItemStatus.Nodump)
                && (this.Name == newOther.Name)
                && (this._md5.IsNullOrWhiteSpace() && newOther._md5.IsNullOrWhiteSpace())
                && (this._ripemd160.IsNullOrWhiteSpace() && newOther._ripemd160.IsNullOrWhiteSpace())
                && (this._sha1.IsNullOrWhiteSpace() && newOther._sha1.IsNullOrWhiteSpace())
                && (this._sha256.IsNullOrWhiteSpace() && newOther._sha256.IsNullOrWhiteSpace())
                && (this._sha384.IsNullOrWhiteSpace() && newOther._sha384.IsNullOrWhiteSpace())
                && (this._sha512.IsNullOrWhiteSpace() && newOther._sha512.IsNullOrWhiteSpace()))
            {
                dupefound = true;
            }

            // If we can determine that the disks have no non-empty hashes in common, we return false
            else if ((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace())
                && (this._ripemd160.IsNullOrWhiteSpace() || newOther._ripemd160.IsNullOrWhiteSpace())
                && (this._sha1.IsNullOrWhiteSpace() || newOther._sha1.IsNullOrWhiteSpace())
                && (this._sha256.IsNullOrWhiteSpace() || newOther._sha256.IsNullOrWhiteSpace())
                && (this._sha384.IsNullOrWhiteSpace() || newOther._sha384.IsNullOrWhiteSpace())
                && (this._sha512.IsNullOrWhiteSpace() || newOther._sha512.IsNullOrWhiteSpace()))
            {
                dupefound = false;
            }

            // Otherwise if we get a partial match
            else if (((this._md5.IsNullOrWhiteSpace() || newOther._md5.IsNullOrWhiteSpace()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
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
