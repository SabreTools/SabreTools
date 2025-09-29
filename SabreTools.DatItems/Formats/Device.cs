using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single device on the machine
    /// </summary>
    [JsonObject("device"), XmlRoot("device")]
    public sealed class Device : DatItem<Data.Models.Metadata.Device>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Device;

        [JsonIgnore]
        public bool InstancesSpecified
        {
            get
            {
                var instances = GetFieldValue<Instance[]?>(Data.Models.Metadata.Device.InstanceKey);
                return instances != null && instances.Length > 0;
            }
        }

        [JsonIgnore]
        public bool ExtensionsSpecified
        {
            get
            {
                var extensions = GetFieldValue<Extension[]?>(Data.Models.Metadata.Device.ExtensionKey);
                return extensions != null && extensions.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Device() : base() { }

        public Device(Data.Models.Metadata.Device item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Device.MandatoryKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Device.MandatoryKey, GetBoolFieldValue(Data.Models.Metadata.Device.MandatoryKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Device.DeviceTypeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Device.DeviceTypeKey, GetStringFieldValue(Data.Models.Metadata.Device.DeviceTypeKey).AsDeviceType().AsStringValue());

            // Handle subitems
            var instance = item.Read<Data.Models.Metadata.Instance>(Data.Models.Metadata.Device.InstanceKey);
            if (instance != null)
                SetFieldValue<Instance?>(Data.Models.Metadata.Device.InstanceKey, new Instance(instance));

            var extensions = item.ReadItemArray<Data.Models.Metadata.Extension>(Data.Models.Metadata.Device.ExtensionKey);
            if (extensions != null)
            {
                Extension[] extensionItems = Array.ConvertAll(extensions, extension => new Extension(extension));
                SetFieldValue<Extension[]?>(Data.Models.Metadata.Device.ExtensionKey, extensionItems);
            }
        }

        public Device(Data.Models.Metadata.Device item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Data.Models.Metadata.Device GetInternalClone()
        {
            var deviceItem = base.GetInternalClone();

            var instance = GetFieldValue<Instance?>(Data.Models.Metadata.Device.InstanceKey);
            if (instance != null)
                deviceItem[Data.Models.Metadata.Device.InstanceKey] = instance.GetInternalClone();

            var extensions = GetFieldValue<Extension[]?>(Data.Models.Metadata.Device.ExtensionKey);
            if (extensions != null)
            {
                Data.Models.Metadata.Extension[] extensionItems = Array.ConvertAll(extensions, extension => extension.GetInternalClone());
                deviceItem[Data.Models.Metadata.Device.ExtensionKey] = extensionItems;
            }

            return deviceItem;
        }

        #endregion
    }
}
