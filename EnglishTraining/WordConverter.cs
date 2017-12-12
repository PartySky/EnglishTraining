using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EnglishTraining
{
    public class WordConverter
    {
        public string jsonPath = "./jsons/words_volume_01.json";
        public void Convert()
        {
            VmWordCollection inWords = GetWordsCollection(jsonPath);
            VmWord[] outWords = new VmWord[inWords.Word.Length];

            for (int i = 0; i < outWords.Length; i++)
            {
                outWords[i] = new VmWord
                {
                    Name_en = null,
                    Name_ru = inWords.Word[i].Items[0].word,
                    FourDausLearnPhase = false,
                    LearnDay = 0,
                    RepeatIterationNum = 0,
                    NextRepeatDate = null,
                    DailyReapeatCountForEng = 0,
                    DailyReapeatCountForRus = 0,
                    Dictors_en = null,
                    Dictors_ru = null
                };
            }
        }

        static VmWordCollection GetWordsCollection(string jsonPath)
        {
            VmWordCollection words;
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
    }
}
