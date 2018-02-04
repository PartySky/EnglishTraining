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
                // Get words to return
                words = db.Words.Where(p => (p.Name_ru.IndexOf(' ') < 0)
                                       && (p.Name_en.IndexOf(' ') < 0)
                                       && (p.NextRepeatDate <= dateToday)).ToArray();
            }

            List<VmWordWithDictors> wordsWithDictors = new List<VmWordWithDictors>();
            int y = 0;

			foreach (VmWord word in words)
			{
				var path = Path.Combine(audioPath, word.Name_ru);
                var dictors_en = GetDictors(path, "en");
                var dictors_ru = GetDictors(path, "ru");

                if((dictors_en.Count() != 0)
                   && (dictors_ru.Count() != 0))
				{
                    wordsWithDictors[y].Id = word.Id;
                    wordsWithDictors[y].Name_en = word.Name_en;
                    wordsWithDictors[y].Name_ru = word.Name_ru;
                    wordsWithDictors[y].FourDaysLearnPhase = word.FourDaysLearnPhase;
                    wordsWithDictors[y].LearnDay = word.LearnDay;
                    wordsWithDictors[y].RepeatIterationNum = word.RepeatIterationNum;
                    wordsWithDictors[y].NextRepeatDate = word.NextRepeatDate;
                    wordsWithDictors[y].DailyReapeatCountForEng = word.DailyReapeatCountForEng;
                    wordsWithDictors[y].Dictors_en = dictors_en;
                    wordsWithDictors[y].Dictors_ru = dictors_ru;
                }
                y++;
			}

            //FileChecker fileChecker = new FileChecker();

            //// TODO: optimaze it
            //int i = 0;
            //foreach (VmWord word in words)
            //{
            //    var path = Path.Combine(audioPath, word.Name_ru) + ".wav";
            //    if (fileChecker.ChecIfExist(path))
            //    {
            //        i++;
            //    }
            //}



            //VmWord[] wordsWithAudio = new VmWord[i];
            //int y = 0;

            //foreach (VmWord word in words)
            //{
            //    var path = Path.Combine(audioPath, word.Name_ru) + ".wav";
            //    if (fileChecker.ChecIfExist(path))
            //    {
            //        wordsWithAudio[y] = word;
            //        y++;
            //    }
            //}

            //return await Task<VmWord[]>.Factory.StartNew(() =>
            //return await Task<List<VmWordExtended>>.Factory.StartNew(() =>
            return await Task<List<VmWordWithDictors>>.Factory.StartNew(() =>
            {
                //return wordsWithAudio;
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
                if (!fileChecker.ChecIfExist(path))
                {
                    Console.WriteLine("File doesn't exist, path: {0}", path);
                    throw new ArgumentNullException("missed audio file");
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
                var minReapeatCountPerDay = 3;
                renewingIteration = db.Words.Where(p => (p.NextRepeatDate <= dateToday)
                                           && (p.DailyReapeatCountForEng >= minReapeatCountPerDay)
                                           && (p.DailyReapeatCountForRus >= minReapeatCountPerDay)
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

        public List<VmDictor> GetDictors(string dirPath, string lang)
        {
			List<VmDictor> SomeList = new List<VmDictor>();
            try
            {
                var ruPath = Path.Combine(dirPath, "ru");

                //List<string> dirs = new List<string>(Directory.EnumerateDirectories(enPath));

                List<string> dirs = new List<string>(Directory.EnumerateDirectories(ruPath));
                FileChecker fileChecker = new FileChecker();

                foreach (var dir in dirs)
                {
                    var isExist = fileChecker.ChecIfExist(dir + ".wav");
                    if(isExist)
                    {
                        
                    }

                //    Console.WriteLine("{0}", dir.Substring(dir.LastIndexOf("\\") + 1));
                }
                Console.WriteLine("{0} directories found.", dirs.Count);
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
            return SomeList;
        }
        #endregion
    }
}
