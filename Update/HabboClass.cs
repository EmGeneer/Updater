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

        internal Dictionary<int, string> HabboConnectionMessageEvents;

        internal HabboClass(string ClassId, List<string> ClassLines, string ParserClass)
        {
            this.ClassId = ClassId;
            this.ClassLines = ClassLines;
            this.ParserClass = ParserClass;
            this.HabboConnectionMessageEvents = new Dictionary<int, string>();
        }

        internal void ReadAllHabboConnectionMessageEvents()
        {
            List<string> FoundLines = new List<string>();
            bool isReading = true;

            foreach (string Line in ClassLines)
            {
                if (Line.Contains("public function IncomingMessages(_arg1:"))
                {
                    isReading = true;
                }

                if (isReading)
                {
                    FoundLines.Add(Line);
                }

                if (Line.Contains("}") && isReading)
                {
                    isReading = false;
                }
            }

            int LineIndex = 1;

            foreach (string Line in FoundLines)
            {
                if (Line.Contains("addHabboConnectionMessageEvent"))
                {
                    string ClassName = Line.Split('(')[1].Split('(')[0].Replace("new ", "");
                    HabboConnectionMessageEvents.Add(LineIndex, ClassName);
                }

                LineIndex++;
            }
        }
    }
}
