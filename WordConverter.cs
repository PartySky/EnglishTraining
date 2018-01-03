using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EnglishTraining
{
    public class WordConverter
    {
        public string curentWordsJsonPath = "./jsons/current-words.json";
        public string inWordsJsonPath = "./jsons/words_volume_01.json";
        public string outWordsPath = "./jsons/out-words.json";
        public void Convert()
        {
            VmWordCollection inWords = GetWordsCollection(inWordsJsonPath);
            VmCurrentWord[] currentWords = GetCurrentWords(curentWordsJsonPath);
            VmWord[] outWords = new VmWord[inWords.Word.Length];

            for (int i = 0; i < outWords.Length; i++)
            {
                outWords[i] = new VmWord
                {
                    Name_en = inWords.Word[i].Items[0].word,
                    Name_ru = currentWords?.FirstOrDefault(p => p.Name_ru == inWords.Word[i].Items[0].word).Name_en,
                    FourDausLearnPhase = false,
                    LearnDay = 0,
                    RepeatIterationNum = 0,
                    NextRepeatDate = null,
                    DailyReapeatCountForEng = 0,
                    DailyReapeatCountForRus = 0
                    //Dictors_en = null,
					//Dictors_ru = new VmDictor[getDictors.lenght]
                };
            }

            SaveOutWords(outWords, outWordsPath);
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

        static VmCurrentWord[] GetCurrentWords(string jsonPath)
        {
            VmCurrentWord[] words;
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

        static void SaveOutWords(VmWord[] words, string path)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, words);
            }
        }
    }
}
