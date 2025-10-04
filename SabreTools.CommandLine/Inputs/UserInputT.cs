namespace SabreTools.CommandLine.Inputs
{
    /// <summary>
    /// Represents a single user input which may contain children
    /// </summary>
    public abstract class UserInput<T> : UserInput
    {
        /// <summary>
        /// Typed value provided by the user
        /// </summary>
        public T? Value { get; protected set; }

        #region Constructors

        internal UserInput(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        internal UserInput(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override abstract bool ProcessInput(string[] args, ref int index);

        /// <inheritdoc/>
        protected override abstract string FormatFlags();

        #endregion
    }
}
