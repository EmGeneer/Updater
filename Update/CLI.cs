using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Update
{
    sealed class CLI
    {
        private Task _BackgroundTask;
        private object _syncRoot;
        private AutoResetEvent _resetEvent;

        internal CLI()
        {
            _syncRoot = new object();
            _resetEvent = new AutoResetEvent(false);

            _BackgroundTask = new Task(BackgroundTask);
            _BackgroundTask.Start();
        }

        internal void BackgroundTask()
        {
            lock (_syncRoot)
            {
                while (true)
                {
                    Console.WriteLine();
                    Console.Write("Next step (1 = Update header | 2 = Find structure): ");
                    string input = Console.ReadLine();

                    HandleInput(input);

                    _resetEvent.WaitOne(10, true);
                }
            }
        }

        internal void HandleInput(string Input)
        {
            switch (int.Parse(Input))
            {
                case 1:
                    {

                        break;
                    }

                case 2:
                    {
                        break;
                    }
            }
        }
    }
}
