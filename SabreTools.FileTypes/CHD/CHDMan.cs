using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Path = RVIO.Path;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;
using SabreTools.Logging;

namespace SabreTools.FileTypes.CHD
{
    internal class CHDManCheck
    {
        public enum hdErr
        {
            HDERR_NONE,
            //HDERR_NO_INTERFACE,
            HDERR_OUT_OF_MEMORY,
            HDERR_INVALID_FILE,
            //HDERR_INVALID_PARAMETER,
            HDERR_INVALID_DATA,
            HDERR_FILE_NOT_FOUND,
            //HDERR_REQUIRES_PARENT,
            //HDERR_FILE_NOT_WRITEABLE,
            HDERR_READ_ERROR,
            HDERR_WRITE_ERROR,
            //HDERR_CODEC_ERROR,
            //HDERR_INVALID_PARENT,
            //HDERR_SECTOR_OUT_OF_RANGE,
            HDERR_DECOMPRESSION_ERROR,
            //HDERR_COMPRESSION_ERROR,
            //HDERR_CANT_CREATE_FILE,
            HDERR_CANT_VERIFY,
            HDERR_UNSUPPORTED,

            HDERR_CANNOT_OPEN_FILE,
            HDERR_CHDMAN_NOT_FOUND
        };

        public static bool isLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }

        private int _outputLineCount;
        private int _errorLines;

        private string _result;
        private hdErr _resultType;

        internal hdErr ChdCheck(string filename, out string result)
        {
            _result = "";
            _resultType = hdErr.HDERR_NONE;

            string chdExe = "chdman.exe";
            if (isLinux)
            {
                chdExe = "chdman";
            }

            string chdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chdExe);
            if (!File.Exists(chdPath))
            {
                result = chdExe + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            if (!File.Exists(filename))
            {
                result = filename + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            using (Process exeProcess = new Process())
            {
                exeProcess.StartInfo.FileName = chdPath;

                exeProcess.StartInfo.Arguments = "verify -i \"" +Path.GetFileName(filename) + "\"";

                exeProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);

                // Set UseShellExecute to false for redirection.
                exeProcess.StartInfo.UseShellExecute = false;
                // Stops the Command window from popping up.
                exeProcess.StartInfo.CreateNoWindow = true;

                // Redirect the standard output.
                // This stream is read asynchronously using an event handler.
                exeProcess.StartInfo.RedirectStandardOutput = true;
                exeProcess.StartInfo.RedirectStandardError = true;

                // Set our event handler to asynchronously read the process output.
                exeProcess.OutputDataReceived += CHDOutputHandler;
                exeProcess.ErrorDataReceived += CHDErrorHandler;

                _outputLineCount = 0;
                _errorLines = 0;

                exeProcess.Start();

                // Start the asynchronous read of the process output stream.
                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();

                // Wait for the process finish.
                exeProcess.WaitForExit();
            }

            result = _result;

            return _resultType;
        }

        internal hdErr ChdUpgrade(string filename, string archiveFileName, out string result)
        {
            _result = "";
            _resultType = hdErr.HDERR_NONE;

            string chdExe = "chdman.exe";
            if (isLinux)
            {
                chdExe = "chdman";
            }

            string chdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chdExe);
            if (!File.Exists(chdPath))
            {
                result = chdExe + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            if (!File.Exists(filename))
            {
                result = filename + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            using (Process exeProcess = new Process())
            {
                exeProcess.StartInfo.FileName = chdPath;

                exeProcess.StartInfo.Arguments = "copy -i \"" + Path.GetFileName(filename) + "\" -o \"" + archiveFileName +"\"";

                exeProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);

                // Set UseShellExecute to false for redirection.
                exeProcess.StartInfo.UseShellExecute = false;
                // Stops the Command window from popping up.
                exeProcess.StartInfo.CreateNoWindow = true;

                // Redirect the standard output.
                // This stream is read asynchronously using an event handler.
                exeProcess.StartInfo.RedirectStandardOutput = true;
                exeProcess.StartInfo.RedirectStandardError = true;

                // Set our event handler to asynchronously read the process output.
                exeProcess.OutputDataReceived += CHDOutputHandler;
                exeProcess.ErrorDataReceived += CHDErrorHandler;

                _outputLineCount = 0;
                _errorLines = 0;

                exeProcess.Start();

                // Start the asynchronous read of the process output stream.
                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();

                // Wait for the process finish.
                exeProcess.WaitForExit();
            }

            result = _result;

            return _resultType;
        }

        internal hdErr ChdCreate(string filename, string archiveFileName, out string result)
        {
            _result = "";
            _resultType = hdErr.HDERR_NONE;

            string chdExe = "chdman.exe";
            if (isLinux)
            {
                chdExe = "chdman";
            }

            string chdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chdExe);
            if (!File.Exists(chdPath))
            {
                result = chdExe + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            if (!File.Exists(filename))
            {
                result = filename + " Not Found.";
                return hdErr.HDERR_CHDMAN_NOT_FOUND;
            }

            using (Process exeProcess = new Process())
            {
                exeProcess.StartInfo.FileName = chdPath;

                exeProcess.StartInfo.Arguments = "createcd -i \"" + Path.GetFileName(filename) + "\" -o \"" + archiveFileName +"\"";

                exeProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);

                // Set UseShellExecute to false for redirection.
                exeProcess.StartInfo.UseShellExecute = false;
                // Stops the Command window from popping up.
                exeProcess.StartInfo.CreateNoWindow = true;

                // Redirect the standard output.
                // This stream is read asynchronously using an event handler.
                exeProcess.StartInfo.RedirectStandardOutput = true;
                exeProcess.StartInfo.RedirectStandardError = true;

                // Set our event handler to asynchronously read the process output.
                exeProcess.OutputDataReceived += CHDOutputHandler;
                exeProcess.ErrorDataReceived += CHDErrorHandler;

                _outputLineCount = 0;
                _errorLines = 0;

                exeProcess.Start();

                // Start the asynchronous read of the process output stream.
                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();

                // Wait for the process finish.
                exeProcess.WaitForExit();
            }

            result = _result;

            return _resultType;
        }
        private void CHDOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the process command output.
            if (string.IsNullOrEmpty(outLine.Data))
            {
                return;
            }

            string sOut = outLine.Data;
            //ReportError.LogOut("CHDOutput: " + _outputLineCount + " : " + sOut);
            switch (_outputLineCount)
            {
                case 0:
                    if (!Regex.IsMatch(sOut, @"^chdman - MAME Compressed Hunks of Data \(CHD\) manager ([0-9\.]+) \(.*\)"))
                    {
                        _result = "Incorrect startup of CHDMan :" + sOut;
                        _resultType = hdErr.HDERR_CANT_VERIFY;
                    }
                    break;
                case 1:
                    if (sOut != "Raw SHA1 verification successful!")
                    {
                        _result = "Raw SHA1 check failed :" + sOut;
                        _resultType = hdErr.HDERR_DECOMPRESSION_ERROR;
                    }
                    break;
                case 2:
                    if (sOut != "Overall SHA1 verification successful!")
                    {
                        _result = "Overall SHA1 check failed :" + sOut;
                        _resultType = hdErr.HDERR_DECOMPRESSION_ERROR;
                    }
                    break;
                default:
                    // chdman copy non-error
                    if (!Regex.IsMatch(sOut, @"^Logical size: ([0-9\.]+) \(.*\)"))
                    {
                        _result = sOut;
                        _resultType = hdErr.HDERR_NONE;
                    }
                    else 
                    {
                        _result = "Unexpected output from chdman :" + sOut;
                        _resultType = hdErr.HDERR_DECOMPRESSION_ERROR;
                    }
                    break;
            }

            _outputLineCount++;
        }

        private void CHDErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the process command output.
            if (string.IsNullOrEmpty(outLine.Data))
            {
                return;
            }

            // We can get fed multiple lines worth of data because of \r line feeds
            string[] sLines = outLine.Data.Split(new[] { "\r" }, StringSplitOptions.None);

            foreach (string sLine in sLines)
            {
                if (String.IsNullOrEmpty(sLine))
                {
                    continue;
                }
                // _progress?.Invoke(sLine);

                if (_resultType != hdErr.HDERR_NONE)
                {
                    if (_errorLines > 0)
                    {
                        _errorLines -= 1;
                        _result += "\r\n" + sLine;
                    }
                }
                else if (Regex.IsMatch(sLine, @"^No verification to be done; CHD has (uncompressed|no checksum)"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_CANT_VERIFY;
                }
                else if (Regex.IsMatch(sLine, @"^Error (opening|reading) CHD file.*"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_DECOMPRESSION_ERROR;
                }
                else if (Regex.IsMatch(sLine, @"^Error opening parent CHD file .*:"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_CANNOT_OPEN_FILE;
                }
                else if (Regex.IsMatch(sLine, @"^Error: (Raw|Overall) SHA1 in header"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_DECOMPRESSION_ERROR;
                }
                else if (Regex.IsMatch(sLine, @"^Out of memory"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_OUT_OF_MEMORY;
                }
                // Verifying messages are a non-error
                else if (Regex.IsMatch(sLine, @"Verifying, \d+\.\d+\% complete\.\.\."))
                {
                }
               // copy messages are a non-error
                else if (Regex.IsMatch(sLine, @"Compressing, \d+\.\d+\% complete\.\.\."))
                {
                }
               // copy success messages are a non-error
                else if (Regex.IsMatch(sLine, @"Compression complete \.\.\."))
                {
                }
                else if (Regex.IsMatch(sLine, @"^Error: file already exists"))
                {
                    _result = sLine;
                    _resultType = hdErr.HDERR_WRITE_ERROR;
                }
                else
                {
                    _result = "Unknown message : " + sLine;
                    _resultType = hdErr.HDERR_INVALID_FILE;
                }
            }
        }

    }
}
