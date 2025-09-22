namespace SabreTools.Core.Filter
{
    /// <summary>
    /// Determines how a filter group should be applied
    /// </summary>
    public enum GroupType
    {
        /// <summary>
        /// Default value, does nothing
        /// </summary>
        NONE,

        /// <summary>
        /// All must pass for the group to pass
        /// </summary>
        AND,

        /// <summary>
        /// Any must pass for the group to pass
        /// </summary>
        OR,
    }

    /// <summary>
    /// Determines what operation is being done
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Default value, does nothing
        /// </summary>
        NONE,

        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }
}
