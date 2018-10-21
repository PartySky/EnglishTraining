using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using EnglishTraining.models.Commonmodels;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    [Route("main/[controller]")]
    public class WordController : Controller
    {
        string audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");

        int minReapeatCountPerDayIteration = 1;
        int minReapeatCountPerDayFourDayPhase = 3;
        string langForWordStoring = "ru";
        //string targetLang = "pl";
        string targetLang = "en";
        List<string> langList = new List<string> { "pl", "en", "ru" };
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("test")]
        public async Task<Word> Test()
        {
            //Dictionary<string, string> dict = new Dictionary<string, string>
            //{
            //    {"en","dog"},
            //    {"ru","собака"},
            //};

            Word word = new Word { 
                Langs = new Dictionary<string, string>
                {
                    {"ru", "value RU"},
                    {"en", "value EN"},
                    {"pl", "value PL"},
                    {"ge", "value GE"}
                }
            };

            return await Task<Word>.Factory.StartNew(() => {
                return word;
            });
        }


        [HttpGet]
        public async Task<List<WordWithLandDictionaryOutput>> GetWords()
        {
            WordWithLandDictionary[] word_new;
            List<VmWord> words_Temp;

            List<VmCollocation> collocations;
            DateTime dateToday = DateTime.Now;
            int? dailyRepeatAmount = 0;

            // TODO: uncomment it
            // Skip for test
            //UpdateSchedule();

            using (var db = new WordContext())
            {
                if (db.Settings.First().DailyRepeatAmount != null)
                {
                    dailyRepeatAmount = db.Settings.First().DailyRepeatAmount;
                }
                // ' ' is check for collocations, but now they should be included if
                // it has audio

                // Should I use Include() there?
                words_Temp = db.Words
                                .Include(p => p.Localization)
                                .Include(p => p.LearnDay)
                                .Include(p => p.FourDaysLearnPhase)
                                .Include(p => p.RepeatIterationNum)
                                .Include(p => p.NextRepeatDate)
                                .Include(p => p.DailyReapeatCount)
                                .ToList();
                List<WordWithLandDictionary> wordWithLandDictionaryList = new List<WordWithLandDictionary>();

                // Test
                //wordWithLandDictionaryList.Add(new WordWithLandDictionary{
                //    LangDictionary = new Dictionary<string, string>
                //    {
                //        {"fucktng key! heah baby!", "there comes value DETKA!"}
                //    }
                //});

                foreach (VmWord word in words_Temp)
                {

                    wordWithLandDictionaryList.Add(new WordWithLandDictionary
                    {
                        Id = word.Id,
                        LangDictionary = new Dictionary<string, string>
                        {
                            {"pl", word.Localization.Name_pl},
                            {"en", word.Localization.Name_en},
                            {"ru", word.Localization.Name_ru},
                        },
                        LearnDay = word.LearnDay,
                        FourDaysLearnPhase = word.FourDaysLearnPhase,
                        RepeatIterationNum = word.RepeatIterationNum,
                        NextRepeatDate = word.NextRepeatDate,
                        DailyReapeatCount = word.DailyReapeatCount,
                        Dictors = null
                    });
                }

                word_new = wordWithLandDictionaryList
                        //.Where(p => langList.All(lang => p.LangDictionary[lang]?.IndexOf(' ') < 0))
                        .Where(p => langList.All(lang => p.NextRepeatDate?
                                                 .FirstOrDefault(z => z.Key == lang)?.Value <= dateToday))
                        .OrderBy(p => p.RepeatIterationNum?.FirstOrDefault(z => z.Key == targetLang)?.Value)
                        .ToArray();

                collocations = db.Collocations.Where(p => p.NextRepeatDate <= dateToday).ToList();

            }

            FileChecker fileChecker = new FileChecker();

            // TODO: add langs ot collocations
            // now there wil be pl added
            // var collocationsUrl_pl = Directory.GetFiles(Path.Combine(audioPath, "collocations", "pl")).ToList();
            var collocationsUrl_en = Directory.GetFiles(Path.Combine(audioPath, "collocations", "en")).ToList();

            // Check It
            //List<VmCollocation> collocationsWithAudio = collocations
            //.Where(p => collocationsUrl_en.FirstOrDefault(z => 
            //z.Substring(z.LastIndexOf("/audio/")) == p.AudioUrl).Any()).ToList();

            // Temp Fix
            List<VmCollocation> collocationsWithAudio = collocations;

            List<VmCollocation> availableCollocations;
            int repeatCount = 0;

            List<WordWithLandDictionaryOutput> wordsOutputList = new List<WordWithLandDictionaryOutput>();

            foreach (WordWithLandDictionary word in word_new)
            {
                if (dailyRepeatAmount != 0
                && repeatCount >= dailyRepeatAmount)
                {
                    break;
                }

                var path = Path.Combine(audioPath, word.LangDictionary[langForWordStoring]);

                Dictionary<string, IList<VmDictor>> dictors = new Dictionary<string, IList<VmDictor>>();

                foreach (string lang in langList)
                {
                    dictors.Add(lang, GetDictors(path, lang, word.LangDictionary[lang]));
                }

                int validDictorsCount = 0;

                // If all variables not null
                foreach (string lang in langList)
                {
                    if (!dictors[lang].Any())
                    {
                        Console.WriteLine("Word has no {0} dictors: {1}", lang, word.LangDictionary[lang]);
                    }
                    else
                    {
                        validDictorsCount++;
                    }
                }

                //var x = dictors.All(p => langList.All(lang => p.Key == lang));
                //var x = dictors.All(p => p.Key != "");

                if (dictors[targetLang].Any() && validDictorsCount >= 2)
                {
                    //var availableCollocationsUrls = collocationsUrl_en.Where(p => p.IndexOf(word.Name_en) > 0);
                    //var availableCollocationsUrls = collocations.Where(p => p.AudioUrl.IndexOf(word.Name_en) > 0);

                    // TODO: check if audio exists
                    // TODO: update collocations
                    availableCollocations = collocationsWithAudio
                       .Where(p => CheckIfContainsPattern(word.LangDictionary[targetLang], p.AudioUrl)).ToList();

                    wordsOutputList.Add(new WordWithLandDictionaryOutput{
                        Id = word.Id,
                        LearnDay = getLearnDayOutputList(word.LearnDay),
                        FourDaysLearnPhase = getFourDaysLearnPhaseOutputList(word.FourDaysLearnPhase),
                        RepeatIterationNum = getRepeatIterationNumOutputList(word.RepeatIterationNum),
                        NextRepeatDate = getNextRepeatDateOutputList(word.NextRepeatDate),
                        DailyReapeatCount = getDailyReapeatCountOutputList(word.DailyReapeatCount),
                        LangDictionary = word.LangDictionary,
                        Dictors = dictors,
                        Collocation = collocations
                    });

                    // TODO: fix it
                    //if (dailyRepeatAmount != 0)
                    //{
                    //    repeatCount = word.FourDaysLearnPhaseOld
                    //        ? repeatCount + 2 * minReapeatCountPerDayFourDayPhase
                    //        : repeatCount + 2 * minReapeatCountPerDayIteration;
                    //}
                }
            }

            return await Task<List<WordWithLandDictionaryOutput>>.Factory.StartNew(() =>
            {
                return wordsOutputList;
            });
        }

        [HttpGet("dictionary")]
        public async Task<VmWord[]> GetDictionary()
        {
            VmWord[] words;
            DateTime dateToday = DateTime.Now;

            UpdateSchedule();

            using (var db = new DictionaryContext())
            {
                words = db.Words.Where(p => p.NextRepeatDateOld <= dateToday).ToArray();
            }

            return await Task<VmWord[]>.Factory.StartNew(() =>
            {
                return words;
            });
        }

        [HttpPost("update")]
        //public string Update([FromBody] VmWord[] words)
        // TODO: use better name for VmWordAndCollocationUpdating
        public string Update([FromBody] VmWordAndCollocationUpdating wordAndCollocationUpdating)
        {
            // Test
            // Uncomend code belowe 
            return null;



            //using (var db = new WordContext())
            //{
            //    foreach (VmWord word in wordAndCollocationUpdating.words)
            //    {
            //        if (word == null)
            //        {
            //            Console.WriteLine("Word is null");
            //            throw new ArgumentNullException("Word is null");
            //        }
            //        else
            //        {
            //            db.Words.Update(word);
            //            Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
            //        }
            //    }
            //    DateTime dateToday = DateTime.Now;
            //    int collocationDelayPeriod = 4;
            //    foreach (VmCollocation collocation in wordAndCollocationUpdating.collocations)
            //    {
            //        if (collocation == null)
            //        {
            //            Console.WriteLine("collocation is null");
            //            throw new ArgumentNullException("collocation is null");
            //        }
            //        else
            //        {
            //            if (collocation.NotUsedToday == false)
            //            {
            //                collocation.NextRepeatDate = dateToday.AddDays(collocationDelayPeriod);
            //                collocation.NotUsedToday = true;
            //            }
            //            db.Collocations.Update(collocation);
            //            Console.WriteLine("Updating collocation \"{0}\"", collocation.AudioUrl);
            //        }
            //    }
            //    db.SaveChanges();
            //}
            //return "succes";
        }

        [HttpPost("updatedictionary")]
        public string UpdateDictionary([FromBody] VmWord[] words)
        {

            // Test
            // Uncomend code belowe 
            return null;

            //using (var db = new DictionaryContext())
            //{
            //    foreach (VmWord word in words)
            //    {
            //        if (word == null)
            //        {
            //            return "word = null";
            //        }
            //        db.Words.Update(word);
            //        Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
            //    }
            //    db.SaveChanges();
            //}
            //return "succes";
        }

        [HttpPost("checkaudio")]
        public string CheckAudio()
        {
            // Test
            // Uncomend code belowe 
            return null;

            //VmWord[] words;

            //using (var db = new WordContext())
            //{
            //    words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0).ToArray();
            //}

            //FileChecker fileChecker = new FileChecker();

            //foreach (VmWord word in words)
            //{
            //    var path = Path.Combine(audioPath, word.Name_ru) + ".wav";
            //    if (!fileChecker.CheckIfExist(path))
            //    {
            //        Console.WriteLine("File doesn't exist, path: {0}", path);
            //        //throw new ArgumentNullException("missed audio file");
            //    }
            //}
            //return null;
        }

        #region Helpers
        public void UpdateSchedule()
        {

            // Test
            // Uncomend code belowe 


            //DateTime dateToday = DateTime.Now;
            //VmWord[] renewingIteration;
            //using (var db = new WordContext())
            //{
            //    // TODO: move it to separated methods
            //    // Auto-removing duplicates
            //    var allWords = db.Words.ToArray();
            //    var duplicates = db.Words.Where(x => allWords
            //                                .Count(n => ((n.Name_ru == x.Name_ru)
            //                                          && (n.Name_en == x.Name_en))) > 1)
            //                                .GroupBy(p => p.Name_ru)
            //                                .Select(p => p.LastOrDefault());

            //    // TODO: dell all duplicates in one time,
            //    // now p.Skip(1) doesn't make it works

            //    foreach (VmWord word in duplicates)
            //    {
            //        db.Words.Remove(word);
            //        Console.WriteLine("Removing duplicate \"{0}\" id {1}", word.Name_en, word.Id);
            //    }

            //    // Manually-removing duplicates
            //    var duplicatesToResolve = db.Words.Where(x => allWords
            //       .Count(n => ((n.Name_ru != x.Name_ru) && (n.Name_en == x.Name_en))
            //                || ((n.Name_ru == x.Name_ru) && (n.Name_en != x.Name_en))) > 1)
            //                                       .GroupBy(p => p.Name_ru)
            //                                       .Select(p => p.LastOrDefault());

            //    if (duplicatesToResolve.Count() > 0)
            //    {
            //        throw new Exception("There are duplicates thet should be resolved");
            //    }

            //    // Renewing Schedule
            //    renewingIteration = db.Words.Where(p => (p.NextRepeatDateOld <= dateToday)
            //                               && (p.DailyReapeatCountForEngOld >= minReapeatCountPerDayIteration)
            //                               && (p.DailyReapeatCountForRusOld >= minReapeatCountPerDayIteration)
            //                               && (p.FourDaysLearnPhaseOld == false)).ToArray();

            //    var iterationIncrement = 7;
            //    foreach (VmWord word in renewingIteration)
            //    {
            //        var iteration = iterationIncrement * getIterationLenght(word.RepeatIterationNumOld);

            //        word.NextRepeatDateOld = dateToday.AddDays(iteration);
            //        word.DailyReapeatCountForEngOld = 0;
            //        word.DailyReapeatCountForRusOld = 0;

            //        word.RepeatIterationNumOld++;

            //        db.Words.Update(word);
            //        Console.WriteLine("Set new day for repeating word \"{0}\" iterations {1} id {2}",
            //                          word.Name_en, word.RepeatIterationNumOld, word.Id);
            //    }
            //    db.SaveChanges();
            //}
        }

        static int getIterationLenght(int i)
        {
            if (i == 0)
            {
                return 1;
            }
            else
            {
                return 2 * getIterationLenght(i - 1);
            }
        }

        public List<VmDictor> GetDictors(string wordPath, string lang, string wordNameInLocal)
        {
            var defaultAudioPath = Path.Combine(audioPath, "default");
            List<VmDictor> dictors = new List<VmDictor>();
            try
            {
                FileChecker fileChecker = new FileChecker();
                var langPath = Path.Combine(wordPath, lang);

                if (!Directory.Exists(langPath))
                {
                    Console.WriteLine("Directory not found: {0}", langPath);

                    var wavePath = Path.Combine(audioPath, defaultAudioPath, lang, wordNameInLocal + ".wav");
                    var mp3Path = Path.Combine(audioPath, defaultAudioPath, lang, wordNameInLocal + ".mp3");

                    if (fileChecker.CheckIfExist(wavePath))
                    {
                        dictors.Add(new VmDictor
                        {
                            username = "default",
                            sex = "",
                            country = "",
                            langname = lang,
                            AudioType = ".wav"
                        });
                    }
                    else if (fileChecker.CheckIfExist(mp3Path))
                    {
                        dictors.Add(new VmDictor
                        {
                            username = "default",
                            sex = "",
                            country = "",
                            langname = lang,
                            AudioType = ".mp3"
                        });
                    }
                    return dictors;
                }

                List<string> dirs = new List<string>(Directory.EnumerateDirectories(langPath));

                foreach (var dir in dirs)
                {
                    var mp3Path = Path.Combine(dir, wordNameInLocal + ".mp3");
                    var wavPath = Path.Combine(dir, wordNameInLocal + ".wav");

                    if (fileChecker.CheckIfExist(mp3Path))
                    {
                        var dictor = new VmDictor
                        {
                            username = dir.Substring(dir.LastIndexOf(lang + "/") + 3),
                            sex = "",
                            country = "",
                            langname = lang,
                            AudioType = ".mp3"
                        };
                        dictors.Add(dictor);
                    }
                    else if (fileChecker.CheckIfExist(wavPath))
                    {
                        var dictor = new VmDictor
                        {
                            username = dir.Substring(dir.LastIndexOf(lang + "/") + 3),
                            sex = "",
                            country = "",
                            langname = lang,
                            AudioType = ".wav"
                        };
                        dictors.Add(dictor);
                    }
                }
                Console.WriteLine("{0} dictor(s) found for word {1}.", dirs.Count, wordNameInLocal);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
            return dictors;
        }

        private static Boolean CheckIfContainsPattern(string pattern, string sentence)
        {
            var punctuation = sentence.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = sentence.Replace("_"," ").Split().Select(x => x.Trim(punctuation));

            // TODO: words should be used in simple form before comparing
            var containsHi = words.Contains(pattern, StringComparer.OrdinalIgnoreCase);
            if (containsHi)
            {
                Console.WriteLine("{0}: {1}", containsHi, sentence);
                return true;
            }
            return false;
        }

        private static IList<LearnDayOutput> getLearnDayOutputList(IList<LearnDay> learnDay)
        {
            IList<LearnDayOutput> result = new List<LearnDayOutput>();
            foreach (LearnDay day in learnDay)
            {
                result.Add(new LearnDayOutput{
                    Id = day.Id,
                    Key = day.Key,
                    Value = day.Value
                });
            }
            return result;
        }
        private static IList<FourDaysLearnPhaseOutput> getFourDaysLearnPhaseOutputList(IList<FourDaysLearnPhase> fourDaysLearnPhase)
        {
            IList<FourDaysLearnPhaseOutput> result = new List<FourDaysLearnPhaseOutput>();
            foreach (FourDaysLearnPhase phase in fourDaysLearnPhase)
            {
                result.Add(new FourDaysLearnPhaseOutput
                {
                    Id = phase.Id,
                    Key = phase.Key,
                    Value = phase.Value
                });
            }
            return result;
        }
        private static IList<RepeatIterationNumOutput> getRepeatIterationNumOutputList(IList<RepeatIterationNum> repeatIterationNum)
        {
            IList<RepeatIterationNumOutput> result = new List<RepeatIterationNumOutput>();
            foreach (RepeatIterationNum num in repeatIterationNum)
            {
                result.Add(new RepeatIterationNumOutput
                {
                    Id = num.Id,
                    Key = num.Key,
                    Value = num.Value
                });
            }
            return result;
        }
        private static IList<NextRepeatDateOutput> getNextRepeatDateOutputList(IList<NextRepeatDate> nextRepeatDate)
        {
            IList<NextRepeatDateOutput> result = new List<NextRepeatDateOutput>();
            foreach (NextRepeatDate date in nextRepeatDate)
            {
                result.Add(new NextRepeatDateOutput
                {
                    Id = date.Id,
                    Key = date.Key,
                    Value = date.Value
                });
            }
            return result;
        }
        private static IList<DailyReapeatCountOutput> getDailyReapeatCountOutputList(IList<DailyReapeatCount> dailyReapeatCount)
        {
            IList<DailyReapeatCountOutput> result = new List<DailyReapeatCountOutput>();
            foreach (DailyReapeatCount count in dailyReapeatCount)
            {
                result.Add(new DailyReapeatCountOutput
                {
                    Id = count.Id,
                    Key = count.Key,
                    Value = count.Value
                });
            }
            return result;
        }

        #endregion
    }
}
