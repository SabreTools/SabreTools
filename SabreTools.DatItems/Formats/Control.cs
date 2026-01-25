using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents control for an input
    /// </summary>
    [JsonObject("control"), XmlRoot("control")]
    public sealed class Control : DatItem<Data.Models.Metadata.Control>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Control;

        #endregion

        #region Constructors

        public Control() : base() { }

        public Control(Data.Models.Metadata.Control item) : base(item)
        {
            // Process flag values
            if (GetInt64FieldValue(Data.Models.Metadata.Control.ButtonsKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.ButtonsKey, GetInt64FieldValue(Data.Models.Metadata.Control.ButtonsKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.KeyDeltaKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.KeyDeltaKey, GetInt64FieldValue(Data.Models.Metadata.Control.KeyDeltaKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.MaximumKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.MaximumKey, GetInt64FieldValue(Data.Models.Metadata.Control.MaximumKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.MinimumKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.MinimumKey, GetInt64FieldValue(Data.Models.Metadata.Control.MinimumKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.PlayerKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.PlayerKey, GetInt64FieldValue(Data.Models.Metadata.Control.PlayerKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.ReqButtonsKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.ReqButtonsKey, GetInt64FieldValue(Data.Models.Metadata.Control.ReqButtonsKey).ToString());
            if (GetBoolFieldValue(Data.Models.Metadata.Control.ReverseKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.ReverseKey, GetBoolFieldValue(Data.Models.Metadata.Control.ReverseKey).FromYesNo());
            if (GetInt64FieldValue(Data.Models.Metadata.Control.SensitivityKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.SensitivityKey, GetInt64FieldValue(Data.Models.Metadata.Control.SensitivityKey).ToString());
            if (GetStringFieldValue(Data.Models.Metadata.Control.ControlTypeKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Control.ControlTypeKey, GetStringFieldValue(Data.Models.Metadata.Control.ControlTypeKey).AsControlType().AsStringValue());
        }

        public Control(Data.Models.Metadata.Control item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
