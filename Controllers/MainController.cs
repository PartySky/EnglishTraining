using System;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnglishTraining
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // TODO: use correct model
        public async Task<VmCurrentWord> GetWords()
        {
            return await Task<VmCurrentWord>.Factory.StartNew(() =>
            {
                var testObject = new VmCurrentWord
                {
                    Name_ru = "кот",
                    Name_en = "kat"
                };
                return testObject;
            });
        }

        // TODO: use correct model
        public async Task<VmCurrentWord> UpdateWords()
        {
            return await Task<VmCurrentWord>.Factory.StartNew(() =>
            {
                var testObject = new VmCurrentWord
                {
                    Name_ru = "кот",
                    Name_en = "kat"
                };
                // TODO: return true and false
                return testObject;
            });
        }
    }
}
