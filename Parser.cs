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
        private readonly IWordLocalizationMapper _wordLocalizationMapper;
        private readonly IVmWordMapper _wordWithLandDictionaryMapperService;
        readonly public static string audioPath = "wwwroot/audio";
        readonly public string jsonConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons");
        readonly string langForWordStoring = "ru";
        // TODO: get it from db
        // readonly string targetLang = "pl";
        readonly string targetLang = "en";
        readonly List<string> langList = new List<string> { "pl", "en", "ru" };
        public Parser(
            IWordLocalizationMapper wordLocalizationMapper,

            IVmWordMapper wordWithLandDictionaryMapperService
        )
        {
            _wordLocalizationMapper = wordLocalizationMapper;
            _wordWithLandDictionaryMapperService = wordWithLandDictionaryMapperService;
        }
        public void Download()
        {
            var apiPath = Path.Combine(jsonConfigPath, "api-config.json");
            VmParserConfig api = JsonConvert.DeserializeObject<VmParserConfig>(File.ReadAllText(apiPath));

            var bestDictorsPath = Path.Combine(jsonConfigPath, "best-dictors.json");
            var bestDictors = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(bestDictorsPath));

            var worstDictorsPath = Path.Combine(jsonConfigPath, "worst-dictors.json");
            var worstDictors = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(worstDictorsPath));

            Word[] wordsTemp;
            List<WordWithoutAudio> wordWithoutAudioList;
            List<ParsedWord> parsedWordList;

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
                wordWithoutAudioList = db.WordsWithoutAudio.ToList();
                parsedWordList = db.ParsedWords.ToList();
            }

            // TODO: map it there
            List<WordWithLangDictionary> words = new List<WordWithLangDictionary>();


            foreach (Word word in wordsTemp)
            {
                words.Add(_wordWithLandDictionaryMapperService.MapToSomething(word));
            }
            words.RemoveAll(p => wordWithoutAudioList.Any(z => z.Lang == targetLang && z.Name == p.LangDictionary[targetLang]));
            words.RemoveAll(p => parsedWordList.Any(z => z.Lang == targetLang && z.Name == p.LangDictionary[targetLang]));

            foreach (WordWithLangDictionary parserWord in words)
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
                if (Directory.Exists(audioPath + "/" + wordNameInlangToStoreInFolder + "/" + targetLang))
                {
                    existDictors = Directory
                        .GetDirectories(audioPath + "/" + wordNameInlangToStoreInFolder + "/" + targetLang).Length;
                }

                var defaultAudioPath = Path.Combine(audioPath, "default", targetLang);

                if (!File.Exists(defaultAudioPath + "/" + wordName + ".mp3") &&
                    !File.Exists(defaultAudioPath + "/" + wordName + ".wav") &&
                    (existDictors < maxDictorsCount) &&
                    (wordName.IndexOf('_') < 0))
                {
                    string wordRequestUrl = api.Url + wordName + "/language/" + targetLang;
                    Console.WriteLine(wordRequestUrl);

                    bool ifAudioExist = checkIfAudioExist(parserWord.LangDictionary[targetLang], targetLang);
                    if (!ifAudioExist)
                    {
                        using (var db = new WordContext())
                        {
                            db.WordsWithoutAudio.Add(new WordWithoutAudio
                            {
                                Name = parserWord.LangDictionary[targetLang],
                                Lang = targetLang,
                                Source = "forvo"
                            });
                            db.SaveChanges();
                        }
                        continue;
                    }

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

                    foreach (VmResponseWordItem dictor in bestDictorsTemp)
                    {
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
                    if (sortedDictors.Length == 0)
                    {
                        using (var db = new WordContext())
                        {
                            db.WordsWithoutAudio.Add(new WordWithoutAudio
                            {
                                Name = parserWord.LangDictionary[targetLang],
                                Lang = targetLang,
                                Source = "forvo"
                            });
                            db.SaveChanges();
                        }
                        continue;
                    }

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
                            // TODO: try to check if word exist checking website first

                            // TODO: create table with words without audio,
                            // it should contain source field
                            // TODO: write log with words without audio
                            Console.WriteLine("Word \"{0}\" hasn't audio", wordName);
                        }
                    }
                    using (var db = new WordContext())
                    {
                        db.ParsedWords.Add(new ParsedWord
                        {
                            Name = parserWord.LangDictionary[targetLang],
                            Lang = targetLang,
                            Source = "forvo"
                        });
                        db.SaveChanges();
                    }
                }
            }
        }

        public void PlainDownloadingFromPolishpod101()
        {
            List<string> pageUrlList = new List<string>
            {
                "",
            };
            var patternForWordStart = "js-wlv-word\" lang=\"pl\">";
            var patternForWordEnd = "</span>";
            var patternForUrlStart = "wlv-item__word-container";
            int patternForUrlStartOffset = 378;
            var patternForUrlEnd = ".mp3\"";

            PlainDownloadingFromWebsites(
                pageUrlList,
                patternForWordStart,
                patternForWordEnd,
                patternForUrlStart,
                patternForUrlStartOffset,
                patternForUrlEnd,
                false);
        }
        public void PlainDownloadingFromMowicpopolsku()
        {
            List<string> pageUrlList = new List<string>
            {
                "",
            };
            var patternForWordStart = "title=\"Listen to Pronunciation: ";
            var patternForWordEnd = "\" href=\"";
            var patternForUrlStart = patternForWordEnd;
            int patternForUrlStartOffset = 00;
            var patternForUrlEnd = ".mp3\"";

            PlainDownloadingFromWebsites(
                pageUrlList,
                patternForWordStart,
                patternForWordEnd,
                patternForUrlStart,
                patternForUrlStartOffset,
                patternForUrlEnd,
                true);
        }

        public void PlainDownloadingFromWebsites(
            List<string> pageUrlList,
            string patternForWordStart,
            string patternForWordEnd,
            string patternForUrlStart,
            int patternForUrlStartOffset,
            string patternForUrlEnd,
            bool isWordBeforeAudio)
        {
            foreach (var page in pageUrlList)
            {
                List<WordWithLangDictionary> wordsWithLangDict = new List<WordWithLangDictionary>();
                List<Word> words = new List<Word>();

                using (var db = new WordContext())
                {
                    words = db.Words.ToList();
                }
                foreach (var word in words)
                {
                    wordsWithLangDict.Add(_wordWithLandDictionaryMapperService.MapToSomething(word));
                }


                var htmlResponse = getWebHtmlResponse(page);
                int i = htmlResponse.IndexOf(patternForWordStart, 0);
                try
                {
                    while (i < htmlResponse.Length)
                    {
                        string word = "";
                        string audioUrl = "";
                        if (isWordBeforeAudio)
                        {
                            word = htmlResponse.Substring(
                                htmlResponse.IndexOf(patternForWordStart, i) + patternForWordStart.Length,
                                htmlResponse.IndexOf(patternForWordEnd, i) -
                                (htmlResponse.IndexOf(patternForWordStart, i) + patternForWordStart.Length));

                            audioUrl = htmlResponse.Substring(
                                htmlResponse.IndexOf(patternForWordEnd, i) + patternForWordEnd.Length,
                                htmlResponse.IndexOf(patternForUrlEnd, i) -
                                (htmlResponse.IndexOf(patternForWordEnd, i) + patternForWordEnd.Length)
                            ) + ".mp3";
                        }
                        else
                        {
                            audioUrl = htmlResponse.Substring(
                                htmlResponse.IndexOf(patternForUrlStart, i) + patternForUrlStart.Length + patternForUrlStartOffset,
                                htmlResponse.IndexOf(patternForUrlEnd, i) -
                                (htmlResponse.IndexOf(patternForUrlStart, i) + patternForUrlStart.Length + patternForUrlStartOffset))
                                + ".mp3";

                            word = htmlResponse.Substring(
                               htmlResponse.IndexOf(patternForWordStart, i) + patternForWordStart.Length,
                               htmlResponse.IndexOf(patternForWordEnd, i) -
                               (htmlResponse.IndexOf(patternForWordStart, i) + patternForWordStart.Length));
                        }



                        Console.WriteLine("word {0}, audioUrl {1}", word, audioUrl);
                        i = htmlResponse.IndexOf(patternForUrlEnd, i) + patternForUrlEnd.Length;

                        //List <LearnDay> learnDayTemp = new List<LearnDay>();
                        //foreach(var lang in langList)
                        //{
                        //    learnDayTemp.Add(new LearnDay
                        //    {
                        //        Key = lang,
                        //        Value = 0
                        //    });
                        //}

                        using (var db = new WordContext())
                        {
                            // TODO: don't use name_pl there
                            var x = db.Words.Any(p => p.Localization.Name_pl.ToLower() == word.ToLower());
                            if (!x)
                            {
                                db.Words.Add(new Word
                                {
                                    Name_en = "",
                                    Name_ru = "",
                                    Localization = new WordLocalization
                                    {
                                        Name_en = "",
                                        Name_ru = "",
                                        Name_pl = word
                                    }
                                });
                                db.SaveChanges();
                            }
                            GetAndSaveDefault(targetLang, word, audioUrl);
                        }

                        if (htmlResponse.IndexOf(patternForUrlEnd, i) + patternForUrlEnd.Length > htmlResponse.Length)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void DownloadFrowWikibooks()
        {
            string url = "";
            string page = "";
            string dictor = "wikibooks";
            string patternUrlStart = "href=\"//upload.wikimedia.org";
            int patternUrlStartOffset = 8;
            string patternUrlEnd = ".ogg";
            string source = "wikibooks.org";
            List<VmWordLocalization> wordList;
            using (var db = new WordContext())
            {
                var checkedWordsWithoutAudio = db.WordsWithoutAudio.Where(p => p.Lang == targetLang && p.Source == source).ToList();
                wordList = _wordLocalizationMapper.MapToVmWordLocalization(db.WordLocalization.ToList());
                wordList.RemoveAll(p => checkedWordsWithoutAudio.Any(z => z.Name == p.LangDictionary[targetLang]));
                //if (checkedWordsWithoutAudio.Count() > 0){
                //}
            }

            foreach (var word in wordList)
            {

                if (string.IsNullOrEmpty(word.LangDictionary[targetLang]))
                {
                    continue;
                }

                string langAndCountryCodes = "";
                langAndCountryCodes = targetLang == "pl" ? "pl" : "";
                langAndCountryCodes = targetLang == "ru" ? "ru" : "";
                langAndCountryCodes = targetLang == "en" ? "en-us" : "";
                if (langAndCountryCodes == "")
                {
                    throw new Exception("Codes should be setted");
                }

                page = String.Format("https://en.wikibooks.org/wiki/File:{0}-{1}.ogg",
                                               langAndCountryCodes, word.LangDictionary[targetLang]);

                if (CheckIfAudioFileExist(word.LangDictionary, targetLang, word.LangDictionary[targetLang],
                                         word.LangDictionary[langForWordStoring], url, dictor))
                {
                    continue;
                }

                var htmlResponse = getWebHtmlResponse(page);

                if (string.IsNullOrEmpty(htmlResponse))
                {
                    Console.WriteLine("page for {0} not found", word.LangDictionary[targetLang]);
                    using (var db = new WordContext())
                    {
                        db.WordsWithoutAudio.Add(new WordWithoutAudio
                        {
                            Name = word.LangDictionary[targetLang],
                            Lang = targetLang,
                            Source = source
                        });
                        db.SaveChanges();
                    }
                    continue;
                }

                url = "https://" + htmlResponse.Substring(htmlResponse.IndexOf(patternUrlStart) + patternUrlStartOffset,
                    htmlResponse.IndexOf(patternUrlEnd, htmlResponse.IndexOf(patternUrlStart))
                        - htmlResponse.IndexOf(patternUrlStart) - patternUrlStartOffset + patternUrlEnd.Length);

                Console.WriteLine();
                GetAndSave(word.LangDictionary, targetLang, word.LangDictionary[targetLang],
                    word.LangDictionary[langForWordStoring], url, dictor);
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
            List<Word> wordsToSave = new List<Word>();
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

                foreach (Word word in words)
                {
                    var word_Temp = imported_words.FirstOrDefault(p => p.Name_en == word.Localization.Name_en);

                    if (word_Temp.Name_en != null)
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
            if (wordNameInlangToStoreInFolder == null)
            {
                return;
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath, wordNameInlangToStoreInFolder, targetLang, dictor);
            // Hack
            //var filePath = Path.Combine(folderPath, wordNameInLang + ".mp3");
            var filePath = Path.Combine(folderPath, wordNameInLang + ".ogg");

            if (File.Exists(filePath))
            {
                return;
            }

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();

            Directory.CreateDirectory(folderPath);
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("{0} downloaded", wordNameInLang);
            Console.WriteLine("path {0}", filePath);
        }

        static bool CheckIfAudioFileExist(
            Dictionary<string, string> langDictionary,
            string targetLang, string wordNameInLang,
            string wordNameInlangToStoreInFolder,
            string url, string dictor)
        {
            bool result = false;

            if (wordNameInlangToStoreInFolder == null)
            {
                // Hack
                return true;
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath, wordNameInlangToStoreInFolder, targetLang, dictor);
            var filePathMp3 = Path.Combine(folderPath, wordNameInLang + ".mp3");
            var filePathOgg = Path.Combine(folderPath, wordNameInLang + ".ogg");
            var filePathWav = Path.Combine(folderPath, wordNameInLang + ".wav");

            return File.Exists(filePathMp3) ||
                File.Exists(filePathOgg) ||
                File.Exists(filePathWav) ||
                result;
        }

        static void GetAndSaveDefault(
            string targetLang,
            string wordNameInLang,
            string url)
        {
            var defaultFolder = "default";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath,
                                          defaultFolder, targetLang);

            var filePath = Path.Combine(folderPath, wordNameInLang + ".mp3");

            if (File.Exists(filePath))
            {
                return;
            }

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();

            Directory.CreateDirectory(folderPath);
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        static bool checkIfAudioExist(string wordName, string lang)
        {
            var result = false;
            var url = String.Format("https://forvo.com/search/{0}/{1}", wordName, lang);
            string responseFromServer = "";
            try
            {
                // Create a request for the URL.       
                WebRequest request = WebRequest.Create(url);
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(responseFromServer);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (responseFromServer.Contains("Wow, you actually found a word not on Forvo!"))
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        static string getWebHtmlResponse(string url)
        {
            string responseFromServer = "";
            try
            {
                // Create a request for the URL.       
                WebRequest request = WebRequest.Create(url);
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine(responseFromServer);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return responseFromServer;
        }
    }
}
