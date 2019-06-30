using GEV.Remoting;
using GEV.Remoting.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestService server = new TestService();
            RemoteService service = new RemoteService(server, 5500);

            TestInterface client = RemoteService.SubscribeToRemoteService<TestInterface>("127.0.0.1", 5500);

            Console.WriteLine("Server from local call is calculating 1+2= {0}", server.Add(1, 2));
            Console.WriteLine("Server from remote call calculating 1+2= {0}", client.Add(1, 2));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Write on client....");

            server.LogOnServer();
            client.LogOnServer();

            Console.ReadLine();
        }
    }
}
