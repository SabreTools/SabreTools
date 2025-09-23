using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents Aaruformat images which use internal hashes
    /// </summary>
    [JsonObject("media"), XmlRoot("media")]
    public sealed class Media : DatItem<Models.Metadata.Media>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Media;

        #endregion

        #region Constructors

        public Media() : base()
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);
        }

        public Media(Models.Metadata.Media item) : base(item)
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

            // Process hash values
            if (GetStringFieldValue(Models.Metadata.Media.MD5Key) != null)
                SetFieldValue<string?>(Models.Metadata.Media.MD5Key, TextHelper.NormalizeMD5(GetStringFieldValue(Models.Metadata.Media.MD5Key)));
            if (GetStringFieldValue(Models.Metadata.Media.SHA1Key) != null)
                SetFieldValue<string?>(Models.Metadata.Media.SHA1Key, TextHelper.NormalizeSHA1(GetStringFieldValue(Models.Metadata.Media.SHA1Key)));
            if (GetStringFieldValue(Models.Metadata.Media.SHA256Key) != null)
                SetFieldValue<string?>(Models.Metadata.Media.SHA256Key, TextHelper.NormalizeSHA256(GetStringFieldValue(Models.Metadata.Media.SHA256Key)));
        }

        public Media(Models.Metadata.Media item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Convert a media to the closest Rom approximation
        /// </summary>
        /// <returns></returns>
        public Rom ConvertToRom()
        {
            var rom = new Rom(_internal.ConvertToRom()!);

            rom.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            rom.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine());
            rom.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            rom.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));

            return rom;
        }

        #endregion

        #region Comparision Methods

        /// <summary>
        /// Fill any missing size and hash information from another Media
        /// </summary>
        /// <param name="other">Media to fill information from</param>
        public void FillMissingInformation(Media other)
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
                    key = GetStringFieldValue(Models.Metadata.Media.MD5Key);
                    break;

                case ItemKey.SHA1:
                    key = GetStringFieldValue(Models.Metadata.Media.SHA1Key);
                    break;

                case ItemKey.SHA256:
                    key = GetStringFieldValue(Models.Metadata.Media.SHA256Key);
                    break;

                case ItemKey.SpamSum:
                    key = GetStringFieldValue(Models.Metadata.Media.SpamSumKey);
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
