using System;
using System.Collections.Generic;
using SabreTools.Core.Filter;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a value-separated DAT
    /// </summary>
    public abstract class SeparatedValue : SerializableDatFile<Models.SeparatedValue.MetadataFile, Serialization.Deserializers.SeparatedValue, Serialization.Serializers.SeparatedValue, Serialization.CrossModel.SeparatedValue>
    {
        #region Fields

        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ];

        /// <summary>
        /// Represents the delimiter between fields
        /// </summary>
        protected char _delim;

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SeparatedValue(DatFile? datFile) : base(datFile)
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
                var metadataFile = new Serialization.Deserializers.SeparatedValue().Deserialize(filename, _delim);
                var metadata = new Serialization.CrossModel.SeparatedValue().Serialize(metadataFile);

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
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key))
                        && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                    {
                        missingFields.Add(Models.Metadata.Disk.SHA1Key);
                    }
                    break;

                case Media media:
                    if (string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.MD5Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SHA1Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SHA256Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey)))
                    {
                        missingFields.Add(Models.Metadata.Media.SHA1Key);
                    }
                    break;

                case Rom rom:
                    if (rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) == null || rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey)))
                    {
                        missingFields.Add(Models.Metadata.Rom.SHA1Key);
                    }
                    break;
            }

            return missingFields;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");

                // Serialize the input file
                var metadata = ConvertToMetadata(ignoreblanks);
                var metadataFile = new Serialization.CrossModel.SeparatedValue().Deserialize(metadata);
                if (!Serialization.Serializers.SeparatedValue.SerializeFile(metadataFile, outfile, _delim))
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
    /// Represents a comma-separated value file
    /// </summary>
    public sealed class CommaSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public CommaSeparatedValue(DatFile? datFile) : base(datFile)
        {
            _delim = ',';
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.CSV);
        }
    }

    /// <summary>
    /// Represents a semicolon-separated value file
    /// </summary>
    public sealed class SemicolonSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SemicolonSeparatedValue(DatFile? datFile) : base(datFile)
        {
            _delim = ';';
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.SSV);
        }
    }

    /// <summary>
    /// Represents a tab-separated value file
    /// </summary>
    public sealed class TabSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public TabSeparatedValue(DatFile? datFile) : base(datFile)
        {
            _delim = '\t';
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.TSV);
        }
    }
}
