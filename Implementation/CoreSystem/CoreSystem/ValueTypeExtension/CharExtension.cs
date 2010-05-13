using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreSystem.RefTypeExtension;

namespace CoreSystem.ValueTypeExtension
{
    public static class CharExtension
    {
        public static bool In(this char value, params char[] chars)
        {
            foreach (char c in chars)
                if (c == value)
                    return true;

            return false;
        }
    }
}
