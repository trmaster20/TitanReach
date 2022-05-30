using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TitanReach.Server;
using static TitanReach_Server.Database;
using static TitanReach_Server.Server;

namespace TitanReach_Server
{
    class Program
    {

        public static async void test()
        {

        }
    
        static async Task Main(string[] args)
        {


            Server.args = args;
            while(true)
            {
                Server.Instance.Loop();
                Thread.Sleep(2);
            }
      
        }
    }
}
