using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Xml;
using Debug = System.Diagnostics.Debug;

namespace LibHitomi.GalleryList
{
    public class ListDownloaderBase
    {
        protected string searchlibUrl = RequestHelper.CreateUrl(DownloadOptions.JsonSubdomain, "/searchlib.js");
        protected Regex jsonCountPattern = new Regex("number_of_gallery_jsons\\s?=\\s?([0-9]+)");
        protected int getJsonCount()
        {
            HttpWebRequest wreq = RequestHelper.CreateRequest(searchlibUrl);
            using (Stream str = wreq.GetResponse().GetResponseStream())
            using (StreamReader sre = new StreamReader(str))
            {
                string res = sre.ReadToEnd();
                Match match = jsonCountPattern.Match(res);
                return int.Parse(match.Groups[1].Value);
            }
        }
        /// <summary xml:lang="ko">
        /// RSS를 읽어 가장 최근에 올라온 갤러리의 작품 ID를 반환합니다.
        /// </summary>
        /// <summary>
        /// Reads rss and return gallery id which is added most recently.
        /// </summary>
        /// <returns></returns>
        public static int GetLatestGalleryID()
        {
            Regex regex = new Regex("https://hitomi\\.la/galleries/([0-9]+).html", RegexOptions.Compiled);
            HttpWebRequest wreq = RequestHelper.CreateRequest("", "/index-all.atom");
            using (WebResponse wres = wreq.GetResponse())
            using (Stream str = wres.GetResponseStream())
            using (XmlReader xmlReader = XmlReader.Create(str))
            {
                bool isEntry = false, isIdNode = false;
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "entry")
                        isEntry = true;
                    else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "id" && isEntry)
                        isIdNode = true;
                    else if (xmlReader.NodeType == XmlNodeType.Text && isIdNode)
                        return int.Parse(regex.Match(xmlReader.Value).Groups[1].Value);
                }
            }
            throw new NotImplementedException();
        }
    }
}
