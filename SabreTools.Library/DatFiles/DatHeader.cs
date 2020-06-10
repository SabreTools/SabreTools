using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents all possible DAT header information
    /// </summary>
    public class DatHeader : ICloneable
    {
        #region Publicly facing variables

        #region Data common to most DAT types

        /// <summary>
        /// External name of the DAT
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Internal name of the DAT
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DAT description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Root directory for the files; currently TruRip/EmuARC-exclusive
        /// </summary>
        public string RootDir { get; set; }

        /// <summary>
        /// General category of items found in the DAT
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Version of the DAT
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Creation or modification date
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// List of authors who contributed to the DAT
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Email address for DAT author(s)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Author or distribution homepage name
        /// </summary>
        public string Homepage { get; set; }

        /// <summary>
        /// Author or distribution URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Any comment that does not already fit an existing field
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Header skipper to be used when loading the DAT
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Classification of the DAT. Generally only used for SuperDAT
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Force a merging style when loaded
        /// </summary>
        public ForceMerging ForceMerging { get; set; }

        /// <summary>
        /// Force nodump handling when loaded
        /// </summary>
        public ForceNodump ForceNodump { get; set; }

        /// <summary>
        /// Force output packing when loaded
        /// </summary>
        public ForcePacking ForcePacking { get; set; }

        /// <summary>
        /// Read or write format
        /// </summary>
        public DatFormat DatFormat { get; set; }

        /// <summary>
        /// List of fields in machine and items to exclude from writing
        /// </summary>
        public bool[] ExcludeFields { get; set; } = new bool[Enum.GetNames(typeof(Field)).Length];

        /// <summary>
        /// Enable "One Rom, One Region (1G1R)" mode
        /// </summary>
        public bool OneRom { get; set; }

        /// <summary>
        /// Keep machines that don't contain any items
        /// </summary>
        public bool KeepEmptyGames { get; set; }

        /// <summary>
        /// Remove scene dates from the beginning of machine names
        /// </summary>
        public bool SceneDateStrip { get; set; }

        /// <summary>
        /// Deduplicate items using the given method
        /// </summary>
        public DedupeType DedupeRoms { get; set; }

        /// <summary>
        /// Strip hash types from items
        /// </summary>
        public Hash StripHash { get; private set; }

        #endregion

        #region Write pre-processing

        /// <summary>
        /// Text to prepend to all outputted lines
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Text to append to all outputted lines
        /// </summary>
        public string Postfix { get; set; }

        /// <summary>
        /// Add a new extension to all items
        /// </summary>
        public string AddExtension { get; set; }

        /// <summary>
        /// Replace all item extensions
        /// </summary>
        public string ReplaceExtension { get; set; }

        /// <summary>
        /// Remove all item extensions
        /// </summary>
        public bool RemoveExtension { get; set; }

        /// <summary>
        /// Romba output mode
        /// </summary>
        public bool Romba { get; set; }

        /// <summary>
        /// Output the machine name
        /// </summary>
        public bool GameName { get; set; }

        /// <summary>
        /// Wrap quotes around the entire line, sans prefix and postfix
        /// </summary>
        public bool Quotes { get; set; }

        #endregion

        #region Data specific to the Miss DAT type

        /// <summary>
        /// Output the item name
        /// </summary>
        public bool UseRomName { get; set; }

        #endregion

        #endregion

        #region Instance Methods

        #region Cloning Methods

        /// <summary>
        /// Clone the current header
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new DatHeader()
            {
                FileName = this.FileName,
                Name = this.Name,
                Description = this.Description,
                RootDir = this.RootDir,
                Category = this.Category,
                Version = this.Version,
                Date = this.Date,
                Author = this.Author,
                Email = this.Email,
                Homepage = this.Homepage,
                Url = this.Url,
                Comment = this.Comment,
                Header = this.Header,
                Type = this.Type,
                ForceMerging = this.ForceMerging,
                ForceNodump = this.ForceNodump,
                ForcePacking = this.ForcePacking,
                DatFormat = this.DatFormat,
                ExcludeFields = this.ExcludeFields,
                OneRom = this.OneRom,
                KeepEmptyGames = this.KeepEmptyGames,
                SceneDateStrip = this.SceneDateStrip,
                DedupeRoms = this.DedupeRoms,
                StripHash = this.StripHash,

                UseRomName = this.UseRomName,
                Prefix = this.Prefix,
                Postfix = this.Postfix,
                Quotes = this.Quotes,
                ReplaceExtension = this.ReplaceExtension,
                AddExtension = this.AddExtension,
                RemoveExtension = this.RemoveExtension,
                GameName = this.GameName,
                Romba = this.Romba,
            };
        }

        #endregion

        #endregion // Instance Methods
    }
}
