using System;
using System.Collections.Generic;
using SabreTools.Core.Filter;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a ClrMamePro DAT
    /// </summary>
    public sealed class ClrMamePro : SerializableDatFile<Data.Models.ClrMamePro.MetadataFile, Serialization.Readers.ClrMamePro, Serialization.Writers.ClrMamePro, Serialization.CrossModel.ClrMamePro>
    {
        #region Fields

        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Archive,
                ItemType.BiosSet,
                ItemType.Chip,
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Display,
                ItemType.Driver,
                ItemType.Input,
                ItemType.Media,
                ItemType.Release,
                ItemType.Rom,
                ItemType.Sample,
                ItemType.Sound,
            ];

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public ClrMamePro(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.ClrMamePro);
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
                var metadataFile = new Serialization.Readers.ClrMamePro().Deserialize(filename, quotes: true);
                var metadata = new Serialization.CrossModel.ClrMamePro().Serialize(metadataFile);

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
            switch (datItem)
            {
                case Release release:
                    if (string.IsNullOrEmpty(release.GetName()))
                        missingFields.Add(Data.Models.Metadata.Release.NameKey);
                    if (string.IsNullOrEmpty(release.GetStringFieldValue(Data.Models.Metadata.Release.RegionKey)))
                        missingFields.Add(Data.Models.Metadata.Release.RegionKey);
                    break;

                case BiosSet biosset:
                    if (string.IsNullOrEmpty(biosset.GetName()))
                        missingFields.Add(Data.Models.Metadata.BiosSet.NameKey);
                    if (string.IsNullOrEmpty(biosset.GetStringFieldValue(Data.Models.Metadata.BiosSet.DescriptionKey)))
                        missingFields.Add(Data.Models.Metadata.BiosSet.DescriptionKey);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetName()))
                        missingFields.Add(Data.Models.Metadata.Rom.NameKey);
                    if (rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) == null || rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Data.Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue("MD2"))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue("MD4"))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey)))
                    {
                        missingFields.Add(Data.Models.Metadata.Rom.SHA1Key);
                    }
                    break;

                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetName()))
                        missingFields.Add(Data.Models.Metadata.Disk.NameKey);
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key))
                        && string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key)))
                    {
                        missingFields.Add(Data.Models.Metadata.Disk.SHA1Key);
                    }
                    break;

                case Sample sample:
                    if (string.IsNullOrEmpty(sample.GetName()))
                        missingFields.Add(Data.Models.Metadata.Sample.NameKey);
                    break;

                case Archive archive:
                    if (string.IsNullOrEmpty(archive.GetName()))
                        missingFields.Add(Data.Models.Metadata.Archive.NameKey);
                    break;

                case Chip chip:
                    if (chip.GetStringFieldValue(Data.Models.Metadata.Chip.ChipTypeKey).AsChipType() == ChipType.NULL)
                        missingFields.Add(Data.Models.Metadata.Chip.ChipTypeKey);
                    if (string.IsNullOrEmpty(chip.GetName()))
                        missingFields.Add(Data.Models.Metadata.Chip.NameKey);
                    break;

                case Display display:
                    if (display.GetStringFieldValue(Data.Models.Metadata.Display.DisplayTypeKey).AsDisplayType() == DisplayType.NULL)
                        missingFields.Add(Data.Models.Metadata.Display.DisplayTypeKey);
                    if (display.GetInt64FieldValue(Data.Models.Metadata.Display.RotateKey) == null)
                        missingFields.Add(Data.Models.Metadata.Display.RotateKey);
                    break;

                case Sound sound:
                    if (sound.GetInt64FieldValue(Data.Models.Metadata.Sound.ChannelsKey) == null)
                        missingFields.Add(Data.Models.Metadata.Sound.ChannelsKey);
                    break;

                case Input input:
                    if (input.GetInt64FieldValue(Data.Models.Metadata.Input.PlayersKey) == null)
                        missingFields.Add(Data.Models.Metadata.Input.PlayersKey);
                    if (!input.ControlsSpecified)
                        missingFields.Add(Data.Models.Metadata.Input.ControlKey);
                    break;

                case DipSwitch dipswitch:
                    if (string.IsNullOrEmpty(dipswitch.GetName()))
                        missingFields.Add(Data.Models.Metadata.DipSwitch.NameKey);
                    break;

                case Driver driver:
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.StatusKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.StatusKey);
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.EmulationKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.EmulationKey);
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
                var metadataFile = new Serialization.CrossModel.ClrMamePro().Deserialize(metadata);
                if (!new Serialization.Writers.ClrMamePro().SerializeFile(metadataFile, outfile, quotes: true))
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
}
