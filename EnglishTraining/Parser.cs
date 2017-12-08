using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using EnglishTraining.models;

namespace EnglishTraining
{
    public class Parser
    {
        public void Download()
        {
            VmWord words = GetWords();
            var filepath = Path.Combine("./parserTests", "hello3.png");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
        }
        
        static VmWord GetWords()
        {
            VmWord words;
            string jsonPath = "./jsons/words.json";
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", jsonPath);
                throw new ArgumentNullException(jsonPath);
            }
            // read file into a string and deserialize JSON to a type
            VmWord movie1 = JsonConvert.DeserializeObject<VmWord>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using(StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                words = (VmWord)serializer.Deserialize(file, typeof(VmWord));
            }
            return words;
        }
    }
}
