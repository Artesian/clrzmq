namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class FairQueueing : ITest
    {
        public string TestName
        {
            get { return "RoundRobin"; }
        }

        private readonly ConcurrentDictionary<string, string> results = new ConcurrentDictionary<string, string>();

        public void RunTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            Task.Factory.StartNew(() => Puller(cancellationTokenSource.Token, "puller"), cancellationTokenSource.Token);

            Thread.Sleep(100);

            Task.Factory.StartNew(() => Pusher("pusher1"));
            Task.Factory.StartNew(() => Pusher("pusher2"));
            Task.Factory.StartNew(() => Pusher("pusher3"));

            while (results.Count < 6) { }

            cancellationTokenSource.Cancel();

            Console.WriteLine(string.Format("Total: {0}", results.Count));
            results.OrderBy(x => x.Key).ToList().ForEach(result => Console.WriteLine(string.Format("{0} : {1}", result.Key, result.Value)));
        }

        private void Pusher(string name)
        {
            Thread.Sleep(10);

            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.PUSH))
                {
                    socket.Connect("tcp://localhost:8989");

                    for (int i = 0; i < 2; i++)
                    {
                        socket.Send(string.Format("Hello World {0} #{1}", name, i), Encoding.UTF8);
                    }
                }
            }
        }

        private void Puller(CancellationToken cancellationToken, object name)
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PULL))
            {
                socket.Bind("tcp://*:8989");

                while (!cancellationToken.IsCancellationRequested)
                {
                    string message = socket.Receive(Encoding.UTF8);
                    Console.WriteLine("Receiving... {0} {1}", name, message);
                    results.TryAdd(message, (string)name);
                }
            }
        }
    }
}