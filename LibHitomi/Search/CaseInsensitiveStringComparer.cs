using System;
using System.Collections.Generic;
using System.Text;

namespace LibHitomi.Search
{
    class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return base.GetHashCode();
        }
    }
    static class StringExtensions
    {
        public static bool CaseInsensitiveEquals(this string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public static bool CaseInsensitiveContains(this string x, string y)
        {
            return x.IndexOf(y, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
