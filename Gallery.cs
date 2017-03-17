using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibHitomi
{
    public class Gallery
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

        public static string[] GetImageUrls(Gallery gallery)
        {
            return GetImageUrls(gallery.Id);
        }

        // Json Deserialization
        [JsonConstructor()]
        internal Gallery()
        {
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
        private string videoFilename;
        [JsonProperty(PropertyName = "videogalleryid")]
        private int videoGalleryId;
        [JsonProperty(PropertyName = "id")]
        internal int id;

        // Public methods
        public string[] getImageUrls()
            => GetImageUrls(this.id);

        public string[] getThumbnailUrls()
            => GetThumbnailUrls(this.id);

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
