using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace RVIO
{
    [Flags]
    [ComVisible(true)]
    [Serializable]
    public enum FileAttributes
    {
        ReadOnly = 1,
        Hidden = 2,
        System = 4,
        Directory = 16,
        Archive = 32,
        Device = 64,
        Normal = 128,
        Temporary = 256,
        SparseFile = 512,
        ReparsePoint = 1024,
        Compressed = 2048,
        Offline = 4096,
        NotContentIndexed = 8192,
        Encrypted = 16384,
    }
    public static class Error
    {
        public static int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }
    }

    public static class unix
    {
        public static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 6) || (p == 128));
            }
        }
    }

    public class FileInfo
    {

        public string Name;
        public string FullName;
        public long LastWriteTime;
        public long Length;

        public FileInfo()
        { }

        public FileInfo(string path)
        {
            FullName = path;
            Name = Path.GetFileName(path);

            if (unix.IsUnix)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(path);

                if (!fi.Exists) return;

                Length = fi.Length;
                LastWriteTime = fi.LastWriteTimeUtc.Ticks;
                return;
            }

            string fileName = NameFix.AddLongPathPrefix(path);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA wIn32FileAttributeData = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();

            bool b = Win32Native.GetFileAttributesEx(fileName, 0, ref wIn32FileAttributeData);

            if (!b || (wIn32FileAttributeData.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) != 0) return;

            Length = Convert.Length(wIn32FileAttributeData.fileSizeHigh, wIn32FileAttributeData.fileSizeLow);
            LastWriteTime = Convert.Time(wIn32FileAttributeData.ftLastWriteTimeHigh, wIn32FileAttributeData.ftLastWriteTimeLow);
        }

    }

    public class DirectoryInfo
    {
        public string Name;
        public string FullName;
        public long LastWriteTime;

        public DirectoryInfo()
        { }
        public DirectoryInfo(string path)
        {
            FullName = path;
            Name = Path.GetFileName(path);

            if (unix.IsUnix)
            {
                System.IO.DirectoryInfo fi = new System.IO.DirectoryInfo(path);

                if (!fi.Exists) return;

                LastWriteTime = fi.LastWriteTimeUtc.Ticks;
                return;
            }

            string fileName = NameFix.AddLongPathPrefix(path);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA wIn32FileAttributeData = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();

            bool b = Win32Native.GetFileAttributesEx(fileName, 0, ref wIn32FileAttributeData);

            if (!b || (wIn32FileAttributeData.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) == 0) return;
            LastWriteTime = Convert.Time(wIn32FileAttributeData.ftLastWriteTimeHigh, wIn32FileAttributeData.ftLastWriteTimeLow);
        }



        public DirectoryInfo[] GetDirectories(bool includeHidden = true)
        {
            return GetDirectories("*", includeHidden);
        }
        public DirectoryInfo[] GetDirectories(string SearchPattern, bool includeHidden = true)
        {
            List<DirectoryInfo> dirs = new List<DirectoryInfo>();

            if (unix.IsUnix)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FullName);
                System.IO.DirectoryInfo[] arrDi = di.GetDirectories(SearchPattern);
                foreach (System.IO.DirectoryInfo tDi in arrDi)
                {
                    DirectoryInfo lDi = new DirectoryInfo
                    {
                        Name = tDi.Name,
                        FullName = Path.Combine(FullName, tDi.Name),
                        LastWriteTime = tDi.LastWriteTimeUtc.Ticks
                    };
                    dirs.Add(lDi);
                }
                return dirs.ToArray();
            }



            string dirName = NameFix.AddLongPathPrefix(FullName);

            Win32Native.WIN32_FIND_DATA findData = new Win32Native.WIN32_FIND_DATA();
            SafeFindHandle findHandle = Win32Native.FindFirstFile(dirName + @"\" + SearchPattern, findData);

            if (!findHandle.IsInvalid)
            {
                do
                {
                    string currentFileName = findData.cFileName;

                    // if this is a directory, find its contents
                    if ((findData.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) == 0) continue;
                    if (currentFileName == "." || currentFileName == "..") continue;
                    if (!includeHidden && (findData.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_HIDDEN) != 0) continue;

                    DirectoryInfo di = new DirectoryInfo
                    {
                        Name = currentFileName,
                        FullName = Path.Combine(FullName, currentFileName),
                        LastWriteTime = Convert.Time(findData.ftLastWriteTimeHigh, findData.ftLastWriteTimeLow)
                    };
                    dirs.Add(di);
                }
                while (Win32Native.FindNextFile(findHandle, findData));
            }

            // close the find handle
            findHandle.Dispose();

            return dirs.ToArray();
        }

        public FileInfo[] GetFiles()
        {
            return GetFiles("*");
        }
        public FileInfo[] GetFiles(string SearchPattern, bool includeHidden = true)
        {
            List<FileInfo> files = new List<FileInfo>();

            if (unix.IsUnix)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FullName);
                System.IO.FileInfo[] arrDi = di.GetFiles(SearchPattern);
                foreach (System.IO.FileInfo tDi in arrDi)
                {
                    FileInfo lDi = new FileInfo
                    {
                        Name = tDi.Name,
                        FullName = Path.Combine(FullName, tDi.Name),
                        Length = tDi.Length,
                        LastWriteTime = tDi.LastWriteTimeUtc.Ticks
                    };
                    files.Add(lDi);
                }
                return files.ToArray();
            }

            string dirName = NameFix.AddLongPathPrefix(FullName);

            Win32Native.WIN32_FIND_DATA findData = new Win32Native.WIN32_FIND_DATA();
            SafeFindHandle findHandle = Win32Native.FindFirstFile(dirName + @"\" + SearchPattern, findData);

            if (!findHandle.IsInvalid)
            {
                do
                {
                    string currentFileName = findData.cFileName;

                    // if this is a directory, find its contents
                    if ((findData.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) != 0) continue;
                    if (!includeHidden && (findData.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_HIDDEN) != 0) continue;

                    FileInfo fi = new FileInfo
                    {
                        Name = currentFileName,
                        FullName = Path.Combine(FullName, currentFileName),
                        Length = Convert.Length(findData.nFileSizeHigh, findData.nFileSizeLow),
                        LastWriteTime = Convert.Time(findData.ftLastWriteTimeHigh, findData.ftLastWriteTimeLow)
                    };
                    files.Add(fi);
                }
                while (Win32Native.FindNextFile(findHandle, findData));
            }

            // close the find handle
            findHandle.Dispose();

            return files.ToArray();
        }
    }

    public static class Directory
    {
        public static bool Exists(string path)
        {
            if (unix.IsUnix)
                return System.IO.Directory.Exists(path);


            string fixPath = NameFix.AddLongPathPrefix(path);

            Win32Native.WIN32_FILE_ATTRIBUTE_DATA wIn32FileAttributeData = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();

            bool b = Win32Native.GetFileAttributesEx(fixPath, 0, ref wIn32FileAttributeData);
            return b && (wIn32FileAttributeData.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) != 0;
        }
        public static void Move(string sourceDirName, string destDirName)
        {
            if (unix.IsUnix)
            {
                System.IO.Directory.Move(sourceDirName, destDirName);
                return;
            }


            if (sourceDirName == null)
                throw new ArgumentNullException("sourceDirName");
            if (sourceDirName.Length == 0)
                throw new ArgumentException("Argument_EmptyFileName", "sourceDirName");

            if (destDirName == null)
                throw new ArgumentNullException("destDirName");
            if (destDirName.Length == 0)
                throw new ArgumentException("Argument_EmptyFileName", "destDirName");

            string fullsourceDirName = NameFix.AddLongPathPrefix(sourceDirName);

            string fulldestDirName = NameFix.AddLongPathPrefix(destDirName);

            if (!Win32Native.MoveFile(fullsourceDirName, fulldestDirName))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr == Win32Native.ERROR_FILE_NOT_FOUND) // Source dir not found
                {
                    throw new Exception("ERROR_PATH_NOT_FOUND " + fullsourceDirName);
                }
                if (hr == Win32Native.ERROR_ACCESS_DENIED) // WinNT throws IOException. This check is for Win9x. We can't change it for backcomp.
                {
                    throw new Exception("UnauthorizedAccess_IODenied_Path" + sourceDirName);
                }
            }
        }
        public static void Delete(string path)
        {
            if (unix.IsUnix)
            {
                System.IO.Directory.Delete(path);
                return;
            }

            string fullPath = NameFix.AddLongPathPrefix(path);

            Win32Native.RemoveDirectory(fullPath);
        }

        public static void CreateDirectory(string path)
        {
            if (unix.IsUnix)
            {
                System.IO.Directory.CreateDirectory(path);
                return;
            }


            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("Argument_PathEmpty");

            string fullPath = NameFix.AddLongPathPrefix(path);

            Win32Native.CreateDirectory(fullPath, IntPtr.Zero);
        }
    }

    public static class File
    {
        public static bool Exists(string path)
        {
            if (unix.IsUnix)
                return System.IO.File.Exists(path);


            string fixPath = NameFix.AddLongPathPrefix(path);

            Win32Native.WIN32_FILE_ATTRIBUTE_DATA wIn32FileAttributeData = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();

            bool b = Win32Native.GetFileAttributesEx(fixPath, 0, ref wIn32FileAttributeData);
            return b && (wIn32FileAttributeData.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) == 0;
        }
        public static void Copy(string sourceFileName, string destfileName)
        {
            Copy(sourceFileName, destfileName, true);
        }
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (unix.IsUnix)
            {
                System.IO.File.Copy(sourceFileName, destFileName, overwrite);
                return;
            }

            if (sourceFileName == null || destFileName == null)
                throw new ArgumentNullException((sourceFileName == null ? "sourceFileName" : "destFileName"), "ArgumentNull_FileName");
            if (sourceFileName.Length == 0 || destFileName.Length == 0)
                throw new ArgumentException("Argument_EmptyFileName", (sourceFileName.Length == 0 ? "sourceFileName" : "destFileName"));

            string fullSourceFileName = NameFix.AddLongPathPrefix(sourceFileName);
            string fullDestFileName = NameFix.AddLongPathPrefix(destFileName);

            bool r = Win32Native.CopyFile(fullSourceFileName, fullDestFileName, !overwrite);
            if (!r)
            {
                // Save Win32 error because subsequent checks will overwrite this HRESULT.
                int errorCode = Marshal.GetLastWin32Error();
                string fileName = destFileName;

                /*
                if (errorCode != Win32Native.ERROR_FILE_EXISTS)
                {
                    // For a number of error codes (sharing violation, path
                    // not found, etc) we don't know if the problem was with
                    // the source or dest file.  Try reading the source file.
                    using (SafeFileHandle handle = Win32Native.UnsafeCreateFile(fullSourceFileName, FileStream.GENERIC_READ, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero))
                    {
                        if (handle.IsInvalid)
                            fileName = sourceFileName;
                    }

                    if (errorCode == Win32Native.ERROR_ACCESS_DENIED)
                    {
                        if (Directory.InternalExists(fullDestFileName))
                            throw new IOException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_FileIsDirectory_Name"), destFileName), Win32Native.ERROR_ACCESS_DENIED, fullDestFileName);
                    }
                }

                __Error.WinIOError(errorCode, fileName);

                 */
            }
        }
        public static void Move(string sourceFileName, string destFileName)
        {
            if (unix.IsUnix)
            {
                System.IO.File.Move(sourceFileName, destFileName);
                return;
            }

            if (sourceFileName == null || destFileName == null)
                throw new ArgumentNullException((sourceFileName == null ? "sourceFileName" : "destFileName"), "ArgumentNull_FileName");
            if (sourceFileName.Length == 0 || destFileName.Length == 0)
                throw new ArgumentException("Argument_EmptyFileName", (sourceFileName.Length == 0 ? "sourceFileName" : "destFileName"));

            string fullSourceFileName = NameFix.AddLongPathPrefix(sourceFileName);
            string fullDestFileName = NameFix.AddLongPathPrefix(destFileName);

            if (!Exists(fullSourceFileName))
                throw new Exception("ERROR_FILE_NOT_FOUND" + fullSourceFileName);

            if (!Win32Native.MoveFile(fullSourceFileName, fullDestFileName))
            {
                int hr = Marshal.GetLastWin32Error();
                throw new Exception(GetErrorCode(hr), new Exception("ERROR_MOVING_FILE. (" + fullSourceFileName + " to " + fullDestFileName + ")"));
            }
        }

        public static void Delete(string path)
        {
            if (unix.IsUnix)
            {
                System.IO.File.Delete(path);
                return;
            }


            string fixPath = NameFix.AddLongPathPrefix(path);

            if (!Win32Native.DeleteFile(fixPath))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr != Win32Native.ERROR_FILE_NOT_FOUND)
                    throw new Exception(GetErrorCode(hr), new Exception("ERROR_DELETING_FILE. (" + path + ")"));
            }
        }


        private static string GetErrorCode(int hr)
        {
            switch (hr)
            {
                case 5: return "ERROR_ACCESS_DENIED: Access is denied.";
                case 32: return "ERROR_FILE_IN_USE: The file is in use by another process.";
                case 39: return "ERROR_HANDLE_DISK_FULL: The disk is full.";
                case 112: return "ERROR_DISK_FULL: There is not enough space on the disk.";
                case 123: return "ERROR_INVALID_NAME: The filename, directory name, or volume label syntax is incorrect.";
                case 183: return "ERROR_ALREADY_EXISTS: Cannot create a file when that file already exists.";
            }

            return hr.ToString();
        }

        public static bool SetAttributes(string path, FileAttributes fileAttributes)
        {
            if (unix.IsUnix)
            {
                try
                {
                    System.IO.File.SetAttributes(path, (System.IO.FileAttributes)fileAttributes);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            string fullPath = NameFix.AddLongPathPrefix(path);
            return Win32Native.SetFileAttributes(fullPath, (int)fileAttributes);
        }
        public static StreamWriter CreateText(string filename)
        {
            int errorCode = FileStream.OpenFileWrite(filename, out Stream fStream);
            return errorCode != 0 ? null : new StreamWriter(fStream);
        }
        public static StreamReader OpenText(string filename, Encoding Enc)
        {
            int errorCode = FileStream.OpenFileRead(filename, out Stream fStream);
            return errorCode != 0 ? null : new StreamReader(fStream, Enc);
        }

        private const int ERROR_INVALID_PARAMETER = 87;
        private const int ERROR_ACCESS_DENIED = 0x5;
    }

    public static class Path
    {
        public static readonly char DirectorySeparatorChar = '\\';
        public static readonly char AltDirectorySeparatorChar = '/';
        public static readonly char VolumeSeparatorChar = ':';

        public static string GetExtension(string path)
        {
            return System.IO.Path.GetExtension(path);
        }
        public static string Combine(string path1, string path2)
        {
            if (unix.IsUnix)
                return System.IO.Path.Combine(path1, path2);

            if (path1 == null || path2 == null)
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            //CheckInvalidPathChars(path1);
            //CheckInvalidPathChars(path2);

            if (path2.Length == 0)
                return path1;

            if (path1.Length == 0)
                return path2;

            if (IsPathRooted(path2))
                return path2;

            char ch = path1[path1.Length - 1];
            if (ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar && ch != VolumeSeparatorChar)
                return path1 + DirectorySeparatorChar + path2;
            return path1 + path2;
        }
        private static bool IsPathRooted(string path)
        {
            if (path != null)
            {
                //CheckInvalidPathChars(path);

                int length = path.Length;
                if (
                    (length >= 1 && (path[0] == DirectorySeparatorChar ||
                    path[0] == AltDirectorySeparatorChar)) ||
                    (length >= 2 && path[1] == VolumeSeparatorChar)
                    ) return true;
            }
            return false;
        }
        /*
        private static void CheckInvalidPathChars(string path)
        {
            for (int index = 0; index < path.Length; ++index)
            {
                int num = path[index];
                switch (num)
                {
                    case 34:
                    case 60:
                    case 62:
                    case 124:
                        ReportError.SendErrorMessage("Invalid Character " + num + " in filename " + path);
                        continue;
                    default:
                        if (num >= 32)
                            continue;

                        goto case 34;
                }
            }
        }
        */

        public static string GetFileNameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public static string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }
        public static string GetDirectoryName(string path)
        {
            if (unix.IsUnix)
                return System.IO.Path.GetDirectoryName(path);


            if (path != null)
            {
                int root = GetRootLength(path);
                int i = path.Length;
                if (i > root)
                {
                    i = path.Length;
                    if (i == root) return null;
                    while (i > root && path[--i] != DirectorySeparatorChar && path[i] != AltDirectorySeparatorChar) ;
                    return path.Substring(0, i);
                }
            }
            return null;
        }

        private static int GetRootLength(string path)
        {
            int i = 0;
            int length = path.Length;

            if (length >= 1 && (IsDirectorySeparator(path[0])))
            {
                // handles UNC names and directories off current drive's root.
                i = 1;
                if (length >= 2 && (IsDirectorySeparator(path[1])))
                {
                    i = 2;
                    int n = 2;
                    while (i < length && ((path[i] != DirectorySeparatorChar && path[i] != AltDirectorySeparatorChar) || --n > 0)) i++;
                }
            }
            else if (length >= 2 && path[1] == VolumeSeparatorChar)
            {
                // handles A:\foo.
                i = 2;
                if (length >= 3 && (IsDirectorySeparator(path[2]))) i++;
            }
            return i;
        }
        private static bool IsDirectorySeparator(char c)
        {
            return (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar);
        }

    }


    public static class FileStream
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        // errorMessage = new Win32Exception(errorCode).Message;

        public static Stream OpenFileRead(string path, out int result)
        {
            result = OpenFileRead(path, out Stream stream);
            return stream;
        }

        public static int OpenFileRead(string path, out Stream stream)
        {
            if (unix.IsUnix)
            {
                try
                {
                    stream = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read);
                    return 0;
                }
                catch (Exception)
                {
                    stream = null;
                    return Marshal.GetLastWin32Error();
                }
            }

            string filename = NameFix.AddLongPathPrefix(path);
            SafeFileHandle hFile = Win32Native.CreateFile(filename,
                                      GENERIC_READ,
                                      System.IO.FileShare.Read,
                                      IntPtr.Zero,
                                      FileMode.Open,
                                      FILE_ATTRIBUTE_NORMAL,
                                      IntPtr.Zero);

            if (hFile.IsInvalid)
            {
                stream = null;
                return Marshal.GetLastWin32Error();
            }
            stream = new System.IO.FileStream(hFile, FileAccess.Read);

            return 0;
        }

        public static int OpenFileWrite(string path, out Stream stream)
        {
            if (unix.IsUnix)
            {
                try
                {
                    stream = new System.IO.FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                    return 0;
                }
                catch (Exception)
                {
                    stream = null;
                    return Marshal.GetLastWin32Error();
                }
            }


            string filename = NameFix.AddLongPathPrefix(path);
            SafeFileHandle hFile = Win32Native.CreateFile(filename,
                                      GENERIC_READ | GENERIC_WRITE,
                                      System.IO.FileShare.None,
                                      IntPtr.Zero,
                                      FileMode.Create,
                                      FILE_ATTRIBUTE_NORMAL,
                                      IntPtr.Zero);

            if (hFile.IsInvalid)
            {
                stream = null;
                return Marshal.GetLastWin32Error();
            }

            stream = new System.IO.FileStream(hFile, FileAccess.ReadWrite);
            return 0;
        }


    }

    public static class NameFix
    {
        public static string GetShortPath(string path)
        {
            if (unix.IsUnix)
                return path;

            int remove = 0;
            string retPath;
            if (path.StartsWith(@"\\"))
            {
                retPath = @"\\?\UNC\" + path.Substring(2);
                remove = 8;
            }
            else
            {
                retPath = path;
                if (path.Substring(1, 1) != ":")
                    retPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), retPath);

                retPath = cleandots(retPath);
                retPath = @"\\?\" + retPath;
                remove = 4;
            }


            const int MAX_PATH = 300;
            StringBuilder shortPath = new StringBuilder(MAX_PATH);
            Win32Native.GetShortPathName(retPath, shortPath, MAX_PATH);
            retPath = shortPath.ToString();

            retPath = retPath.Substring(remove);
            if (remove == 8) retPath = "\\" + retPath;

            return retPath;
        }



        internal static string AddLongPathPrefix(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\"))
                return path;

            if (path.StartsWith(@"\\"))
                return @"\\?\UNC\" + path.Substring(2);

            string retPath = path;
            if (path.Substring(1, 1) != ":")
                retPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), retPath);

            retPath = cleandots(retPath);

            return @"\\?\" + retPath;

        }

        private static string cleandots(string path)
        {
            string retPath = path;
            while (retPath.Contains(@"\..\"))
            {
                int index = retPath.IndexOf(@"\..\");
                string path1 = retPath.Substring(0, index);
                string path2 = retPath.Substring(index + 4);

                int path1Back = path1.LastIndexOf(@"\");

                retPath = path1.Substring(0, path1Back + 1) + path2;
            }
            return retPath;

        }
    }
}