using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one ListXML input
    /// </summary>
    [JsonObject("input"), XmlRoot("input")]
    public sealed class Input : DatItem<Data.Models.Metadata.Input>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Input;

        [JsonIgnore]
        public bool ControlsSpecified
        {
            get
            {
                var controls = GetFieldValue<Control[]?>(Data.Models.Metadata.Input.ControlKey);
                return controls != null && controls.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Input() : base() { }

        public Input(Data.Models.Metadata.Input item) : base(item)
        {
            // Process flag values
            if (GetInt64FieldValue(Data.Models.Metadata.Input.ButtonsKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Input.ButtonsKey, GetInt64FieldValue(Data.Models.Metadata.Input.ButtonsKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Input.CoinsKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Input.CoinsKey, GetInt64FieldValue(Data.Models.Metadata.Input.CoinsKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Input.PlayersKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Input.PlayersKey, GetInt64FieldValue(Data.Models.Metadata.Input.PlayersKey).ToString());
            if (GetBoolFieldValue(Data.Models.Metadata.Input.ServiceKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Input.ServiceKey, GetBoolFieldValue(Data.Models.Metadata.Input.ServiceKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Input.TiltKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Input.TiltKey, GetBoolFieldValue(Data.Models.Metadata.Input.TiltKey).FromYesNo());

            // Handle subitems
            var controls = item.ReadItemArray<Data.Models.Metadata.Control>(Data.Models.Metadata.Input.ControlKey);
            if (controls != null)
            {
                Control[] controlItems = Array.ConvertAll(controls, control => new Control(control));
                SetFieldValue<Control[]?>(Data.Models.Metadata.Input.ControlKey, controlItems);
            }
        }

        public Input(Data.Models.Metadata.Input item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Data.Models.Metadata.Input GetInternalClone()
        {
            var inputItem = base.GetInternalClone();

            var controls = GetFieldValue<Control[]?>(Data.Models.Metadata.Input.ControlKey);
            if (controls != null)
            {
                Data.Models.Metadata.Control[] controlItems = Array.ConvertAll(controls, control => control.GetInternalClone());
                inputItem[Data.Models.Metadata.Input.ControlKey] = controlItems;
            }

            return inputItem;
        }

        #endregion
    }
}
