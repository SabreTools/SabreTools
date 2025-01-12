﻿using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which Configuration(s) is associated with a set
    /// </summary>
    [JsonObject("configuration"), XmlRoot("configuration")]
    public sealed class Configuration : DatItem<Models.Metadata.Configuration>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Configuration;

        /// <inheritdoc>/>
        protected override string? NameKey => Models.Metadata.Configuration.NameKey;

        [JsonIgnore]
        public bool ConditionsSpecified
        {
            get
            {
                var conditions = GetFieldValue<Condition[]?>(Models.Metadata.Configuration.ConditionKey);
                return conditions != null && conditions.Length > 0;
            }
        }

        [JsonIgnore]
        public bool LocationsSpecified
        {
            get
            {
                var locations = GetFieldValue<ConfLocation[]?>(Models.Metadata.Configuration.ConfLocationKey);
                return locations != null && locations.Length > 0;
            }
        }

        [JsonIgnore]
        public bool SettingsSpecified
        {
            get
            {
                var settings = GetFieldValue<ConfSetting[]?>(Models.Metadata.Configuration.ConfSettingKey);
                return settings != null && settings.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Configuration() : base() { }
        public Configuration(Models.Metadata.Configuration item) : base(item)
        {
            // Handle subitems
            var condition = item.Read<Models.Metadata.Condition>(Models.Metadata.Configuration.ConditionKey);
            if (condition != null)
                SetFieldValue<Condition?>(Models.Metadata.Configuration.ConditionKey, new Condition(condition));

            var confLocations = item.ReadItemArray<Models.Metadata.ConfLocation>(Models.Metadata.Configuration.ConfLocationKey);
            if (confLocations != null)
            {
                ConfLocation[] confLocationItems = Array.ConvertAll(confLocations, confLocation => new ConfLocation(confLocation));
                SetFieldValue<ConfLocation[]?>(Models.Metadata.Configuration.ConfLocationKey, confLocationItems);
            }

            var confSettings = item.ReadItemArray<Models.Metadata.ConfSetting>(Models.Metadata.Configuration.ConfSettingKey);
            if (confSettings != null)
            {
                ConfSetting[] confSettingItems = Array.ConvertAll(confSettings, confSetting => new ConfSetting(confSetting));
                SetFieldValue<ConfSetting[]?>(Models.Metadata.Configuration.ConfSettingKey, confSettingItems);
            }
        }

        #endregion
    }
}
