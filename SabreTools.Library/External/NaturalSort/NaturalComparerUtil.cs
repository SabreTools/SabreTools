using System.IO;

namespace NaturalSort
{
    public static class NaturalComparerUtil
    {
		public static int CompareNumeric(string s1, string s2)
		{
			// Save the orginal strings, for later comparison
			string s1orig = s1;
			string s2orig = s2;

			// We want to normalize the strings, so we set both to lower case
			s1 = s1.ToLowerInvariant();
			s2 = s2.ToLowerInvariant();

			// If the strings are the same exactly, return
			if (s1 == s2)
				return s1orig.CompareTo(s2orig);

			// If one is null, then say that's less than
			if (s1 == null)
				return -1;
			if (s2 == null)
				return 1;

			// Now split into path parts after converting AltDirSeparator to DirSeparator
			s1 = s1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			s2 = s2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string[] s1parts = s1.Split(Path.DirectorySeparatorChar);
			string[] s2parts = s2.Split(Path.DirectorySeparatorChar);

			// Then compare each part in turn
			for (int j = 0; j < s1parts.Length && j < s2parts.Length; j++)
			{
				int compared = CompareNumericPart(s1parts[j], s2parts[j]);
				if (compared != 0)
					return compared;
			}

			// If we got out here, then it looped through at least one of the strings
			if (s1parts.Length > s2parts.Length)
				return 1;
			if (s1parts.Length < s2parts.Length)
				return -1;

			return s1orig.CompareTo(s2orig);
		}

		private static int CompareNumericPart(string s1, string s2)
		{
			// Otherwise, loop through until we have an answer
			for (int i = 0; i < s1.Length && i < s2.Length; i++)
			{
				int s1c = s1[i];
				int s2c = s2[i];

				// If the characters are the same, continue
				if (s1c == s2c)
					continue;

				// If they're different, check which one was larger
				if (s1c > s2c)
					return 1;
				if (s1c < s2c)
					return -1;
			}

			// If we got out here, then it looped through at least one of the strings
			if (s1.Length > s2.Length)
				return 1;
			if (s1.Length < s2.Length)
				return -1;

			return 0;
		}
	}
}
