using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace EnglishTraining
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string runMode = "Host"; 

            switch (runMode)
            {
                case "Host":
                    var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                    host.Run();
                    break;

                case "Parser":
                    Parser parser = new Parser();
                    parser.Download();
                    break;

                case "WordConverter":
                    WordConverter wordConverter = new WordConverter();
                    wordConverter.Convert();
                    break;
                
                case "Tests":
                    TestRunner.RunTests();
                    break;

            }

        }
    }
}
