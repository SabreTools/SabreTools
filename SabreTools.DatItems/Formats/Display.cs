using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one machine display
    /// </summary>
    [JsonObject("display"), XmlRoot("display")]
    public sealed class Display : DatItem<Models.Metadata.Display>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Display;

        #endregion

        #region Constructors

        public Display() : base() { }

        public Display(Models.Metadata.Display item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.Display.FlipXKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.FlipXKey, GetBoolFieldValue(Models.Metadata.Display.FlipXKey).FromYesNo());
            if (GetInt64FieldValue(Models.Metadata.Display.HBEndKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.HBEndKey, GetInt64FieldValue(Models.Metadata.Display.HBEndKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.HBStartKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.HBStartKey, GetInt64FieldValue(Models.Metadata.Display.HBStartKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.HeightKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.HeightKey, GetInt64FieldValue(Models.Metadata.Display.HeightKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.HTotalKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.HTotalKey, GetInt64FieldValue(Models.Metadata.Display.HTotalKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.PixClockKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.PixClockKey, GetInt64FieldValue(Models.Metadata.Display.PixClockKey).ToString());
            if (GetDoubleFieldValue(Models.Metadata.Display.RefreshKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.RefreshKey, GetDoubleFieldValue(Models.Metadata.Display.RefreshKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.RotateKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.RotateKey, GetInt64FieldValue(Models.Metadata.Display.RotateKey).ToString());
            if (GetStringFieldValue(Models.Metadata.Display.DisplayTypeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.DisplayTypeKey, GetStringFieldValue(Models.Metadata.Display.DisplayTypeKey).AsDisplayType().AsStringValue());
            if (GetInt64FieldValue(Models.Metadata.Display.VBEndKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.VBEndKey, GetInt64FieldValue(Models.Metadata.Display.VBEndKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.VBStartKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.VBStartKey, GetInt64FieldValue(Models.Metadata.Display.VBStartKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.VTotalKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.VTotalKey, GetInt64FieldValue(Models.Metadata.Display.VTotalKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Display.WidthKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.WidthKey, GetInt64FieldValue(Models.Metadata.Display.WidthKey).ToString());
        }

        public Display(Models.Metadata.Display item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        public Display(Models.Metadata.Video item) : base()
        {
            SetFieldValue<long?>(Models.Metadata.Video.AspectXKey, NumberHelper.ConvertToInt64(item.ReadString(Models.Metadata.Video.AspectXKey)));
            SetFieldValue<long?>(Models.Metadata.Video.AspectYKey, NumberHelper.ConvertToInt64(item.ReadString(Models.Metadata.Video.AspectYKey)));
            SetFieldValue<string?>(Models.Metadata.Display.DisplayTypeKey, item.ReadString(Models.Metadata.Video.ScreenKey).AsDisplayType().AsStringValue());
            SetFieldValue<long?>(Models.Metadata.Display.HeightKey, NumberHelper.ConvertToInt64(item.ReadString(Models.Metadata.Video.HeightKey)));
            SetFieldValue<double?>(Models.Metadata.Display.RefreshKey, NumberHelper.ConvertToDouble(item.ReadString(Models.Metadata.Video.RefreshKey)));
            SetFieldValue<long?>(Models.Metadata.Display.WidthKey, NumberHelper.ConvertToInt64(item.ReadString(Models.Metadata.Video.WidthKey)));

            switch (item.ReadString(Models.Metadata.Video.OrientationKey))
            {
                case "horizontal":
                    SetFieldValue<long?>(Models.Metadata.Display.RotateKey, 0);
                    break;
                case "vertical":
                    SetFieldValue<long?>(Models.Metadata.Display.RotateKey, 90);
                    break;
            }

            // Process flag values
            if (GetInt64FieldValue(Models.Metadata.Video.AspectXKey) != null)
                SetFieldValue<string?>(Models.Metadata.Video.AspectXKey, GetInt64FieldValue(Models.Metadata.Video.AspectXKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Video.AspectYKey) != null)
                SetFieldValue<string?>(Models.Metadata.Video.AspectYKey, GetInt64FieldValue(Models.Metadata.Video.AspectYKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.Video.HeightKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.HeightKey, GetInt64FieldValue(Models.Metadata.Video.HeightKey).ToString());
            if (GetDoubleFieldValue(Models.Metadata.Video.RefreshKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.RefreshKey, GetDoubleFieldValue(Models.Metadata.Video.RefreshKey).ToString());
            if (GetStringFieldValue(Models.Metadata.Video.ScreenKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.DisplayTypeKey, GetStringFieldValue(Models.Metadata.Video.ScreenKey).AsDisplayType().AsStringValue());
            if (GetInt64FieldValue(Models.Metadata.Video.WidthKey) != null)
                SetFieldValue<string?>(Models.Metadata.Display.WidthKey, GetInt64FieldValue(Models.Metadata.Video.WidthKey).ToString());
        }

        public Display(Models.Metadata.Video item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
