using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Update
{
    sealed class HabboClass
    {
        internal string ClassId { get; set; }
        internal string ParserClass { get; set; }
        internal List<string> ClassLines { get; set; }

        internal HabboClass(string ClassId, List<string> ClassLines, string ParserClass)
        {
            this.ClassId = ClassId;
            this.ClassLines = ClassLines;
            this.ParserClass = ParserClass;
        }
    }
}
