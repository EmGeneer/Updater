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
        internal string Release;
        internal Dictionary<int, HabboClass> CachedMessageComposer;
        internal Dictionary<int, HabboClass> CachedMessageEvents;

        internal Dictionary<string, HabboClass> CachedHabboClasses;

        internal HabboClassManager(FileHolder FileHolder)
        {
            CachedMessageComposer = new Dictionary<int, HabboClass>();
            CachedMessageEvents = new Dictionary<int, HabboClass>();
            CachedHabboClasses = new Dictionary<string, HabboClass>();
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
