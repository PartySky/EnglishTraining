using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EnglishTraining
{
    public class Parser
    {
        private readonly IWordWithLandDictionaryMapper _wordWithLandDictionaryMapperService;
        public static string audioPath = "wwwroot/audio";
        public string jsonConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons");
        string langForWordStoring = "ru";
        string targetLang = "pl";
        List<string> langList = new List<string> { "pl", "en", "ru" };
        public Parser(
            IWordWithLandDictionaryMapper wordWithLandDictionaryMapperService
        )
        {
            _wordWithLandDictionaryMapperService = wordWithLandDictionaryMapperService;
        }
        public void Download()
        {
            var apiPath = Path.Combine(jsonConfigPath, "api-config.json");
            VmParserConfig api = JsonConvert.DeserializeObject<VmParserConfig>(File.ReadAllText(apiPath));

            var parsedWodListPath = Path.Combine(jsonConfigPath, "parsed-wod-list.json");
            var parsedWods = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(parsedWodListPath));

            var bestDictorsPath = Path.Combine(jsonConfigPath, "best-dictors.json");
            var bestDictors = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(bestDictorsPath));

            var worstDictorsPath = Path.Combine(jsonConfigPath, "worst-dictors.json");
            var worstDictors = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(worstDictorsPath));
            
            VmWord[] wordsTemp;

            using (var db = new WordContext())
            {
                wordsTemp = db.Words
                               .Include(p => p.Localization)
                               .Include(p => p.LearnDay)
                               .Include(p => p.FourDaysLearnPhase)
                               .Include(p => p.RepeatIterationNum)
                               .Include(p => p.NextRepeatDate)
                               .Include(p => p.DailyReapeatCount)
                               .ToArray();
            }

            // TODO: map it there
            List<WordWithLandDictionary> words = new List<WordWithLandDictionary>();
            var removeList = words.Where(p => langList.Any(lang => p.LangDictionary[lang].Contains("")));

            words.RemoveAll(p => removeList.Any(z => z == p));
            
            foreach (VmWord word in wordsTemp)
            {
                words.Add(_wordWithLandDictionaryMapperService.MapToWordWithLandDictionary(word));
            }

            foreach (WordWithLandDictionary parserWord in words)
            {
                string wordNameInlangToStoreInFolder = parserWord.LangDictionary
                                                .FirstOrDefault(p => p.Key == langForWordStoring).Value;

                string wordName = parserWord.LangDictionary
                                                .FirstOrDefault(p => p.Key == targetLang).Value;

                if (wordName == null)
                {
                    Console.WriteLine("Word {0} doesn't contain target localization {1}", 
                                      wordNameInlangToStoreInFolder, targetLang);
                    continue;
                }

                Console.WriteLine(audioPath + "/" + parserWord.LangDictionary
                                  .FirstOrDefault(p => p.Key == targetLang).Value + ".mp3");

				var maxDictorsCount = 5;

                var existDictors = 0;
                if(Directory.Exists(audioPath + "/" + wordNameInlangToStoreInFolder + "/" + targetLang)){
                    existDictors = Directory
                        .GetDirectories(audioPath + "/" + wordNameInlangToStoreInFolder + "/" + targetLang).Length;
                }

                var defaultAudioPath = Path.Combine(audioPath, "default", targetLang);

                if (parsedWods.IndexOf(wordName) < 0
                    && !File.Exists(defaultAudioPath + "/" + wordName + ".mp3")
                    && !File.Exists(defaultAudioPath + "/" + wordName + ".wav")
                    && (existDictors < maxDictorsCount)
                    && (wordName.IndexOf('_') < 0))
                {
                    string wordRequestUrl = api.Url + wordName + "/language/" + targetLang;
                    Console.WriteLine(wordRequestUrl);

                    VmResponseWord wordCollection = GetWordColletion(wordRequestUrl);

                    var bestDictorsTemp = wordCollection.items.Where(
                        p => bestDictors.Any(z => z.Equals(p.username))
                    );
                    var normalDictorsTemp = wordCollection.items.Where(
                        p => (!bestDictors.Any(z => z.Equals(p.username))
                              && !worstDictors.Any(z => z.Equals(p.username)))
                    );
                    var worstDictorsTemp = wordCollection.items.Where(
                        p => worstDictors.Any(z => z.Equals(p.username))
                    );

                    int dictorCount = wordCollection.items.Count();
                    VmResponseWordItem[] sortedDictors = new VmResponseWordItem[dictorCount];

                    int iForDictors = 0;

                    foreach(VmResponseWordItem dictor in bestDictorsTemp) {
                        sortedDictors[iForDictors] = dictor;
                        iForDictors++;
                    }
                    foreach (VmResponseWordItem dictor in normalDictorsTemp)
                    {
                        sortedDictors[iForDictors] = dictor;
                        iForDictors++;
                    }
                    foreach (VmResponseWordItem dictor in worstDictorsTemp)
                    {
                        sortedDictors[iForDictors] = dictor;
                        iForDictors++;
                    }

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

                    foreach (VmResponseWordItem item in sortedDictors)
                    {
                        dictorLang = item.code;
                        System.Threading.Thread.Sleep(10);
                        if ((item.pathmp3 != null) && langList.Any(lang => lang == dictorLang))
                        {
                            if (i <= maxDictorsCount)
                            {
                                GetAndSave(parserWord.LangDictionary,
                                           targetLang,
                                           parserWord.LangDictionary.FirstOrDefault(p => p.Key == targetLang).Value,
                                            wordNameInlangToStoreInFolder, 
                                            item.pathmp3,
                                            item.username);
                            }
                            i++;
                        }
                        else
                        {
                            // TODO: write log with words without audio
                            Console.WriteLine("Word \"{0}\" hasn't audio", wordName);
                        }
                    }

                    parsedWods.Add(wordName);
                }
            }

            using (StreamWriter file = File.CreateText(parsedWodListPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, parsedWods);
            }
        }

        public void UpdateDictionary()
        {
            // Test
            // TODO: uncomment it

            //VmCurrentWord[] words = GetWordsCollectionFromJson();
            //using (var db = new WordContext())
            //{
            //    var existedWords = db.Words.ToArray();
            //    foreach (VmCurrentWord word in words)
            //    {
            //        if (!Array.Exists(existedWords, element => element.Name_en == word.Name_en))
            //        {
            //            db.Words.Update(new VmWord
            //            {
            //                Name_ru = word.Name_ru,
            //                Name_en = word.Name_en,
            //                FourDaysLearnPhaseOld = true,
            //                LearnDayOld = 0,
            //                RepeatIterationNumOld = 0,
            //                NextRepeatDateOld = DateTime.Today,
            //                DailyReapeatCountForEngOld = 0,
            //                DailyReapeatCountForRusOld = 0
            //            });
            //            Console.WriteLine("Updating word \"{0}\"", word.Name_en);
            //        }
            //        else
            //        {
            //            Console.WriteLine("Skipped word \"{0}\"", word.Name_en);
            //        }
            //    }
            //    var count = db.SaveChanges();
            //    Console.WriteLine("{0} records saved to database", count);
            //}
        }


        public void AddWordsToDb()
        {
            // TODO: refactor it
            List<VmWord> wordsToSave = new List<VmWord>();
            VmCurrentWord[] imported_words = GetWordsCollectionFromJson();
            using (var db = new WordContext())
            {
                var words = db.Words
                                .Include(p => p.Localization)
                                .Include(p => p.LearnDay)
                                .Include(p => p.FourDaysLearnPhase)
                                .Include(p => p.RepeatIterationNum)
                                .Include(p => p.NextRepeatDate)
                                .Include(p => p.DailyReapeatCount)
                                .Where(p => p.Localization.Name_pl == null)
                              // ?
                              //.Where(z => imported_words.Any(i => i.Name_en == z.Localization.Name_en))
                              .ToList();

                foreach (VmWord word in words)
                {
                    var word_Temp = imported_words.FirstOrDefault(p => p.Name_en == word.Localization.Name_en);

                    if(word_Temp.Name_en != null)
                    {
                        word.Localization.Name_pl = word_Temp.Name_pl;
                    }
                }
                db.SaveChanges();
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

        //static void GetAndSave(string wordName_en, string wordName_ru, string wordNameInLang, 
        //string lang, string url, string dictor)

        static void GetAndSave(
            Dictionary<string, string> langDictionary,
            string targetLang, string wordNameInLang, 
            string wordNameInlangToStoreInFolder,
            string url, string dictor)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath, wordNameInlangToStoreInFolder, targetLang, dictor);
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
