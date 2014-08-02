using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update
{
    sealed class SearchResult
    {
        internal int Header { get; set; }
        internal double MethodResult { get; set; }
        internal int StructureDifference { get; set; }
        internal string ParserClassName { get; set; }
        internal string OldStructure { get; set; }
        internal string NewStructure { get; set; }
    }
}
