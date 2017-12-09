using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading;
using System.Net;

namespace EnglishTraining
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool RunHost = false;

            if(RunHost)
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                host.Run();
			}

            Parser parser = new Parser();
            parser.Download();

            //TestRunner.RunTests();
        }
    }
}
