using System;
using System.Threading;
using MicroQueue;

namespace Playground {
    class Program {
        static void Main(string[] args) {
            var mainQueue = new MainQueue<int>(5);
            mainQueue.Start(() => new MooProcessor());

            for (int i = 0; i < 20; i++) {
                mainQueue.EnqueueForProcessing(i);
            }

            Console.ReadLine();
            mainQueue.Stop();
            Console.ReadLine();
        }
    }

    public class MooProcessor : IEnqueuedObjectProcessor<int> {
        public void Process(int obj) {
            Thread.Sleep(5000);
        }
    }
}
