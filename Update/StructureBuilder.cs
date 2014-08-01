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

            Regex regex = new Regex(@"\b(readInteger|readString|readBoolean|readShort|readByte|readFloat|while|new .*\(_arg1)\b");
            var match = regex.Matches(classLinesText.ToString());
            #endregion

            foreach (var Match in match)
            {
                if (Match.ToString().Contains("while"))
                {
                    #region Add Lines from while loop to a list
                    List<string> LinesOfWhileLoop = new List<string>();
                    bool isReadingWhileLoop = false;

                    foreach (string Line in LinesOfParseMethod)
                    {
                        if (Line.Contains("while ("))
                        {
                            LinesOfWhileLoop.Add(Line);
                            isReadingWhileLoop = true;
                        }

                        if (isReadingWhileLoop)
                        {
                            LinesOfWhileLoop.Add(Line);
                        }

                        if (Line == "};")
                        {
                            LinesOfWhileLoop.Add(Line);
                            isReadingWhileLoop = false;
                        }
                    }
                    #endregion
                    #region Check the while loop for readables
                    foreach (string Line in LinesOfWhileLoop)
                    {
                        // check if constructor contains readables
                        if (Line.Contains("= new ") || Line.Contains("(new "))
                        {
                            if (!ConstructorContainsReadables(Line))
                            {
                                int IndexOfNew = Line.IndexOf("new ");
                                string ClassNameToRead = Line.Substring(IndexOfNew).Split('(')[0].Replace("new ", "");
                                HabboClass NewClassToRead = this.ClassManager.GetClassByName(ClassNameToRead);

                                if (NewClassToRead != null)
                                {
                                    List<string> LinesOfConstructor = new List<string>();

                                    bool isReadingConstructor = false;

                                    foreach (string nLine in NewClassToRead.ClassLines)
                                    {
                                        if (nLine.Contains("public function " + NewClassToRead.ClassId) && !isReadingConstructor)
                                        {
                                            LinesOfConstructor.Add(nLine);
                                            isReadingConstructor = true;
                                        }

                                        if (isReadingConstructor)
                                        {
                                            LinesOfConstructor.Add(nLine);
                                        }

                                        if (nLine == "}" && isReadingConstructor)
                                        {
                                            LinesOfParseMethod.Add(nLine);
                                            isReadingConstructor = false;
                                        }
                                    }

                                    StringBuilder constructorLines = new StringBuilder();
                                    foreach (string nLine in LinesOfConstructor)
                                    {
                                        constructorLines.AppendLine(nLine);
                                    }

                                    Regex nregex = new Regex(@"\b(readInteger|readString|readBoolean|readShort|readByte|readFloat|while|new .*\(_arg1)\b");
                                    var nmatch = regex.Matches(constructorLines.ToString());

                                    if (nmatch.Count > 0)
                                    {
                                        mStructure.Append("{loop}");
                                        foreach (var smatch in nmatch)
                                        {
                                            mStructure.Append(ConvertStringToChar(smatch.ToString()) + ",");
                                        }
                                        mStructure.Append("{/loop}");
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (Match.ToString().Contains("new "))
                {
                    mStructure.Append("{object}");
                    #region get readables from object
                    string NewClassName = Match.ToString().Split(' ')[1].Split('(')[0];
                    HabboClass Class = null;
                    if (this.ClassManager.CachedHabboClasses.TryGetValue(NewClassName, out Class))
                    {
                        if (Class != null)
                        {
                            List<string> LinesOfConstructor = new List<string>();

                            bool isReadingConstructor = false;

                            foreach (string nLine in Class.ClassLines)
                            {
                                if (nLine.Contains("public function " + Class.ClassId) && !isReadingConstructor)
                                {
                                    LinesOfConstructor.Add(nLine);
                                    isReadingConstructor = true;
                                }

                                if (isReadingConstructor)
                                {
                                    LinesOfConstructor.Add(nLine);
                                }

                                if (nLine == "}" && isReadingConstructor)
                                {
                                    LinesOfParseMethod.Add(nLine);
                                    isReadingConstructor = false;
                                }
                            }

                            StringBuilder constructorLines = new StringBuilder();
                            foreach (string nLine in LinesOfConstructor)
                            {
                                constructorLines.AppendLine(nLine);
                            }

                            Regex nregex = new Regex(@"\b(readInteger|readString|readBoolean|readShort|readByte|readFloat|while|new .*\(_arg1)\b");
                            var nmatch = regex.Matches(constructorLines.ToString());

                            if (nmatch.Count > 0)
                            {
                                foreach (var smatch in nmatch)
                                {
                                    if (smatch.ToString().Contains("new"))
                                    {
                                        // get the function class again... have to rewrite the whole class :)
                                        // todo: write a clean plan how to parse whiles in whiles and objects in objects..
                                    }
                                    else
                                    {
                                        mStructure.Append(ConvertStringToChar(smatch.ToString()) + ",");
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                    mStructure.Append("{/object}");
                }
                else
                {
                    mStructure.Append(ConvertStringToChar(Match.ToString()) + ",");
                }
            }

            Console.WriteLine("{0}: {1}", Header, mStructure.ToString());
        }

        internal void GetStructureForConstructor(List<string> Lines)
        {

        }

        internal void GetStructureForWhileLoop(List<string> Lines)
        {

        }

        internal bool ConstructorContainsReadables(string Line)
        {
            Regex regex = new Regex(@"\b(readInteger|readString|readBoolean|readShort|readByte|readFloat)\b");
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
                return "UNDEFINED";
        }

        public override string ToString()
        {
            return mStructure.ToString();
        }
    }
}
