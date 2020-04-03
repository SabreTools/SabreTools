
namespace Compress.Utils
{
    public static class Reporter
    {
        public static string ToArrayString(this ulong[] arr)
        {
            if (arr == null)
                return "NULL";

            string ret = $"({arr.Length}) " + arr[0].ToString();
            for (int i = 1; i < arr.Length; i++)
            {
                ret += "," + arr[i].ToString();
            }

            return ret;
        }
        public static string ToArrayString(this byte[] arr)
        {
            if (arr == null)
                return "NULL";

            string ret = $"({arr.Length}) " + arr[0].ToString("X2");
            for (int i = 1; i < arr.Length; i++)
            {
                ret += "," + arr[i].ToString("X2");
            }

            return ret;
        }


        public static string ToHex(this uint? v)
        {
            return v == null ? "NULL" : ((uint)v).ToString("X8");
        }
        public static string ToHex(this ulong? v)
        {
            return v == null ? "NULL" : ((ulong)v).ToString("X8");
        }
    }
}