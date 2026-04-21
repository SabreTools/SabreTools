using System;
using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;
using SabreTools.IO;
using SabreTools.Logging;
using MergingFlag = SabreTools.Data.Models.Metadata.MergingFlag;

namespace SabreTools.DatTools
{
    public class MergeSplit
    {
        #region Fields

        /// <summary>
        /// Splitting mode to apply
        /// </summary>
        public MergingFlag SplitType { get; set; }

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        #region Running

        /// <summary>
        /// Apply splitting on the DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the DatFile was split, false on error</returns>
        public bool ApplySplitting(DatFile datFile, bool useTags, bool throwOnError = false)
        {
            InternalStopwatch watch = new("Applying splitting to DAT");

            try
            {
                // If we are using tags from the DAT, set the proper input for split type unless overridden
                if (useTags && SplitType == MergingFlag.None)
                    SplitType = datFile.Header.ForceMerging;

#pragma warning disable IDE0010
                // Run internal splitting
                switch (SplitType)
                {
                    // Standard
                    case MergingFlag.None:
                        // No-op
                        break;
                    case MergingFlag.Split:
                        datFile.ApplySplit();
                        break;
                    case MergingFlag.Merged:
                        datFile.ApplyMerged();
                        break;
                    case MergingFlag.NonMerged:
                        datFile.ApplyNonMerged();
                        break;

                    // Nonstandard
                    case MergingFlag.FullMerged:
                        datFile.ApplyFullyMerged();
                        break;
                    case MergingFlag.DeviceNonMerged:
                        datFile.ApplyDeviceNonMerged();
                        break;
                    case MergingFlag.FullNonMerged:
                        datFile.ApplyFullyNonMerged();
                        break;
                }
#pragma warning restore IDE0010
            }
            catch (Exception ex) when (!throwOnError)
            {
                _staticLogger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
        }

        /// <summary>
        /// Apply SuperDAT naming logic to a merged DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        /// <param name="inputs">List of inputs to use for renaming</param>
        public static void ApplySuperDAT(DatFile datFile, List<ParentablePath> inputs)
        {
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem>? items = datFile.GetItemsForBucket(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                List<DatItem> newItems = [];
                foreach (DatItem item in items)
                {
                    DatItem newItem = item;
                    var source = newItem.Source;
                    if (source is null)
                        continue;

                    string filename = inputs[source.Index].CurrentPath;
                    string rootpath = inputs[source.Index].ParentPath ?? string.Empty;

                    if (rootpath.Length > 0
#if NETFRAMEWORK || NETSTANDARD
                        && !rootpath.EndsWith(Path.DirectorySeparatorChar.ToString())
                        && !rootpath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
#else
                        && !rootpath.EndsWith(Path.DirectorySeparatorChar)
                        && !rootpath.EndsWith(Path.AltDirectorySeparatorChar))
#endif
                    {
                        rootpath += Path.DirectorySeparatorChar.ToString();
                    }

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    filename = filename[rootpath.Length..];
#else
                    filename = filename.Remove(0, rootpath.Length);
#endif

                    var machine = newItem.Machine;
                    if (machine is null)
                        continue;

                    string machineName = Path.GetDirectoryName(filename)
                        + Path.DirectorySeparatorChar
                        + Path.GetFileNameWithoutExtension(filename)
                        + Path.DirectorySeparatorChar
                        + machine.Name;
                    if (machineName.Length == 0)
                        machineName = "Default";

                    machine.Name = machineName;

                    newItems.Add(newItem);
                }

                datFile.RemoveBucket(key);
                newItems.ForEach(item => datFile.AddItem(item, statsOnly: false));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Apply SuperDAT naming logic to a merged DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        /// <param name="inputs">List of inputs to use for renaming</param>
        public static void ApplySuperDATDB(DatFile datFile, List<ParentablePath> inputs)
        {
            List<string> keys = [.. datFile.ItemsDB.SortedKeys];
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                var items = datFile.GetItemsForBucketDB(key);
                if (items is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                foreach (var item in items)
                {
                    var source = datFile.GetSourceDB(item.Value.SourceIndex);
                    if (source.Value is null)
                        continue;

                    var machine = datFile.GetMachineDB(item.Value.MachineIndex);
                    if (machine.Value is null)
                        continue;

                    string filename = inputs[source.Value.Index].CurrentPath;
                    string rootpath = inputs[source.Value.Index].ParentPath ?? string.Empty;

                    if (rootpath.Length > 0
#if NETFRAMEWORK || NETSTANDARD
                        && !rootpath!.EndsWith(Path.DirectorySeparatorChar.ToString())
                        && !rootpath!.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
#else
                        && !rootpath.EndsWith(Path.DirectorySeparatorChar)
                        && !rootpath.EndsWith(Path.AltDirectorySeparatorChar))
#endif
                    {
                        rootpath += Path.DirectorySeparatorChar.ToString();
                    }

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    filename = filename[rootpath.Length..];
#else
                    filename = filename.Remove(0, rootpath.Length);
#endif

                    string machineName = Path.GetDirectoryName(filename)
                        + Path.DirectorySeparatorChar
                        + Path.GetFileNameWithoutExtension(filename)
                        + Path.DirectorySeparatorChar
                        + machine.Value.Name;
                    if (machineName.Length == 0)
                        machineName = "Default";

                    machine.Value.Name = machineName;
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion
    }
}
