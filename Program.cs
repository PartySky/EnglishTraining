using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EnglishTraining
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Parser parser = new Parser();
            NewCollocationParser collocationParser = new NewCollocationParser();
            WordFinder wordFinder = new WordFinder();
            string runMode = "Host";

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

                case "WriteExistingCollocationFromFolderToDB":
                    collocationParser.WriteExistingCollocationFromFolderToDB();
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

                case "Word finder":
                    wordFinder.FindNewWords();
                    break;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
