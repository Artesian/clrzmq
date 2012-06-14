namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    internal class HelloWorld2 : ITest
    {
        public string TestName
        {
            get { return "Hello World 2"; }
        }

        public void RunTest()
        {
            var client = new Thread(ClientThread);
            var server = new Thread(ServerThread);

            server.Start();
            client.Start();

            server.Join();
            client.Join();
        }

        private static void ClientThread()
        {
            Thread.Sleep(10);

            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PUSH))
            {
                socket.Connect("tcp://localhost:8989");

                var frames = new List<Frame>
                {
                    new Frame(Encoding.UTF8.GetBytes("Hello")),
                    new Frame(Encoding.UTF8.GetBytes("World"))
                };

				socket.SendMessage(new ZmqMessage(frames));
            }
        }

        private static void ServerThread()
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.PULL))
            {
                socket.Bind("tcp://*:8989");

                ZmqMessage request = socket.ReceiveMessage();
				Console.WriteLine("{0} {1}", Encoding.UTF8.GetString(request[0]), Encoding.UTF8.GetString(request[1]));
            }
        }
    }
}
