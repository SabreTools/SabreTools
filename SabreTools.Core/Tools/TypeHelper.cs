using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using SabreTools.Data.Attributes;
using SabreTools.Data.Models.Metadata;

namespace SabreTools.Core.Tools
{
    public static class TypeHelper
    {
        /// <summary>
        /// Get constant values for the given type, if possible
        /// </summary>
        public static string[]? GetConstants(Type? type)
        {
            if (type == null)
                return null;

            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fields == null)
                return null;

            FieldInfo[] noFilterFields = Array.FindAll(fields,
                f => f.IsLiteral
                && !f.IsInitOnly
                && Attribute.GetCustomAttributes(f, typeof(NoFilterAttribute)).Length == 0);
            string[] constantValues = Array.ConvertAll(noFilterFields,
                f => (f.GetRawConstantValue() as string) ?? string.Empty);
            return Array.FindAll(constantValues, s => s.Length > 0);
        }

        /// <summary>
        /// Attempt to get all DatItem types
        /// </summary>
        public static string[] GetDatItemTypeNames()
        {
            List<string> typeNames = [];

            // Loop through all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // If not all types can be loaded, use the ones that could be
                Type?[] assemblyTypes = [];
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    assemblyTypes = Array.FindAll(rtle.Types ?? [], t => t != null);
                }

                // Loop through all types 
                foreach (Type? type in assemblyTypes)
                {
                    // If the type is invalid
                    if (type == null)
                        continue;

                    // If the type isn't a class or doesn't implement the interface
                    if (!type.IsClass || !typeof(DatItem).IsAssignableFrom(type))
                        continue;

                    // Get the XML type name
                    string? elementName = GetXmlRootAttributeElementName(type);
                    if (elementName != null)
                        typeNames.Add(elementName);
                }
            }

            return [.. typeNames];
        }

        /// <summary>
        /// Attempt to get the DatItem type from the name
        /// </summary>
        public static Type? GetDatItemType(string? itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                return null;

            // Loop through all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // If not all types can be loaded, use the ones that could be
                Type?[] assemblyTypes = [];
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    assemblyTypes = Array.FindAll(rtle.Types ?? [], t => t != null);
                }

                // Loop through all types 
                foreach (Type? type in assemblyTypes)
                {
                    // If the type is invalid
                    if (type == null)
                        continue;

                    // If the type isn't a class or doesn't implement the interface
                    if (!type.IsClass || !typeof(DatItem).IsAssignableFrom(type))
                        continue;

                    // Get the XML type name
                    string? elementName = GetXmlRootAttributeElementName(type);
                    if (elementName == null)
                        continue;

                    // If the name matches
                    if (string.Equals(elementName, itemType, StringComparison.OrdinalIgnoreCase))
                        return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Attempt to get the XmlRootAttribute.ElementName value from a type
        /// </summary>
        public static string? GetXmlRootAttributeElementName(Type? type)
        {
            if (type == null)
                return null;

#if NET20 || NET35 || NET40
            return (Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute)) as XmlRootAttribute)!.ElementName;
#else
            return type.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
#endif
        }
    }
}
