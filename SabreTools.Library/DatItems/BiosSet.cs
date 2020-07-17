using SabreTools.Library.Data;
using Newtonsoft.Json;

namespace SabreTools.Library.DatItems
{
    /// <summary>
    /// Represents which BIOS(es) is associated with a set
    /// </summary>
    public class BiosSet : DatItem
    {
        #region Publicly facing variables

        /// <summary>
        /// Description of the BIOS
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Determine whether the BIOS is default
        /// </summary>
        [JsonProperty("default")]
        public bool? Default { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Sample object
        /// </summary>
        public BiosSet()
        {
            this.Name = string.Empty;
            this.ItemType = ItemType.BiosSet;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new BiosSet()
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

                Description = this.Description,
                Default = this.Default,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            // If we don't have a biosset, return false
            if (this.ItemType != other.ItemType)
                return false;

            // Otherwise, treat it as a biosset
            BiosSet newOther = other as BiosSet;

            // If the archive information matches
            return (this.Name == newOther.Name && this.Description == newOther.Description && this.Default == newOther.Default);
        }

        #endregion
    }
}
