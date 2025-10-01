using System;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public abstract class Input<T> : Input
    {
        #region Properties

        /// <summary>
        /// Represents the last value stored
        /// </summary>
        public T? Value { get; protected set; }

        /// <inheritdoc/>
        public override bool ValueSet => Value != null;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public Input(string name, string[] flags, string description)
            : base(name, flags, description) { }

        /// <inheritdoc/>
        public Input(string name, string[] flags, string description, bool required)
            : base(name, flags, description, required) { }

        /// <inheritdoc/>
        public Input(string name, string[] flags, string description, string longDescription, bool required)
            : base(name, flags, description, longDescription, required) { }

        #endregion

        #region Functionality

        /// <inheritdoc/>
        public override void ClearValue()
        {
            Value = default;
        }

        /// <summary>
        /// Set a new value
        /// </summary>
        public void SetValue(T value)
        {
            Value = value;
        }

        #endregion
    }
}