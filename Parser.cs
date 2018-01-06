using System;
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

            VmCurrentWord[] words = GetWordsCollection();

            foreach (VmCurrentWord parserWords in words)
            {
                string wordName = parserWords.Name_ru;

                Console.WriteLine(audioPath + "/" + parserWords.Name_ru + ".mp3");

                if (!File.Exists(audioPath + "/" + parserWords.Name_ru + ".mp3")){

                    string wordRequestUrl = api.Url + wordName + "/language/ru";
                    Console.WriteLine(wordRequestUrl);

                    string url = GetMp3Url(wordRequestUrl);

                    Console.WriteLine("");
                    Console.WriteLine("delay");
                    Console.WriteLine(wordName);
                    Console.WriteLine(url);

                    System.Threading.Thread.Sleep(50);
                    if (url != null){
						GetAndSave(wordName, url);
                    } else {
                        // TODO: write log with words without audio
                        Console.WriteLine("Word \"{0}\" hasn't audio", wordName);
                    }
				}
            }
        }

        public void UpdateDictionary()
        {
            VmCurrentWord[] words = GetWordsCollection();
            using (var db = new WordContext())
            {
                var existedWords = db.Words.ToArray();
                foreach(VmCurrentWord word in words){
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
                    } else {
                        Console.WriteLine("Skipped word \"{0}\"", word.Name_en);
                    }
				}
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);
            }
        }

        static VmCurrentWord[] GetWordsCollection()
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

        static void GetAndSave(string filename, string url)
        {
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), audioPath, filename + ".mp3");

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }

        static string GetMp3Url(string url)
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

            VmResponseWord wordCollection = JsonConvert.DeserializeObject<VmResponseWord>(responseText);
            if (wordCollection.items.Count > 0){
				var mp3Url = wordCollection.items[0].pathmp3;
				Console.WriteLine(mp3Url);
				return mp3Url;
            } else {
                return null;
            }

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
