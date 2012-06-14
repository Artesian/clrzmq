namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class RoundRobin : ITest
    {
        public string TestName
        {
            get { return "RoundRobin"; }
        }

        private readonly ConcurrentDictionary<string, string> results = new ConcurrentDictionary<string, string>();

        public void RunTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            Task.Factory.StartNew(() => Puller(cancellationTokenSource.Token, "puller1"), cancellationTokenSource.Token);
            Task.Factory.StartNew(() => Puller(cancellationTokenSource.Token, "puller2"), cancellationTokenSource.Token);
            Task.Factory.StartNew(Pusher).Wait();

            while (results.Count < 1000) { }

            cancellationTokenSource.Cancel();

            Console.WriteLine(string.Format("Total: {0}", results.Count));
            Console.WriteLine(string.Format("Puller1: {0}", results.Count(v => v.Value == "puller1")));
            Console.WriteLine(string.Format("Puller1: {0}", results.Count(v => v.Value == "puller2")));
        }

        private void Pusher()
        {
            Thread.Sleep(10);

            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.PUSH))
                {
                    socket.Bind("tcp://*:8989");

                    for (int i = 0; i < 1000; i++)
                    {
                        socket.Send(string.Format("Hello World #{0}", i), Encoding.UTF8);
                    }
                }
            }
        }

        private void Puller(CancellationToken cancellationToken, object name)
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PULL))
            {
                socket.Connect("tcp://localhost:8989");

                while (!cancellationToken.IsCancellationRequested)
                {
                    string message = socket.Receive(Encoding.UTF8);
                    Console.WriteLine("{0} {1}", name, message);
                    results.TryAdd(message, (string)name);
                }
            }
        }
    }
}