﻿using System;
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
    /// <summary xml:lang="ko">
    /// 검색 쿼리를 입력받아 검색 제안을 제공합니다.
    /// </summary>
    /// <summary>
    /// Makes search suggetions from search query
    /// </summary>
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
        private string[] getNamespaces(bool downloadable = true, bool undownloadable = false)
        {
            string[] arr = new string[] { };
            if (downloadable) arr = arr.Concat(new String[] { "artist", "character", "female", "group", "language", "male", "series", "tag" }).ToArray();
            if (undownloadable) arr = arr.Concat(new String[] { "type", "name" }).ToArray();
            return arr;
        }
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
                foreach (string prop in getNamespaces(true, true))
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
                foreach (string ns in getNamespaces(true, false))
                {
                    HitomiTagInfo[] tagInfos = (HitomiTagInfo[])(typeof(HitomiTags).GetProperty(ns).GetValue(tagAutocompleteList));
                    foreach (HitomiTagInfo tagInfo in tagInfos)
                    {
                        string text = tagInfo.Text.Trim().Replace(' ', '_');
                        suggestions[ns].Add(text);
                        sortRankings[ns].Add(text, tagInfo.Count);
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
                string[] everyNamespaces = getNamespaces(true, true);
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
                        string match = lastThing.Split(':').Last().Trim().ToLower();
                        bool isExclusive = false;

                        if(!everyNamespaces.Contains(ns) && !everyNamespaces.Select(i => "-" + i).Contains(ns))
                        {
                            foreach (string i in everyNamespaces)
                            {
                                suggests.Add(query + " " + i + ":");
                            }
                            return suggests.ToArray();
                        }
                        if (ns.StartsWith("-"))
                        {
                            isExclusive = true;
                            ns = ns.Substring(1);
                        }
                        string[] simillarMatches = suggestions[ns].ToList().FindAll(new Predicate<string>((string i) =>
                        {
                            return i.ToLower().StartsWith(match); // Optimization for C# Textbox Autocomplete
                        })).OrderBy((i) => { return (ns == "name" || ns == "type" || !sortRankings[ns].ContainsKey(match)) ? 0 : sortRankings[ns][match]; }).ToArray();
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
        /// <summary>
        /// 초기화가 시작됐을때 호출됩니다.
        /// </summary>
        public event InitializationStartedDelegate InitializationStarted;
        /// <summary>
        /// 초기화가 완료됐을때 호출됩니다.
        /// </summary>
        public event InitializationCompletedDelegate InitializaitonCompleted;
        /// <summary>
        /// 검색 제안이 생성된 후에 호출됩니다.
        /// </summary>
        public event autoCompleteSuggestedDelegate Suggested;
        /// <summary>
        /// 초기화 됐는지의 여부입니다.
        /// </summary>
        public bool Initialized { get { return inited; } }
        /// <summary>
        /// 이 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="gallery">검색 제안 제공에 쓰일 모든 갤러리들의 목록입니다.</param>
        public void Init(Gallery[] gallery)
        {
            Thread thr = new Thread(init);
            thr.Start(gallery);
        }
        /// <summary>
        /// 비동기로 검색 제안을 생성합니다. 생성된 후엔 Suggested 이벤트가 발생됩니다.
        /// </summary>
        /// <param name="query">검색어</param>
        public void SuggestAsync(string query)
        {
            Thread thr = new Thread(suggestThread);
            thr.Start(query);
        }
        /// <summary>
        /// 동기적으로 검색 제안을 생성합니다.
        /// </summary>
        /// <param name="query">검색어</param>
        /// <returns>생성된 검색 제안</returns>
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
