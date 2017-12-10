using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using EnglishTraining.models;

namespace EnglishTraining
{
    public class Parser
    {
        public static string audioPath = "./Audio";
        public void Download()
        {
            VmWordCollection words = GetWordsCollection();

            foreach(VmParserWord parserWords in words.Word)
            {
                string wordName = parserWords.Items[0].word;
                string url = parserWords.Items[0].pathmp3;

                Console.WriteLine("");
                Console.WriteLine("delay");
                Console.WriteLine(wordName);
                Console.WriteLine(url);

                System.Threading.Thread.Sleep(1000);
				GetAndSave(wordName, url);
            }
        }

        static VmWordCollection GetWordsCollection()
        {
            VmWordCollection words;
            string jsonPath = "./jsons/words_volume_01.json";
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
            var filepath = Path.Combine(audioPath, filename + ".mp3");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }

        static void GetAndSavePng()
        {
            var filepath = Path.Combine(audioPath, "hello3.png");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
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
