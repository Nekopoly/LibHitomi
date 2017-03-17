using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace LibHitomi.Downloader
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
            Gallery gallery = new LibHitomi.Gallery();
            Regex pattern = new Regex("^/([a-zA-Z]+)/(.+)-all-1\\.html$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex languagePattern = new Regex("^/index-([a-zA-Z]+)-1\\.html$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            HttpWebRequest wreq = RequestHelper.CreateRequest("", $"/galleryblock/{id}.html");
            try
            {
                using (WebResponse wres = wreq.GetResponse())
                using (Stream str = wres.GetResponseStream())
                    htmlDocument.Load(str);
                
                gallery.name = htmlDocument.DocumentNode.SelectNodes("/h1")[0].InnerText;
                foreach (HtmlNode linkNode in htmlDocument.DocumentNode.SelectNodes("/a[ @href ]"))
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

                        if (propName.EndsWith("s"))
                        {
                            object current = gallery.GetType().GetField(propName).GetValue(gallery);
                            gallery.GetType().GetField(propName).SetValue(gallery, (string[])(((string[])current).Concat(new string[] { value })));
                        }
                        else
                        {
                            gallery.GetType().GetField(propName).SetValue(gallery, value);
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
                parsedGallery = gallery;
                return true;
            }
            catch (WebException ex)
            {
                // catch 404
                if((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                {
                    parsedGallery = null;
                    return false;
                } else
                {
                    throw ex;
                }
            }
        }
    }
}
