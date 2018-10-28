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
            Word[] words;
            List<VmCollocation> collocations;

            using (var db = new WordContext())
            {
                words = db.Words.Where(p => p.Name_ru.IndexOf(' ') < 0
                && (p.Name_en.IndexOf(' ') < 0)).ToArray();
            }

            string htmlCode;
            string tempURL;

            foreach (Word word in words)
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

                        using (var db = new WordContext())
                        {
                            tempURL = Path.Combine("/" + "audio", collocationPath, lang, fileName + ".mp3");

                            if (fileName != "")
                            {
                                GetAndSave(sourcePath + url, fileName, lang);
                                var isCollocationExist = db.Collocations.Any(p => p.AudioUrl == tempURL);

                                if (!isCollocationExist)
                                {
                                    db.Collocations.Add(new VmCollocation
                                    {
                                        Lang = "en",
                                        AudioUrl = tempURL,
                                        NotUsedToday = true
                                    });
                                }
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }

            using (var db = new WordContext())
            {
                // TODO: check for dublicates
                var allCollocations = db.Collocations.ToList();
                var duplicates = allCollocations.Where(p => allCollocations
                                                    .Count(z => z.AudioUrl == p.AudioUrl) > 1)
                                                    .GroupBy(j => j.AudioUrl)
                                                    .Select(p => p.LastOrDefault()).ToList();

                foreach (VmCollocation collocation in duplicates)
                {
                    db.Collocations.Remove(collocation);
                    Console.WriteLine("Removing duplicate \"{0}\" id {1}", collocation.AudioUrl, collocation.Id);
                }

                collocations = db.Collocations.ToList();
                var collocationsTemp = Directory.GetFiles(Path.Combine(audioPath, collocationPath)).ToList();

                var collocationsForExclude = collocationsTemp
                    .Where(p => collocations.All(z => z.AudioUrl == p)).ToList();

                var collocationsNew = collocationsTemp.Except(collocationsForExclude);

                foreach (string collocation in collocationsNew)
                {
                    collocations.Add(new VmCollocation
                    {
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
            try
            {
                WebResponse response = request.GetResponseAsync().Result;
                var responseStream = response.GetResponseStream();

                Directory.CreateDirectory(folderPath);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }
                System.Threading.Thread.Sleep(2);
            }
            catch
            {
                Console.WriteLine("some error happend");
            }
        }

        public void WriteExistingCollocationFromFolderToDB()
        {
            string lang = "en";
            string audioPathLocal = "audio";
            List<string> newCollocations = null;
            List<string> listToExclude = null;

            using (var db = new WordContext())
            {
                listToExclude = db.Collocations.Select(p => p.AudioUrl).ToList();
            }
            var files = Directory.GetFiles(
                Path.Combine(Directory.GetCurrentDirectory(), audioPath, collocationPath, lang))
                .Select(p => Path.Combine("/", audioPathLocal, collocationPath, lang, Path.GetFileName(p)))
                                 .ToList();

            files.Remove(files.Find(p => p.Contains("DS_Store")));

            newCollocations = files.Except(listToExclude).ToList();
            using (var db = new WordContext())
            {
                foreach (var collocationAudio in newCollocations)
                {
                    db.Collocations.Add(new VmCollocation
                    {
                        Lang = lang,
                        AudioUrl = collocationAudio,
                        NotUsedToday = true
                    });
                }
                db.SaveChanges();
            }
        }
    }
}

