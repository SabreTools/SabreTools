using SabreTools.Library.Data;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents generic archive files to be included in a set
    /// </summary>
    public class Archive : DatItem
    {
        #region Constructors

        /// <summary>
        /// Create a default, empty Archive object
        /// </summary>
        public Archive()
        {
            this.Name = string.Empty;
            this.ItemType = ItemType.Archive;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new Archive()
            {
                Name = this.Name,
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Supported = this.Supported,
                Publisher = this.Publisher,
                Category = this.Category,
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

                IndexId = this.IndexId,
                IndexSource = this.IndexSource,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            // If we don't have an archive, return false
            if (this.ItemType != other.ItemType)
                return false;

            // Otherwise, treat it as an archive
            Archive newOther = other as Archive;

            // If the archive information matches
            return (this.Name == newOther.Name);
        }

        #endregion
    }
}
