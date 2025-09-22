using System;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Represents various modifiers that can be applied to a DAT
    /// </summary>
    public sealed class DatModifiers : ICloneable
    {
        #region Fields

        /// <summary>
        /// Text to prepend to all outputted lines
        /// </summary>
        public string? Prefix { get; set; } = null;

        /// <summary>
        /// Text to append to all outputted lines
        /// </summary>
        public string? Postfix { get; set; } = null;

        /// <summary>
        /// Add a new extension to all items
        /// </summary>
        public string? AddExtension { get; set; } = null;

        /// <summary>
        /// Remove all item extensions
        /// </summary>
        public bool RemoveExtension { get; set; } = false;

        /// <summary>
        /// Replace all item extensions
        /// </summary>
        public string? ReplaceExtension { get; set; } = null;

        /// <summary>
        /// Output the machine name before the item name
        /// </summary>
        public bool GameName { get; set; } = false;

        /// <summary>
        /// Wrap quotes around the entire line, sans prefix and postfix
        /// </summary>
        public bool Quotes { get; set; } = false;

        /// <summary>
        /// Use the item name instead of machine name on output
        /// </summary>
        public bool UseRomName { get; set; } = false;

        /// <summary>
        /// Input depot information
        /// </summary>
        public DepotInformation? InputDepot { get; set; } = null;

        /// <summary>
        /// Output depot information
        /// </summary>
        public DepotInformation? OutputDepot { get; set; } = null;

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Clone the current modifiers
        /// </summary>
        public object Clone()
        {
            return new DatModifiers
            {
                Prefix = this.Prefix,
                Postfix = this.Postfix,
                AddExtension = this.AddExtension,
                RemoveExtension = this.RemoveExtension,
                ReplaceExtension = this.ReplaceExtension,
                GameName = this.GameName,
                Quotes = this.Quotes,
                UseRomName = this.UseRomName,
                InputDepot = (DepotInformation?)this.InputDepot?.Clone(),
                OutputDepot = (DepotInformation?)this.OutputDepot?.Clone(),
            };
        }

        #endregion
    }
}
