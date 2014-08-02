using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update
{
    class MessageComparer : IDisposable
    {
        private FileHolder _FileOne;
        private FileHolder _FileTwo;

        internal MessageComparer(FileHolder FileOne, FileHolder FileTwo)
        {
            this._FileOne = FileOne;
            this._FileTwo = FileTwo;
        }

        internal void UpdateHeader(int OldHeader, bool Composer)
        {
            if (Composer)
                UpdateMessageComposer(OldHeader);
            else
                UpdateMessageEvent(OldHeader);
        }

        internal void GetStrucutreForHeader(int Header, bool searchInOldFile)
        {
            HabboClass Class = null;
            HabboClass ParserClass = null;

            if (searchInOldFile)
            {
                if (_FileOne.HabboClassManager.CachedMessageEvents.TryGetValue(Header, out Class))
                {
                    if (Class.ParserClass != string.Empty)
                    {
                        ParserClass = _FileOne.HabboClassManager.GetClassByName(Class.ParserClass);

                        StructureBuilder builder = new StructureBuilder(Header, _FileOne.HabboClassManager, Class, ParserClass);
                        builder.CreateStructure();
                        Console.WriteLine(builder.ToString());
                    }
                    else
                    {
                        Console.WriteLine("There is no parser class for class {0}", Class.ClassId);
                    }
                }
                else
                {
                    Console.WriteLine("Can't find header {0} in file {1}", Header, _FileOne.File);
                }
            }
            else
            {

            }
        }

        /*
         * Todo this whole method
         */
        internal void UpdateMessageComposer(int OldHeader)
        {
            HabboClass Class = null;

            if (_FileOne.HabboClassManager.CachedMessageComposer.TryGetValue(OldHeader, out Class))
            {
                // without parser class..
            }
            else
            {
                Console.WriteLine("Can't find header {0} in CachedMessageComposer in {1}", OldHeader, _FileOne.File);
            }
        }

        internal void UpdateMessageEvent(int OldHeader)
        {
            Console.WriteLine("Please wait while searching for matches..\n");

            HabboClass Class = null;
            HabboClass ParserClass = null;

            if (_FileOne.HabboClassManager.CachedMessageEvents.TryGetValue(OldHeader, out Class))
            {
                ParserClass = _FileOne.HabboClassManager.GetClassByName(Class.ParserClass);

                if (ParserClass != null)
                {
                    StructureBuilder builder = new StructureBuilder(OldHeader, _FileOne.HabboClassManager, Class, ParserClass);
                    builder.CreateStructure();
                    string structure = builder.ToString().Split(':')[1]; // split out the header id

                    ArrayList SearchResults = new ArrayList();

                    // Maybe it's faster with Parallel
                    Parallel.ForEach(_FileTwo.HabboClassManager.CachedMessageEvents.Values, (NewClass, option) =>
                    {
                        if (NewClass != null)
                        {
                            HabboClass NewParserClass = _FileTwo.HabboClassManager.GetClassByName(NewClass.ParserClass);

                            if (NewParserClass != null)
                            {
                                StructureBuilder structBuild = new StructureBuilder(_FileTwo.HabboClassManager.GetMessageEventHeaderByClassName(NewClass.ClassId), _FileTwo.HabboClassManager, NewClass, NewParserClass);
                                structBuild.CreateStructure();
                                string newstructure = structBuild.ToString().Split(':')[1];

                                double Result = Math.Round(Compare(ParserClass.ClassLines, NewParserClass.ClassLines), 3);
                                int structResult = Compute(structure, newstructure);

                                if (structResult < 2 && Result > 0.5)
                                {
                                    SearchResult searchResult = new SearchResult()
                                    {
                                        Header = _FileTwo.HabboClassManager.GetMessageEventHeaderByClassName(NewClass.ClassId),
                                        MethodResult = Result,
                                        StructureDifference = structResult,
                                        OldStructure = structure,
                                        NewStructure = newstructure,
                                        ParserClassName = NewParserClass.ClassId
                                    };

                                    SearchResults.Add(searchResult);
                                }
                            }
                        }
                    });

                    SearchResults.Sort(new SearchResultComparer());

                    Console.WriteLine("\nFound {0} results, sorted by structure difference.", SearchResults.Count);

                    foreach (SearchResult Result in SearchResults)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Found new header: {0}", Result.Header);
                        Console.WriteLine("CalcSimilarity {0}% : {1}", Result.MethodResult, Result.ParserClassName);
                        Console.WriteLine("Structure diff: {0}", Result.StructureDifference);
                        Console.WriteLine("New structure: {0}", Result.NewStructure);
                        Console.WriteLine("Old structure: {0}", Result.OldStructure);
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                    Console.WriteLine("Finished..");
                }
                else
                {
                    Console.WriteLine("Can't find the ParserClass {0} for the MessageEvent {1}", Class.ParserClass, OldHeader);
                }
            }
            else
            {
                Console.WriteLine("Can't find header {0} in CachedMessageEvents in {1}", OldHeader, _FileOne.File);
            }
        }

        internal double Compare(List<string> LinesOne, List<string> LinesTwo)
        {
            StringBuilder LinesOneBuild = new StringBuilder();
            StringBuilder LinesTwoBuild = new StringBuilder();

            foreach (string Line in LinesOne)
            {
                LinesOneBuild.AppendLine(Line);
            }

            foreach (string Line in LinesTwo)
            {
                LinesTwoBuild.AppendLine(Line);
            }

            double Result = CalcSimilarity(LinesOneBuild.ToString(), LinesTwoBuild.ToString(), true);
            return Result;
        }

        internal double CompareStructure(string structureOne, string structureTwo)
        {
            double Result = CalcSimilarity(structureOne, structureTwo, true);
            return Result;
        }

        private Int32 Compute(String a, String b)
        {

            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b))
                {
                    return b.Length;
                }
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a))
                {
                    return a.Length;
                }
                return 0;
            }

            Int32 cost;
            Int32[,] d = new int[a.Length + 1, b.Length + 1];
            Int32 min1;
            Int32 min2;
            Int32 min3;

            for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];

        }

        internal double CalcSimilarity(string left, string right, bool ignoreCase)
        {
            if (ignoreCase)
            {
                left = left.ToLower();
                right = right.ToLower();
            }

            double distance = Compute(left, right);

            if (distance == 0.0f)
                return 1.0f;

            double longestStringSize = System.Math.Max(left.Length, right.Length);
            double percent = distance / longestStringSize;
            return 1.0f - percent;
        }

        public void Dispose()
        {
            _FileOne = null;
            _FileTwo = null;
        }
    }
}
