using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiteLoader.Modules
{
    internal sealed class ModuleComparer : IComparer<Module>
    {
        public static ModuleComparer Instance { get; } = new ModuleComparer();

        public int Compare(Module x, Module y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            int val = x.GetType().Name.CompareTo(y.GetType().Name);

            if (val == 0)
            {
                val = x.Version.CompareTo(y.Version);

                if (val == 0)
                {
                    val = x.Author.CompareTo(y.Author);
                }
            }

            return val;
        }
    }
}
