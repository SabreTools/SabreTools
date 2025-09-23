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
    public sealed class Input : DatItem<Models.Metadata.Input>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Input;

        [JsonIgnore]
        public bool ControlsSpecified
        {
            get
            {
                var controls = GetFieldValue<Control[]?>(Models.Metadata.Input.ControlKey);
                return controls != null && controls.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Input() : base() { }

        public Input(Models.Metadata.Input item) : base(item)
        {
            // Process flag values
            if (GetInt64FieldValue(Models.Metadata.Input.ButtonsKey) != null)
                SetFieldValue<string?>(Models.Metadata.Input.ButtonsKey, GetInt64FieldValue(Models.Metadata.Input.ButtonsKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Input.CoinsKey) != null)
                SetFieldValue<string?>(Models.Metadata.Input.CoinsKey, GetInt64FieldValue(Models.Metadata.Input.CoinsKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Input.PlayersKey) != null)
                SetFieldValue<string?>(Models.Metadata.Input.PlayersKey, GetInt64FieldValue(Models.Metadata.Input.PlayersKey).ToString());
            if (GetBoolFieldValue(Models.Metadata.Input.ServiceKey) != null)
                SetFieldValue<string?>(Models.Metadata.Input.ServiceKey, GetBoolFieldValue(Models.Metadata.Input.ServiceKey).FromYesNo());
            if (GetBoolFieldValue(Models.Metadata.Input.TiltKey) != null)
                SetFieldValue<string?>(Models.Metadata.Input.TiltKey, GetBoolFieldValue(Models.Metadata.Input.TiltKey).FromYesNo());

            // Handle subitems
            var controls = item.ReadItemArray<Models.Metadata.Control>(Models.Metadata.Input.ControlKey);
            if (controls != null)
            {
                Control[] controlItems = Array.ConvertAll(controls, control => new Control(control));
                SetFieldValue<Control[]?>(Models.Metadata.Input.ControlKey, controlItems);
            }
        }

        public Input(Models.Metadata.Input item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Models.Metadata.Input GetInternalClone()
        {
            var inputItem = base.GetInternalClone();

            var controls = GetFieldValue<Control[]?>(Models.Metadata.Input.ControlKey);
            if (controls != null)
            {
                Models.Metadata.Control[] controlItems = Array.ConvertAll(controls, control => control.GetInternalClone());
                inputItem[Models.Metadata.Input.ControlKey] = controlItems;
            }

            return inputItem;
        }

        #endregion
    }
}
