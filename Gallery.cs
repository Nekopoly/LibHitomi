using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LibHitomi
{
    /// <summary>
    /// 갤러리가 크롤링된 방법입니다.
    /// </summary>
    public enum GalleryCrawlMethod
    {
        /// <summary>
        /// ListUpdater 클래스에 의해 정상적으로 크롤링됐습니다.
        /// </summary>
        Normal,
        /// <summary>
        /// Gallery.GetGalleryByParsingGalleryBlock() 메소드를 이용하여 크롤링됐습니다.
        /// </summary>
        ParsedGalleryBlock,
        /// <summary>
        /// ListUpdater의 ExtraGalleries 관련 속성/기능을 이용하여 크롤링되지 아니하고 수동으로 추가됐습니다.
        /// </summary>
        AddedManually
    }
    public class Gallery : IEqualityComparer<Gallery>
    {
        // Static Members
        private static string[] getImageNumbs(int galleryId)
        {
            HttpWebRequest wreq = RequestHelper.CreateRequest("", $"/galleries/{galleryId}.js");
            string responseText;
            List<string> urls = new List<string>();
            // request
            using (Stream str = wreq.GetResponse().GetResponseStream())
            using (StreamReader sr = new StreamReader(str))
                responseText = sr.ReadToEnd();
            // cut
            responseText = responseText.Substring("var galleryinfo = ".Length);
            // parse
            JArray arr = JArray.Parse(responseText);
            return JArray.Parse(responseText).Select(obj => obj["name"].ToString()).ToArray();
        }
        public static string[] GetImageUrls(int galleryId)
            => getImageNumbs(galleryId)
            .Select(n => RequestHelper.CreateUrl(DownloadOptions.ImageSubdomain, $"/galleries/{galleryId}/{n}")).ToArray();
        
        public static string[] GetThumbnailUrls(int galleryId)
            => getImageNumbs(galleryId)
            .Select(n => RequestHelper.CreateUrl(DownloadOptions.ThumbnailSubdomain, $"/smalltn/{galleryId}/{n}.jpg")).ToArray();

        public static void SerializeToJson(IEnumerable<Gallery> galleries, Stream stream)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.None;
                serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
                serializer.NullValueHandling = NullValueHandling.Ignore;
#if DEBUG
                DiagnosticsTraceWriter tw = new DiagnosticsTraceWriter();
                tw.LevelFilter = System.Diagnostics.TraceLevel.Verbose;
                serializer.TraceWriter = tw;
#endif
                serializer.Serialize(sw, galleries.ToArray());
            }
        }

        public static bool GetGalleryByParsingGalleryBlock(int id, out Gallery gallery)
        {
            GalleryBlockParser parser = new GalleryBlockParser();
            return parser.TryParse(id, out gallery);
        }

        public static string[] GetImageUrls(Gallery gallery)
        {
            return GetImageUrls(gallery.Id);
        }

        // instance members
        internal Gallery(GalleryCrawlMethod method)
        {
            this.GalleryCrawlMethod = method;
            UnNull();
        }
        // Json Deserialization
        [JsonConstructor()]
        internal Gallery()
        {
            this.GalleryCrawlMethod = GalleryCrawlMethod.Normal;
            UnNull();
        }
        [JsonProperty(PropertyName = "type")]
        internal string type;
        [JsonProperty(PropertyName = "a")]
        internal string[] artists;
        [JsonProperty(PropertyName = "g")]
        internal string[] groups;
        [JsonProperty(PropertyName = "p")]
        internal string[] parodies;
        [JsonProperty(PropertyName = "t")]
        internal string[] tags;
        [JsonProperty(PropertyName = "c")]
        internal string[] characters;
        [JsonProperty(PropertyName = "l")]
        internal string language;
        [JsonProperty(PropertyName = "n")]
        internal string name;
        [JsonProperty(PropertyName = "videofilename")]
        internal string videoFilename;
        [JsonProperty(PropertyName = "videogalleryid")]
        internal int videoGalleryId;
        [JsonProperty(PropertyName = "id")]
        internal int id;

        // Public properties
        [JsonIgnore()]
        public GalleryCrawlMethod GalleryCrawlMethod { get; internal set; }
        [JsonIgnore()]
        public string[] ThumbnailUrls { get { return getThumbnailUrls(); } }

        // Public methods
        public string[] getImageUrls()
            => GetImageUrls(this.id);

        public string[] getThumbnailUrls()
            => GetThumbnailUrls(this.id);

        public Stream CreateThumbnailStream(int index)
            => RequestHelper.CreateRequest(getThumbnailUrls()[index]).GetResponse().GetResponseStream();

        public string getDownloadableVideoUrl()
            => RequestHelper.CreateUrl(DownloadOptions.ImageSubdomain, $"/videos/{this.videoFilename}");

        public string getStreamingVideoUrl()
            => RequestHelper.CreateUrl(DownloadOptions.VideoStreamingSubdomain, $"/videos/{this.videoFilename}");

        internal void UnNull()
        {
            if (artists == null) artists = new string[] { };
            if (characters == null) characters = new string[] { };
            if (groups == null) groups = new string[] { };
            if (parodies == null) parodies = new string[] { };
            if (tags == null) tags = new string[] { };
            if (language == null) language = "";
            if (name == null) name = "";
        }

        public bool Equals(Gallery x, Gallery y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Gallery obj)
        {
            return obj.Id;
        }

        // Public members
        [JsonIgnore()]
        public string[] Artists
        {
            get { return artists; }
        }
        [JsonIgnore()]
        public string Type
        {
            get { return type; }
        }
        [JsonIgnore()]
        public string[] Groups
        {
            get { return groups; }
        }
        [JsonIgnore()]
        public string[] Parodies
        {
            get { return parodies; }
        }
        [JsonIgnore()]
        public string[] Tags
        {
            get { return tags; }
        }
        [JsonIgnore()]
        public string[] Characters
        {
            get { return characters; }
        }
        [JsonIgnore()]
        public string Language
        {
            get { return language; }
        }
        [JsonIgnore()]
        public string Name
        {
            get { return name; }
        }
        [JsonIgnore()]
        public string VideoFilename
        {
            get { return videoFilename; }
        }
        [JsonIgnore()]
        public int VideoGalleryId
        {
            get { return videoGalleryId; }
        }
        [JsonIgnore()]
        public int Id
        {
            get { return id; }
        }
    }
}
