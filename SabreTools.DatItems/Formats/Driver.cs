using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the a driver of the machine
    /// </summary>
    [JsonObject("driver"), XmlRoot("driver")]
    public sealed class Driver : DatItem<Models.Metadata.Driver>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Driver;

        #endregion

        #region Constructors

        public Driver() : base() { }

        public Driver(Models.Metadata.Driver item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.Driver.CocktailKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.CocktailKey, GetStringFieldValue(Models.Metadata.Driver.CocktailKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Driver.ColorKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.ColorKey, GetStringFieldValue(Models.Metadata.Driver.ColorKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Driver.EmulationKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.EmulationKey, GetStringFieldValue(Models.Metadata.Driver.EmulationKey).AsSupportStatus().AsStringValue());
            if (GetBoolFieldValue(Models.Metadata.Driver.IncompleteKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.IncompleteKey, GetBoolFieldValue(Models.Metadata.Driver.IncompleteKey).FromYesNo());
            if (GetBoolFieldValue(Models.Metadata.Driver.NoSoundHardwareKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.NoSoundHardwareKey, GetBoolFieldValue(Models.Metadata.Driver.NoSoundHardwareKey).FromYesNo());
            if (GetInt64FieldValue(Models.Metadata.Driver.PaletteSizeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.PaletteSizeKey, GetInt64FieldValue(Models.Metadata.Driver.PaletteSizeKey).ToString());
            if (GetBoolFieldValue(Models.Metadata.Driver.RequiresArtworkKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.RequiresArtworkKey, GetBoolFieldValue(Models.Metadata.Driver.RequiresArtworkKey).FromYesNo());
            if (GetStringFieldValue(Models.Metadata.Driver.SaveStateKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.SaveStateKey, GetStringFieldValue(Models.Metadata.Driver.SaveStateKey).AsSupported().AsStringValue(useSecond: true));
            if (GetStringFieldValue(Models.Metadata.Driver.SoundKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.SoundKey, GetStringFieldValue(Models.Metadata.Driver.SoundKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Driver.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.StatusKey, GetStringFieldValue(Models.Metadata.Driver.StatusKey).AsSupportStatus().AsStringValue());
            if (GetBoolFieldValue(Models.Metadata.Driver.UnofficialKey) != null)
                SetFieldValue<string?>(Models.Metadata.Driver.UnofficialKey, GetBoolFieldValue(Models.Metadata.Driver.UnofficialKey).FromYesNo());
        }

        public Driver(Models.Metadata.Driver item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
