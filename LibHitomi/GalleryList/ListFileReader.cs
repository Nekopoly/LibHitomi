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

namespace LibHitomi.GalleryList
{
    /// <summary>
    /// 갤러리 정보가 담긴 json파일을 읽습니다. 주로 403/404 표시되는 갤러리 정보를 읽을 때 쓰입니다.
    /// </summary>
    /// <summary lang="en-US">
    /// Read json file which contains gallery infos. This class is usually used to get gallery informations which are hidden or forbidden.
    /// </summary>
    public class ListFileReader
    {
        /// <summary>
        /// Json 파일을 읽습니다.
        /// </summary>
        /// <summary lang="en-US">
        /// Reads json file
        /// </summary>
        /// <param name="stream">Json 파일의 스트림입니다.</param>
        /// <param name="stream" lang="en-US">Stream of json file</param>
        /// <returns></returns>
        public IEnumerable<Gallery> LoadJson(Stream stream)
        {
            using (StreamReader sre = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(sre))
            {
                JsonSerializer serializer = new JsonSerializer();
                Gallery[] result = serializer.Deserialize<Gallery[]>(reader);
                for (int i = 0; i < result.Length; i++)
                {
                    result[i].GalleryCrawlMethod = GalleryCrawlMethod.AddedManually;
                }
                return result;
            }
        }
        /// <summary>
        /// Json 파일을 읽습니다.
        /// </summary>
        /// <param name="filename">Json 파일 경로입니다.</param>
        /// <param name="filename" lang="en-US">Path to json file</param>
        /// <returns></returns>
        public IEnumerable<Gallery> LoadJson(string filename)
        {
            using (FileStream fstr = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return LoadJson(fstr);
            }
        }
    }
}