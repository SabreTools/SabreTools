using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a generic file within a set
    /// </summary>
    [JsonObject("rom"), XmlRoot("rom")]
    public sealed class Rom : DatItem<Models.Metadata.Rom>
    {
        #region Constants

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string DataAreaKey = "DATAAREA";

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string PartKey = "PART";

        #endregion

        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Rom;

        [JsonIgnore]
        public bool ItemStatusSpecified
        {
            get
            {
                var status = GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus();
                return status != ItemStatus.NULL && status != ItemStatus.None;
            }
        }

        [JsonIgnore]
        public bool OriginalSpecified
        {
            get
            {
                var original = GetFieldValue<Original?>("ORIGINAL");
                return original != null && original != default;
            }
        }

        [JsonIgnore]
        public bool DataAreaSpecified
        {
            get
            {
                var dataArea = GetFieldValue<DataArea?>(Rom.DataAreaKey);
                return dataArea != null
                    && (!string.IsNullOrEmpty(dataArea.GetName())
                        || dataArea.GetInt64FieldValue(Models.Metadata.DataArea.SizeKey) != null
                        || dataArea.GetInt64FieldValue(Models.Metadata.DataArea.WidthKey) != null
                        || dataArea.GetStringFieldValue(Models.Metadata.DataArea.EndiannessKey).AsEndianness() != Endianness.NULL);
            }
        }

        [JsonIgnore]
        public bool PartSpecified
        {
            get
            {
                var part = GetFieldValue<Part?>(Rom.PartKey);
                return part != null
                    && (!string.IsNullOrEmpty(part.GetName())
                        || !string.IsNullOrEmpty(part.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)));
            }
        }

        #endregion

        #region Constructors

        public Rom() : base()
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);
            SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.None.AsStringValue());
        }

        public Rom(Models.Metadata.Rom item) : base(item)
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.Rom.DisposeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.DisposeKey, GetBoolFieldValue(Models.Metadata.Rom.DisposeKey).FromYesNo());
            if (GetBoolFieldValue(Models.Metadata.Rom.InvertedKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.InvertedKey, GetBoolFieldValue(Models.Metadata.Rom.InvertedKey).FromYesNo());
            if (GetStringFieldValue(Models.Metadata.Rom.LoadFlagKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.LoadFlagKey, GetStringFieldValue(Models.Metadata.Rom.LoadFlagKey).AsLoadFlag().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.OpenMSXMediaType, GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType).AsOpenMSXSubType().AsStringValue());
            if (GetBoolFieldValue(Models.Metadata.Rom.MIAKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.MIAKey, GetBoolFieldValue(Models.Metadata.Rom.MIAKey).FromYesNo());
            if (GetBoolFieldValue(Models.Metadata.Rom.OptionalKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.OptionalKey, GetBoolFieldValue(Models.Metadata.Rom.OptionalKey).FromYesNo());
            if (GetBoolFieldValue(Models.Metadata.Rom.SoundOnlyKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SoundOnlyKey, GetBoolFieldValue(Models.Metadata.Rom.SoundOnlyKey).FromYesNo());
            if (GetStringFieldValue(Models.Metadata.Rom.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus().AsStringValue());

            // Process hash values
            if (GetInt64FieldValue(Models.Metadata.Rom.SizeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SizeKey, GetInt64FieldValue(Models.Metadata.Rom.SizeKey).ToString());
            if (GetStringFieldValue(Models.Metadata.Rom.CRCKey) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, TextHelper.NormalizeCRC32(GetStringFieldValue(Models.Metadata.Rom.CRCKey)));
            if (GetStringFieldValue(Models.Metadata.Rom.MD2Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, TextHelper.NormalizeMD2(GetStringFieldValue(Models.Metadata.Rom.MD2Key)));
            if (GetStringFieldValue(Models.Metadata.Rom.MD4Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, TextHelper.NormalizeMD4(GetStringFieldValue(Models.Metadata.Rom.MD4Key)));
            if (GetStringFieldValue(Models.Metadata.Rom.MD5Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, TextHelper.NormalizeMD5(GetStringFieldValue(Models.Metadata.Rom.MD5Key)));
            if (GetStringFieldValue("RIPEMD128") != null)
                SetFieldValue<string?>("RIPEMD128", TextHelper.NormalizeRIPEMD128(GetStringFieldValue("RIPEMD128")));
            if (GetStringFieldValue("RIPEMD160") != null)
                SetFieldValue<string?>("RIPEMD160", TextHelper.NormalizeRIPEMD160(GetStringFieldValue("RIPEMD160")));
            if (GetStringFieldValue(Models.Metadata.Rom.SHA1Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, TextHelper.NormalizeSHA1(GetStringFieldValue(Models.Metadata.Rom.SHA1Key)));
            if (GetStringFieldValue(Models.Metadata.Rom.SHA256Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, TextHelper.NormalizeSHA256(GetStringFieldValue(Models.Metadata.Rom.SHA256Key)));
            if (GetStringFieldValue(Models.Metadata.Rom.SHA384Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, TextHelper.NormalizeSHA384(GetStringFieldValue(Models.Metadata.Rom.SHA384Key)));
            if (GetStringFieldValue(Models.Metadata.Rom.SHA512Key) != null)
                SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, TextHelper.NormalizeSHA512(GetStringFieldValue(Models.Metadata.Rom.SHA512Key)));
        }

        #endregion

        #region Comparision Methods

        /// <summary>
        /// Fill any missing size and hash information from another Rom
        /// </summary>
        /// <param name="other">Rom to fill information from</param>
        public void FillMissingInformation(Rom other)
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
                case ItemKey.CRC:
                    key = GetStringFieldValue(Models.Metadata.Rom.CRCKey);
                    break;

                case ItemKey.MD2:
                    key = GetStringFieldValue(Models.Metadata.Rom.MD2Key);
                    break;

                case ItemKey.MD4:
                    key = GetStringFieldValue(Models.Metadata.Rom.MD4Key);
                    break;

                case ItemKey.MD5:
                    key = GetStringFieldValue(Models.Metadata.Rom.MD5Key);
                    break;

                case ItemKey.RIPEMD128:
                    key = GetStringFieldValue("RIPEMD128");
                    break;

                case ItemKey.RIPEMD160:
                    key = GetStringFieldValue("RIPEMD160");
                    break;

                case ItemKey.SHA1:
                    key = GetStringFieldValue(Models.Metadata.Rom.SHA1Key);
                    break;

                case ItemKey.SHA256:
                    key = GetStringFieldValue(Models.Metadata.Rom.SHA256Key);
                    break;

                case ItemKey.SHA384:
                    key = GetStringFieldValue(Models.Metadata.Rom.SHA384Key);
                    break;

                case ItemKey.SHA512:
                    key = GetStringFieldValue(Models.Metadata.Rom.SHA512Key);
                    break;

                case ItemKey.SpamSum:
                    key = GetStringFieldValue(Models.Metadata.Rom.SpamSumKey);
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
