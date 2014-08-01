using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Update
{
    sealed class StructureBuilder
    {
        private readonly int Header;
        private readonly HabboClassManager ClassManager;
        private readonly HabboClass MainClass;
        private readonly HabboClass ParserClass;

        private StringBuilder mStructure;

        internal StructureBuilder(int Header, HabboClassManager Manager, HabboClass Class, HabboClass ParserClass)
        {
            this.Header = Header;
            this.ClassManager = Manager;
            this.MainClass = Class;
            this.ParserClass = ParserClass;

            this.mStructure = new StringBuilder();
        }

        /// <summary>
        /// Read structure from Parser class
        /// 1. Read all lines contains readInteger|readString etc..
        /// 2. Find while() loops and analize them for more readables
        /// 3. Find new created objects and analize them for more readables
        /// </summary>
        
        // I know the LinesOfParse and LineOfWhileLoop are hard coded :)
        internal void BuildStructure()
        {
            List<string> LinesOfParseMethod = new List<string>();

            bool isReading = false;

            foreach (string Line in ParserClass.ClassLines)
            {
                if (Line.Contains("public function parse") && !isReading)
                {
                    LinesOfParseMethod.Add(Line);
                    isReading = true;
                }

                if (isReading)
                {
                    LinesOfParseMethod.Add(Line);
                }

                if (Line == "}" && isReading)
                {
                    LinesOfParseMethod.Add(Line);
                    isReading = false;
                }
            }

            StringBuilder classLinesText = new StringBuilder();
            foreach (string Line in LinesOfParseMethod)
            {
                classLinesText.AppendLine(Line);
            }

            Regex regex = new Regex(@"\b(readInteger|readString|readBoolean|readShort|readByte|readFloat|while)\b");
            var match = regex.Matches(classLinesText.ToString());

            Console.WriteLine("Found {0} matches.", match.Count);

            foreach (var Match in match)
            {
                if (Match.ToString().Contains("while"))
                {
                    List<string> LinesOfWhileLoop = new List<string>();
                }
                else
                {
                    mStructure.Append(ConvertStringToChar(Match.ToString()) + ",");
                }
            }

            Console.WriteLine("{0}: {1}", Header, mStructure.ToString());
        }

        internal string ConvertStringToChar(string Input)
        {
            if (Input == "readInteger")
                return "I";
            else if (Input == "readString")
                return "S";
            else if (Input == "readBoolean")
                return "B";
            else if (Input == "readShort")
                return "SH";
            else if (Input == "readByte")
                return "BYTE";
            else if (Input == "readFloat")
                return "F";
            else
                return "";
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
