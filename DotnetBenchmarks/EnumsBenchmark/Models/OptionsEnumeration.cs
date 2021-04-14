using System.Collections.Generic;
using System.Linq;

namespace EnumsBenchmark.Models
{
    public sealed class OptionsEnumeration : Enumeration<OptionsEnumeration>
    {
        private OptionsEnumeration(int id, string name, string description) : base(id, name, description)
        {
        }

        public static readonly OptionsEnumeration Read = new(0, "Read", "Read");
        public static readonly OptionsEnumeration Write = new(1, "Write", "Write");
        public static readonly OptionsEnumeration Delete = new(2, "Delete", "Delete");
    }
}