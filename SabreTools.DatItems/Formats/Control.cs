using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents control for an input
    /// </summary>
    [JsonObject("control"), XmlRoot("control")]
    public sealed class Control : DatItem<Models.Metadata.Control>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Control;

        #endregion

        #region Constructors

        public Control() : base() { }

        public Control(Models.Metadata.Control item) : base(item)
        {
            // Process flag values
            if (GetInt64FieldValue(Models.Metadata.Control.ButtonsKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.ButtonsKey, GetInt64FieldValue(Models.Metadata.Control.ButtonsKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Control.KeyDeltaKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.KeyDeltaKey, GetInt64FieldValue(Models.Metadata.Control.KeyDeltaKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Control.MaximumKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.MaximumKey, GetInt64FieldValue(Models.Metadata.Control.MaximumKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Control.MinimumKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.MinimumKey, GetInt64FieldValue(Models.Metadata.Control.MinimumKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Control.PlayerKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.PlayerKey, GetInt64FieldValue(Models.Metadata.Control.PlayerKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Control.ReqButtonsKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.ReqButtonsKey, GetInt64FieldValue(Models.Metadata.Control.ReqButtonsKey).ToString());
            if (GetBoolFieldValue(Models.Metadata.Control.ReverseKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.ReverseKey, GetBoolFieldValue(Models.Metadata.Control.ReverseKey).FromYesNo());
            if (GetInt64FieldValue(Models.Metadata.Control.SensitivityKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.SensitivityKey, GetInt64FieldValue(Models.Metadata.Control.SensitivityKey).ToString());
            if (GetStringFieldValue(Models.Metadata.Control.ControlTypeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Control.ControlTypeKey, GetStringFieldValue(Models.Metadata.Control.ControlTypeKey).AsControlType().AsStringValue());
        }

        public Control(Models.Metadata.Control item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
