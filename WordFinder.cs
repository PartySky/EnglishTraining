using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EnglishTraining
{
    public class WordFinder
    {
        public string newWordsPath = Path.Combine(Directory.GetCurrentDirectory(), "txt", "new-words.txt");
        public void FindNewWords()
        {
            VmWord[] wordsDB;
            string excludedWordsFile = File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory(), "txt", "excluded-words.txt"));

            List<string> excludedWords = new HashSet<string>(excludedWordsFile
                                                             .Split(new[] { '\r', '\n', ' ' })).ToList();

            string newWordsFile = Path.Combine(Directory.GetCurrentDirectory(), "txt", "text-for-word-finding.txt");

            string input = File.ReadAllText(newWordsFile);

            using (var db = new WordContext())
            {
                wordsDB = db.Words.Where(p => (p.Name_ru.IndexOf(' ') < 0)
                                         && (p.Name_en.IndexOf(' ') < 0)).ToArray();
            }

            List<string> wordsToCheck = new HashSet<string>((GetWordsFromCollocations().ToLower() + input.ToLower())
                                                            .Replace("'", " ")
                                                            .Split(new[] { '\r', '\n', ' ', '’', '-', '_', '.', ',', '!', '?', '(', ')', '[', ']', ':', ';', '“',
                                                                            '0','1','2','3','4','5','6','7','8','9', '/','|','=',' ', }))
                .ToList();

            List<string> wordsInSimpleFormToCheck = new List<string> { };

            string wordTemp;

            foreach (string word in wordsToCheck)
            {
                if (word.IndexOf("/") >= 0)
                {
                    wordTemp = null;
                    Console.WriteLine(word + " passed word");
                }
                else
                {
                    wordTemp = GetWordSimpleForm(word);
                    wordsInSimpleFormToCheck.Add(wordTemp);
                }
            }

            var newWords = new HashSet<string>(wordsInSimpleFormToCheck
                                               .Where(p => !wordsDB.Any(z => z.Name_en.ToLower() == p)
                                                && !excludedWords.Any(z => z.ToLower() == p)).ToList());

            using (StreamWriter file = File.CreateText(newWordsPath))
            {
                foreach (string word in newWords)
                {
                    file.WriteLine(word);
                    Console.Write(word);
                }
            }
        }

        public string GetWordsFromCollocations()
        {
            //Test
            return "";

            string collocationPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio", "collocations", "en");
            var filesNames = Directory.GetFiles(collocationPath).ToList();
            string result = String.Empty;

            foreach (var fileName in filesNames)
            {
                result = result + fileName + ' ';
            }
            return result;
        }

        public string GetWordSimpleForm(string word)
        {
            string htmlCode;
            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                if (word.IndexOf("ing", 0) > 0)
                {
                    htmlCode = client.DownloadString("http://wooordhunt.ru/word/" + word);
                }
                else
                {
                    // TODO: make it more simle if ldoce will not used
                    //htmlCode = client.DownloadString("https://www.ldoceonline.com/dictionary/" + word);
                    htmlCode = client.DownloadString("http://wooordhunt.ru/word/" + word);
                }
            }
            string wordInSimpleForm = word;


            // Present tense(he/she/it)
            if (htmlCode.IndexOf("\"word_forms\"", 0) > 0
                && htmlCode.IndexOf("используется как present tense(he/she/it) для глагола", 0) > 0)
            {
                var tenseTagIndex = htmlCode.IndexOf("используется как present tense(he/she/it) для глагола");

                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "Present tense(he/she/it)");
            }

            // TODO: Optimaze it
            // Past tense
            if (htmlCode.IndexOf("\"word_forms\"", 0) > 0
                && htmlCode.IndexOf("является 2-й формой неправильного глагола", 0) > 0)
            {
                var tenseTagIndex = htmlCode.IndexOf("является 2-й формой неправильного глагола");

                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "Past tense irregular V2");
            }

            if (htmlCode.IndexOf("\"word_forms\"", 0) > 0
                && htmlCode.IndexOf("является 2-й формой глагола", 0) > 0)
            {
                var tenseTagIndex = htmlCode.IndexOf("является 2-й формой глагола");

                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "Past tense regular V2");
            }

            if (htmlCode.IndexOf("\"word_forms\"", 0) > 0
                && htmlCode.IndexOf("является 3-й формой неправильного глагола", 0) > 0)
            {
                var tenseTagIndex = htmlCode.IndexOf("является 2-й формой неправильного глагола");

                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "Past tense V3");
            }

            // Plural
            if (htmlCode.IndexOf("\"word_forms\"", 0) > 0
                && htmlCode.IndexOf("используется как мн.ч. для существительного", 0) > 0)
            {
                var tenseTagIndex = htmlCode.IndexOf("используется как мн.ч. для существительного");

                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "Plural");
            }

            // Verb 3d form
            if (word.IndexOf("ing", 0) > 0
                && htmlCode.IndexOf("\"word_forms\"", 0) > 0)
            {
                var searchStartPattern = "href=\"/word/";
                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);


                int urlSubstrStart, urlSubstrEnd;

                urlSubstrStart = htmlCode.IndexOf(searchStartPattern, searchStartIndex) + searchStartPattern.Length;
                //urlSubstrEnd = htmlCode.IndexOf(@"\" + "\">", urlSubstrStart);
                urlSubstrEnd = htmlCode.IndexOf(">", urlSubstrStart) - 1;

                wordInSimpleForm = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);
                Console.WriteLine(word + " - " + wordInSimpleForm + " - " + "V3");

            }

            if (wordInSimpleForm == word)
            {
                Console.WriteLine(word + " - unrecognuzed case");
            }

            return wordInSimpleForm;
        }
    }
}
