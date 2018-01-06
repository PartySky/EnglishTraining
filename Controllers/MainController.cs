using System;
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
	}
}
