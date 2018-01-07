﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnglishTraining
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Parser parser = new Parser();
            string runMode = "Host"; 

            switch (runMode)
            {
                case "Host":
                    BuildWebHost(args).Run();

                    break;

                case "Parser":
                    parser.Download();
                    break;

                case "WordConverter":
                    WordConverter wordConverter = new WordConverter();
                    wordConverter.Convert();
                    break;

                case "Tests":
                    TestRunner.RunTests();
                    break;

                case "Dictionary Update":
                    parser.UpdateDictionary();
                    break;

            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
