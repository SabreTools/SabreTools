using System.Collections.Generic;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.DatTools;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using ItemType = SabreTools.Data.Models.Metadata.ItemType;

namespace SabreTools.Features
{
    internal class Split : BaseFeature
    {
        public const string DisplayName = "Split";

        private static readonly string[] _flags = ["sp", "split"];

        private const string _description = "Split input DATs by a given criteria";

        private const string _detailed = "This feature allows the user to split input DATs by a number of different possible criteria. See the individual input information for details. More than one split type is allowed at a time.";

        public Split()
            : base(DisplayName, _flags, _description, _detailed)
        {
            RequiresInputs = true;

            // Common Features
            AddCommonFeatures();

            Add(OutputTypeListInput);
            this[OutputTypeListInput]!.Add(DeprecatedFlag);
            Add(OutputDirStringInput);
            Add(InplaceFlag);
            Add(ExtensionFlag);
            this[ExtensionFlag]!.Add(ExtaListInput);
            this[ExtensionFlag]!.Add(ExtbListInput);
            Add(HashFlag);
            Add(LevelFlag);
            this[LevelFlag]!.Add(ShortFlag);
            this[LevelFlag]!.Add(BaseFlag);
            Add(SizeFlag);
            this[SizeFlag]!.Add(RadixInt64Input);
            Add(TotalSizeFlag);
            this[TotalSizeFlag]!.Add(ChunkSizeInt64Input);
            Add(TypeFlag);
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // If the base fails, just fail out
            if (!base.Execute())
                return false;

            // Get the splitting mode
            SplittingMode splittingMode = GetSplittingMode();
            if (splittingMode == SplittingMode.None)
            {
                _logger.Error("No valid splitting mode found!");
                return false;
            }

            // Get only files from the inputs
            List<ParentablePath> files = IOExtensions.GetFilesOnly(Inputs, appendParent: true);

            // Loop over the input files
            foreach (ParentablePath file in files)
            {
                // Tell users if their file doesn't have a recognized extension
                if (!Parser.HasValidDatExtension(file.CurrentPath))
                {
                    _logger.Warning($"'{file.CurrentPath} does not have a recognized extension! Skipping...");
                    continue;
                }

                // Create and fill the new DAT
                DatFile internalDat = Parser.CreateDatFile(Header!, Modifiers!);
                internalDat.Header.DatFormat = null;
                Parser.ParseInto(internalDat, file.CurrentPath);
                if (FilterRunner is not null)
                    internalDat.ExecuteFilters(FilterRunner);

                // Ensure there are output formats
                var datFormats = DatFormats;
                if (datFormats is null || datFormats.Count == 0)
                    datFormats = [internalDat.Header.DatFormat ?? DatFormat.Logiqx];

                // Get the output directory
                OutputDir = OutputDir.EnsureDirectory();
                OutputDir = file.GetOutputPath(OutputDir, GetBoolean(InplaceValue));

                // Extension splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.Extension) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.Extension))
#endif
                {
                    (DatFile? extADat, DatFile? extBDat) = DatTools.Splitter.SplitByExtension(internalDat, GetStringList(ExtAListValue), GetStringList(ExtBListValue));
                    if (extADat is not null && extBDat is not null)
                    {
                        var watch = new InternalStopwatch("Outputting extension-split DATs");

                        // Output both possible DatFiles
                        extADat.Write(datFormats, OutputDir);
                        extBDat.Write(datFormats, OutputDir);

                        watch.Stop();
                    }
                }

                // Hash splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.Hash) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.Hash))
#endif
                {
                    Dictionary<string, DatFile> typeDats = DatTools.Splitter.SplitByHash(internalDat);

                    var watch = new InternalStopwatch("Outputting hash-split DATs");

                    // Loop through each type DatFile
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    Parallel.ForEach(typeDats.Keys, itemType =>
#else
                    foreach (var itemType in typeDats.Keys)
#endif
                    {
                        typeDats[itemType].Write(datFormats, OutputDir!);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    });
#else
                    }
#endif

                    watch.Stop();
                }

                // Level splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.Level) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.Level))
#endif
                {
                    _logger.Warning("This feature is not implemented: level-split");
                    DatTools.Splitter.SplitByLevel(
                        internalDat,
                        OutputDir!,
                        GetBoolean(ShortValue),
                        GetBoolean(BaseValue));
                }

                // Size splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.Size) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.Size))
#endif
                {
                    (DatFile lessThan, DatFile greaterThan) = DatTools.Splitter.SplitBySize(internalDat, GetInt64(RadixInt64Value));

                    var watch = new InternalStopwatch("Outputting size-split DATs");

                    // Output both possible DatFiles
                    lessThan.Write(datFormats, OutputDir);
                    greaterThan.Write(datFormats, OutputDir);

                    watch.Stop();
                }

                // Total Size splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.TotalSize) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.TotalSize))
#endif
                {
                    _logger.Warning("This feature is not implemented: level-split");
                    List<DatFile> sizedDats = DatTools.Splitter.SplitByTotalSize(internalDat, GetInt64(ChunkSizeInt64Value));

                    var watch = new InternalStopwatch("Outputting total-size-split DATs");

                    // Loop through each type DatFile
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    Parallel.ForEach(sizedDats, sizedDat =>
#else
                    foreach (var sizedDat in sizedDats)
#endif
                    {
                        sizedDat.Write(datFormats, OutputDir);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    });
#else
                    }
#endif

                    watch.Stop();
                }

                // Type splitting
#if NET20 || NET35
                if ((splittingMode & SplittingMode.Type) != 0)
#else
                if (splittingMode.HasFlag(SplittingMode.Type))
#endif
                {
                    Dictionary<ItemType, DatFile> typeDats = DatTools.Splitter.SplitByType(internalDat);

                    var watch = new InternalStopwatch("Outputting ItemType DATs");

                    // Loop through each type DatFile
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    Parallel.ForEach(typeDats.Keys, itemType =>
#else
                    foreach (var itemType in typeDats.Keys)
#endif
                    {
                        typeDats[itemType].Write(datFormats, OutputDir);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    });
#else
                    }
#endif

                    watch.Stop();
                }
            }

            return true;
        }
    }
}
