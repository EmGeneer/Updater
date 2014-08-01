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

        private readonly StringBuilder mStructure;

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
        internal void CreateStructure()
        {
            mStructure.Append(Header + ": ");

            #region Add the lines from parse function to a list and read the readables with regex
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

            Regex regex = new Regex(@ClassManager.RegexString);
            var match = regex.Matches(classLinesText.ToString());
            #endregion

            foreach (var Match in match)
            {
                ProgressRegexMatch(LinesOfParseMethod, Match);
            }

            Console.WriteLine(mStructure.ToString());
        }

        internal void ProgressRegexMatch(List<string> LinesOfMethod, object Match)
        {
            if (Match.ToString().Contains("while"))
            {
                // analyze everything, no check
                mStructure.Append("{loop}");
                ReadOutWhileLoop(LinesOfMethod);
                mStructure.Append("{/loop}");
            }
            else if (Match.ToString().Contains("new"))
            {
                // check if this instance is already analyzed in a while loop
                mStructure.Append("{object}");
                ReadOutObject(LinesOfMethod, Match.ToString());
                mStructure.Append("{/object}");
            }
            else
            {
                // add to structure builder, no check needed
                mStructure.Append(ConvertStringToChar(Match.ToString()) + ",");
            }
        }

        internal List<string> ReadOutWhileLoop(List<string> LinesOfMethod)
        {
            #region Reading lines of while loop
            List<string> LinesOfWhileLoop = new List<string>();
            bool isReading = false;
            LinesOfWhileLoop.Clear();

            foreach (string Line in LinesOfMethod)
            {
                if (Line.Contains("while"))
                {
                    isReading = true;
                    LinesOfWhileLoop.Add(Line);
                }

                if (isReading)
                {
                    LinesOfWhileLoop.Add(Line);
                }

                if (Line.Contains("};") && isReading)
                {
                    LinesOfWhileLoop.Add(Line);
                    isReading = false;
                }
            }
            #endregion

            #region Progress matches
            StringBuilder classLinesText = new StringBuilder();
            foreach (string Line in LinesOfWhileLoop)
            {
                classLinesText.AppendLine(Line);
            }

            Regex regex = new Regex(@ClassManager.RegexString);
            var match = regex.Matches(classLinesText.ToString());

            foreach (var xMatch in match)
            {
                // Fixed endless loop
                if (!xMatch.ToString().Contains("while"))
                {
                    ProgressRegexMatch(LinesOfWhileLoop, xMatch);
                }
            }
            #endregion

            return LinesOfWhileLoop;
        }

        internal void ReadOutObject(List<string> LinesOfMethod, string Match)
        {
            string ClassName = Match.Substring(Match.IndexOf("new ")).Split('(')[0].Replace("new ", "");
            HabboClass Class = null;

            if (ClassManager.CachedHabboClasses.TryGetValue(ClassName, out Class))
            {
                List<string> LinesOfObject = new List<string>();
                bool isReading = false;

                #region Reading lines from object constructor
                foreach (string Line in Class.ClassLines)
                {
                    if (Line.Contains("public function " + Class.ClassId) && !isReading)
                    {
                        isReading = true;
                        LinesOfObject.Add(Line);
                    }

                    if (isReading)
                    {
                        LinesOfObject.Add(Line);
                    }

                    if (Line.Contains("}") && isReading)
                    {
                        LinesOfObject.Add(Line);
                        isReading = false;
                    }
                }
                #endregion
                #region Progress matches
                StringBuilder classLinesText = new StringBuilder();
                foreach (string Line in LinesOfObject)
                {
                    classLinesText.AppendLine(Line);
                }

                Regex regex = new Regex(@ClassManager.RegexString);
                var match = regex.Matches(classLinesText.ToString());

                foreach (var xMatch in match)
                {
                    ProgressRegexMatch(LinesOfObject, xMatch);
                }
                #endregion
            }
        }
        
        internal bool ConstructorContainsReadables(string Line)
        {
            Regex regex = new Regex(@ClassManager.RegexString);
            var match = regex.Matches(Line);

            if (match.Count == 0)
                return false;
            else
                return true;
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
            {
                return "UNDEFINED";
            }
        }

        public override string ToString()
        {
            return mStructure.ToString();
        }
    }
}
