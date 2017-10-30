using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace LibHitomi
{
    // 이게 존재하는 이유 : 갤러리가 403/404으로 막혀도 갤러리블럭은 접근 가능한 경우가 있어서.
    internal class GalleryBlockParser
    {
        private Dictionary<string, string> namespaceMap = new Dictionary<string, string>()
        {
            {"artist", "artists" },
            {"series", "parodies" },
            {"type", "type" },
            {"tag", "tags" },
            // 참고 : 갤러리 블록이라 여기까지 있는 것들밖에 못 긁음. 그래도 혹시 몰라서 더 넣어놨긴 함.
            {"group", "groups" },
            {"character", "characters" }
        };
        internal GalleryBlockParser()
        {

        }
        public bool TryParse(int id, out Gallery parsedGallery)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            Gallery gallery = new LibHitomi.Gallery(GalleryCrawlMethod.ParsedGalleryBlock);
            Regex pattern = new Regex("^/([a-zA-Z]+)/(.+)-all-1\\.html$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex languagePattern = new Regex("^/index-([a-zA-Z]+)-1\\.html$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            gallery.UnNull();
            gallery.id = id;

            htmlDocument.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            HttpWebRequest wreq = RequestHelper.CreateRequest("", $"/galleryblock/{id}.html");
            try
            {
                using (WebResponse wres = wreq.GetResponse())
                using (Stream str = wres.GetResponseStream())
                    htmlDocument.Load(str);

                gallery.name = htmlDocument.DocumentNode.SelectSingleNode(".//h1").InnerText;
                List<string> artistsList = new List<string>(), parodiesList = new List<string>(), tagsList = new List<string>(), groupsList = new List<string>(), charactersList = new List<string>();
                foreach (HtmlNode linkNode in htmlDocument.DocumentNode.SelectNodes(".//a[ @href ]"))
                {
                    string href = linkNode.GetAttributeValue("href", "");
                    bool isLanguageHref = languagePattern.IsMatch(href), isNormalHref = pattern.IsMatch(href);
                    if (isLanguageHref)
                    {
                        Match match = languagePattern.Match(href);
                        // 제목이 없는 좆같은 경우는 봤지만(씨발) 언어가 두개인 좆같은 경우는 보지 못했음. json 형식도 한개라는 가정하에 만들어졌기도 하고.
                        gallery.language = match.Groups[1].Value;
                    }
                    else if (isNormalHref)
                    {
                        Match match = pattern.Match(href);
                        string propName = namespaceMap[match.Groups[1].Value].ToLower();
                        string value = Uri.UnescapeDataString(match.Groups[2].Value).ToLower();

                        if (propName == "artists")
                            artistsList.Add(value);
                        else if (propName == "parodies")
                            parodiesList.Add(value);
                        else if (propName == "tags")
                            tagsList.Add(value);
                        else if (propName == "groups")
                            groupsList.Add(value);
                        else if (propName == "characters")
                            charactersList.Add(value);
                        else if (propName == "type")
                            gallery.type = value;

                    }
                    else
                    {
                        continue;
                    }
                }
                gallery.artists = artistsList.ToArray();
                gallery.parodies = parodiesList.ToArray();
                gallery.tags = tagsList.ToArray();
                gallery.groups = groupsList.ToArray();
                gallery.characters = charactersList.ToArray();
                parsedGallery = gallery;
                return true;
            }
            catch (WebException ex)
            {
                // catch 404
                if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                {
                    parsedGallery = null;
                    return false;
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
