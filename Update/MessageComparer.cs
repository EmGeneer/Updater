using System;
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
                        //Console.WriteLine(builder.ToString());
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
            HabboClass Class = null;
            HabboClass ParserClass = null;

            if (_FileOne.HabboClassManager.CachedMessageEvents.TryGetValue(OldHeader, out Class))
            {
                ParserClass = _FileOne.HabboClassManager.GetClassByName(Class.ParserClass);

                if (ParserClass != null)
                {
                    StructureBuilder builder = new StructureBuilder(OldHeader, _FileOne.HabboClassManager, Class, ParserClass);
                    builder.CreateStructure();
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

        public void Dispose()
        {
            _FileOne = null;
            _FileTwo = null;
        }
    }
}
