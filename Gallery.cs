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
        public static string[] GetImageUrls(int galleryId)
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
            foreach(JObject obj in arr)
            {
                // galleries/20267/kairakuten200608_001.jpg
                urls.Add(RequestHelper.CreateUrl(DownloadOptions.ImageSubdomain, $"/galleries/{galleryId}/{obj["name"]}"));
            }
            return urls.ToArray();
        }
        public static string[] GetImageUrls(Gallery gallery)
        {
            return GetImageUrls(gallery.Id);
        }

        // Json Deserialization
        [JsonConstructor()]
        internal Gallery()
        {

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
        [JsonProperty(PropertyName = "id")]
        internal int id;

        // Public methods
        public string[] getImageUrls()
        {
            return GetImageUrls(this.id);
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
        public int Id
        {
            get { return id; }
        }
    }
}
