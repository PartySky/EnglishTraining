using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace EnglishTraining
{
    public class Parser
    {
        public static string audioPath = "wwwroot/audio";
        public void Download()
        {
            var jsonConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons", "api-config.json");
            VmParserConfig api = JsonConvert.DeserializeObject<VmParserConfig>(File.ReadAllText(jsonConfigPath));

            VmWord[] words;

            using (var db = new WordContext())
            {
                //words = db.Words.Where(p => (p.Name_en.IndexOf(' ') < 0)).ToArray();
                words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0
                && (p.Name_en.IndexOf(' ') < 0)).ToArray();
            }

            foreach (VmWord parserWord in words)
            {
                var lang = "en";
                string wordName;

                switch (lang)
                {
                    case "ru":
						wordName = parserWord.Name_ru.ToLower();
                        break;
                    case "en":
                        wordName = parserWord.Name_en.ToLower();
                        break;
                    default:
                        throw new Exception("lang should be setted");
                }

                Console.WriteLine(audioPath + "/" + parserWord.Name_ru + ".mp3");

                var maxDictorsCount = 5;
                var existDictors = 0;
                if(Directory.Exists(audioPath + "/" + parserWord.Name_ru + "/" + lang)){
                    existDictors = Directory
                        .GetDirectories(audioPath + "/" + parserWord.Name_ru + "/" + lang).Length;
                }

                if (!File.Exists(audioPath + "/" + wordName + ".mp3")
                    && !File.Exists(audioPath + "/" + wordName + ".wav")
                    && (existDictors < maxDictorsCount)
                    && (wordName.IndexOf('_') < 0))
                {
                    string wordRequestUrl = api.Url + wordName + "/language/" + lang;
                    Console.WriteLine(wordRequestUrl);

                    VmResponseWord wordCollection = GetWordColletion(wordRequestUrl);

                    // Adding woordhunt's dictors
                    // TODO: check if mp3 exist
                    //if (lang == "en"){
                    //    wordCollection.items.Add(new VmResponseWordItem
                    //    {
                    //        id = 0,
                    //        word = wordName,
                    //        code = "en",
                    //        username = "WooordhuntUk",
                    //        pathmp3 = "http://wooordhunt.ru/data/sound/word/uk/mp3/" + wordName + ".mp3"
                    //    });
                    //    wordCollection.items.Add(new VmResponseWordItem
                    //    {
                    //        id = 0,
                    //        word = wordName,
                    //        code = "en",
                    //        username = "WooordhuntUs",
                    //        pathmp3 = "http://wooordhunt.ru/data/sound/word/us/mp3/" + wordName + ".mp3"
                    //    });
                    //}

                    Console.WriteLine("");
                    Console.WriteLine("delay");
                    Console.WriteLine(wordName);

                    string dictorLang;

					int i = 0;
                    // TODO: add all not found words to log list
                    // TODO: don't make request for words in not-found list
                    foreach (VmResponseWordItem item in wordCollection.items)
                    {
                        dictorLang = item.code;
                        System.Threading.Thread.Sleep(20);
                        if ((item.pathmp3 != null) && ((dictorLang == "en") || (dictorLang == "ru")))
                        {
                            //GetAndSave(wordName, url);
                            // TODO: made lang switcher

                            // TODO: if not default, save to separated folders
                            if (i <= maxDictorsCount)
                            {
                                GetAndSave(parserWord.Name_en, parserWord.Name_ru, 
                                           wordName, dictorLang, item.pathmp3, item.username);
                            }
                            i++;
                        }
                        else
                        {
                            // TODO: write log with words without audio
                            Console.WriteLine("Word \"{0}\" hasn't audio", wordName);
                        }
                    }
                }
            }
        }

        public void UpdateDictionary()
        {
            VmCurrentWord[] words = GetWordsCollectionFromJson();
            using (var db = new WordContext())
            {
                var existedWords = db.Words.ToArray();
                foreach (VmCurrentWord word in words)
                {
                    if (!Array.Exists(existedWords, element => element.Name_en == word.Name_en))
                    {
                        db.Words.Update(new VmWord
                        {
                            Name_ru = word.Name_ru,
                            Name_en = word.Name_en,
                            FourDaysLearnPhase = true,
                            LearnDay = 0,
                            RepeatIterationNum = 0,
                            NextRepeatDate = DateTime.Today,
                            DailyReapeatCountForEng = 0,
                            DailyReapeatCountForRus = 0
                        });
                        Console.WriteLine("Updating word \"{0}\"", word.Name_en);
                    }
                    else
                    {
                        Console.WriteLine("Skipped word \"{0}\"", word.Name_en);
                    }
                }
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);
            }
        }

        static VmCurrentWord[] GetWordsCollectionFromJson()
        {
            VmCurrentWord[] words;
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons", "current-words.json");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", jsonPath);
                throw new ArgumentNullException(jsonPath);
            }
            // read file into a string and deserialize JSON to a type
            VmCurrentWord[] wordCollection = JsonConvert
                .DeserializeObject<VmCurrentWord[]>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                words = (VmCurrentWord[])serializer.Deserialize(file, typeof(VmCurrentWord[]));
            }
            return words;
        }

        static void GetAndSave(string wordName_en, string wordName_ru, string wordNameInLang, 
                               string lang, string url, string dictor)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath, wordName_ru, lang, dictor);
            var filePath = Path.Combine(folderPath, wordNameInLang + ".mp3");

            if (File.Exists(filePath)) {
                return;
            }

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();

            Directory.CreateDirectory(folderPath);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }

        static VmResponseWord GetWordColletion(string url)
        {
            string responseText;
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponseAsync().Result;

            WebHeaderCollection header = response.Headers;

            var encoding = Encoding.ASCII;
            using (var reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                responseText = reader.ReadToEnd();
                Console.WriteLine(responseText);
            }

            // TODO: sort by best dictors
            // TODO: get dictors from config
            ////  //              if((dictor == "Selene71") ||
            ////  //                 (dictor == "manyaha") ||
            ////  //                 (dictor == "NatalyaT") ||
            ////  //                 (dictor == "Skvodo") ||
            ////  //                 (dictor == "1640max"))
            ////  //              {
			return JsonConvert.DeserializeObject<VmResponseWord>(responseText);
        }

        static void GetAndSavePng()
        {
            var filepath = Path.Combine(audioPath, "hello9.png");

            WebRequest request = WebRequest.Create("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");
			WebResponse response = request.GetResponseAsync().Result;

            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }

        static VmParserWord GetWords()
        {
            VmParserWord words;
            string jsonPath = "./jsons/words.json";
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", jsonPath);
                throw new ArgumentNullException(jsonPath);
            }
            // read file into a string and deserialize JSON to a type
            VmParserWord word = JsonConvert.DeserializeObject<VmParserWord>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                words = (VmParserWord)serializer.Deserialize(file, typeof(VmParserWord));
            }
            return words;
        }
    }
}
