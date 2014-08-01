using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Update
{
    class Program
    {
        internal static FileHolder FileOne;
        internal static FileHolder FileTwo;

        static void Main(string[] args)
        {
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
                compare.GetStrucutreForHeader(3937, true);
            }

            Console.ReadLine();
        }
    }
}
