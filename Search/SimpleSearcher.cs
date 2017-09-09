using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
    public delegate List<Gallery> FromQueryCalledDelegate(string from, out bool success);
    /// <summary>
    /// 잘못된 검색어를 입력한 경우 발생하는 예외입니다.
    /// </summary>
    public class InvalidQueryException : Exception
    {
        internal InvalidQueryException() : base("잘못된 검색어를 입력하셨습니다.")
        {

        }
    }
    /// <summary>
    /// 검색 쿼리를 받아 갤러리 중에서 특정 조건에 부합하는 갤러리를 찾아냅니다.
    /// </summary>
    public class Searcher
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
        [Obsolete()]
        public Gallery[] Search(List<Gallery> galleries, string query, string from = "")
        {
            HitomiQueryStringParser parser = new HitomiQueryStringParser();
            return this.Search(galleries, parser.Parse(query), from);
        }
        /// <summary>
        /// 검색을 수행합니다.
        /// </summary>
        /// <param name="galleries">검색 대상인 갤러리들</param>
        /// <param name="query">QueryStringParser를 구현한 클래스에 의해 분석된 쿼리 데이터</param>
        /// <returns></returns>
        public Gallery[] Search(List<Gallery> galleries, IEnumerable<QueryEntry> query, string from = "")
        {
            if (query.Count() == 0)
                return galleries.ToArray();
            List<Gallery> result = new List<Gallery>(galleries);
            int limit = -1, offset = -1;
            string ns;
            foreach (QueryEntry i in query)
            {
                if (i.Namespace == TagNamespace.Tag)
                    ns = "Tags";
                else if (i.Namespace == TagNamespace.Artist)
                    ns = "Artists";
                else if (i.Namespace == TagNamespace.Group)
                    ns = "Groups";
                else if (i.Namespace == TagNamespace.Series)
                    ns = "Parodies";
                else if (i.Namespace == TagNamespace.Character)
                    ns = "Characters";
                else if (i.Namespace == TagNamespace.Language)
                    ns = "Language";
                else if (i.Namespace == TagNamespace.Name)
                    ns = "Name";
                else if (i.Namespace == TagNamespace.Type)
                    ns = "Type";
                else if (i.Namespace == TagNamespace.LibHitomi_Id)
                {
#if SupportIdSelectQuery
                    int parsedId = int.Parse(i.Query);
                    result.Add(galleries.Where((Gallery gallery) => { return gallery.id == parsedId; }).First());
#endif
                    continue;
                }
                else if (i.Namespace == TagNamespace.LibHitomi_Debug)
                {
#if DEBUG && SupportDebugQuery
                    if (i.Query.StartsWith("clear"))
                        result.Clear();
#endif
                    continue;
                }
                else if (i.Namespace == TagNamespace.LibHitomi_From)
                {
                    bool success = false;
                    if (FromQueryCalled == null)
                    {
                        continue;
                    }
                    List<Gallery> fromGalleries = FromQueryCalled(i.Query, out success);
                    if (success && i.Query != from)
                    {
                        return Search(fromGalleries, query, i.Query);
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (i.Namespace == TagNamespace.LibHitomi_Limit)
                {
                    limit = int.Parse(i.Query);
                    continue;
                }
                else if (i.Namespace == TagNamespace.LibHitomi_Offset)
                {
                    offset = int.Parse(i.Query);
                    continue;
                }
                else
                    throw new InvalidQueryException();

                // http://stackoverflow.com/a/1197004
                result = result.FindAll(new Predicate<Gallery>((Gallery gallery) =>
                {
                    if (i.isForArrayNamespace)
                    {
                        string[] values = (string[])gallery.GetType().GetProperty(ns).GetValue(gallery);
                        if(i.QueryType == QueryMatchType.NA)
                        {
                            return values.Length == 0;
                        }
                        foreach (string j in values)
                        {
                            if (j.ToLower() == i.Query.ToLower())
                                return !i.isExclusion;
                        }
                        return i.isExclusion;
                    }
                    else
                    {
                        string gallval = (string)gallery.GetType().GetProperty(ns).GetValue(gallery);
                        gallval = gallval.ToLower();
                        if (i.QueryType == QueryMatchType.NA) return gallval == "" || gallval == null;
                        bool matched = i.QueryType == QueryMatchType.Contains ? gallval.Contains(i.Query.ToLower()) : gallval == i.Query.ToLower();
                        if (i.isExclusion) matched = !matched;
                        return matched;
                    }
                }));
            }
            if (offset > 0)
            {
                result = new List<Gallery>(result.Skip(offset));
            }
            if (limit > 0)
            {
                result = new List<Gallery>(result.Take(limit));
            }
            return result.ToArray();
        }
    }
}
