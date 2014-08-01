using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Update
{
    sealed class FileHolder
    {
        internal readonly string MessageComposerNamespace = string.Empty;
        internal readonly string MessageEventNamespace = string.Empty;

        internal string File { get; set; }
        internal string WholeFile { get; set; }
        internal List<string> FileLines { get; set; }
        internal HabboClassManager HabboClassManager { get; set; }

        internal FileHolder(string File, string MessageComposerNamespace, string MessageEventNamespace)
        {
            this.File = File;
            this.MessageComposerNamespace = MessageComposerNamespace;
            this.MessageEventNamespace = MessageEventNamespace;
            this.WholeFile = string.Empty;
            this.FileLines = new List<string>();
        }

        #region Reading the text file
        internal void ReadTextFile()
        {
            using (StreamReader reader = new StreamReader(this.File))
            {
                WholeFile = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(this.File))
            {
                while (!reader.EndOfStream)
                {
                    FileLines.Add(reader.ReadLine());
                }
            }
        }
        #endregion

        #region Reading some habbo stuff such as all classes
        internal void ReadHabboStuff()
        {
            this.HabboClassManager = new HabboClassManager(this);

            string[] Classes = Regex.Split(WholeFile, "//------------------------------------------------------------");

            foreach (string Class in Classes)
            {
                string[] Lines = Regex.Split(Class, "\n");

                List<string> ClassLines = new List<string>();
                List<string> ImportLines = new List<string>();

                string ClassName = string.Empty;
                string ParserClass = string.Empty;

                foreach (string Line in Lines)
                {
                    if (Line.Contains("import "))
                    {
                        ImportLines.Add(Line);
                    }

                    if (Line.Contains(" class "))
                    {
                        int IndexOfClassName = Line.IndexOf("class ");
                        ClassName = Line.Substring(IndexOfClassName).Split(' ')[1].Split(' ')[0];
                    }

                    ClassLines.Add(Line);
                }

                int ImportCount = 0;

                foreach (string Line in ImportLines)
                {
                    ImportCount++;

                    if (ImportCount == 2)
                    {
                        ParserClass = Line.Split('.')[1].Split(';')[0];
                    }
                }

                HabboClass hhClass = new HabboClass(ClassName, ClassLines, ParserClass);
                this.HabboClassManager.AddHabboClass(hhClass);
            }

            Console.WriteLine("[{0}] Found Release: {1}", File, HabboClassManager.GetRelease());

            this.HabboClassManager.GetMessageComposer();
            this.HabboClassManager.GetMessageEvents();
            this.HabboClassManager.ReadReadableNamespaces();
        }
        #endregion
    }
}
