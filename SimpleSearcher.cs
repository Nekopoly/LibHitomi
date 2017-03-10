using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi
{
    public delegate List<Gallery> FromQueryCalledDelegate(string from, out bool success);
    /// <summary>
    /// 문자열 형태의 검색어를 받아 갤러리 중에서 특정 조건에 부합하는 갤러리를 찾아냅니다.
    /// </summary>
    public class SimpleSearcher
    {
        /// <summary>
        /// from 태그가 포함될 때 호출됩니다.
        /// </summary>
        public event FromQueryCalledDelegate FromQueryCalled;
        /// <summary>
        /// 검색을 수행합니다.
        /// </summary>
        /// <param name="galleries">검색 대상인 갤러리들</param>
        /// <param name="query">문자열로 된 검색어</param>
        /// <returns></returns>
        public Gallery[] Search(List<Gallery> galleries, string query)
        {
            return search(galleries, query);
        }
        public Gallery[] search(List<Gallery> galleries, string query, string from = "")
        {
            if (query.Trim().Length == 0)
                return galleries.ToArray();
            string[] splitted = query.Trim().Split(' ');
            List<Gallery> result = new List<Gallery>(galleries);
            int limit = -1;
            foreach (string i in splitted)
            {
                if (!i.Contains(':'))
                    continue;
                bool isExclusion = false;
                string ns = i.Split(':')[0].ToLower();
                string match = i.Split(':')[1].ToLower().Replace('_', ' ');
                if(ns.StartsWith("-"))
                {
                    isExclusion = true;
                    ns = ns.Substring(1);
                }


                if (ns == "male" || ns == "female")
                {
                    match = ns + ":" + match;
                    ns = "Tags";
                }
                else if (ns == "tag")
                    ns = "Tags";
                else if (ns == "artist")
                    ns = "Artists";
                else if (ns == "group" || ns == "circle")
                    ns = "Groups";
                else if (ns == "series" || ns == "parody")
                    ns = "Parodies";
                else if (ns == "character")
                    ns = "Characters";
                else if (ns == "language")
                    ns = "Language";
                else if (ns == "name" || ns == "title")
                    ns = "Name";
                else if (ns == "type")
                    ns = "Type";
                else if (ns == "from")
                {
                    bool success = false;
                    if (FromQueryCalled == null)
                    {
                        continue;
                    }
                    List<Gallery> fromGalleries = FromQueryCalled(match, out success);
                    if(success && match != from)
                    {
                        return search(fromGalleries, query, match);
                    } else
                    {
                        continue;
                    }
                }
                else if (ns == "limit") {
                    limit = int.Parse(match);
                    continue;
                }
                else
                    throw new Exception("읽을 수 없는 검색어입니다.");

                // http://stackoverflow.com/a/1197004
                result = result.FindAll(new Predicate<Gallery>((Gallery gallery) =>
                {
                    if (ns != "Name" && ns != "Language" && ns != "Type")
                    {
                        string[] values = (string[])gallery.GetType().GetProperty(ns).GetValue(gallery);
                        foreach (string j in values)
                        {
                            if (j.ToLower() == match.ToLower())
                                return !isExclusion;
                        }
                        return isExclusion;
                    }
                    else if (ns == "Name")
                    {
                        string name = gallery.Name.ToLower();
                        bool matched = name.Contains(match.ToLower());
                        if (isExclusion) matched = !matched;
                        return matched;
                    } else if (ns == "Language" || ns == "Type")
                    {
                        bool matched = (string)gallery.GetType().GetProperty(ns).GetValue(gallery) == match.ToLower();
                        if (isExclusion) matched = !matched;
                        return matched;
                    } else
                    {
                        throw new Exception("알 수 없는 네임스페이스입니다");
                    }
                }));
            }
            if (limit > 0)
            {
                result = new List<Gallery>(result.Take(limit));
            }
            return result.ToArray();
        }
    }
}
