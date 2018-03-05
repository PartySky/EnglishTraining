using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using EnglishTraining.models.Commonmodels;
using System.Linq;

namespace EnglishTraining
{
    [Route("main/[controller]")]
    public class WordController : Controller
    {
        string audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<List<VmWordWithDictors>> GetWords()
        {
            VmWord[] words;
            DateTime dateToday = DateTime.Now;

            UpdateSchedule();

            using (var db = new WordContext())
            {
                words = db.Words.Where(p=> (p.Name_ru.IndexOf(' ') < 0)
                                       && (p.Name_en.IndexOf(' ') < 0)
                                       && (p.NextRepeatDate <= dateToday)).ToArray();
            }

            List<VmWordWithDictors> wordsWithDictors = new List<VmWordWithDictors>();

            FileChecker fileChecker = new FileChecker();
            var collocationsUrl_en = Directory.GetFiles(Path.Combine(audioPath, "collocations", "en")).ToList();

            foreach (VmWord word in words)
            {
                var path = Path.Combine(audioPath, word.Name_ru);
                var dictors_en = GetDictors(path, "en", word.Name_en);
                var dictors_ru = GetDictors(path, "ru", word.Name_ru);

                //List<VmDictor> tempDictors_en = (dictors_en.Any()) ? dictors_en : new List<VmDictor>();
                //List<VmDictor> tempDictors_ru = (dictors_ru.Any()) ? dictors_ru : new List<VmDictor>();

                //var pathTempRu = Path.Combine(audioPath, word.Name_en) + ".wav";
                // TODO: add english words
                //if (!tempDictors_en.Any()
                //   && !fileChecker.ChecIfExist(pathTempEn))
                //{
                //    break;
                //}

                // TODO: check for mp3 too
                if (!dictors_ru.Any()){
                    Console.WriteLine("Word has no ru dictors: {0}", word.Name_ru);
                }
                else if (!dictors_en.Any())
                {
                    Console.WriteLine("Word has no en dictors: {0}", word.Name_en);
                }
                else 
                {
                    var availableCollocationsUrls = collocationsUrl_en.Where(p => p.IndexOf(word.Name_en) > 0);
                    List<VmCollocation> collocationsTemp = new List<VmCollocation>();

                    foreach(string collocation in availableCollocationsUrls) {
                        var langTemp = "en";
                        collocationsTemp.Add(new VmCollocation{
                            Lang = langTemp,
                            AudioUrl = "/audio/collocations/en/"
                                + collocation.Substring(collocation.LastIndexOf(langTemp + "/") + 3),
                            NotUsedToday = true
                        });
                    }

                    wordsWithDictors.Add(new VmWordWithDictors{
                        Id = word.Id,
                        Name_en = word.Name_en,
                        Name_ru = word.Name_ru,
                        FourDaysLearnPhase = word.FourDaysLearnPhase,
                        LearnDay = word.LearnDay,
                        RepeatIterationNum = word.RepeatIterationNum,
                        NextRepeatDate = word.NextRepeatDate,
                        DailyReapeatCountForEng = word.DailyReapeatCountForEng,
                        DailyReapeatCountForRus = word.DailyReapeatCountForRus,
                        Dictors_en = dictors_en,
                        Dictors_ru = dictors_ru,
                        Collocation = collocationsTemp
                    });
                }
            }

            return await Task<List<VmWordWithDictors>>.Factory.StartNew(() =>
            {
                return wordsWithDictors;
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
                words = db.Words.Where(p => p.NextRepeatDate <= dateToday).ToArray();
            }

            return await Task<VmWord[]>.Factory.StartNew(() =>
            {
                return words;
            });
        }

        [HttpPost("update")]
        public string Update([FromBody] VmWord[] words)
        {
            using (var db = new WordContext())
            {
                foreach (VmWord word in words)
                {
                    if (word == null)
                    {
                        Console.WriteLine("Word is null");
                        throw new ArgumentNullException("Word is null");
                    }
                    else
                    {
                        db.Words.Update(word);
                        Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
                    }
                }
                db.SaveChanges();
            }
            return "succes";
        }

        [HttpPost("updatedictionary")]
        public string UpdateDictionary([FromBody] VmWord[] words)
        {
            using (var db = new DictionaryContext())
            {
                foreach (VmWord word in words)
                {
                    if (word == null)
                    {
                        return "word = null";
                    }
                    db.Words.Update(word);
                    Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
                }
                db.SaveChanges();
            }
            return "succes";
        }

        [HttpPost("checkaudio")]
        public string CheckAudio()
        {
            VmWord[] words;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0).ToArray();
            }

            FileChecker fileChecker = new FileChecker();

            foreach (VmWord word in words)
            {
                var path = Path.Combine(audioPath, word.Name_ru) + ".wav";
                if (!fileChecker.CheckIfExist(path))
                {
                    Console.WriteLine("File doesn't exist, path: {0}", path);
                    //throw new ArgumentNullException("missed audio file");
                }
            }
            return null;
        }

        #region Helpers
        public void UpdateSchedule()
        {
            DateTime dateToday = DateTime.Now;
            VmWord[] renewingIteration;
            using (var db = new WordContext())
            {
                // TODO: move it to separated methods
                // Auto-removing duplicates
                var allWords = db.Words.ToArray();
                var duplicates = db.Words.Where(x => allWords
                                            .Count(n => ((n.Name_ru == x.Name_ru)
                                                      && (n.Name_en == x.Name_en))) > 1)
                                            .GroupBy(p => p.Name_ru)
                                            .Select(p => p.LastOrDefault());

                // TODO: dell all duplicates in one time,
                // now p.Skip(1) doesn't make it works

                foreach (VmWord word in duplicates)
                {
                    db.Words.Remove(word);
                    Console.WriteLine("Removing duplicate \"{0}\" id {1}", word.Name_en, word.Id);
                }

                // Manually-removing duplicates
                var duplicatesToResolve = db.Words.Where(x => allWords
                   .Count(n => ((n.Name_ru != x.Name_ru) && (n.Name_en == x.Name_en))
                            || ((n.Name_ru == x.Name_ru) && (n.Name_en != x.Name_en))) > 1)
                                                   .GroupBy(p => p.Name_ru)
                                                   .Select(p => p.LastOrDefault());

                if (duplicatesToResolve.Count() > 0)
                {
                    throw new Exception("There are duplicates thet should be resolved");
                }

                // Renewing Schedule
                var minReapeatCountPerDayIteration = 1;
                renewingIteration = db.Words.Where(p => (p.NextRepeatDate <= dateToday)
                                           && (p.DailyReapeatCountForEng >= minReapeatCountPerDayIteration)
                                           && (p.DailyReapeatCountForRus >= minReapeatCountPerDayIteration)
                                           && (p.FourDaysLearnPhase == false)).ToArray();

                var iterationIncrement = 7;
                foreach (VmWord word in renewingIteration)
                {
                    var iteration = iterationIncrement * getIterationLenght(word.RepeatIterationNum);

                    word.NextRepeatDate = dateToday.AddDays(iteration);
                    word.DailyReapeatCountForEng = 0;
                    word.DailyReapeatCountForRus = 0;

                    word.RepeatIterationNum++;

                    db.Words.Update(word);
                    Console.WriteLine("Set new day for repeating word \"{0}\" iterations {1} id {2}",
                                      word.Name_en, word.RepeatIterationNum, word.Id);
                }
                db.SaveChanges();
            }
        }

        static int getIterationLenght(int i)
        {
            if (i == 0)
            {
                return 1;
            }
            else
            {
                return 2 *  getIterationLenght(i - 1);
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

                if (!Directory.Exists(langPath)) {
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
        #endregion
    }
}
