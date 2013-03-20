using System;
using System.Threading;
using System.Threading.Tasks;
using MicroQueue;

namespace Playground {
    class Program {

        public static Random Random = new Random();

        static void Main(string[] args) {
            var mainQueue = new MainQueue<int>(1000);
            mainQueue.Start(() => new MooProcessor());

            Parallel.For(0, 2000, i =>  mainQueue.EnqueueForProcessing(i));

            Console.ReadLine();
            mainQueue.Stop(true);
            Console.ReadLine();
        }
    }

    public class MooProcessor : IEnqueuedObjectProcessor<int> {
        public void Process(int obj) {
            int wait = Program.Random.Next(int.MaxValue/2, int.MaxValue);
            Thread.SpinWait(wait);
        }
    }
}
