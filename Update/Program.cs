using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Update
{
    class Program
    {
        internal static object syncRoot = new object();
        internal static AutoResetEvent resetEvent = new AutoResetEvent(false);

        internal static FileHolder FileOne;
        internal static FileHolder FileTwo;

        static void Main(string[] args)
        {
            Console.Title = "Habbo Update - Emgeneer";

            FileOne = new FileHolder("File1.txt", "_-fY", "_-4Fl");
            FileOne.ReadTextFile();
            FileOne.ReadHabboStuff();

            Console.WriteLine();

            FileTwo = new FileHolder("File2.txt", "_-37b", "_-2g2");
            FileTwo.ReadTextFile();
            FileTwo.ReadHabboStuff();

            Console.WriteLine();

            StringBuilder StringToSave = new StringBuilder();

            using (MessageComparer compare = new MessageComparer(FileOne, FileTwo))
            {
                StringToSave.AppendLine(string.Format("MessageEvents for {0}", FileTwo.HabboClassManager.Release));
                StringToSave.AppendLine("{loop} => While loop");
                StringToSave.AppendLine("{object} => New object in swf");
                StringToSave.AppendLine("I => Integer");
                StringToSave.AppendLine("S => String");
                StringToSave.AppendLine("B => Boolean");
                StringToSave.AppendLine("F => Float");
                StringToSave.AppendLine("BYTE => Byte");
                StringToSave.AppendLine("SH => Short");
                StringToSave.AppendLine("----------------------------------------------------");
                StringToSave.AppendLine("");

                foreach (int Header in FileTwo.HabboClassManager.CachedMessageEvents.Keys)
                {
                    if (Header > 0)
                    {
                        string structure = compare.GetStrucutreForHeader(Header, false);
                        StringToSave.AppendLine(structure);
                    }
                }

                using (StreamWriter writer = new StreamWriter("structures_" + FileTwo.HabboClassManager.Release + ".txt"))
                {
                    writer.WriteLine(StringToSave.ToString());
                }

                Console.WriteLine("Finished saving the message events..");
            }

            lock (syncRoot)
            {
                while (true)
                {
                    resetEvent.WaitOne(5000, true);
                }
            }
        }
    }
}
