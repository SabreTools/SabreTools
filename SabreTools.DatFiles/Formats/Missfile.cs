using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Core.Filter;
using SabreTools.DatItems;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a Missfile
    /// </summary>
    public sealed class Missfile : DatFile
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => Enum.GetValues(typeof(ItemType)) as ItemType[] ?? [];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Missfile(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.MissFile);
        }

        /// <inheritdoc/>
        /// <remarks>There is no consistent way to parse a missfile</remarks>
        public override void ParseFile(string filename,
            int indexId,
            bool keep,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            // TODO: Check required fields
            return null;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");
                FileStream fs = File.Create(outfile);

                // If we get back null for some reason, just log and return
                if (fs == null)
                {
                    _logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                StreamWriter sw = new(fs, new UTF8Encoding(false));

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

                        // Check for a "null" item
                        datItem = ProcessNullifiedItem(datItem);

                        // Write out the item if we're using machine names or we're not ignoring
                        if (!Modifiers.UseRomName || !ShouldIgnore(datItem, ignoreblanks))
                            WriteDatItem(sw, datItem, lastgame);

                        // Set the new data to compare against
                        lastgame = datItem.GetMachine()!.GetName();
                    }
                }

                _logger.User($"'{outfile}' written!{Environment.NewLine}");
                sw.Dispose();
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
                if (fs == null)
                {
                    _logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                StreamWriter sw = new(fs, new UTF8Encoding(false));

                // Write out each of the machines and roms
                string? lastgame = null;

                // Use a sorted list of games to output
                foreach (string key in ItemsDB.SortedKeys)
                {
                    // If this machine doesn't contain any writable items, skip
                    var itemsDict = GetItemsForBucketDB(key, filter: true);
                    if (itemsDict == null || !ContainsWritable([.. itemsDict.Values]))
                        continue;

                    // Resolve the names in the block
                    var items = ResolveNamesDB([.. itemsDict]);

                    foreach (var kvp in items)
                    {
                        // Check for a "null" item
                        var datItem = new KeyValuePair<long, DatItem>(kvp.Key, ProcessNullifiedItem(kvp.Value));

                        // Get the machine for the item
                        var machine = GetMachineForItemDB(datItem.Key);

                        // Write out the item if we're using machine names or we're not ignoring
                        if (!Modifiers.UseRomName || !ShouldIgnore(datItem.Value, ignoreblanks))
                            WriteDatItemDB(sw, datItem, lastgame);

                        // Set the new data to compare against
                        lastgame = machine.Value!.GetName();
                    }
                }

                _logger.User($"'{outfile}' written!{Environment.NewLine}");
                sw.Dispose();
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
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="lastgame">The name of the last game to be output</param>
        private void WriteDatItem(StreamWriter sw, DatItem datItem, string? lastgame)
        {
            var machine = datItem.GetMachine();
            WriteDatItemImpl(sw, datItem, machine!, lastgame);
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="lastgame">The name of the last game to be output</param>
        private void WriteDatItemDB(StreamWriter sw, KeyValuePair<long, DatItem> datItem, string? lastgame)
        {
            var machine = GetMachineForItemDB(datItem.Key).Value;
            WriteDatItemImpl(sw, datItem.Value, machine!, lastgame);
        }

        /// <summary>
        /// Write out DatItem using the supplied StreamWriter
        /// </summary>
        /// <param name="sw">StreamWriter to output to</param>
        /// <param name="datItem">DatItem object to be output</param>
        /// <param name="machine">Machine object representing the set the item is in</param>
        /// <param name="lastgame">The name of the last game to be output</param>
        private void WriteDatItemImpl(StreamWriter sw, DatItem datItem, Machine machine, string? lastgame)
        {
            // Process the item name
            ProcessItemName(datItem, machine, forceRemoveQuotes: false, forceRomName: false);

            // Romba mode automatically uses item name
            if (Modifiers.OutputDepot?.IsActive == true || Modifiers.UseRomName)
                sw.Write($"{datItem.GetName() ?? string.Empty}\n");
            else if (!Modifiers.UseRomName && machine!.GetName() != lastgame)
                sw.Write($"{machine!.GetName() ?? string.Empty}\n");

            sw.Flush();
        }
    }
}
