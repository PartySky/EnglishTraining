using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace EnglishTraining
{
    public class NewCollocationParser
    {
        public static string audioPath = Path.Combine("wwwroot", "audio");
        public static string collocationPath = "collocations";

        public void Download()
        {
            VmWord[] words;
            List<VmCollocation> collocations;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0
                && (p.Name_en.IndexOf(' ') < 0)).ToArray();
            }

            string htmlCode;

            foreach (VmWord word in words)
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    htmlCode = client.DownloadString("https://www.ldoceonline.com/dictionary/" + word.Name_en);
                }

                var sourcePath = "https://www.ldoceonline.com/";
                var urlFirstPart = "data-src-mp3=\"/";
                var searchStartPattern = "\"EXAMPLE\"";

                var searchStartIndex = htmlCode.IndexOf(searchStartPattern);
                var indexesOfUrl = new List<int>();
                var urls = new List<string>();
                string lang = "en";
                int tagCounter = 0;
                string[] nameParts = new string[2];

                if (searchStartIndex >= 0)
                {
                    int urlIndex = searchStartIndex;
                    int nameTempIndex;
                    int urlSubstrStart, urlSubstrEnd;
                    string url;
                    string nameSearchStartTag = "/span>";
                    int nameStartIndex;
                    string nextCharPair, fileName;
                    int nameEnd;
                    string excludeTagClass = "\"GLOSS\"";
                    while (urlIndex < htmlCode.LastIndexOf(urlFirstPart))
                    {
                        urlSubstrStart = htmlCode.IndexOf(urlFirstPart, urlIndex) + urlFirstPart.Length;
                        if (urlSubstrStart < 0)
                        {
                            break;
                        }
                        urlSubstrEnd = htmlCode.IndexOf(".mp3", urlSubstrStart) + 4;

                        url = htmlCode.Substring(urlSubstrStart, urlSubstrEnd - urlSubstrStart);

                        urls.Add(url);
                        Console.WriteLine(url);

                        nameTempIndex = urlSubstrEnd;

                        nameStartIndex = htmlCode.IndexOf(nameSearchStartTag, nameTempIndex) + 6;
                        nextCharPair = htmlCode.Substring(nameStartIndex, 2);

                        tagCounter = 0;

                        if (nextCharPair == "<s")
                        {
                            tagCounter++;
                            nameStartIndex = htmlCode.IndexOf(">", nameStartIndex) + 1;

                            // TODO: remove repetitation
                            nextCharPair = htmlCode.Substring(nameStartIndex, 2);
                            if (nextCharPair == "<s")
                            {
                                throw new ArgumentNullException("Unknow tags arangment");
                            }
                        }

                        nameParts[0] = "";
                        nameParts[1] = "";

                        if (tagCounter > 0)
                        {
                            if (tagCounter > 1)
                            {
                                throw new ArgumentNullException("Unknow tags arangment");
                            }
                            nameEnd = htmlCode.IndexOf("</span>", nameStartIndex);
                            fileName = htmlCode.Substring(nameStartIndex, nameEnd - nameStartIndex);

                            nameParts[0] = fileName;
                            nameStartIndex = nameEnd + 7;
                        }

                        nameEnd = htmlCode.IndexOf("</span>", nameStartIndex);
                        fileName = htmlCode.Substring(nameStartIndex, nameEnd - nameStartIndex);

                        // TODO: solve hotfixes
                        // COLLOINEXA found there:
                        // I promise not to <span class="COLLOINEXA">disturb anything
                        if (fileName.IndexOf(excludeTagClass) < 0
                            && fileName.IndexOf("COLLOINEXA") < 0)
                        {
                            nameParts[1] = fileName;
                        }


                        fileName = nameParts[0] + nameParts[1];
                        urlIndex = urlSubstrEnd;
                        Console.WriteLine(fileName);
                        if (fileName != "")
                        {
                            GetAndSave(sourcePath + url, fileName, lang);
                        }
                    }
                }
            }

            using (var db = new WordContext())
            {
                // TODO: check for dublicates
                collocations = db.Collocations.ToList();
                var collocationsTemp = Directory.GetFiles(Path.Combine(audioPath, collocationPath)).ToList();
                
                var collocationsForExclude = collocationsTemp
                    .Where(p => collocations.All(z => z.AudioUrl == p)).ToList();
                
                var collocationsNew = collocationsTemp.Except(collocationsForExclude);

                foreach (string collocation in collocationsNew) {
                    collocations.Add(new VmCollocation {
                        Lang = "en",
                        AudioUrl = collocation,
                        NotUsedToday = true,
                    });
                }

                db.SaveChanges();
            }
        }

        static void GetAndSave(string url, string fileName, string lang)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                          audioPath, collocationPath, lang);
            var filePath = Path.Combine(folderPath, fileName + ".mp3");

            if (File.Exists(filePath))
            {
                return;
            }

            WebRequest request = WebRequest.Create(url);
            try {
				WebResponse response = request.GetResponseAsync().Result;
                var responseStream = response.GetResponseStream();

                Directory.CreateDirectory(folderPath);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }
				System.Threading.Thread.Sleep(2);
			} catch {
                Console.WriteLine("some error happend");
			}
        }
    }
}

