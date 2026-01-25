using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the a driver of the machine
    /// </summary>
    [JsonObject("driver"), XmlRoot("driver")]
    public sealed class Driver : DatItem<Data.Models.Metadata.Driver>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Driver;

        #endregion

        #region Constructors

        public Driver() : base() { }

        public Driver(Data.Models.Metadata.Driver item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.Driver.CocktailKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.CocktailKey, GetStringFieldValue(Data.Models.Metadata.Driver.CocktailKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Driver.ColorKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.ColorKey, GetStringFieldValue(Data.Models.Metadata.Driver.ColorKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Driver.EmulationKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.EmulationKey, GetStringFieldValue(Data.Models.Metadata.Driver.EmulationKey).AsSupportStatus().AsStringValue());
            if (GetBoolFieldValue(Data.Models.Metadata.Driver.IncompleteKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.IncompleteKey, GetBoolFieldValue(Data.Models.Metadata.Driver.IncompleteKey).FromYesNo());
            if (GetBoolFieldValue(Data.Models.Metadata.Driver.NoSoundHardwareKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.NoSoundHardwareKey, GetBoolFieldValue(Data.Models.Metadata.Driver.NoSoundHardwareKey).FromYesNo());
            if (GetInt64FieldValue(Data.Models.Metadata.Driver.PaletteSizeKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.PaletteSizeKey, GetInt64FieldValue(Data.Models.Metadata.Driver.PaletteSizeKey).ToString());
            if (GetBoolFieldValue(Data.Models.Metadata.Driver.RequiresArtworkKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.RequiresArtworkKey, GetBoolFieldValue(Data.Models.Metadata.Driver.RequiresArtworkKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Driver.SaveStateKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.SaveStateKey, GetStringFieldValue(Data.Models.Metadata.Driver.SaveStateKey).AsSupported().AsStringValue(useSecond: true));
            if (GetStringFieldValue(Data.Models.Metadata.Driver.SoundKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.SoundKey, GetStringFieldValue(Data.Models.Metadata.Driver.SoundKey).AsSupportStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Driver.StatusKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.StatusKey, GetStringFieldValue(Data.Models.Metadata.Driver.StatusKey).AsSupportStatus().AsStringValue());
            if (GetBoolFieldValue(Data.Models.Metadata.Driver.UnofficialKey) is not null)
                SetFieldValue<string?>(Data.Models.Metadata.Driver.UnofficialKey, GetBoolFieldValue(Data.Models.Metadata.Driver.UnofficialKey).FromYesNo());
        }

        public Driver(Data.Models.Metadata.Driver item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
