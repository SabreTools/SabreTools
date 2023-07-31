using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SabreTools.Serialization
{
    /// <summary>
    /// XML serializer for nullable types
    /// </summary>
    public abstract partial class XmlSerializer<T>
    {
        /// <summary>
        /// Serializes the defined type to an XML file
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        /// <param name="path">Path to the file to serialize to</param>
        /// <param name="obj">Data to serialize</param>
        /// <returns>True on successful serialization, false otherwise</returns>
        public static bool SerializeToFile(T? obj, string path)
            => SerializeToFile(obj, path, null, null, null, null);

        /// <summary>
        /// Serializes the defined type to an XML file
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        /// <param name="path">Path to the file to serialize to</param>
        /// <param name="obj">Data to serialize</param>
        /// <param name="name">Optional DOCTYPE name</param>
        /// <param name="pubid">Optional DOCTYPE pubid</param>
        /// <param name="sysid">Optional DOCTYPE sysid</param>
        /// <param name="subset">Optional DOCTYPE name</param>
        /// <returns>True on successful serialization, false otherwise</returns>
        protected static bool SerializeToFile(T? obj, string path, string? name = null, string? pubid = null, string? sysid = null, string? subset = null)
        {
            using var stream = SerializeToStream(obj, name, pubid, sysid, subset);
            if (stream == null)
                return false;

            using var fs = File.OpenWrite(path);
            stream.CopyTo(fs);
            return true;
        }

        /// <summary>
        /// Serializes the defined type to a stream
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        /// <returns>Stream containing serialized data on success, null otherwise</returns>
        public static Stream? SerializeToStream(T? obj)
            => SerializeToStream(obj, null, null, null, null);
    
        /// <summary>
        /// Serializes the defined type to a stream
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        /// <param name="name">Optional DOCTYPE name</param>
        /// <param name="pubid">Optional DOCTYPE pubid</param>
        /// <param name="sysid">Optional DOCTYPE sysid</param>
        /// <param name="subset">Optional DOCTYPE name</param>
        /// <returns>Stream containing serialized data on success, null otherwise</returns>
        protected static Stream? SerializeToStream(T? obj, string? name = null, string? pubid = null, string? sysid = null, string? subset = null)
        {
            // If the object is null
            if (obj == null)
                return null;

            // Setup the serializer and the reader
            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t",
                NewLineChars = "\n",
            };
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            var xmlWriter = XmlWriter.Create(streamWriter, settings);

            // Write the doctype if provided
            if (!string.IsNullOrWhiteSpace(name))
                xmlWriter.WriteDocType(name, pubid, sysid, subset);

            // Perform the deserialization and return
            serializer.Serialize(xmlWriter, obj);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    
    }
}