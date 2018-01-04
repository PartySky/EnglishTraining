﻿using System;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using EnglishTraining.models.Commonmodels;
using System.Linq;

namespace EnglishTraining
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<VmWord[]> GetWords()
        {
            VmWord[] words;

            using (var db = new WordContext())
            {
                words = db.Words.ToArray();
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

        public async Task<VmWord> UpdateWord(VmWord word)
        {
            return await Task<VmWord>.Factory.StartNew(() =>
            {
                // TODO: return true and false
                Console.WriteLine("updating");
                Console.WriteLine(word);
                return null;
            });
        }
    }
}