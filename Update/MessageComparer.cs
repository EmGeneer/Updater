﻿using System;
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
                    builder.BuildStructure();
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
