using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Update
{
    sealed class HabboClassManager
    {
        internal FileHolder FileHolder;
        internal Dictionary<int, HabboClass> CachedMessageComposer;
        internal Dictionary<int, HabboClass> CachedMessageEvents;
        internal Dictionary<string, HabboClass> CachedIncomingMessagesClasses;
        internal Dictionary<string, HabboClass> CachedHabboClasses;
        internal Dictionary<string, string> ReadableNamespaces;

        internal string RegexString;
        internal string Release;

        internal HabboClassManager(FileHolder FileHolder)
        {
            CachedMessageComposer = new Dictionary<int, HabboClass>();
            CachedMessageEvents = new Dictionary<int, HabboClass>();
            CachedHabboClasses = new Dictionary<string, HabboClass>();
            ReadableNamespaces = new Dictionary<string, string>();
            CachedIncomingMessagesClasses = new Dictionary<string, HabboClass>();
            this.FileHolder = FileHolder;
        }

        internal void AddHabboClass(HabboClass TheClass)
        {
            if (!this.CachedHabboClasses.ContainsKey(TheClass.ClassId))
                this.CachedHabboClasses.Add(TheClass.ClassId, TheClass);
        }

        internal HabboClass GetClassByName(string ClassName)
        {
            HabboClass Class = null;

            if (CachedHabboClasses.TryGetValue(ClassName, out Class))
            {
                return Class;
            }

            return Class;
        }

        #region Get all incoming messages classes
        internal void ReadIncomingMessageClasses()
        {
            string[] Classes = Regex.Split(FileHolder.WholeFile, "//------------------------------------------------------------");
            
            foreach (string Class in Classes)
            {
                string[] Lines = Regex.Split(Class, "\n");

                List<string> ClassLines = new List<string>();

                string ClassName = string.Empty;
                string IncomingHandler = string.Empty;
                string ParserClass = string.Empty;

                foreach (string Line in Lines)
                {
                    if (Line.Contains(" class "))
                    {
                        int IndexOfClassName = Line.IndexOf("class ");
                        ClassName = Line.Substring(IndexOfClassName).Split(' ')[1].Split(' ')[0];
                    }

                    if (Line.Contains("public function IncomingMessages(_arg1:"))
                    {
                        IncomingHandler = Line.Split(':')[1].Split(')')[0];
                    }

                    ClassLines.Add(Line);
                }

                if (IncomingHandler != string.Empty && ClassName == "IncomingMessages")
                {
                    var newClass = new HabboClass(ClassName, ClassLines, ParserClass);
                    newClass.ReadAllHabboConnectionMessageEvents();

                    this.CachedIncomingMessagesClasses.Add(IncomingHandler, newClass);
                }
            }

            Console.WriteLine("[{0}] Found {1} IncomingMessages classes", FileHolder.File, CachedIncomingMessagesClasses.Count);
        }
        #endregion

        #region Reads all Namespaces for readables
        internal void ReadReadableNamespaces()
        {
            HabboClass ConnectionClass = null;

            if (CachedHabboClasses.TryGetValue("SocketConnection", out ConnectionClass))
            {
                List<string> LinesOfFunction = new List<string>();
                bool isReading = false;

                #region Read constructor lines
                foreach (string Line in ConnectionClass.ClassLines)
                {
                    if (Line.Contains("public function SocketConnection"))
                    {
                        LinesOfFunction.Add(Line);
                        isReading = true;
                    }

                    if (isReading)
                    {
                        LinesOfFunction.Add(Line);
                    }

                    if (Line == "}" && isReading)
                    {
                        LinesOfFunction.Add(Line);
                        isReading = false;
                    }
                }
                #endregion

                // line 6 contains the class we are searching for
                int indexOfNew = LinesOfFunction[6].IndexOf("new ");
                string ClassName = LinesOfFunction[6].Substring(indexOfNew).Split('(')[0].Replace("new ", "");

                HabboClass SearchedClass = null;

                #region Replace function names..
                if (CachedHabboClasses.TryGetValue(ClassName, out SearchedClass))
                {
                    string searchFor = (from i in SearchedClass.ClassLines
                                        where i.Contains("return (!NULL!);")
                                        select i).FirstOrDefault();

                    int indexInList = SearchedClass.ClassLines.IndexOf(searchFor);
                    string searchedLine = SearchedClass.ClassLines[indexInList - 2];
                    string ByteReaderName = searchedLine.Substring(searchedLine.IndexOf("new ")).Split('(')[0].Replace("new ", "");

                    HabboClass ByteReaderClass = null;

                    // Now read out the functions..
                    if (CachedHabboClasses.TryGetValue(ByteReaderName, out ByteReaderClass))
                    {
                        List<string> FunctionLines = new List<string>();
                        bool isReadingFunction = false;

                        foreach (string Line in ByteReaderClass.ClassLines)
                        {
                            if (Line.Contains("public function") && (Line.Contains(":String") || Line.Contains(":int") || Line.Contains(":Boolean") || Line.Contains(":Number")))
                            {
                                FunctionLines.Add(Line);
                                isReadingFunction = true;
                            }

                            if (isReadingFunction)
                            {
                                FunctionLines.Add(Line);
                            }

                            if (Line.Contains("}") && isReadingFunction)
                            {
                                FunctionLines.Add(Line);
                                ReadFunctionLines(FunctionLines);
                                FunctionLines.Clear();
                                isReadingFunction = false;
                            }
                        }

                        RegexString = string.Format(@"\b({0}|{1}|{2}|{3}|{4}|{5}|while|new .*\(_arg1)\b", 
                            this.ReadableNamespaces["readInteger"], 
                            this.ReadableNamespaces["readString"], 
                            this.ReadableNamespaces["readBoolean"], 
                            this.ReadableNamespaces["readByte"], 
                            this.ReadableNamespaces["readShort"], 
                            this.ReadableNamespaces["readFloat"]);
                    }
                    else
                    {
                        Console.WriteLine("Can't find the ByteReader class...");
                    }
                }
                else
                {
                    Console.WriteLine("Can't find the searched class {0}", ClassName);
                }
                #endregion
            }
            else
            {
                Console.WriteLine("Didn't find the SocketConnection class..Report this problem!");
            }
        }

        #region Analyze a method
        internal void ReadFunctionLines(List<string> Lines)
        {
            string FunctionName = string.Empty;

            foreach (string Line in Lines)
            {
                if (Line.Contains("public function "))
                {
                    FunctionName = Line.Substring(Line.IndexOf("function ")).Split(':')[0].Replace("function ", "");
                }

                if (Line.Contains(".readUTF());"))
                {
                    ReadableNamespaces["readString"] = FunctionName;
                }
                else if (Line.Contains(".readInt());"))
                {
                    ReadableNamespaces["readInteger"] = FunctionName;
                }
                else if (Line.Contains(".readBoolean());"))
                {
                    ReadableNamespaces["readBoolean"] = FunctionName;
                }
                else if (Line.Contains(".readShort());"))
                {
                    ReadableNamespaces["readShort"] = FunctionName;
                }
                else if (Line.Contains(".readByte());"))
                {
                    ReadableNamespaces["readByte"] = FunctionName;
                }
                else if (Line.Contains(".readFloat());"))
                {
                    ReadableNamespaces["readFloat"] = FunctionName;
                }
            }
        }
        #endregion
        #endregion

        #region Get all MessageEvents & Composer
        internal void GetMessageComposer()
        {
            var Operation = Parallel.ForEach(FileHolder.FileLines, (line, option) =>
            {
                bool match = Regex.IsMatch(line, "\\[[0-9]*\\]");

                if (match && line.Contains(FileHolder.MessageComposerNamespace))
                {
                    try
                    {
                        line = line.Replace(" ", "");

                        int Header = int.Parse(line.Split('[')[1].Split(']')[0]);
                        string Class = line.Split('=')[1].Split(';')[0];

                        if (Header > 0 && Class != String.Empty)
                        {
                            HabboClass TheClass;

                            if (CachedHabboClasses.TryGetValue(Class, out TheClass))
                            {
                                CachedMessageComposer.Add(Header, TheClass);
                            }
                        }
                    }
                    catch { }
                }
            });

            if (Operation.IsCompleted)
            {
                Console.WriteLine("[{0}] Found {1} MessageComposer to work with..", FileHolder.File, CachedMessageComposer.Count);
            }
        }

        internal void GetMessageEvents()
        {
            var Operation = Parallel.ForEach(FileHolder.FileLines, (line, option) =>
            {
                bool match = Regex.IsMatch(line, "\\[[0-9]*\\]");

                if (match && line.Contains(FileHolder.MessageEventNamespace))
                {
                    try
                    {
                        line = line.Replace(" ", "");

                        int Header = int.Parse(line.Split('[')[1].Split(']')[0]);
                        string Class = line.Split('=')[1].Split(';')[0];

                        if (Header > 0 && Class != String.Empty)
                        {
                            HabboClass TheClass;

                            if (CachedHabboClasses.TryGetValue(Class, out TheClass))
                            {
                                CachedMessageEvents.Add(Header, TheClass);
                            }
                        }
                    }
                    catch { }
                }
            });

            if (Operation.IsCompleted)
            {
                Console.WriteLine("[{0}] Found {1} MessageEvents to work with..", FileHolder.File, CachedMessageEvents.Count);
            }
        }
        #endregion

        #region Get a messageeventheader by a classname
        internal int GetMessageEventHeaderByClassName(string classname)
        {
            foreach (var Class in CachedMessageEvents)
            {
                if (Class.Value != null)
                {
                    if (Class.Value.ClassId == classname)
                        return Class.Key;
                }
            }
            return 0;
        }
        #endregion

        #region Parse out Release
        internal string GetRelease()
        {
            Parallel.ForEach(FileHolder.FileLines, (line, option) =>
            {
                if (line.Contains("RELEASE63"))
                {
                    Release = line.Split('"')[1].Split('"')[0];
                    option.Stop();
                }
            });

            return Release;
        }
        #endregion
    }
}
