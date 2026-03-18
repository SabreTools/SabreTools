using System;

namespace SabreTools.Core.Tools
{
    // TODO: Replace when IO is updated
    public static class DateTimeHelper
    {
        // <summary>
        /// Convert .NET DateTime to MS-DOS date format
        /// </summary>
        /// <remarks>Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:FileTimeToDosTime</remarks>
        public static long ConvertToMsDosTimeFormat(DateTime dateTime)
        {
            uint year = (uint)((dateTime.Year - 1980) % 128);
            uint mon = (uint)dateTime.Month;
            uint day = (uint)dateTime.Day;
            uint hour = (uint)dateTime.Hour;
            uint min = (uint)dateTime.Minute;
            uint sec = (uint)dateTime.Second;

            return (year << 25) | (mon << 21) | (day << 16) | (hour << 11) | (min << 5) | (sec >> 1);
        }

        /// <summary>
        /// Convert MS-DOS date format to .NET DateTime
        /// </summary>
        /// <remarks>Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:DosTimeToFileTime</remarks>
        public static DateTime ConvertFromMsDosTimeFormat(uint msDosDateTime)
        {
            return new DateTime((int)(1980 + (msDosDateTime >> 25)), (int)((msDosDateTime >> 21) & 0xF), (int)((msDosDateTime >> 16) & 0x1F),
                (int)((msDosDateTime >> 11) & 0x1F), (int)((msDosDateTime >> 5) & 0x3F), (int)((msDosDateTime & 0x1F) * 2));
        }
    }
}
