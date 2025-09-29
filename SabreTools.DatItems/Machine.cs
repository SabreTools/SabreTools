using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Represents the information specific to a set/game/machine
    /// </summary>
    [JsonObject("machine"), XmlRoot("machine")]
    public sealed class Machine : ModelBackedItem<Data.Models.Metadata.Machine>, ICloneable, IEquatable<Machine>
    {
        #region Constructors

        public Machine()
        {
            _internal = new Data.Models.Metadata.Machine();
        }

        public Machine(Data.Models.Metadata.Machine machine)
        {
            // Get all fields to automatically copy without processing
            var nonItemFields = TypeHelper.GetConstants(typeof(Data.Models.Metadata.Machine));
            if (nonItemFields == null)
                return;

            // Populate the internal machine from non-filter fields
            _internal = new Data.Models.Metadata.Machine();
            foreach (string fieldName in nonItemFields)
            {
                if (machine.ContainsKey(fieldName))
                    _internal[fieldName] = machine[fieldName];
            }

            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.Machine.Im1CRCKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.Im1CRCKey, TextHelper.NormalizeCRC32(GetStringFieldValue(Data.Models.Metadata.Machine.Im1CRCKey)));
            if (GetStringFieldValue(Data.Models.Metadata.Machine.Im2CRCKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.Im2CRCKey, TextHelper.NormalizeCRC32(GetStringFieldValue(Data.Models.Metadata.Machine.Im2CRCKey)));
            if (GetBoolFieldValue(Data.Models.Metadata.Machine.IsBiosKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.IsBiosKey, GetBoolFieldValue(Data.Models.Metadata.Machine.IsBiosKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Machine.IsDeviceKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.IsDeviceKey, GetBoolFieldValue(Data.Models.Metadata.Machine.IsDeviceKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Machine.IsMechanicalKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.IsMechanicalKey, GetBoolFieldValue(Data.Models.Metadata.Machine.IsMechanicalKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Machine.SupportedKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Machine.SupportedKey, GetStringFieldValue(Data.Models.Metadata.Machine.SupportedKey).AsSupported().AsStringValue());

            // Handle Trurip object, if it exists
            if (machine.ContainsKey(Data.Models.Metadata.Machine.TruripKey))
            {
                var truripItem = machine.Read<Data.Models.Logiqx.Trurip>(Data.Models.Metadata.Machine.TruripKey);
                if (truripItem != null)
                    SetFieldValue<Trurip>(Data.Models.Metadata.Machine.TruripKey, new Trurip(truripItem));
            }
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Gets the name to use for a Machine
        /// </summary>
        /// <returns>Name if available, null otherwise</returns>
        public string? GetName() => _internal.GetName();

        /// <summary>
        /// Sets the name to use for a Machine
        /// </summary>
        /// <param name="name">Name to set for the item</param>
        public void SetName(string? name) => _internal.SetName(name);

        #endregion

        #region Cloning methods

        /// <summary>
        /// Create a clone of the current machine
        /// </summary>
        /// <returns>New machine with the same values as the current one</returns>
        public object Clone()
        {
            return new Machine()
            {
                _internal = _internal.Clone() as Data.Models.Metadata.Machine ?? [],
            };
        }

        /// <summary>
        /// Get a clone of the current internal model
        /// </summary>
        public Data.Models.Metadata.Machine GetInternalClone() => (_internal.Clone() as Data.Models.Metadata.Machine)!;

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(ModelBackedItem? other)
        {
            // If other is null
            if (other == null)
                return false;

            // If the type is mismatched
            if (other is not Machine otherItem)
                return false;

            // Compare internal models
            return _internal.EqualTo(otherItem._internal);
        }

        /// <inheritdoc/>
        public override bool Equals(ModelBackedItem<Data.Models.Metadata.Machine>? other)
        {
            // If other is null
            if (other == null)
                return false;

            // If the type is mismatched
            if (other is not Machine otherItem)
                return false;

            // Compare internal models
            return _internal.EqualTo(otherItem._internal);
        }

        /// <inheritdoc/>
        public bool Equals(Machine? other)
        {
            // If other is null
            if (other == null)
                return false;

            // Compare internal models
            return _internal.EqualTo(other._internal);
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
