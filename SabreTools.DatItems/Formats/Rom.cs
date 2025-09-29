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
    public sealed class Rom : DatItem<Data.Models.Metadata.Rom>
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
                var status = GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus();
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
                        || dataArea.GetInt64FieldValue(Data.Models.Metadata.DataArea.SizeKey) != null
                        || dataArea.GetInt64FieldValue(Data.Models.Metadata.DataArea.WidthKey) != null
                        || dataArea.GetStringFieldValue(Data.Models.Metadata.DataArea.EndiannessKey).AsEndianness() != Endianness.NULL);
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
                        || !string.IsNullOrEmpty(part.GetStringFieldValue(Data.Models.Metadata.Part.InterfaceKey)));
            }
        }

        #endregion

        #region Constructors

        public Rom() : base()
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);
            SetFieldValue<string?>(Data.Models.Metadata.Rom.StatusKey, ItemStatus.None.AsStringValue());
        }

        public Rom(Data.Models.Metadata.Dump item, Machine machine, Source source, int index)
        {
            // If we don't have rom data, we can't do anything
            Data.Models.Metadata.Rom? rom = null;
            OpenMSXSubType subType = OpenMSXSubType.NULL;
            if (item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.RomKey) != null)
            {
                rom = item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.RomKey);
                subType = OpenMSXSubType.Rom;
            }
            else if (item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.MegaRomKey) != null)
            {
                rom = item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.MegaRomKey);
                subType = OpenMSXSubType.MegaRom;
            }
            else if (item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.SCCPlusCartKey) != null)
            {
                rom = item.Read<Data.Models.Metadata.Rom>(Data.Models.Metadata.Dump.SCCPlusCartKey);
                subType = OpenMSXSubType.SCCPlusCart;
            }

            // Just return if nothing valid was found
            if (rom == null)
                return;

            string name = $"{machine.GetName()}_{index++}{(!string.IsNullOrEmpty(rom!.ReadString(Data.Models.Metadata.Rom.RemarkKey)) ? $" {rom.ReadString(Data.Models.Metadata.Rom.RemarkKey)}" : string.Empty)}";

            SetName(name);
            SetFieldValue<string?>(Data.Models.Metadata.Rom.OffsetKey, rom.ReadString(Data.Models.Metadata.Rom.StartKey));
            SetFieldValue<string?>(Data.Models.Metadata.Rom.OpenMSXMediaType, subType.AsStringValue());
            SetFieldValue<string?>(Data.Models.Metadata.Rom.OpenMSXType, rom.ReadString(Data.Models.Metadata.Rom.OpenMSXType) ?? rom.ReadString(Data.Models.Metadata.Rom.TypeKey));
            SetFieldValue<string?>(Data.Models.Metadata.Rom.RemarkKey, rom.ReadString(Data.Models.Metadata.Rom.RemarkKey));
            SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, rom.ReadString(Data.Models.Metadata.Rom.SHA1Key));
            SetFieldValue<string?>(Data.Models.Metadata.Rom.StartKey, rom.ReadString(Data.Models.Metadata.Rom.StartKey));
            SetFieldValue<Source?>(DatItem.SourceKey, source);

            if (item.Read<Data.Models.Metadata.Original>(Data.Models.Metadata.Dump.OriginalKey) != null)
            {
                var original = item.Read<Data.Models.Metadata.Original>(Data.Models.Metadata.Dump.OriginalKey)!;
                SetFieldValue<Original?>("ORIGINAL", new Original
                {
                    Value = original.ReadBool(Data.Models.Metadata.Original.ValueKey),
                    Content = original.ReadString(Data.Models.Metadata.Original.ContentKey),
                });
            }

            CopyMachineInformation(machine);

            // Process hash values
            if (GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey).ToString());
            if (GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, TextHelper.NormalizeCRC32(GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD2Key, TextHelper.NormalizeMD2(GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD4Key, TextHelper.NormalizeMD5(GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, TextHelper.NormalizeMD5(GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD128Key, TextHelper.NormalizeRIPEMD128(GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD160Key, TextHelper.NormalizeRIPEMD160(GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, TextHelper.NormalizeSHA1(GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, TextHelper.NormalizeSHA256(GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA384Key, TextHelper.NormalizeSHA384(GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA512Key, TextHelper.NormalizeSHA512(GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key)));
        }

        public Rom(Data.Models.Metadata.Rom item) : base(item)
        {
            SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Rom.DisposeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.DisposeKey, GetBoolFieldValue(Data.Models.Metadata.Rom.DisposeKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Rom.InvertedKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.InvertedKey, GetBoolFieldValue(Data.Models.Metadata.Rom.InvertedKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Rom.LoadFlagKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.LoadFlagKey, GetStringFieldValue(Data.Models.Metadata.Rom.LoadFlagKey).AsLoadFlag().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.OpenMSXMediaType, GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType).AsOpenMSXSubType().AsStringValue());
            if (GetBoolFieldValue(Data.Models.Metadata.Rom.MIAKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MIAKey, GetBoolFieldValue(Data.Models.Metadata.Rom.MIAKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Rom.OptionalKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.OptionalKey, GetBoolFieldValue(Data.Models.Metadata.Rom.OptionalKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Rom.SoundOnlyKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SoundOnlyKey, GetBoolFieldValue(Data.Models.Metadata.Rom.SoundOnlyKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.StatusKey, GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus().AsStringValue());

            // Process hash values
            if (GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey).ToString());
            if (GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, TextHelper.NormalizeCRC32(GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD2Key, TextHelper.NormalizeMD2(GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD4Key, TextHelper.NormalizeMD4(GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, TextHelper.NormalizeMD5(GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD128Key, TextHelper.NormalizeRIPEMD128(GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD160Key, TextHelper.NormalizeRIPEMD160(GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, TextHelper.NormalizeSHA1(GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, TextHelper.NormalizeSHA256(GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA384Key, TextHelper.NormalizeSHA384(GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key)));
            if (GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA512Key, TextHelper.NormalizeSHA512(GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key)));
        }

        public Rom(Data.Models.Metadata.Rom item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
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
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey);
                    break;

                case ItemKey.MD2:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key);
                    break;

                case ItemKey.MD4:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key);
                    break;

                case ItemKey.MD5:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key);
                    break;

                case ItemKey.RIPEMD128:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key);
                    break;

                case ItemKey.RIPEMD160:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key);
                    break;

                case ItemKey.SHA1:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key);
                    break;

                case ItemKey.SHA256:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key);
                    break;

                case ItemKey.SHA384:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key);
                    break;

                case ItemKey.SHA512:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key);
                    break;

                case ItemKey.SpamSum:
                    key = GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey);
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
