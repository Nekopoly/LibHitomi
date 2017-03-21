using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Text;

namespace LibHitomi
{
    public delegate void InitializationCompletedDelegate();
    public delegate void InitializationStartedDelegate();
    public delegate void autoCompleteSuggestedDelegate(string[] results);
    public class SearchSuggester
    {
        // These classes are used for deserialization
        private class HitomiTags
        {
            public HitomiTagInfo[] artist { get; set; }
            public HitomiTagInfo[] character { get; set; }
            public HitomiTagInfo[] female { get; set; }
            public HitomiTagInfo[] group { get; set; }
            public HitomiTagInfo[] language { get; set; }
            public HitomiTagInfo[] male { get; set; }
            public HitomiTagInfo[] series { get; set; }
            public HitomiTagInfo[] tag { get; set; }
        }
        private class HitomiTagInfo
        {
            [JsonProperty(PropertyName = "s")]
            public string Text { get; set; }
            [JsonProperty(PropertyName = "t")]
            public int Count { get; set; }
        }
        private string[] downloadableNamespaces = { "artist", "character", "female", "group", "language", "male", "series", "tag" };
        private string[] undownloadableNamespaces = { "type", "name" };
        Dictionary<string, HashSet<string>> suggestions = new Dictionary<string, HashSet<string>>();
        Dictionary<string, Dictionary<string, int>> sortRankings = new Dictionary<string, Dictionary<string, int>>();
        bool inited = false;
        private void init(object _galleries)
        {
            InitializationStarted();
            lock (suggestions)
            {
                Gallery[] galleries = (Gallery[])_galleries;
                suggestions.Clear();
                foreach (string prop in downloadableNamespaces.Concat(undownloadableNamespaces).ToArray())
                {
                    suggestions[prop] = new HashSet<string>();
                    sortRankings[prop] = new Dictionary<string, int>();
                }
                HttpWebRequest wreq = RequestHelper.CreateRequest("", "/tags.json");
                HitomiTags tagAutocompleteList = null;
                using (WebResponse wres = wreq.GetResponse())
                using (Stream str = wres.GetResponseStream())
                using (StreamReader sre = new StreamReader(str))
                using (JsonReader reader = new JsonTextReader(sre))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    tagAutocompleteList = serializer.Deserialize<HitomiTags>(reader);
                }
                foreach (string ns in downloadableNamespaces)
                {
                    HitomiTagInfo[] tagInfos = (HitomiTagInfo[])(typeof(HitomiTags).GetProperty(ns).GetValue(tagAutocompleteList));
                    foreach (HitomiTagInfo tagInfo in tagInfos)
                    {
                        suggestions[ns].Add(tagInfo.Text);
                        sortRankings[ns].Add(tagInfo.Text, tagInfo.Count);
                    }
                }
                foreach (Gallery gallery in galleries)
                {
                    suggestions["type"].Add(gallery.Type);
                    suggestions["name"].Add(gallery.Name);
                }
                inited = true;
            }
            InitializaitonCompleted();
        }
        private string[] suggest(string query)
        {
            if (!inited)
                return new string[] { };
            lock (suggestions)
            {
                string[] everyNamespaces = downloadableNamespaces.Concat(undownloadableNamespaces).ToArray();
                List<string> suggests = new List<string>();
                if (query.EndsWith(" ") || query.Trim().Length == 0)
                {
                    foreach (string i in everyNamespaces)
                    {
                        suggests.Add(query + i + ":");
                    }
                    return suggests.ToArray();
                }
                else if (!query.Contains(":"))
                {
                    foreach (string i in everyNamespaces)
                    {
                        suggests.Add(i + ":");
                        suggests.Add("-" + i + ":");
                    }
                    return suggests.ToArray();
                }
                else
                {
                    string[] splitted = query.Split(' ');
                    string lastThing = splitted.Last();
                    string withoutLastThing = splitted.Length == 1 ? "" : string.Join(" ", splitted.Take(splitted.Length - 1).ToArray());
                    if (lastThing.Contains(":"))
                    {
                        string ns = lastThing.Split(':').First();
                        string matchInput = lastThing.Split(':').Last().Trim();
                        string matchWithWhitespaces = matchInput.Trim().Replace('_', ' ');
                        bool isExclusive = false;

                        if (ns.StartsWith("-"))
                        {
                            isExclusive = true;
                            ns = ns.Substring(1);
                        }
                        if (!everyNamespaces.Contains(ns))
                            return new string[] { };
                        string[] simillarMatches = suggestions[ns].ToList().FindAll(new Predicate<string>((string i) =>
                        {
                            return i.ToLower().StartsWith(matchInput.ToLower()); // Optimization for C# Textbox Autocomplete
                        })).OrderBy((i) => { return (ns == "name" || ns == "type" || !sortRankings[ns].ContainsKey(matchWithWhitespaces)) ? 0 : sortRankings[ns][matchWithWhitespaces]; }).ToArray();
                        foreach (string i in simillarMatches)
                        {
                            suggests.Add(withoutLastThing + (withoutLastThing == "" ? "" : " ") + (isExclusive ? "-" : "") + ns + ":" + i);
                        }
                        return suggests.ToArray();
                    }
                    else
                    {
                        foreach (string i in everyNamespaces)
                        {
                            suggests.Add(withoutLastThing + " " + i + ":");
                            suggests.Add(withoutLastThing + " -" + i + ":");
                        }
                    }
                    return suggests.ToArray();
                }
            }
        }
        private void suggestThread(object query)
        {
            string[] suggestions = suggest((string)query);
            Suggested(suggestions);
        }
        public event InitializationStartedDelegate InitializationStarted;
        public event InitializationCompletedDelegate InitializaitonCompleted;
        public event autoCompleteSuggestedDelegate Suggested;
        public bool Initialized { get { return inited; } }
        public void Init(Gallery[] gallery)
        {
            Thread thr = new Thread(init);
            thr.Start(gallery);
        }
        public void SuggestAsync(string query)
        {
            Thread thr = new Thread(suggestThread);
            thr.Start(query);
        }
        public string[] SuggestSync(string query)
        {
            return suggest(query);
        }
        public SearchSuggester()
        {
            InitializationStarted += () => { };
            InitializaitonCompleted += () => { };
            Suggested += (a) => { };
        }

    }
}
