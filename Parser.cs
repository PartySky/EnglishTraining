using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace EnglishTraining
{
    public class Parser
    {
        public static string audioPath = "Audio";
        public void Download()
        {
            var jsonConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons", "api-config.json");
            VmApiCongig api = JsonConvert.DeserializeObject<VmApiCongig>(File.ReadAllText(jsonConfigPath));

            VmCurrentWord[] words = GetWordsCollection();

            foreach (VmCurrentWord parserWords in words)
            {
                string wordName = parserWords.Name_ru;
                string wordRequestUrl = api.Url + wordName + "/language/ru";
                Console.WriteLine(wordRequestUrl);

                Console.WriteLine("");
                Console.WriteLine("delay");
                Console.WriteLine(wordName);

                System.Threading.Thread.Sleep(1000);
                //GetAndSave(wordName, url);
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
            VmCurrentWord[] wordCollection = JsonConvert.DeserializeObject<VmCurrentWord[]>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                words = (VmCurrentWord[])serializer.Deserialize(file, typeof(VmCurrentWord[]));
            }
            return words;
        }

        static VmWordCollection GetWordsCollectionOld()
        {
            VmWordCollection words;
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "jsons", "words_volume_02.json");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", jsonPath);
                throw new ArgumentNullException(jsonPath);
            }
            // read file into a string and deserialize JSON to a type
            VmWordCollection wordCollection = JsonConvert.DeserializeObject<VmWordCollection>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                words = (VmWordCollection)serializer.Deserialize(file, typeof(VmWordCollection));
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
