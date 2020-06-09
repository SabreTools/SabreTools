using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using SabreTools.Library.Tools;

namespace SabreTools.Library.Data
{
    /// <summary>
    /// Globally-accessible objects for the library
    /// </summary>
    public class Globals
    {
        #region Private implementations

        private static Logger _logger = null;
        private static int _maxDegreeOfParallelism = System.Environment.ProcessorCount;

        #endregion

        #region Public accessors

        public static Logger Logger
        {
            get
            {
                if (_logger == null)
                    _logger = new Logger();

                return _logger;
            }
            set { _logger = value; }
        }

        public static int MaxThreads
        {
            get { return _maxDegreeOfParallelism; }
            set { _maxDegreeOfParallelism = value; }
        }

        public static ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = _maxDegreeOfParallelism
                };
            }
        }

        public static string ExeName
        {
            get { return new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath; }
        }

        public static string ExeDir
        {
            get { return Path.GetDirectoryName(ExeName); }
        }

        public static string CommandLineArgs
        {
            get { return string.Join(" ", Environment.GetCommandLineArgs()); }
        }

        #endregion
    }
}
