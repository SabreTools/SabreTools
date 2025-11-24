using System;
using SabreTools.Core.Filter;
using SabreTools.Data.Models.Metadata;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a DAT that can be serialized
    /// </summary>
    /// <typeparam name="TModel">Base internal model for the DAT type</typeparam>
    /// <typeparam name="TFileReader">IFileReader type to use for conversion</typeparam>
    /// <typeparam name="TFileWriter">IFileWriter type to use for conversion</typeparam>
    /// <typeparam name="TCrossModel">ICrossModel for cross-model serialization</typeparam>
    public abstract class SerializableDatFile<TModel, TFileReader, TFileWriter, TCrossModel> : DatFile
        where TFileReader : IFileReader<TModel>
        where TFileWriter : IFileWriter<TModel>
        where TCrossModel : ICrossModel<TModel, MetadataFile>
    {
        #region Static Serialization Instances

        /// <summary>
        /// File deserializer instance
        /// </summary>
        private static readonly TFileReader FileDeserializer = Activator.CreateInstance<TFileReader>();

        /// <summary>
        /// File serializer instance
        /// </summary>
        private static readonly TFileWriter FileSerializer = Activator.CreateInstance<TFileWriter>();

        /// <summary>
        /// Cross-model serializer instance
        /// </summary>
        private static readonly TCrossModel CrossModelSerializer = Activator.CreateInstance<TCrossModel>();

        #endregion

        /// <inheritdoc/>
        protected SerializableDatFile(DatFile? datFile) : base(datFile) { }

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
                // Deserialize the input file in two steps
                var specificFormat = FileDeserializer.Deserialize(filename);
                var internalFormat = CrossModelSerializer.Serialize(specificFormat);

                // Convert to the internal format
                ConvertFromMetadata(internalFormat, filename, indexId, keep, statsOnly, filterRunner);
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

                // Serialize the input file in two steps
                var internalFormat = ConvertToMetadata(ignoreblanks);
                var specificFormat = CrossModelSerializer.Deserialize(internalFormat);
                if (!FileSerializer.SerializeFile(specificFormat, outfile))
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

        /// <inheritdoc/>
        public override bool WriteToFileDB(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");

                // Serialize the input file in two steps
                var internalFormat = ConvertToMetadataDB(ignoreblanks);
                var specificFormat = CrossModelSerializer.Deserialize(internalFormat);
                if (!FileSerializer.SerializeFile(specificFormat, outfile))
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
