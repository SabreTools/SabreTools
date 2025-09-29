using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;

// TODO: Add item mappings for all fields
namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single file item
    /// </summary>
    [JsonObject("file"), XmlRoot("file")]
    public sealed class File : DatItem
    {
        #region Private instance variables

        private byte[]? _crc; // 8 bytes
        private byte[]? _md5; // 16 bytes
        private byte[]? _sha1; // 20 bytes
        private byte[]? _sha256; // 32 bytes

        #endregion

        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.File;

        /// <summary>
        /// ID value
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Extension value
        /// </summary>
        [JsonProperty("extension", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("extension")]
        public string? Extension { get; set; }

        /// <summary>
        /// Byte size of the rom
        /// </summary>
        [JsonProperty("size", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("size")]
        public long? Size { get; set; } = null;

        /// <summary>
        /// File CRC32 hash
        /// </summary>
        [JsonProperty("crc", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("crc")]
        public string? CRC
        {
            get { return _crc.ToHexString(); }
            set { _crc = (value == "null" ? ZeroHash.CRC32Arr : TextHelper.NormalizeCRC32(value).FromHexString()); }
        }

        /// <summary>
        /// File MD5 hash
        /// </summary>
        [JsonProperty("md5", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("md5")]
        public string? MD5
        {
            get { return _md5.ToHexString(); }
            set { _md5 = TextHelper.NormalizeMD5(value).FromHexString(); }
        }

        /// <summary>
        /// File SHA-1 hash
        /// </summary>
        [JsonProperty("sha1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("sha1")]
        public string? SHA1
        {
            get { return _sha1.ToHexString(); }
            set { _sha1 = TextHelper.NormalizeSHA1(value).FromHexString(); }
        }

        /// <summary>
        /// File SHA-256 hash
        /// </summary>
        [JsonProperty("sha256", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("sha256")]
        public string? SHA256
        {
            get { return _sha256.ToHexString(); }
            set { _sha256 = TextHelper.NormalizeSHA256(value).FromHexString(); }
        }

        /// <summary>
        /// Format value
        /// </summary>
        [JsonProperty("format", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("format")]
        public string? Format { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty File object
        /// </summary>
        public File()
        {
            SetFieldValue<ItemType>(Data.Models.Metadata.DatItem.TypeKey, ItemType);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            var file = new File()
            {
                Id = this.Id,
                Extension = this.Extension,
                Size = this.Size,
                _crc = this._crc,
                _md5 = this._md5,
                _sha1 = this._sha1,
                _sha256 = this._sha256,
                Format = this.Format,
            };
            file.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            file.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine()!.Clone() as Machine ?? new Machine());
            file.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            file.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));

            return file;
        }

        /// <summary>
        /// Convert a disk to the closest Rom approximation
        /// </summary>
        /// <returns></returns>
        public Rom ConvertToRom()
        {
            var rom = new Rom();

            rom.SetName($"{Id}.{Extension}");
            rom.SetFieldValue<long?>(Data.Models.Metadata.Rom.SizeKey, Size);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, CRC);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, MD5);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, SHA1);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, SHA256);

            rom.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            rom.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine()?.Clone() as Machine);
            rom.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            rom.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));

            return rom;
        }

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(DatItem? other)
        {
            bool dupefound = false;

            // If we don't have a file, return false
            if (GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey) != other?.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey))
                return dupefound;

            // Otherwise, treat it as a File
            File? newOther = other as File;

            // If all hashes are empty, then they're dupes
            if (!HasHashes() && !newOther!.HasHashes())
            {
                dupefound = true;
            }

            // If we have a file that has no known size, rely on the hashes only
            else if (Size == null && HashMatch(newOther!))
            {
                dupefound = true;
            }

            // Otherwise if we get a partial match
            else if (Size == newOther!.Size && HashMatch(newOther))
            {
                dupefound = true;
            }

            return dupefound;
        }

        /// <summary>
        /// Fill any missing size and hash information from another Rom
        /// </summary>
        /// <param name="other">File to fill information from</param>
        public void FillMissingInformation(File other)
        {
            if (Size == null && other.Size != null)
                Size = other.Size;

            if (_crc.IsNullOrEmpty() && !other._crc.IsNullOrEmpty())
                _crc = other._crc;

            if (_md5.IsNullOrEmpty() && !other._md5.IsNullOrEmpty())
                _md5 = other._md5;

            if (_sha1.IsNullOrEmpty() && !other._sha1.IsNullOrEmpty())
                _sha1 = other._sha1;

            if (_sha256.IsNullOrEmpty() && !other._sha256.IsNullOrEmpty())
                _sha256 = other._sha256;
        }

        /// <summary>
        /// Returns if the File contains any hashes
        /// </summary>
        /// <returns>True if any hash exists, false otherwise</returns>
        public bool HasHashes()
        {
            return !_crc.IsNullOrEmpty()
                || !_md5.IsNullOrEmpty()
                || !_sha1.IsNullOrEmpty()
                || !_sha256.IsNullOrEmpty();
        }

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values
        /// </summary>
        /// <returns>True if any hash matches the 0-byte value, false otherwise</returns>
        public bool HasZeroHash()
        {
            bool crcNull = string.IsNullOrEmpty(CRC) || string.Equals(CRC, ZeroHash.CRC32Str, StringComparison.OrdinalIgnoreCase);
            bool md5Null = string.IsNullOrEmpty(MD5) || string.Equals(MD5, ZeroHash.MD5Str, StringComparison.OrdinalIgnoreCase);
            bool sha1Null = string.IsNullOrEmpty(SHA1) || string.Equals(SHA1, ZeroHash.SHA1Str, StringComparison.OrdinalIgnoreCase);
            bool sha256Null = string.IsNullOrEmpty(SHA256) || string.Equals(SHA256, ZeroHash.SHA256Str, StringComparison.OrdinalIgnoreCase);

            return crcNull && md5Null && sha1Null && sha256Null;
        }

        /// <summary>
        /// Returns if there are no, non-empty hashes in common with another File
        /// </summary>
        /// <param name="other">File to compare against</param>
        /// <returns>True if at least one hash is not mutually exclusive, false otherwise</returns>
        private bool HasCommonHash(File other)
        {
            return !(_crc.IsNullOrEmpty() ^ other._crc.IsNullOrEmpty())
                || !(_md5.IsNullOrEmpty() ^ other._md5.IsNullOrEmpty())
                || !(_sha1.IsNullOrEmpty() ^ other._sha1.IsNullOrEmpty())
                || !(_sha256.IsNullOrEmpty() ^ other._sha256.IsNullOrEmpty());
        }

        /// <summary>
        /// Returns if any hashes are common with another File
        /// </summary>
        /// <param name="other">File to compare against</param>
        /// <returns>True if any hashes are in common, false otherwise</returns>
        private bool HashMatch(File other)
        {
            // If either have no hashes, we return false, otherwise this would be a false positive
            if (!HasHashes() || !other.HasHashes())
                return false;

            // If neither have hashes in common, we return false, otherwise this would be a false positive
            if (!HasCommonHash(other))
                return false;

            // Return if all hashes match according to merge rules
            return Utilities.ConditionalHashEquals(_crc, other._crc)
                && Utilities.ConditionalHashEquals(_md5, other._md5)
                && Utilities.ConditionalHashEquals(_sha1, other._sha1)
                && Utilities.ConditionalHashEquals(_sha256, other._sha256);
        }

        #endregion

        #region Sorting and Merging

        /// <inheritdoc/>
        public override string GetKey(ItemKey bucketedBy, Machine? machine, Source? source, bool lower = true, bool norename = true)
        {
            // Set the output key as the default blank string
            string? key;

            // Now determine what the key should be based on the bucketedBy value
            switch (bucketedBy)
            {
                case ItemKey.CRC:
                    key = CRC;
                    break;

                case ItemKey.MD5:
                    key = MD5;
                    break;

                case ItemKey.SHA1:
                    key = SHA1;
                    break;

                case ItemKey.SHA256:
                    key = SHA256;
                    break;

                // Let the base handle generic stuff
                default:
                    return base.GetKey(bucketedBy, machine, source, lower, norename);
            }

            // Double and triple check the key for corner cases
            key ??= string.Empty;
            if (lower)
                key = key.ToLowerInvariant();

            return key;
        }

        #endregion
    }
}
