using System;

namespace SabreTools.Core.Tools
{
    public static class AttributeHelper<T>
    {
        /// <summary>
        /// Get the MappingAttribute from a supported value
        /// </summary>
        /// <param name="value">Value to use</param>
        /// <returns>MappingAttribute attached to the value</returns>
        public static MappingAttribute? GetAttribute(T? value)
        {
            // Null value in, null value out
            if (value == null)
                return null;

            // Current enumeration type
            var enumType = typeof(T);
            if (Nullable.GetUnderlyingType(enumType) != null)
                enumType = Nullable.GetUnderlyingType(enumType);

            // If the value returns a null on ToString, just return null
            string? valueStr = value.ToString();
            if (string.IsNullOrEmpty(valueStr))
                return null;

            // Get the member info array
            var memberInfos = enumType?.GetMember(valueStr);
            if (memberInfos == null)
                return null;

            // Get the enum value info from the array, if possible
            var enumValueMemberInfo = Array.Find(memberInfos, m => m.DeclaringType == enumType);
            if (enumValueMemberInfo == null)
                return null;

            // Try to get the relevant attribute
            var attributes = enumValueMemberInfo.GetCustomAttributes(typeof(MappingAttribute), true);
            if (attributes == null || attributes.Length == 0)
                return null;

            // Return the first attribute, if possible
            return (MappingAttribute?)attributes[0];
        }
    }
}
