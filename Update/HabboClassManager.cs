﻿using System;
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
        internal Dictionary<string, HabboClass> CachedHabboClasses;
        internal Dictionary<string, string> ReadableNamespaces;

        internal string Release;

        internal HabboClassManager(FileHolder FileHolder)
        {
            CachedMessageComposer = new Dictionary<int, HabboClass>();
            CachedMessageEvents = new Dictionary<int, HabboClass>();
            CachedHabboClasses = new Dictionary<string, HabboClass>();
            ReadableNamespaces = new Dictionary<string, string>();
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

                        Console.WriteLine("Replaced names: ");
                        foreach (var reader in this.ReadableNamespaces)
                        {
                            Console.WriteLine(reader.Key + " - " + reader.Value);
                        }
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
