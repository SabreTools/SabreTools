using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Represents all possible DAT header information
    /// </summary>
    [JsonObject("header"), XmlRoot("header")]
    public sealed class DatHeader : ModelBackedItem<Data.Models.Metadata.Header>, ICloneable
    {
        #region Constants

        /// <summary>
        /// Read or write format
        /// </summary>
        public const string DatFormatKey = "DATFORMAT";

        /// <summary>
        /// External name of the DAT
        /// </summary>
        public const string FileNameKey = "FILENAME";

        #endregion

        #region Fields

        [JsonIgnore]
        public bool CanOpenSpecified
        {
            get
            {
                var canOpen = GetStringArrayFieldValue(Data.Models.Metadata.Header.CanOpenKey);
                return canOpen != null && canOpen.Length > 0;
            }
        }

        [JsonIgnore]
        public bool ImagesSpecified
        {
            get
            {
                return GetFieldValue<Data.Models.OfflineList.Images?>(Data.Models.Metadata.Header.ImagesKey) != null;
            }
        }

        [JsonIgnore]
        public bool InfosSpecified
        {
            get
            {
                return GetFieldValue<Data.Models.OfflineList.Infos?>(Data.Models.Metadata.Header.InfosKey) != null;
            }
        }

        [JsonIgnore]
        public bool NewDatSpecified
        {
            get
            {
                return GetFieldValue<Data.Models.OfflineList.NewDat?>(Data.Models.Metadata.Header.NewDatKey) != null;
            }
        }

        [JsonIgnore]
        public bool SearchSpecified
        {
            get
            {
                return GetFieldValue<Data.Models.OfflineList.Search?>(Data.Models.Metadata.Header.SearchKey) != null;
            }
        }

        #endregion

        #region Constructors

        public DatHeader() { }

        public DatHeader(Data.Models.Metadata.Header header)
        {
            // Create a new internal model
            _internal = [];

            // Get all fields to automatically copy without processing
            var nonItemFields = TypeHelper.GetConstants(typeof(Data.Models.Metadata.Header));
            if (nonItemFields != null)
            {
                // Populate the internal machine from non-filter fields
                foreach (string fieldName in nonItemFields)
                {
                    if (header.ContainsKey(fieldName))
                        _internal[fieldName] = header[fieldName];
                }
            }

            // Get all fields specific to the DatFiles implementation
            var nonStandardFields = TypeHelper.GetConstants(typeof(DatHeader));
            if (nonStandardFields != null)
            {
                // Populate the internal machine from filter fields
                foreach (string fieldName in nonStandardFields)
                {
                    if (header.ContainsKey(fieldName))
                        _internal[fieldName] = header[fieldName];
                }
            }

            // Get all no-filter fields
            if (header.ContainsKey(Data.Models.Metadata.Header.CanOpenKey))
                _internal[Data.Models.Metadata.Header.CanOpenKey] = header[Data.Models.Metadata.Header.CanOpenKey];
            if (header.ContainsKey(Data.Models.Metadata.Header.ImagesKey))
                _internal[Data.Models.Metadata.Header.ImagesKey] = header[Data.Models.Metadata.Header.ImagesKey];
            if (header.ContainsKey(Data.Models.Metadata.Header.InfosKey))
                _internal[Data.Models.Metadata.Header.InfosKey] = header[Data.Models.Metadata.Header.InfosKey];
            if (header.ContainsKey(Data.Models.Metadata.Header.NewDatKey))
                _internal[Data.Models.Metadata.Header.NewDatKey] = header[Data.Models.Metadata.Header.NewDatKey];
            if (header.ContainsKey(Data.Models.Metadata.Header.SearchKey))
                _internal[Data.Models.Metadata.Header.SearchKey] = header[Data.Models.Metadata.Header.SearchKey];
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Clone the current header
        /// </summary>
        public object Clone() => new DatHeader(GetInternalClone());

        /// <summary>
        /// Clone just the format from the current header
        /// </summary>
        public DatHeader CloneFormat()
        {
            var header = new DatHeader();

            header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey,
                GetFieldValue<DatFormat>(DatHeader.DatFormatKey));

            return header;
        }

        /// <summary>
        /// Get a clone of the current internal model
        /// </summary>
        public Data.Models.Metadata.Header GetInternalClone()
        {
            var header = (_internal.Clone() as Data.Models.Metadata.Header)!;

            // Remove fields with default values
            if (header.ReadString(Data.Models.Metadata.Header.ForceMergingKey).AsMergingFlag() == MergingFlag.None)
                header.Remove(Data.Models.Metadata.Header.ForceMergingKey);
            if (header.ReadString(Data.Models.Metadata.Header.ForceNodumpKey).AsNodumpFlag() == NodumpFlag.None)
                header.Remove(Data.Models.Metadata.Header.ForceNodumpKey);
            if (header.ReadString(Data.Models.Metadata.Header.ForcePackingKey).AsPackingFlag() == PackingFlag.None)
                header.Remove(Data.Models.Metadata.Header.ForcePackingKey);
            if (header.ReadString(Data.Models.Metadata.Header.BiosModeKey).AsMergingFlag() == MergingFlag.None)
                header.Remove(Data.Models.Metadata.Header.BiosModeKey);
            if (header.ReadString(Data.Models.Metadata.Header.RomModeKey).AsMergingFlag() == MergingFlag.None)
                header.Remove(Data.Models.Metadata.Header.RomModeKey);
            if (header.ReadString(Data.Models.Metadata.Header.SampleModeKey).AsMergingFlag() == MergingFlag.None)
                header.Remove(Data.Models.Metadata.Header.SampleModeKey);

            // Convert subheader values
            if (CanOpenSpecified)
                header[Data.Models.Metadata.Header.CanOpenKey] = new Data.Models.OfflineList.CanOpen { Extension = GetStringArrayFieldValue(Data.Models.Metadata.Header.CanOpenKey) };
            if (ImagesSpecified)
                header[Data.Models.Metadata.Header.ImagesKey] = GetFieldValue<Data.Models.OfflineList.Images>(Data.Models.Metadata.Header.ImagesKey);
            if (InfosSpecified)
                header[Data.Models.Metadata.Header.InfosKey] = GetFieldValue<Data.Models.OfflineList.Infos>(Data.Models.Metadata.Header.InfosKey);
            if (NewDatSpecified)
                header[Data.Models.Metadata.Header.NewDatKey] = GetFieldValue<Data.Models.OfflineList.NewDat>(Data.Models.Metadata.Header.NewDatKey);
            if (SearchSpecified)
                header[Data.Models.Metadata.Header.SearchKey] = GetFieldValue<Data.Models.OfflineList.Search>(Data.Models.Metadata.Header.SearchKey);

            return header;
        }

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(ModelBackedItem? other)
        {
            // If other is null
            if (other == null)
                return false;

            // If the type is mismatched
            if (other is not DatHeader otherItem)
                return false;

            // Compare internal models
            return _internal.EqualTo(otherItem._internal);
        }

        /// <inheritdoc/>
        public override bool Equals(ModelBackedItem<Data.Models.Metadata.Header>? other)
        {
            // If other is null
            if (other == null)
                return false;

            // If the type is mismatched
            if (other is not DatHeader otherItem)
                return false;

            // Compare internal models
            return _internal.EqualTo(otherItem._internal);
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Runs a filter and determines if it passes or not
        /// </summary>
        /// <param name="filterRunner">Filter runner to use for checking</param>
        /// <returns>True if the Machine passes the filter, false otherwise</returns>
        public bool PassesFilter(FilterRunner filterRunner) => filterRunner.Run(_internal);

        #endregion
    }
}
