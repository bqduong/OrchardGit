using System;
using System.Linq;

namespace Magelia.WebStore.Extensions
{
    internal static class StringExtensions
    {
        public static Boolean EqualsOrdinalIgnoreCase(this String @string, params String[] compares)
        {
            return compares.Any(c => @string.Equals(c, StringComparison.OrdinalIgnoreCase));
        }
    }
}