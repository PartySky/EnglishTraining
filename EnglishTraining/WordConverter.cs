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
            List<VmWord> outWords = new List<VmWord>()
            {
                new VmWord {
                    Name_en = "Craig",
                    Name_ru = "Playstead",
                    FourDausLearnPhase = true,
                    LearnDay = 1,
                    RepeatIterationNum = 0,
                    NextRepeatDate = "12.12.17",
                    DailyReapeatCountForEng = 0,
                    DailyReapeatCountForRus = 0,
                    Dictors_en = null,
                    Dictors_ru = null
                },
                new VmWord {
                    Name_en = "Craig",
                    Name_ru = "Playstead",
                    FourDausLearnPhase = true,
                    LearnDay = 1,
                    RepeatIterationNum = 0,
                    NextRepeatDate = "12.12.17",
                    DailyReapeatCountForEng = 0,
                    DailyReapeatCountForRus = 0,
                    Dictors_en = null,
                    Dictors_ru = null
                }
            };

            VmWordCollection inWords = GetWordsCollection(jsonPath);

            int iForCycle = 0;
            //iForCycle = inWords.Word.Length;

            foreach (VmParserWord word in inWords.Word)
            {
                outWords[iForCycle].Name_ru = word.Items[0].word;
                iForCycle++;
            };
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
