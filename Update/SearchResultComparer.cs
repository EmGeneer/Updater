using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update
{
    class SearchResultComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            if (x.GetType() != y.GetType())
                throw new ArgumentException("Can't compare two different types..");

            return ((SearchResult)x).StructureDifference.CompareTo(((SearchResult)y).StructureDifference);
        }
    }
}
