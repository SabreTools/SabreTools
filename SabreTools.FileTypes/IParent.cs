using System.Collections.Generic;
using System.IO;

namespace SabreTools.FileTypes
{
    /// <summary>
    /// Represents an item that can contain children
    /// </summary>
    public interface IParent
    {
        #region Extraction

        /// <summary>
        /// Attempt to extract a file as an archive
        /// </summary>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>True if the extraction was a success, false otherwise</returns>
        bool CopyAll(string outDir);

        /// <summary>
        /// Attempt to extract a file from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>Name of the extracted file, null on error</returns>
        string? CopyToFile(string entryName, string outDir);

        /// <summary>
        /// Attempt to extract a stream from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="realEntry">Real name of the entry, reported by the parent</param>
        /// <returns>Stream representing the entry, null on error</returns>
        Stream? GetEntryStream(string entryName, out string? realEntry);

        #endregion

        #region Information

        /// <summary>
        /// Generate a list of immediate children from the current folder
        /// </summary>
        /// <returns>List of BaseFile objects representing the found data</returns>
        List<BaseFile>? GetChildren();

        /// <summary>
        /// Generate a list of empty folders in an archive
        /// </summary>
        /// <param name="input">Input file to get data from</param>
        /// <returns>List of empty folders in the folder</returns>
        List<string>? GetEmptyFolders();

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to an output folder
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFile">BaseFile representing the new information</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        bool Write(string file, string outDir, BaseFile? baseFile);

        /// <summary>
        /// Write an input stream to an output folder
        /// </summary>
        /// <param name="inputStream">Input stream to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFile">BaseFile representing the new information</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        bool Write(Stream? inputStream, string outDir, BaseFile? baseFile);

        /// <summary>
        /// Write a set of input files to an output folder (assuming the same output archive name)
        /// </summary>
        /// <param name="inputFiles">Input files to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFiles">BaseFiles representing the new information</param>
        /// <returns>True if the inputs were written properly, false otherwise</returns>
        bool Write(List<string> inputFiles, string outDir, List<BaseFile>? baseFiles);

        #endregion
    }
}
