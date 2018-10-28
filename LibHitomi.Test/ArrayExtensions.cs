using System;
using System.Collections.Generic;
using System.Text;

namespace LibHitomi.Test
{
    public static class ArrayExtensions
    {
        public static Array Sort(this Array arr)
        {
            Array.Sort(arr);
            return arr;
        }
    }
}
