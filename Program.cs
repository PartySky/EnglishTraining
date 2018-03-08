using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            NewCollocationParser collocationParser = new NewCollocationParser();
            string runMode = "Host";

            const string FILENAME = @"c:\temp\test.txt";

            string input = File.ReadAllText(FILENAME);

            string pattern = @"steam";
            MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Groups["steam"]);
            }
            Console.ReadLine();


            StringReader reader = new StringReader(input);

            string[] words = reader.ReadToEnd().Split(' ');

            switch (runMode)
            {
                case "Host":
                    BuildWebHost(args).Run();
                    break;

                case "Parser":
                    parser.Download();
                    break;

                case "CollocationParser":
                    collocationParser.Download();
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
