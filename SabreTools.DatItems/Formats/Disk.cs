using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents Compressed Hunks of Data (CHD) formatted disks which use internal hashes
    /// </summary>
    [JsonObject("disk"), XmlRoot("disk")]
    public sealed class Disk : DatItem<Models.Metadata.Disk>
    {
        #region Constants

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string DiskAreaKey = "DISKAREA";

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string PartKey = "PART";

        #endregion

        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Disk;

        [JsonIgnore]
        public bool DiskAreaSpecified
        {
            get
            {
                var diskArea = GetFieldValue<DiskArea?>(Disk.DiskAreaKey);
                return diskArea != null && !string.IsNullOrEmpty(diskArea.GetName());
            }
        }

        [JsonIgnore]
        public bool PartSpecified
        {
            get
            {
                var part = GetFieldValue<Part?>(Disk.PartKey);
                return part != null
                    && (!string.IsNullOrEmpty(part.GetName())
                        || !string.IsNullOrEmpty(part.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)));
            }
        }

        #endregion

        #region Constructors

        public Disk() : base()
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);
            SetFieldValue<string?>(Models.Metadata.Disk.StatusKey, ItemStatus.None.AsStringValue());
        }

        public Disk(Models.Metadata.Disk item) : base(item)
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.Disk.OptionalKey) != null)
                SetFieldValue<string?>(Models.Metadata.Disk.OptionalKey, GetBoolFieldValue(Models.Metadata.Disk.OptionalKey).FromYesNo());
            if (GetStringFieldValue(Models.Metadata.Disk.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.Disk.StatusKey, GetStringFieldValue(Models.Metadata.Disk.StatusKey).AsItemStatus().AsStringValue());
            if (GetBoolFieldValue(Models.Metadata.Disk.WritableKey) != null)
                SetFieldValue<string?>(Models.Metadata.Disk.WritableKey, GetBoolFieldValue(Models.Metadata.Disk.WritableKey).FromYesNo());

            // Process hash values
            if (GetStringFieldValue(Models.Metadata.Disk.MD5Key) != null)
                SetFieldValue<string?>(Models.Metadata.Disk.MD5Key, TextHelper.NormalizeMD5(GetStringFieldValue(Models.Metadata.Disk.MD5Key)));
            if (GetStringFieldValue(Models.Metadata.Disk.SHA1Key) != null)
                SetFieldValue<string?>(Models.Metadata.Disk.SHA1Key, TextHelper.NormalizeSHA1(GetStringFieldValue(Models.Metadata.Disk.SHA1Key)));
        }

        public Disk(Models.Metadata.Disk item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Convert a disk to the closest Rom approximation
        /// </summary>
        /// <returns></returns>
        public Rom ConvertToRom()
        {
            var rom = new Rom(_internal.ConvertToRom()!);

            // Create a DataArea if there was an existing DiskArea
            var diskArea = GetFieldValue<DiskArea?>(Disk.DiskAreaKey);
            if (diskArea != null)
            {
                var dataArea = new DataArea();

                string? diskAreaName = diskArea.GetStringFieldValue(Models.Metadata.DiskArea.NameKey);
                dataArea.SetFieldValue(Models.Metadata.DataArea.NameKey, diskAreaName);

                rom.SetFieldValue<DataArea?>(Rom.DataAreaKey, dataArea);
            }

            rom.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            rom.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine()?.Clone() as Machine);
            rom.SetFieldValue<Part>(Rom.PartKey, GetFieldValue<Part>(Disk.PartKey)?.Clone() as Part);
            rom.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            rom.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey)?.Clone() as Source);

            return rom;
        }

        #endregion

        #region Comparision Methods

        /// <summary>
        /// Fill any missing size and hash information from another Disk
        /// </summary>
        /// <param name="other">Disk to fill information from</param>
        public void FillMissingInformation(Disk other)
            => _internal.FillMissingHashes(other._internal);

        /// <summary>
        /// Returns if the Rom contains any hashes
        /// </summary>
        /// <returns>True if any hash exists, false otherwise</returns>
        public bool HasHashes() => _internal.HasHashes();

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values
        /// </summary>
        /// <returns>True if any hash matches the 0-byte value, false otherwise</returns>
        public bool HasZeroHash() => _internal.HasZeroHash();

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
                case ItemKey.MD5:
                    key = GetStringFieldValue(Models.Metadata.Disk.MD5Key);
                    break;

                case ItemKey.SHA1:
                    key = GetStringFieldValue(Models.Metadata.Disk.SHA1Key);
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
