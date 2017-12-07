using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Microsoft.VisualBasic;
using System.Net;

namespace WaveChart
{
    public class Parser
    {
        public void Download()
        {
            var filepath = Path.Combine("./parserTests", "hello2.png");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }

            //FileStream fileStream = new FileStream(filepath, FileMode.Create);
            //responseStream.CopyTo(fileStream);
            //fileStream.Dispose();
        }
    }
}
