using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which Slot(s) is associated with a set
    /// </summary>
    [JsonObject("slot"), XmlRoot("slot")]
    public sealed class Slot : DatItem<Data.Models.Metadata.Slot>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Slot;

        [JsonIgnore]
        public bool SlotOptionsSpecified
        {
            get
            {
                var slotOptions = GetFieldValue<SlotOption[]?>(Data.Models.Metadata.Slot.SlotOptionKey);
                return slotOptions != null && slotOptions.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Slot() : base() { }

        public Slot(Data.Models.Metadata.Slot item) : base(item)
        {
            // Handle subitems
            var slotOptions = item.ReadItemArray<Data.Models.Metadata.SlotOption>(Data.Models.Metadata.Slot.SlotOptionKey);
            if (slotOptions != null)
            {
                SlotOption[] slotOptionItems = Array.ConvertAll(slotOptions, slotOption => new SlotOption(slotOption));
                SetFieldValue<SlotOption[]?>(Data.Models.Metadata.Slot.SlotOptionKey, slotOptionItems);
            }
        }

        public Slot(Data.Models.Metadata.Slot item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Data.Models.Metadata.Slot GetInternalClone()
        {
            var slotItem = base.GetInternalClone();

            var slotOptions = GetFieldValue<SlotOption[]?>(Data.Models.Metadata.Slot.SlotOptionKey);
            if (slotOptions != null)
            {
                Data.Models.Metadata.SlotOption[] slotOptionItems = Array.ConvertAll(slotOptions, slotOption => slotOption.GetInternalClone());
                slotItem[Data.Models.Metadata.Slot.SlotOptionKey] = slotOptionItems;
            }

            return slotItem;
        }

        #endregion
    }
}
