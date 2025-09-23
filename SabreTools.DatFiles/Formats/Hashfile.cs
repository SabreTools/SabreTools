using System;
using System.Collections.Generic;
using SabreTools.Core.Filter;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a hashfile such as an SFV, MD5, or SHA-1 file
    /// </summary>
    public abstract class Hashfile : SerializableDatFile<Models.Hashfile.Hashfile, Serialization.Deserializers.Hashfile, Serialization.Serializers.Hashfile, Serialization.CrossModel.Hashfile>
    {
        #region Fields

        // Private instance variables specific to Hashfile DATs
        protected HashType _hash;

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Hashfile(DatFile? datFile) : base(datFile)
        {
        }

        /// <inheritdoc/>
        public override void ParseFile(string filename,
            int indexId,
            bool keep,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false)
        {
            try
            {
                // Deserialize the input file
                var hashfile = new Serialization.Deserializers.Hashfile().Deserialize(filename, _hash);
                var metadata = new Serialization.CrossModel.Hashfile().Serialize(hashfile);

                // Convert to the internal format
                ConvertFromMetadata(metadata, filename, indexId, keep, statsOnly, filterRunner);
            }
            catch (Exception ex) when (!throwOnError)
            {
                string message = $"'{filename}' - An error occurred during parsing";
                _logger.Error(ex, message);
            }
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");

                // Serialize the input file
                var metadata = ConvertToMetadata(ignoreblanks);
                var hashfile = new Serialization.CrossModel.Hashfile().Deserialize(metadata, _hash);
                if (!Serialization.Serializers.Hashfile.SerializeFile(hashfile, outfile, _hash))
                {
                    _logger.Warning($"File '{outfile}' could not be written! See the log for more details.");
                    return false;
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            _logger.User($"'{outfile}' written!{Environment.NewLine}");
            return true;
        }
    }

    /// <summary>
    /// Represents an SFV (CRC-32) hashfile
    /// </summary>
    public sealed class SfvFile : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SfvFile(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.CRC32;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSFV);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey)))
                        missingFields.Add(Models.Metadata.Rom.CRCKey);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an MD2 hashfile
    /// </summary>
    public sealed class Md2File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Md2File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.MD2;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpMD2);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key)))
                        missingFields.Add(Models.Metadata.Rom.MD2Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an MD4 hashfile
    /// </summary>
    public sealed class Md4File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Md4File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.MD4;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpMD4);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key)))
                        missingFields.Add(Models.Metadata.Rom.MD4Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an MD5 hashfile
    /// </summary>
    public sealed class Md5File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Md5File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.MD5;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpMD5);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key)))
                        missingFields.Add(Models.Metadata.Disk.MD5Key);
                    break;

                case Media medium:
                    if (string.IsNullOrEmpty(medium.GetStringFieldValue(Models.Metadata.Media.MD5Key)))
                        missingFields.Add(Models.Metadata.Media.MD5Key);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key)))
                        missingFields.Add(Models.Metadata.Rom.MD5Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an RIPEMD128 hashfile
    /// </summary>
    public sealed class RipeMD128File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public RipeMD128File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.RIPEMD128;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpRIPEMD128);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.RIPEMD128Key)))
                        missingFields.Add(Models.Metadata.Rom.RIPEMD128Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an RIPEMD160 hashfile
    /// </summary>
    public sealed class RipeMD160File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public RipeMD160File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.RIPEMD160;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpRIPEMD160);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.RIPEMD160Key)))
                        missingFields.Add(Models.Metadata.Rom.RIPEMD160Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an SHA-1 hashfile
    /// </summary>
    public sealed class Sha1File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Sha1File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.SHA1;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSHA1);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                        missingFields.Add(Models.Metadata.Disk.SHA1Key);
                    break;

                case Media medium:
                    if (string.IsNullOrEmpty(medium.GetStringFieldValue(Models.Metadata.Media.SHA1Key)))
                        missingFields.Add(Models.Metadata.Media.SHA1Key);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA1Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an SHA-256 hashfile
    /// </summary>
    public sealed class Sha256File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Media,
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Sha256File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.SHA256;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSHA256);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Media medium:
                    if (string.IsNullOrEmpty(medium.GetStringFieldValue(Models.Metadata.Media.SHA256Key)))
                        missingFields.Add(Models.Metadata.Media.SHA256Key);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA256Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an SHA-384 hashfile
    /// </summary>
    public sealed class Sha384File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Sha384File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.SHA384;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSHA384);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA384Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an SHA-512 hashfile
    /// </summary>
    public sealed class Sha512File : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Sha512File(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.SHA512;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSHA512);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA512Key);
                    break;
            }

            return missingFields;
        }
    }

    /// <summary>
    /// Represents an SpamSum hashfile
    /// </summary>
    public sealed class SpamSumFile : Hashfile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Media,
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SpamSumFile(DatFile? datFile) : base(datFile)
        {
            _hash = HashType.SpamSum;
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.RedumpSpamSum);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Media medium:
                    if (string.IsNullOrEmpty(medium.GetStringFieldValue(Models.Metadata.Media.SpamSumKey)))
                        missingFields.Add(Models.Metadata.Media.SpamSumKey);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey)))
                        missingFields.Add(Models.Metadata.Rom.SpamSumKey);
                    break;
            }

            return missingFields;
        }
    }
}
