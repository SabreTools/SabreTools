using SabreTools.Library.Data;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents release information about a set
    /// </summary>
    public class Release : DatItem
    {
        #region Publicly facing variables

        /// <summary>
        /// Release region(s)
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Release language(s)
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Date of release
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Default release, if applicable
        /// </summary>
        public bool? Default { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Release object
        /// </summary>
        public Release()
        {
            this.Name = string.Empty;
            this.ItemType = ItemType.Release;
            this.Region = string.Empty;
            this.Language = string.Empty;
            this.Date = string.Empty;
            this.Default = null;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new Release()
            {
                Name = this.Name,
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Supported = this.Supported,
                Publisher = this.Publisher,
                Infos = this.Infos,
                PartName = this.PartName,
                PartInterface = this.PartInterface,
                Features = this.Features,
                AreaName = this.AreaName,
                AreaSize = this.AreaSize,

                MachineName = this.MachineName,
                Comment = this.Comment,
                MachineDescription = this.MachineDescription,
                Year = this.Year,
                Manufacturer = this.Manufacturer,
                RomOf = this.RomOf,
                CloneOf = this.CloneOf,
                SampleOf = this.SampleOf,
                SourceFile = this.SourceFile,
                Runnable = this.Runnable,
                Board = this.Board,
                RebuildTo = this.RebuildTo,
                Devices = this.Devices,
                MachineType = this.MachineType,

                SystemID = this.SystemID,
                System = this.System,
                SourceID = this.SourceID,
                Source = this.Source,

                Region = this.Region,
                Language = this.Language,
                Date = this.Date,
                Default = this.Default,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            // If we don't have a release return false
            if (this.ItemType != other.ItemType)
                return false;

            // Otherwise, treat it as a Release
            Release newOther = other as Release;

            // If the archive information matches
            return (this.Name == newOther.Name
                && this.Region == newOther.Region
                && this.Language == newOther.Language
                && this.Date == newOther.Date
                && this.Default == newOther.Default);
        }

        #endregion
    }
}
