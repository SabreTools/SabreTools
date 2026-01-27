using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SabreTools.Core.Filter;
using SabreTools.DatItems;

#pragma warning disable IDE0060 // Remove unused parameter
namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of a SabreDAT XML
    /// </summary>
    /// TODO: Transform this into direct serialization and deserialization of the Metadata type
    public sealed class SabreXML : DatFile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => Enum.GetValues(typeof(ItemType)) as ItemType[] ?? [];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SabreXML(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue(DatHeader.DatFormatKey, DatFormat.SabreXML);
        }

        /// <inheritdoc/>
        public override void ParseFile(string filename,
            int indexId,
            bool keep,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false)
        {
            // Prepare all internal variables
            XmlReader? xtr = XmlReader.Create(filename, new XmlReaderSettings
            {
                CheckCharacters = false,
#if NET40_OR_GREATER
                DtdProcessing = DtdProcessing.Ignore,
#endif
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            });
            var source = new Source(indexId, filename);
            long sourceIndex = AddSourceDB(source);

            // If we got a null reader, just return
            if (xtr is null)
                return;

            // Otherwise, read the file to the end
            try
            {
                xtr.MoveToContent();
                while (!xtr.EOF)
                {
                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        case "header":
                            XmlSerializer xs = new(typeof(DatHeader));
                            DatHeader? header = xs.Deserialize(xtr.ReadSubtree()) as DatHeader;
                            SetHeader(header);
                            xtr.Skip();
                            break;

                        case "directory":
                            ReadDirectory(xtr.ReadSubtree(), statsOnly, source, sourceIndex, filterRunner);

                            // Skip the directory node now that we've processed it
                            xtr.Read();
                            break;
                        default:
                            xtr.Read();
                            break;
                    }
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Warning(ex, $"Exception found while parsing '{filename}'");

                // For XML errors, just skip the affected node
                xtr?.Read();
            }

#if NET452_OR_GREATER
            xtr?.Dispose();
#endif
        }

        /// <summary>
        /// Read directory information
        /// </summary>
        /// <param name="xtr">XmlReader to use to parse the header</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="source">Source representing the DAT</param>
        /// <param name="sourceIndex">Index of the Source representing the DAT</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ReadDirectory(XmlReader xtr,
            bool statsOnly,
            Source source,
            long sourceIndex,
            FilterRunner? filterRunner)
        {
            // If the reader is invalid, skip
            if (xtr is null)
                return;

            // Prepare internal variables
            Machine? machine = null;
            long machineIndex = -1;

            // Otherwise, read the directory
            xtr.MoveToContent();
            while (!xtr.EOF)
            {
                // We only want elements
                if (xtr.NodeType != XmlNodeType.Element)
                {
                    xtr.Read();
                    continue;
                }

                switch (xtr.Name)
                {
                    case "machine":
                        XmlSerializer xs = new(typeof(Machine));
                        machine = xs?.Deserialize(xtr.ReadSubtree()) as Machine;

                        // If the machine doesn't pass the filter
                        if (machine is not null && filterRunner is not null && !machine.PassesFilter(filterRunner))
                            machine = null;

                        if (machine is not null)
                            machineIndex = AddMachineDB(machine);

                        xtr.Skip();
                        break;

                    case "files":
                        ReadFiles(xtr.ReadSubtree(),
                            machine,
                            machineIndex,
                            statsOnly,
                            source,
                            sourceIndex,
                            filterRunner);

                        // Skip the directory node now that we've processed it
                        xtr.Read();
                        break;
                    default:
                        xtr.Read();
                        break;
                }
            }
        }

        /// <summary>
        /// Read Files information
        /// </summary>
        /// <param name="xtr">XmlReader to use to parse the header</param>
        /// <param name="machine">Machine to copy information from</param>
        /// <param name="machineIndex">Index of the Machine to add to the parsed items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="source">Source representing the DAT</param>
        /// <param name="sourceIndex">Index of the Source representing the DAT</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ReadFiles(XmlReader xtr,
            Machine? machine,
            long machineIndex,
            bool statsOnly,
            Source source,
            long sourceIndex,
            FilterRunner? filterRunner)
        {
            // If the reader is invalid, skip
            if (xtr is null)
                return;

            // Otherwise, read the items
            xtr.MoveToContent();
            while (!xtr.EOF)
            {
                // We only want elements
                if (xtr.NodeType != XmlNodeType.Element)
                {
                    xtr.Read();
                    continue;
                }

                switch (xtr.Name)
                {
                    case "datitem":
                        XmlSerializer xs = new(typeof(DatItem));
                        if (xs.Deserialize(xtr.ReadSubtree()) is DatItem item)
                        {
                            // If the item doesn't pass the filter
                            if (filterRunner is not null && !item.PassesFilter(filterRunner))
                            {
                                xtr.Skip();
                                break;
                            }

                            item.CopyMachineInformation(machine);
                            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
                            AddItem(item, statsOnly);
                            // AddItemDB(item, machineIndex, sourceIndex, statsOnly);
                        }

                        xtr.Skip();
                        break;
                    default:
                        xtr.Read();
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");
                FileStream fs = File.Create(outfile);

                // If we get back null for some reason, just log and return
                if (fs is null)
                {
                    _logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                XmlTextWriter xtw = new(fs, new UTF8Encoding(false))
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1,
                };

                // Write out the header
                WriteHeader(xtw);

                // Write out each of the machines and roms
                string? lastgame = null;

                // Use a sorted list of games to output
                foreach (string key in Items.SortedKeys)
                {
                    List<DatItem> datItems = GetItemsForBucket(key, filter: true);

                    // If this machine doesn't contain any writable items, skip
                    if (!ContainsWritable(datItems))
                        continue;

                    // Resolve the names in the block
                    datItems = ResolveNames(datItems);

                    for (int index = 0; index < datItems.Count; index++)
                    {
                        DatItem datItem = datItems[index];

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame is not null && !string.Equals(lastgame, datItem.GetMachine()!.GetName(), StringComparison.OrdinalIgnoreCase))
                            WriteEndGame(xtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame is null || !string.Equals(lastgame, datItem.GetMachine()!.GetName(), StringComparison.OrdinalIgnoreCase))
                            WriteStartGame(xtw, datItem);

                        // Check for a "null" item
                        datItem = ProcessNullifiedItem(datItem);

                        // Write out the item if we're not ignoring
                        if (!ShouldIgnore(datItem, ignoreblanks))
                            WriteDatItem(xtw, datItem);

                        // Set the new data to compare against
                        lastgame = datItem.GetMachine()!.GetName();
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                _logger.User($"'{outfile}' written!{Environment.NewLine}");
#if NET452_OR_GREATER
                xtw.Dispose();
#endif
                fs.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool WriteToFileDB(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");
                FileStream fs = File.Create(outfile);

                // If we get back null for some reason, just log and return
                if (fs is null)
                {
                    _logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                XmlTextWriter xtw = new(fs, new UTF8Encoding(false))
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1,
                };

                // Write out the header
                WriteHeader(xtw);

                // Write out each of the machines and roms
                string? lastgame = null;

                // Use a sorted list of games to output
                foreach (string key in ItemsDB.SortedKeys)
                {
                    // If this machine doesn't contain any writable items, skip
                    var itemsDict = GetItemsForBucketDB(key, filter: true);
                    if (itemsDict is null || !ContainsWritable([.. itemsDict.Values]))
                        continue;

                    // Resolve the names in the block
                    var items = ResolveNamesDB([.. itemsDict]);

                    foreach (var kvp in items)
                    {
                        // Get the machine for the item
                        var machine = GetMachineForItemDB(kvp.Key);

                        // If we have a different game and we're not at the start of the list, output the end of last item
                        if (lastgame is not null && !string.Equals(lastgame, machine.Value!.GetName(), StringComparison.OrdinalIgnoreCase))
                            WriteEndGame(xtw);

                        // If we have a new game, output the beginning of the new item
                        if (lastgame is null || !string.Equals(lastgame, machine.Value!.GetName(), StringComparison.OrdinalIgnoreCase))
                            WriteStartGame(xtw, kvp.Value);

                        // Check for a "null" item
                        var datItem = new KeyValuePair<long, DatItem>(kvp.Key, ProcessNullifiedItem(kvp.Value));

                        // Write out the item if we're not ignoring
                        if (!ShouldIgnore(datItem.Value, ignoreblanks))
                            WriteDatItemDB(xtw, datItem);

                        // Set the new data to compare against
                        lastgame = machine.Value!.GetName();
                    }
                }

                // Write the file footer out
                WriteFooter(xtw);

                _logger.User($"'{outfile}' written!{Environment.NewLine}");
#if NET452_OR_GREATER
                xtw.Dispose();
#endif
                fs.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out DAT header using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private void WriteHeader(XmlTextWriter xtw)
        {
            xtw.WriteStartDocument();

            xtw.WriteStartElement("datafile");

            XmlSerializer xs = new(typeof(DatHeader));
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            xs.Serialize(xtw, Header, ns);

            xtw.WriteStartElement("data");

            xtw.Flush();
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        private static void WriteStartGame(XmlTextWriter xtw, DatItem datItem)
        {
            // No game should start with a path separator
            datItem.GetMachine()!.SetName(datItem.GetMachine()!.GetName()?.TrimStart(Path.DirectorySeparatorChar) ?? string.Empty);

            // Write the machine
            xtw.WriteStartElement("directory");
            XmlSerializer xs = new(typeof(Machine));
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            xs.Serialize(xtw, datItem.GetMachine(), ns);

            xtw.WriteStartElement("files");

            xtw.Flush();
        }

        /// <summary>
        /// Write out Game start using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private static void WriteEndGame(XmlTextWriter xtw)
        {
            // End files
            xtw.WriteEndElement();

            // End directory
            xtw.WriteEndElement();

            xtw.Flush();
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        private void WriteDatItem(XmlTextWriter xtw, DatItem datItem)
        {
            // Get the machine for the item
            var machine = datItem.GetMachine();

            // Pre-process the item name
            ProcessItemName(datItem, machine, forceRemoveQuotes: true, forceRomName: false);

            // Write the DatItem
            XmlSerializer xs = new(typeof(DatItem));
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            xs.Serialize(xtw, datItem, ns);

            xtw.Flush();
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        private void WriteDatItemDB(XmlTextWriter xtw, KeyValuePair<long, DatItem> datItem)
        {
            // Get the machine for the item
            var machine = GetMachineForItemDB(datItem.Key);

            // Pre-process the item name
            ProcessItemName(datItem.Value, machine.Value, forceRemoveQuotes: true, forceRomName: false);

            // Write the DatItem
            XmlSerializer xs = new(typeof(DatItem));
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            xs.Serialize(xtw, datItem, ns);

            xtw.Flush();
        }

        /// <summary>
        /// Write out DAT footer using the supplied StreamWriter
        /// </summary>
        /// <param name="xtw">XmlTextWriter to output to</param>
        private static void WriteFooter(XmlTextWriter xtw)
        {
            // End files
            xtw.WriteEndElement();

            // End directory
            xtw.WriteEndElement();

            // End data
            xtw.WriteEndElement();

            // End datafile
            xtw.WriteEndElement();

            xtw.Flush();
        }
    }
}
