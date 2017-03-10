using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace LibHitomi
{
    public delegate void InitializationCompletedDelegate();
    public delegate void autoCompleteSuggestedDelegate(string[] results);
    public class SearchSuggester
    {
        Dictionary<string, HashSet<string>> suggestions = new Dictionary<string, HashSet<string>>();
        Dictionary<string, string> namespaceMap = null;
        bool inited = false;
        static string[] arrayProps = { "Artists", "Groups", "Parodies", "Tags", "Characters" };
        static string[] nonArrayProps = { "Type", "Language", "Name" };
        static string[] allProps = arrayProps.Concat(nonArrayProps).ToArray();
        private void initNamespaceMap()
        {
            if (namespaceMap == null)
            {
                namespaceMap = new Dictionary<string, string>() {
                    { "male", "MaleTags" },
                    { "female", "FemaleTags" }, // male:, female:도 내부적으론 Tag 속성에 있음.
                    { "tag", "Tags" },
                    { "artist", "Artists" },
                    { "group", "Groups" },
                    { "circle", "Groups" },
                    { "series", "Parodies" },
                    { "parody", "Parodies" },
                    { "character", "Character" },
                    { "language", "Language" },
                    { "name", "Name" },
                    { "title", "Name" },
                    { "type", "Type" }
                };
            }

        }
        private void init(object _galleries)
        {
            Gallery[] galleries = (Gallery[])_galleries;
            initNamespaceMap();
            suggestions.Clear();
            foreach (string prop in allProps.Concat(new string[] { "FemaleTags", "MaleTags" }).ToArray())
            {
                suggestions[prop] = new HashSet<string>();
            }
            foreach (Gallery gallery in galleries)
            {
                foreach (string arrayProp in arrayProps)
                {
                    string[] a = (string[])gallery.GetType().GetProperty(arrayProp).GetValue(gallery);
                    if (arrayProp == "Tags")
                    {
                        // male, female 분리
                        foreach (string i in a)
                        {
                            if (i.ToLower().StartsWith("female:"))
                                suggestions["FemaleTags"].Add(i.ToLower().Substring("female:".Length).Replace(' ', '_'));
                            else if (i.ToLower().StartsWith("male:"))
                                suggestions["MaleTags"].Add(i.ToLower().Substring("male:".Length).Replace(' ', '_'));
                            else
                                suggestions["Tags"].Add(i.ToLower().Replace(' ', '_'));
                        }
                    }
                    else
                    {
                        foreach (string i in a)
                            suggestions[arrayProp].Add(i.Replace(' ', '_'));
                    }
                }
                foreach(string nonArrayProp in nonArrayProps)
                {
                    string a = (string)gallery.GetType().GetProperty(nonArrayProp).GetValue(gallery);
                    suggestions[nonArrayProp].Add(a.Replace(' ', '_'));
                }
            }
            inited = true;
            InitializaitonCompleted();
        }
        private string[] suggest(string query)
        {
            if (!inited)
                return new string[] { };
            List<string> suggests = new List<string>();
            if (query.EndsWith(" ") || query.Trim().Length == 0)
            {
                foreach (string i in namespaceMap.Keys)
                {
                    suggests.Add(query + i + ":");
                }
                return suggests.ToArray();
            }
            else if (!query.Contains(":"))
            {
                foreach (string i in namespaceMap.Keys)
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
                    string match = lastThing.Split(':').Last();
                    bool isExclusive = false;

                    if (ns.StartsWith("-"))
                    {
                        isExclusive = true;
                        ns = ns.Substring(1);
                    }
                    if (!namespaceMap.ContainsKey(ns))
                        return new string[] { };
                    string matchedProperty = namespaceMap[ns];
                    string[] simillarMatches = suggestions[matchedProperty].ToList().FindAll(new Predicate<string>((string i) =>
                    {
                        return i.ToLower().Contains(match.ToLower());
                    })).OrderBy((i) => { return i.ToLower().IndexOf(match.ToLower()); }).ToArray();
                    foreach (string i in simillarMatches)
                    {
                        suggests.Add(withoutLastThing + (withoutLastThing == "" ? "" : " ") + (isExclusive ? "-" : "") + ns + ":" + i);
                    }
                    return suggests.ToArray();
                }
                else
                {
                    foreach (string i in namespaceMap.Keys)
                    {
                        suggests.Add(withoutLastThing + " " + i + ":");
                        suggests.Add(withoutLastThing + " -" + i + ":");
                    }
                }
                return suggests.ToArray();
            }
        }
        private void suggestThread(object query)
        {
            string[] suggestions = suggest((string)query);
            Suggested(suggestions);
        }
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
            InitializaitonCompleted += () => { };
            Suggested += (a) => { };
        }

    }
}
