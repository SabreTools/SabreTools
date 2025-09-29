using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one machine display
    /// </summary>
    [JsonObject("display"), XmlRoot("display")]
    public sealed class Display : DatItem<Data.Models.Metadata.Display>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Display;

        #endregion

        #region Constructors

        public Display() : base() { }

        public Display(Data.Models.Metadata.Display item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Display.FlipXKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.FlipXKey, GetBoolFieldValue(Data.Models.Metadata.Display.FlipXKey).FromYesNo());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.HBEndKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.HBEndKey, GetInt64FieldValue(Data.Models.Metadata.Display.HBEndKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.HBStartKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.HBStartKey, GetInt64FieldValue(Data.Models.Metadata.Display.HBStartKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.HeightKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.HeightKey, GetInt64FieldValue(Data.Models.Metadata.Display.HeightKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.HTotalKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.HTotalKey, GetInt64FieldValue(Data.Models.Metadata.Display.HTotalKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.PixClockKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.PixClockKey, GetInt64FieldValue(Data.Models.Metadata.Display.PixClockKey).ToString());
            if (GetDoubleFieldValue(Data.Models.Metadata.Display.RefreshKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.RefreshKey, GetDoubleFieldValue(Data.Models.Metadata.Display.RefreshKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.RotateKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.RotateKey, GetInt64FieldValue(Data.Models.Metadata.Display.RotateKey).ToString());
            if (GetStringFieldValue(Data.Models.Metadata.Display.DisplayTypeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.DisplayTypeKey, GetStringFieldValue(Data.Models.Metadata.Display.DisplayTypeKey).AsDisplayType().AsStringValue());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.VBEndKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.VBEndKey, GetInt64FieldValue(Data.Models.Metadata.Display.VBEndKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.VBStartKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.VBStartKey, GetInt64FieldValue(Data.Models.Metadata.Display.VBStartKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.VTotalKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.VTotalKey, GetInt64FieldValue(Data.Models.Metadata.Display.VTotalKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Display.WidthKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.WidthKey, GetInt64FieldValue(Data.Models.Metadata.Display.WidthKey).ToString());
        }

        public Display(Data.Models.Metadata.Display item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        public Display(Data.Models.Metadata.Video item) : base()
        {
            SetFieldValue<long?>(Data.Models.Metadata.Video.AspectXKey, NumberHelper.ConvertToInt64(item.ReadString(Data.Models.Metadata.Video.AspectXKey)));
            SetFieldValue<long?>(Data.Models.Metadata.Video.AspectYKey, NumberHelper.ConvertToInt64(item.ReadString(Data.Models.Metadata.Video.AspectYKey)));
            SetFieldValue<string?>(Data.Models.Metadata.Display.DisplayTypeKey, item.ReadString(Data.Models.Metadata.Video.ScreenKey).AsDisplayType().AsStringValue());
            SetFieldValue<long?>(Data.Models.Metadata.Display.HeightKey, NumberHelper.ConvertToInt64(item.ReadString(Data.Models.Metadata.Video.HeightKey)));
            SetFieldValue<double?>(Data.Models.Metadata.Display.RefreshKey, NumberHelper.ConvertToDouble(item.ReadString(Data.Models.Metadata.Video.RefreshKey)));
            SetFieldValue<long?>(Data.Models.Metadata.Display.WidthKey, NumberHelper.ConvertToInt64(item.ReadString(Data.Models.Metadata.Video.WidthKey)));

            switch (item.ReadString(Data.Models.Metadata.Video.OrientationKey))
            {
                case "horizontal":
                    SetFieldValue<long?>(Data.Models.Metadata.Display.RotateKey, 0);
                    break;
                case "vertical":
                    SetFieldValue<long?>(Data.Models.Metadata.Display.RotateKey, 90);
                    break;
            }

            // Process flag values
            if (GetInt64FieldValue(Data.Models.Metadata.Video.AspectXKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Video.AspectXKey, GetInt64FieldValue(Data.Models.Metadata.Video.AspectXKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Video.AspectYKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Video.AspectYKey, GetInt64FieldValue(Data.Models.Metadata.Video.AspectYKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.Video.HeightKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.HeightKey, GetInt64FieldValue(Data.Models.Metadata.Video.HeightKey).ToString());
            if (GetDoubleFieldValue(Data.Models.Metadata.Video.RefreshKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.RefreshKey, GetDoubleFieldValue(Data.Models.Metadata.Video.RefreshKey).ToString());
            if (GetStringFieldValue(Data.Models.Metadata.Video.ScreenKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.DisplayTypeKey, GetStringFieldValue(Data.Models.Metadata.Video.ScreenKey).AsDisplayType().AsStringValue());
            if (GetInt64FieldValue(Data.Models.Metadata.Video.WidthKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Display.WidthKey, GetInt64FieldValue(Data.Models.Metadata.Video.WidthKey).ToString());
        }

        public Display(Data.Models.Metadata.Video item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
