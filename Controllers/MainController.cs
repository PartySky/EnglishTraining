using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    [Route("main/[controller]")]
    public class WordController : Controller
    {
        private readonly IVmWordMapper _vmWordMapper;
        public WordController(
            IVmWordMapper vmWordMapper
        )
        {
            _vmWordMapper = vmWordMapper;
        }
        // string audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
        string audioPath = Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles", "audio");

        short minReapeatCountPerDayIteration = 1;
        short minReapeatCountPerDayFourDayPhase = 3;
        string langForWordStoring = "ru";
        List<string> langList = new List<string> { "pl", "en", "ru" };
        string dictorSex = "all";
        string targetLang;

        public IActionResult Index()
        {
            return View();
        }

        // [HttpGet("getTargetLang")]
        // [ProducesResponseType(200, Type = typeof(string))]
        // public async Task<string> GetTargetLang()
        // {
        //     return await Task<string>.Factory.StartNew(() =>
        //     {

        //         using (var db = new WordContext())
        //         {
        //             // return  GetTargetLangHelper(db);
        //             // return Ok(GetTargetLangHelper(db));
        //             return Ok("GetTargetLangHelper(db)");
        //         }
        //     });
        // }

        [HttpGet("getTargetLang")]
        public async Task<JsonResult> GetTargetLang()
        {
            return await Task<JsonResult>.Factory.StartNew(() =>
            {

                using (var db = new WordContext())
                {
                    return Json(GetTargetLangHelper(db));
                }
            });
        }

        // [HttpGet("getTargetLang")]
        // public IActionResult GetTargetLang()
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     // var employee = await _context.employee.SingleOrDefaultAsync(m => m.ID == id);
        //     // if (employee == null)
        //     // {
        //     //     return NotFound();
        //     // }
        //     using (var db = new WordContext())
        //     {
        //         // return  GetTargetLangHelper(db);
        //         // return Ok(GetTargetLangHelper(db));
        //         return Ok("This is ok response string");
        //         // return Ok("GetTargetLangHelper(db)");

        //         // return await Task<IActionResult>.Factory.StartNew(() =>
        //         // {
        //         //     return Ok(GetTargetLangHelper(db));
        //         // });
        //     }

        //     // return Ok("string test");
        //     // return Ok("This is ok response");
        // }

        [HttpGet("test")]
        public async Task<WordTest> Test()
        {
            WordTest word = new WordTest
            {
                Langs = new Dictionary<string, string>
                {
                    {"ru", "value RU"},
                    {"en", "value EN"},
                    {"pl", "value PL"},
                    {"ge", "value GE"}
                }
            };

            return await Task<WordTest>.Factory.StartNew(() =>
            {
                return word;
            });
        }

        [HttpPost]
        public async Task<JsonResult> GetWords([FromBody] WordRequest wellknownMode)
        {
            List<WordWithLangDictionary> word_new;
            List<Word> words_Temp;

            List<VmCollocation> collocations;
            DateTime dateToday = DateTime.Now;
            int? dailyRepeatAmount = 0;

            RemoveDublicates();
            UpdateSchedule();

            using (var db = new WordContext())
            {
                targetLang = GetTargetLangHelper(db);

                if (db.Settings.First().DailyRepeatAmount != null)
                {
                    dailyRepeatAmount = db.Settings.First().DailyRepeatAmount;
                }
                // ' ' is check for collocations, but now they should be included if
                // it has audio

                words_Temp = db.Words
                                .Include(p => p.Localization)
                                .Include(p => p.LearnDay)
                                .Include(p => p.FourDaysLearnPhase)
                                .Include(p => p.RepeatIterationNum)
                                .Include(p => p.NextRepeatDate)
                                .Include(p => p.DailyReapeatCount)
                                .ToList();

                List<WordWithLangDictionary> wordWithLandDictionaryList = new List<WordWithLangDictionary>();


                foreach (Word word in words_Temp)
                {
                    wordWithLandDictionaryList.Add(_vmWordMapper.MapToSomething(word));
                }

                var prioritizedWords = db.PrioritizedWords.Where(p => p.Lang == targetLang).ToList();

                word_new = GetSortedWords(wordWithLandDictionaryList, prioritizedWords, dateToday, wellknownMode.wellknownMode);

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
            short repeatCount = 0;

            List<VmWord> wordsOutputList = new List<VmWord>();

            foreach (WordWithLangDictionary word in word_new)
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

                short validDictorsInAllLocalsCount = 0;

                // Checking if all variables not null
                foreach (string lang in langList)
                {
                    if (!dictors[lang].Any())
                    {
                        Console.WriteLine("Word has no {0} dictors: {1}", lang, word.LangDictionary[lang]);
                    }
                    else
                    {
                        validDictorsInAllLocalsCount++;
                    }
                }

                if (dictors[targetLang].Any() && validDictorsInAllLocalsCount >= langList.Count - 1)
                {
                    // TODO: check if audio exists
                    // TODO: update collocations
                    availableCollocations = collocationsWithAudio
                       .Where(p => CheckIfContainsPattern(word.LangDictionary[targetLang], p.AudioUrl)).ToList();

                    wordsOutputList.Add(_vmWordMapper.MapToVmWord(word, dictors));

                    if (dailyRepeatAmount != 0)
                    {
                        repeatCount = (short)(word.FourDaysLearnPhase.FirstOrDefault(p => p.Key == targetLang).Value
                            ? repeatCount + 2 * minReapeatCountPerDayFourDayPhase
                            : repeatCount + 2 * minReapeatCountPerDayIteration);
                    }
                }
            }

            return await Task<JsonResult>.Factory.StartNew(() =>
            {
                return Json(wordsOutputList);
            });
        }

        [HttpGet("dictionary")]
        // public async Task<Word[]> GetDictionary()
        public async Task<JsonResult> GetDictionary()
        {
            Word[] words;
            DateTime dateToday = DateTime.Now;

            RemoveDublicates();
            UpdateSchedule();

            using (var db = new DictionaryContext())
            {
                string targetLang = "en";
                words = db.Words.Where(p => p.NextRepeatDate
                    .FirstOrDefault(z => z.Key == targetLang).Value <= dateToday).ToArray();
            }

            return await Task<JsonResult>.Factory.StartNew(() =>
            {
                return Json(words);
            });
        }

        [HttpPost("update")]
        public JsonResult Update([FromBody] VmWordAndCollocationUpdating wordAndCollocationUpdating)
        {
            using (var db = new WordContext())
            {
                try
                {
                    foreach (VmWord word in wordAndCollocationUpdating.words)
                    {
                        if (word == null)
                        {
                            Console.WriteLine("Word is null");
                            throw new ArgumentNullException("Word is null");
                        }
                        var updatingWord = db.Words
                               .Include(p => p.Localization)
                               .Include(p => p.LearnDay)
                               .Include(p => p.FourDaysLearnPhase)
                               .Include(p => p.RepeatIterationNum)
                               .Include(p => p.NextRepeatDate)
                               .Include(p => p.DailyReapeatCount)
                               .FirstOrDefault(p => p.Id == word.Id);

                        foreach (string lang in langList)
                        {
                            updatingWord.LearnDay.FirstOrDefault(p => p.Key == lang).Value = word.LearnDay[lang];
                            updatingWord.FourDaysLearnPhase.FirstOrDefault(p => p.Key == lang).Value = word.FourDaysLearnPhase[lang];
                            updatingWord.RepeatIterationNum.FirstOrDefault(p => p.Key == lang).Value = word.RepeatIterationNum[lang];
                            updatingWord.NextRepeatDate.FirstOrDefault(p => p.Key == lang).Value = word.NextRepeatDate[lang];
                            updatingWord.DailyReapeatCount.FirstOrDefault(p => p.Key == lang).Value = word.DailyReapeatCount[lang];

                        }
                        db.Words.Update(updatingWord);
                        Console.WriteLine("Updating word \"{0}\" id {1}", updatingWord.Localization.Name_en, updatingWord.Id);
                    }
                    DateTime dateToday = DateTime.Now;
                    short collocationDelayPeriod = 4;
                    foreach (VmCollocation collocation in wordAndCollocationUpdating.collocations)
                    {
                        if (collocation == null)
                        {
                            Console.WriteLine("collocation is null");
                            throw new ArgumentNullException("collocation is null");
                        }
                        else
                        {
                            if (collocation.NotUsedToday == false)
                            {
                                collocation.NextRepeatDate = dateToday.AddDays(collocationDelayPeriod);
                                collocation.NotUsedToday = true;
                            }
                            db.Collocations.Update(collocation);
                            Console.WriteLine("Updating collocation \"{0}\"", collocation.AudioUrl);
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return Json("succes");
        }

        [HttpPost("updatedictionary")]
        public JsonResult UpdateDictionary([FromBody] Word[] words)
        {
            // Test
            // Uncomend code belowe 
            return Json(null);

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
            Word[] words;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => p.Localization.Name_ru.IndexOf(' ') < 0).ToArray();
            }

            FileChecker fileChecker = new FileChecker();

            foreach (Word word in words)
            {
                var path = Path.Combine(audioPath, word.Localization.Name_ru) + ".wav";
                if (!fileChecker.CheckIfExist(path))
                {
                    Console.WriteLine("File doesn't exist, path: {0}", path);
                    //throw new ArgumentNullException("missed audio file");
                }
            }
            return null;
        }


        [HttpGet("getdictorsex")]
        public async Task<JsonResult> GetDictorSex()
        {
            return await Task<JsonResult>.Factory.StartNew(() => {
                using (var db = new WordContext())
                {
                    return Json(db.Settings.First().DictorSex);
                }
            });
        }

        [HttpPost("updatedictorsex")]
        public async Task<JsonResult> UpdateDictorSex([FromBody] Gender)
        {
            return await Task<JsonResult>.Factory.StartNew(() => {
                using(var db = new WordContext())
                {
                    db.Settings.First().DictorSex = dictorSex;
                    db.SaveChanges();
                }
                return Json("ok");
            });
        }

        #region Helpers
        public void UpdateSchedule()
        {
            Word[] renewingIteration;
            Word[] renewingFDphase;
            string targetLang;
            using (var db = new WordContext())
            {
                targetLang = GetTargetLangHelper(db);
                renewingIteration = getWordListForUpdate(db, minReapeatCountPerDayIteration, targetLang, false);
                renewingFDphase = getWordListForUpdate(db, minReapeatCountPerDayFourDayPhase, targetLang, true);

                renewingIteration = setNextRepeateDateInWordIterationPhaseList(db, renewingIteration, targetLang);
                renewingFDphase = setNextRepeateDateInWordFDPhaseList(db, renewingFDphase, targetLang);

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

                List<Dictor> dictorsTemp;

                using (var db = new WordContext())
                {
                    dictorSex = db.Settings.First().DictorSex;
                    dictorsTemp = db.Dictors.ToList();
                }

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
                        if(dictorSex == "all")
                        {
                            dictors.Add(dictor);
                        }
                        else if (GetDictorSex(dictor.username, dictorsTemp) == dictorSex)
                        {
                            dictors.Add(dictor);
                        }
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
                        if (dictorSex == "all")
                        {
                            dictors.Add(dictor);
                        }
                        else if (GetDictorSex(dictor.username, dictorsTemp) == dictorSex)
                        {
                            dictors.Add(dictor);
                        }

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

        static string GetDictorSex(string name, List<Dictor> dictorList)
        {
            return dictorList.FirstOrDefault(p => p.Username == name)?.Sex;
        }

        private static Boolean CheckIfContainsPattern(string pattern, string sentence)
        {
            var punctuation = sentence.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = sentence.Replace("_", " ").Split().Select(x => x.Trim(punctuation));

            // TODO: words should be used in simple form before comparing
            var containsHi = words.Contains(pattern, StringComparer.OrdinalIgnoreCase);
            if (containsHi)
            {
                Console.WriteLine("{0}: {1}", containsHi, sentence);
                return true;
            }
            return false;
        }

        private static string GetTargetLangHelper(WordContext db)
        {
            return db.Settings.FirstOrDefault()?.LearningLanguage;
        }

        private Word[] getWordListForUpdate(
            WordContext context,
            short minRepeatCount,
            string targetLang,
            bool isFourDayPhase)
        {
            DateTime dateToday = DateTime.Now;

            List<Word> result = new List<Word>();

            foreach(string lang in langList)
            {
                result.AddRange( context.Words
                        .Include(p => p.Localization)
                        .Include(p => p.LearnDay)
                        .Include(p => p.FourDaysLearnPhase)
                        .Include(p => p.RepeatIterationNum)
                        .Include(p => p.NextRepeatDate)
                        .Include(p => p.DailyReapeatCount)
                        .Where(p =>
                        (p.NextRepeatDate.FirstOrDefault(date => date.Key == lang).Value <= dateToday) &&
                        (p.DailyReapeatCount.Where(count => count.Key != lang)
                                                .Sum(count => count.Value) >= minRepeatCount) &&
                        (p.DailyReapeatCount.FirstOrDefault(count => count.Key == lang)
                                                .Value >= minRepeatCount) &&
                        (p.FourDaysLearnPhase
                                .FirstOrDefault(phase => phase.Key == lang).Value == isFourDayPhase)).ToList());
            }

            return result.ToArray();
        }

        private static Word[] setNextRepeateDateInWordFDPhaseList(WordContext db, Word[] wordList, string targetLang)
        {
            DateTime dateToday = DateTime.Now;

            foreach (Word word in wordList)
            {
                foreach (DailyReapeatCount dailyRepeatCount in word.DailyReapeatCount)
                {
                    dailyRepeatCount.Value = 0;
                }
                word.NextRepeatDate.FirstOrDefault(p => p.Key == targetLang).Value = dateToday.AddDays(1);
                db.Words.Update(word);
            }
            return wordList;
        }

        private static Word[] setNextRepeateDateInWordIterationPhaseList(WordContext db, Word[] wordList, string targetLang)
        {
            DateTime dateToday = DateTime.Now;
            var iterationIncrement = 7;

            foreach (Word word in wordList)
            {
                var iteration = iterationIncrement * getIterationLenght(word
                                    .RepeatIterationNum.FirstOrDefault(p => p.Key == targetLang).Value);

                word.NextRepeatDate.FirstOrDefault(p => p.Key == targetLang).Value = dateToday.AddDays(iteration);

                foreach (DailyReapeatCount count in word.DailyReapeatCount)
                {
                    count.Value = 0;
                }

                word.RepeatIterationNum.FirstOrDefault(p => p.Key == targetLang);
                db.Words.Update(word);
            }
            return wordList;
        }

        private static void RemoveDublicates()
        {
            // TODO: remove duplicates
            // Auto-removing duplicates
            // var allWords = db.Words.ToArray();
            // var duplicates = db.Words.Where(x => allWords
            // .Count(n => ((n.Name_ru == x.Name_ru)
            //          && (n.Name_en == x.Name_en))) > 1)
            // .GroupBy(p => p.Name_ru)
            // .Select(p => p.LastOrDefault());

            // List<WordWithLandDictionary> wordsTemp = new List<WordWithLandDictionary>();

            // foreach (VmWord word in allWords)
            // foreach (Word word in allWords)
            // {
            //    wordsTemp.Add(new WordWithLandDictionary
            //    {
            //        Id = word.Id,
            //        LangDictionary = new Dictionary<string, string>
            //        {
            //            {"pl", word.Localization.Name_pl},
            //            {"en", word.Localization.Name_en},
            //            {"ru", word.Localization.Name_ru},
            //        },
            //        LearnDay = word.LearnDay,
            //        FourDaysLearnPhase = word.FourDaysLearnPhase,
            //        RepeatIterationNum = word.RepeatIterationNum,
            //        NextRepeatDate = word.NextRepeatDate,
            //        DailyReapeatCount = word.DailyReapeatCount,
            //        Dictors = null
            //    });
            // }

            //var duplicates = wordsTemp.Where(p => wordsTemp
            //.Count(n => langList.All(lang => n.LangDictionary[lang] = p. )));

            // TODO: dell all duplicates in one time,
            // now p.Skip(1) doesn't make it works

            //foreach (VmWord word in duplicates)
            //{
            //    db.Words.Remove(word);
            //    Console.WriteLine("Removing duplicate \"{0}\" id {1}", word.Name_en, word.Id);
            //}

            //// Manually-removing duplicates
            //var duplicatesToResolve = db.Words.Where(x => allWords
            //.Count(n => ((n.Name_ru != x.Name_ru) && (n.Name_en == x.Name_en))
            //|| ((n.Name_ru == x.Name_ru) && (n.Name_en != x.Name_en))) > 1)
            //.GroupBy(p => p.Name_ru)
            //.Select(p => p.LastOrDefault());

            //if (duplicatesToResolve.Count() > 0)
            //{
            //    throw new Exception("There are duplicates thet should be resolved");
            //}
        }

        List<WordWithLangDictionary> GetSortedWords(List<WordWithLangDictionary> list,
                                                    List<PrioritizedWords> prioritized,
                                                    DateTime dateToday,
                                                    bool wellknownMode)
        {
            List<WordWithLangDictionary> result = new List<WordWithLangDictionary>();

            try
            {
                var repeatIterationWords = list
                    .Except(list.Where(p => prioritized.Any(pr => pr.Name == p.LangDictionary[targetLang])))
                    .Where(p => p.FourDaysLearnPhase?.FirstOrDefault(phase => phase.Key == targetLang)?.Value == false)?
                    .OrderBy(p => p?.RepeatIterationNum?.FirstOrDefault(z => z.Key == targetLang)?.Value)
                    .ToList();

                var prioritizedWords = list
                    .Where(p => prioritized.Any(pr => pr.Name == p?.LangDictionary[targetLang]))
                    .ToList();

                var fourDayPhaseWords = list
                    .Except(list.Where(p => prioritized.Any(pr => pr?.Name == p?.LangDictionary[targetLang])))?
                    .Where(p => p.FourDaysLearnPhase?.FirstOrDefault(phase => phase?.Key == targetLang)?.Value == true)?
                    .OrderBy(p => p?.LearnDay?.FirstOrDefault(z => z.Key == targetLang)?.Value)?
                    .ToList();

                if (wellknownMode)
                {
                    fourDayPhaseWords = fourDayPhaseWords
                        .Where(p => p.LearnDay.FirstOrDefault(day => day?.Key == targetLang)?.Value > 2)
                        .ToList();
                }

                result.AddRange(repeatIterationWords);
                if (!wellknownMode) 
                {
                    result.AddRange(prioritizedWords);
                }
                result.AddRange(fourDayPhaseWords);

                result.RemoveAll(p => langList.All(lang => p.NextRepeatDate?
                        .FirstOrDefault(z => z.Key == targetLang)?.Value > dateToday));

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return result;
        }
        #endregion

        public class WordRequest 
        {
            public string dictorSex { get; set; }
            public bool wellknownMode { get; set; }
        }

        public class Gender 
        {
            public string dictorSex { get; set; }
        }
    }
}
