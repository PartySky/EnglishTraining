using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace EnglishTraining
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IVmWordMapper wordWithLandDictionaryMapper = new VmWordMapper();
            // TODO: find out how to inject it properly
            Parser parser = new Parser(wordWithLandDictionaryMapper);
            NewCollocationParser collocationParser = new NewCollocationParser();
            WordFinder wordFinder = new WordFinder();
            //string runMode = "Parser_Add_Words_List_Into_Db";
            string runMode = "Host";


            switch (runMode)
            {
                case "Host":
                    BuildWebHost(args).Run();
                    break;

                case "Parser":
                    parser.Download();
                    break;

                case "Parser_Add_Words_List_Into_Db":
                    parser.AddWordsToDb();
                    break;

                case "CollocationParser":
                    collocationParser.Download();
                    break;

                case "Write_Existing_Collocation_From_Folder_To_DB":
                    collocationParser.WriteExistingCollocationFromFolderToDB();
                    break;

                case "WordConverter":
                    //WordConverter wordConverter = new WordConverter();
                    //wordConverter.Convert();
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
