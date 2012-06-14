namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    internal class PushPull : ITest
    {
        public string TestName
        {
            get { return "PushPull"; }
        }

        public void RunTest()
        {
            var pusher = new Thread(PusherThread);
            var puller = new Thread(PullerThread);

            puller.Start();
            pusher.Start();

            puller.Join();
            pusher.Join();
        }

        private static void PusherThread()
        {
            Thread.Sleep(10);

            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PUSH))
            {
                socket.Bind("tcp://*:8989");

                var frames = new List<Frame>
                {
                    new Frame(Encoding.UTF8.GetBytes("Hello")),
                    new Frame(Encoding.UTF8.GetBytes("World"))
                };

                socket.SendMessage(new ZmqMessage(frames));
            }
        }

        private static void PullerThread()
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PULL))
            {
                socket.Connect("tcp://localhost:8989");

                ZmqMessage request = socket.ReceiveMessage();
                Console.WriteLine("{0} {1}", Encoding.UTF8.GetString(request[0]), Encoding.UTF8.GetString(request[1]));
            }
        }
    }
}
