using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a blank set from an input DAT
    /// </summary>
    [JsonObject("blank"), XmlRoot("blank")]
    public sealed class Blank : DatItem
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Blank;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Blank object
        /// </summary>
        public Blank()
        {
            SetFieldValue<ItemType>(Data.Models.Metadata.DatItem.TypeKey, ItemType);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            var blank = new Blank();
            blank.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine());
            blank.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            blank.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));
            blank.SetFieldValue<string?>(Data.Models.Metadata.DatItem.TypeKey, GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue());

            return blank;
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
            if (other is not DatItem otherItem)
                return false;

            // Compare internal models
            return Equals(otherItem);
        }

        /// <inheritdoc/>
        public override bool Equals(ModelBackedItem<Data.Models.Metadata.DatItem>? other)
        {
            // If other is null
            if (other == null)
                return false;

            // If the type is mismatched
            if (other is not DatItem otherItem)
                return false;

            // Compare internal models
            return Equals(otherItem);
        }

        /// <inheritdoc/>
        public override bool Equals(DatItem? other)
        {
            // If we don't have a blank, return false
            if (GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey) != other?.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey))
                return false;

            // Otherwise, treat it as a Blank
            Blank? newOther = other as Blank;

            // If the machine information matches
            return GetMachine() == newOther!.GetMachine();
        }

        #endregion
    }
}
