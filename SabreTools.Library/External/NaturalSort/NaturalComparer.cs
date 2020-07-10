/*
 * 
 * Links for info and original source code:
 * 
 * https://blog.codinghorror.com/sorting-for-humans-natural-sort-order/
 * http://www.codeproject.com/Articles/22517/Natural-Sort-Comparer
 *
 * Exact code implementation used with permission, originally by motoschifo
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalSort
{
	public class NaturalComparer : Comparer<string>, IDisposable
	{
		private Dictionary<string, string[]> table;

		public NaturalComparer()
		{
			table = new Dictionary<string, string[]>();
		}

		public void Dispose()
		{
			table.Clear();
			table = null;
		}

		public override int Compare(string x, string y)
		{
			if (x.ToLowerInvariant() == y.ToLowerInvariant())
			{
				return x.CompareTo(y);
			}
			if (!table.TryGetValue(x, out string[] x1))
			{
				//x1 = Regex.Split(x.Replace(" ", string.Empty), "([0-9]+)");
				x1 = Regex.Split(x.ToLowerInvariant(), "([0-9]+)").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				table.Add(x, x1);
			}
			if (!table.TryGetValue(y, out string[] y1))
			{
				//y1 = Regex.Split(y.Replace(" ", string.Empty), "([0-9]+)");
				y1 = Regex.Split(y.ToLowerInvariant(), "([0-9]+)").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				table.Add(y, y1);
			}

			for (int i = 0; i < x1.Length && i < y1.Length; i++)
			{
				if (x1[i] != y1[i])
				{
					return PartCompare(x1[i], y1[i]);
				}
			}
			if (y1.Length > x1.Length)
			{
				return 1;
			}
			else if (x1.Length > y1.Length)
			{
				return -1;
			}
			else
			{
				return x.CompareTo(y);
			}
		}

		private static int PartCompare(string left, string right)
		{
			if (!long.TryParse(left, out long x))
			{
				return NaturalComparerUtil.CompareNumeric(left, right);
			}

			if (!long.TryParse(right, out long y))
			{
				return NaturalComparerUtil.CompareNumeric(left, right);
			}

			// If we have an equal part, then make sure that "longer" ones are taken into account
			if (x.CompareTo(y) == 0)
			{
				return left.Length - right.Length;
			}

			return x.CompareTo(y);
		}
	}
}
