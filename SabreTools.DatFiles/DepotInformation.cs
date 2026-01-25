using System;
using SabreTools.Hashing;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Depot information wrapper
    /// </summary>
    public class DepotInformation : ICloneable
    {
        /// <summary>
        /// Name or path of the Depot
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Whether to use this Depot or not
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// Depot byte-depth
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isActive">Set active state</param>
        /// <param name="depth">Set depth between 0 and SHA-1's byte length</param>
        public DepotInformation(bool isActive, int depth)
            : this(null, isActive, depth)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Identifier for the depot</param>
        /// <param name="isActive">Set active state</param>
        /// <param name="depth">Set depth between 0 and SHA-1's byte length</param>
        public DepotInformation(string? name, bool isActive, int depth)
        {
            Name = name;
            IsActive = isActive;
            Depth = depth;

            // Limit depth value
            if (Depth == int.MinValue)
                Depth = 4;
            else if (Depth < 0)
                Depth = 0;
            else if (Depth > ZeroHash.SHA1Arr.Length)
                Depth = ZeroHash.SHA1Arr.Length;
        }

        #region Cloning

        /// <summary>
        /// Clone the current object
        /// </summary>
        public object Clone() => new DepotInformation(Name, IsActive, Depth);

        #endregion
    }
}
