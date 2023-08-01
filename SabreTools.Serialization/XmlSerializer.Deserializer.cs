using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SabreTools.Serialization
{
    /// <summary>
    /// XML deserializer for nullable types
    /// </summary>
    public abstract partial class XmlSerializer<T>
    {
        /// <summary>
        /// Deserializes an XML file to the defined type
        /// </summary>
        /// <param name="path">Path to the file to deserialize</param>
        /// <returns>Deserialized data on success, null on failure</returns>
        public static T? Deserialize(string path)
        {
            using var stream = PathProcessor.OpenStream(path);
            return Deserialize(stream);
        }

        /// <summary>
        /// Deserializes an XML file in a stream to the defined type
        /// </summary>
        /// <param name="stream">Stream to deserialize</param>
        /// <returns>Deserialized data on success, null on failure</returns>
        public static T? Deserialize(Stream? stream)
        {
            // If the stream is null
            if (stream == null)
                return default;

            // Setup the serializer and the reader
            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            };
            var streamReader = new StreamReader(stream);
            var xmlReader = XmlReader.Create(streamReader, settings);

            // Perform the deserialization and return
            return (T?)serializer.Deserialize(xmlReader);
        }
    }
}