using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single port on a machine
    /// </summary>
    [JsonObject("port"), XmlRoot("port")]
    public sealed class Port : DatItem<Models.Metadata.Port>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Port;

        [JsonIgnore]
        public bool AnalogsSpecified
        {
            get
            {
                var analogs = GetFieldValue<Analog[]?>(Models.Metadata.Port.AnalogKey);
                return analogs != null && analogs.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Port() : base() { }

        public Port(Models.Metadata.Port item) : base(item)
        {
            // Handle subitems
            var analogs = item.ReadItemArray<Models.Metadata.Analog>(Models.Metadata.Port.AnalogKey);
            if (analogs != null)
            {
                Analog[] analogItems = Array.ConvertAll(analogs, analog => new Analog(analog));
                SetFieldValue<Analog[]?>(Models.Metadata.Port.AnalogKey, analogItems);
            }
        }

        public Port(Models.Metadata.Port item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Models.Metadata.Port GetInternalClone()
        {
            var portItem = base.GetInternalClone();

            var analogs = GetFieldValue<Analog[]?>(Models.Metadata.Port.AnalogKey);
            if (analogs != null)
            {
                Models.Metadata.Analog[] analogItems = Array.ConvertAll(analogs, analog => analog.GetInternalClone());
                portItem[Models.Metadata.Port.AnalogKey] = analogItems;
            }

            return portItem;
        }

        #endregion
    }
}
