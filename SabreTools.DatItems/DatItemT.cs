using System;
using System.Reflection;
using SabreTools.Core;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Base class for all items included in a set that are backed by an internal model
    /// </summary>
    public abstract class DatItem<T> : DatItem, IEquatable<DatItem<T>>, IComparable<DatItem<T>>, ICloneable where T : Data.Models.Metadata.DatItem
    {
        #region Constructors

        /// <summary>
        /// Create a default, empty object
        /// </summary>
        public DatItem()
        {
            _internal = Activator.CreateInstance<T>();

            SetName(string.Empty);
            SetFieldValue<ItemType>(Data.Models.Metadata.DatItem.TypeKey, ItemType);
            SetFieldValue<Machine>(MachineKey, new Machine());
        }

        /// <summary>
        /// Create an object from the internal model
        /// </summary>
        public DatItem(T item)
        {
            _internal = item;

            SetFieldValue<ItemType>(Data.Models.Metadata.DatItem.TypeKey, ItemType);
            SetFieldValue<Machine>(MachineKey, new Machine());
        }

        #endregion

        #region Cloning Methods

        /// <summary>
        /// Clone the DatItem
        /// </summary>
        /// <returns>Clone of the DatItem</returns>
        /// <remarks>
        /// Throws an exception if there is a DatItem implementation
        /// that is not a part of this library.
        /// </remarks>
        public override object Clone()
        {
            var concrete = Array.Find(Assembly.GetExecutingAssembly().GetTypes(),
                t => !t.IsAbstract && t.IsClass && t.BaseType == typeof(DatItem<T>));

            var clone = Activator.CreateInstance(concrete!);
            (clone as DatItem<T>)!._internal = _internal?.Clone() as T ?? Activator.CreateInstance<T>();
            return clone;
        }

        /// <summary>
        /// Get a clone of the current internal model
        /// </summary>
        public virtual T GetInternalClone() => (_internal.Clone() as T)!;

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public int CompareTo(DatItem<T>? other)
        {
            // If the other item doesn't exist
            if (other == null)
                return 1;

            // Get the names to avoid changing values
            string? selfName = GetName();
            string? otherName = other.GetName();

            // If the names are equal
            if (selfName == otherName)
                return Equals(other) ? 0 : 1;

            // If `otherName` is null, Compare will return > 0
            // If `selfName` is null, Compare will return < 0
            return string.Compare(selfName, otherName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determine if an item is a duplicate using partial matching logic
        /// </summary>
        /// <param name="other">DatItem to use as a baseline</param>
        /// <returns>True if the items are duplicates, false otherwise</returns>
        public virtual bool Equals(DatItem<T>? other)
        {
            // If the other value is null
            if (other == null)
                return false;

            // Get the types for comparison
            ItemType selfType = GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType();
            ItemType otherType = other.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType();

            // If we don't have a matched type, return false
            if (selfType != otherType)
                return false;

            // Compare the internal models
            return _internal.EqualTo(other._internal);
        }

        #endregion
    }
}
