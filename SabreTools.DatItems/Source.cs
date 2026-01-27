using System;

#pragma warning disable IDE0290 // Use primary constructor
namespace SabreTools.DatItems
{
    /// <summary>
    /// Source information wrapper
    /// </summary>
    public class Source : ICloneable
    {
        /// <summary>
        /// Source index
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Source name
        /// </summary>
        public readonly string? Name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Source ID</param>
        /// <param name="source">Source name, optional</param>
        public Source(int id, string? source = null)
        {
            Index = id;
            Name = source;
        }

        #region Cloning

        /// <summary>
        /// Clone the current object
        /// </summary>
        public object Clone() => new Source(Index, Name);

        #endregion
    }
}
