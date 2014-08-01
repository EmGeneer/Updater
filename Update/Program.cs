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

            //using (MessageComparer compare = new MessageComparer(FileOne, FileTwo))
            //{
            //    compare.UpdateHeader(3937, false);
            //}

            using (MessageComparer compare = new MessageComparer(FileOne, FileTwo))
            {
                compare.GetStrucutreForHeader(3885, true);
            }

            CLI cli = new CLI();

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
