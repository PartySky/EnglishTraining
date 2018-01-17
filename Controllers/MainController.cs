﻿using System;
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
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<VmWord[]> GetWords()
        {
            VmWord[] words;
            DateTime dateToday = DateTime.Now;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => (p.Name_ru.IndexOf(' ') < 0) 
                                       && (p.NextRepeatDate <= dateToday)).ToArray();
            }

            return await Task<VmWord[]>.Factory.StartNew(() =>
            {
                return words;
            });


            // TODO: use VmCommonTableResponse
            // TODO: use mapper from dto to vm
            //return await Task<VmCommonTableResponse<VmWord>>.Factory.StartNew(() =>
            //{
            //    //return words;
            //});
        }

        [HttpGet("dictionary")]
        public async Task<VmWord[]> GetDictionary()
        {
            VmWord[] words;
            DateTime dateToday = DateTime.Now;

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
        public string Update([FromBody] VmWord word)
        {
            if (word == null)
            {
                return "word = null";
            }
            using (var db = new WordContext())
            {
                db.Words.Update(word);
                db.SaveChanges();

                Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
            }
            return "succes";
        }

        [HttpPost("updatedictionary")]
        public string UpdateDictionary([FromBody] VmWord word)
        {
            if (word == null)
            {
                return "word = null";
            }
            using (var db = new DictionaryContext())
            {
                db.Words.Update(word);
                db.SaveChanges();

                Console.WriteLine("Updating word \"{0}\" id {1}", word.Name_en, word.Id);
            }
            return "succes";
        }

        [HttpPost("checkaudio")]
        public string CheckAudio()
        {
            string audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
            VmWord[] words;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0).ToArray();
            }

            FileChecker fileChecker = new FileChecker();

            foreach (VmWord word in words)
            {
                var path = audioPath + word.Name_ru + ".wav";
                if (!fileChecker.ChecIfkExist(path))
                {
                    Console.WriteLine("File doesn't exist, path: {0}", path);
                    throw new ArgumentNullException("missed audio file");
                }
            }

            return null;
        }
    }
}
