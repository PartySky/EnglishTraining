using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnglishTraining
{
    public class WordFinder
    {
        public string newWordsPath = Path.Combine(Directory.GetCurrentDirectory(), "txt", "new-words.txt");
        public void FindNewWords()
        {
            VmWord[] wordsDB;
            string excludedWordsFile = Path.Combine(Directory.GetCurrentDirectory(), "txt", "excluded-words.txt");

            List<string> excludedWords = new HashSet<string>(excludedWordsFile
                                                             .Split(new[] { '\r', '\n', ' ' })).ToList();

            string newWordsFile = Path.Combine(Directory.GetCurrentDirectory(), "txt", "text-for-word-finding.txt");

            string input = File.ReadAllText(newWordsFile);

            using (var db = new WordContext())
            {
                wordsDB = db.Words.Where(p => (p.Name_ru.IndexOf(' ') < 0)
                                         && (p.Name_en.IndexOf(' ') < 0)).ToArray();
            }

            List<string> words = new HashSet<string>(input.Split(new[] { '\r', '\n', ' ' })).ToList();
            var newWords = words.Where(p => !wordsDB.Any(z => z.Name_en == p)
                                         && !excludedWords.Any(z => z == p)).ToList();

            using (StreamWriter file = File.CreateText(newWordsPath))
            {
                foreach(string word in newWords){
                    file.WriteLine(word);
                    Console.Write(word);
                }
            }
        }
    }
}
