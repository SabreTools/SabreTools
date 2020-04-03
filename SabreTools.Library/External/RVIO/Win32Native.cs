/******************************************************
 *     ROMVault3 is written by Gordon J.              *
 *     Contact gordon@romvault.com                    *
 *     Copyright 2019                                 *
 ******************************************************/

using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace RVIO
{
    internal static class Win32Native
    {
        private const string KERNEL32 = "kernel32.dll";

        public const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const int FILE_ATTRIBUTE_HIDDEN = 0x00000002;


        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_ACCESS_DENIED = 0x5;
        internal const int ERROR_FILE_EXISTS = 0x50;

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetFileAttributesEx(string fileName, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeFindHandle FindFirstFile(string fileName, [In] [Out] WIN32_FIND_DATA data);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool FindNextFile(SafeFindHandle hndFindFile, [In] [Out] [MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);

        [DllImport(KERNEL32)]
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool FindClose(IntPtr handle);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileHandle CreateFile(string lpFileName,
            uint dwDesiredAccess, FileShare dwShareMode,
            IntPtr securityAttrs, FileMode dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool CreateDirectory(string path, IntPtr lpSecurityAttributes);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool RemoveDirectory(string path);


        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool CopyFile(string src, string dst, bool failIfExists);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool MoveFile(string src, string dst);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool DeleteFile(string path);

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool SetFileAttributes(string name, int attr);


        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string path,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
            int shortPathLength
        );


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [BestFitMapping(false)]
        internal class WIN32_FIND_DATA
        {
            internal int dwFileAttributes = 0;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal int nFileSizeHigh = 0;
            internal int nFileSizeLow = 0;
            internal int dwReserved0 = 0;
            internal int dwReserved1 = 0;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] internal string cFileName = null;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] internal string cAlternateFileName = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal int fileSizeHigh;
            internal int fileSizeLow;
        }
    }

    internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeFindHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Win32Native.FindClose(handle);
        }
    }


    internal static class Convert
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;
        public const long FileTimeOffset = DaysTo1601 * TicksPerDay;


        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;
        private const long MinTicks = 0;
        private const long MaxTicks = DaysTo10000 * TicksPerDay - 1;


        public static long Length(int high, int low)
        {
            return ((long)high << 32) | (low & 0xFFFFFFFFL);
        }

        public static long Time(uint high, uint low)
        {
            return ((long)high << 32) | low;
        }
    }
}