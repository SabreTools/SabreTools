namespace SabreTools.Help
{
    public abstract class Feature<T> : Feature
    {
        public T? Value { get; protected set; }

        #region Constructors

        internal Feature(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        internal Feature(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override abstract bool ValidateInput(string input, bool exact = false, bool ignore = false);

        /// <inheritdoc/>
        public override abstract bool IsEnabled();

        /// <inheritdoc/>
        protected override abstract string FormatFlags();

        #endregion
    }
}
