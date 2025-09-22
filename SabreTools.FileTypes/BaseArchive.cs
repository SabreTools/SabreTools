using System.Collections.Generic;
using System.IO;
using SabreTools.Hashing;
using SabreTools.IO.Logging;

namespace SabreTools.FileTypes
{
    public abstract class BaseArchive : BaseFile, IParent
    {
        #region Protected instance variables

        /// <summary>
        /// Hashes that are available for children
        /// </summary>
        protected HashType[] _hashTypes = [HashType.CRC32, HashType.MD5, HashType.SHA1];

        /// <summary>
        /// Set of children file objects
        /// </summary>
        protected List<BaseFile>? _children;

        /// <summary>
        /// Determines if real dates are written
        /// </summary>
        protected bool _realDates = false;

        /// <summary>
        /// Buffer size used by archives
        /// </summary>
        protected const int _bufferSize = 4096 * 128;

        /// <summary>
        /// Logging object
        /// </summary>
        protected Logger _logger;

        #endregion

        #region Construtors

        /// <summary>
        /// Create a new Archive with no base file
        /// </summary>
        public BaseArchive()
        {
            _logger = new Logger(this);
        }

        /// <summary>
        /// Create a new BaseArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use</param>
        public BaseArchive(string filename) : base(filename)
        {
            _logger = new Logger(this);
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public abstract bool CopyAll(string outDir);

        /// <inheritdoc/>
        public abstract string? CopyToFile(string entryName, string outDir);

        /// <inheritdoc/>
        public abstract (Stream?, string?) GetEntryStream(string entryName);

        #endregion

        #region Information

        /// <summary>
        /// Set the hash type that can be included in children
        /// </summary>
        public void SetHashType(HashType hashType)
            => SetHashTypes([hashType]);

        /// <summary>
        /// Set the hash types that can be included in children
        /// </summary>
        public void SetHashTypes(HashType[] hashTypes)
            => _hashTypes = hashTypes;

        /// <inheritdoc/>
        public abstract List<BaseFile>? GetChildren();

        /// <inheritdoc/>
        public abstract List<string> GetEmptyFolders();

        /// <summary>
        /// Check whether the input file is a standardized format
        /// </summary>
        public abstract bool IsStandardized();

        #endregion

        #region Writing

        /// <summary>
        /// Set if real dates are written
        /// </summary>
        public void SetRealDates(bool realDates)
        {
            _realDates = realDates;
        }

        /// <inheritdoc/>
        public abstract bool Write(string file, string outDir, BaseFile? baseFile);

        /// <inheritdoc/>
        public abstract bool Write(Stream? stream, string outDir, BaseFile? baseFile);

        /// <inheritdoc/>
        public abstract bool Write(List<string> files, string outDir, List<BaseFile>? baseFiles);

        #endregion
    }
}
