using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Microsoft.VisualBasic;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace WaveChart
{
    public class Parser
    {
        public void Download()
        {
            LoadJson();
            var filepath = Path.Combine("./parserTests", "hello3.png");
            var filepath2 = Path.Combine("./parserTests", "hello4.png");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            var responseStream = response.GetResponseStream();
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }
            using (FileStream fileStream = new FileStream(filepath2, FileMode.Create))
            {
                responseStream.CopyTo(fileStream);
            }

            //FileStream fileStream = new FileStream(filepath, FileMode.Create);
            //responseStream.CopyTo(fileStream);
            //fileStream.Dispose();
        }
        
        public void LoadJson()
        {
            string jsonPath = "./parserTests/words.json";
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", jsonPath);
                throw new ArgumentNullException(jsonPath);
            }
            // read file into a string and deserialize JSON to a type
            Movie movie1 = JsonConvert.DeserializeObject<Movie>(File.ReadAllText(jsonPath));
            // deserialize JSON directly from a file
            using(StreamReader file = File.OpenText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                Movie movie2 = (Movie)serializer.Deserialize(file, typeof(Movie));
            }
        }

        public class Movie
        {
            public string Name { get; set; }
            public int Year { get; set; }
        }
    }
}
