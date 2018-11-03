using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
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
    public class SimpleSearcher
    {
        /// <summary>
        /// 검색을 수행합니다.
        /// </summary>
        /// <param name="galleries">검색 대상인 갤러리들</param>
        /// <param name="query">문자열로 된 검색어</param>
        /// <returns></returns>
        public IEnumerable<Gallery> Search(IEnumerable<Gallery> galleries, string query)
        {
            HitomiQueryStringParser parser = new HitomiQueryStringParser();
            return this.Search(galleries, parser.Parse(query));
        }
        /// <summary>
        /// 검색을 수행합니다.
        /// </summary>
        /// <param name="galleries">검색 대상인 갤러리들</param>
        /// <param name="query">QueryStringParser를 구현한 클래스에 의해 분석된 쿼리 데이터</param>
        /// <returns></returns>
        private IEnumerable<Gallery> Search(IEnumerable<Gallery> galleries, IEnumerable<QueryEntry> query)
        {
            if (query.Count() == 0)
                return galleries.ToArray();
            List<Gallery> result = new List<Gallery>(galleries);
            CaseInsensitiveStringComparer caseInsensitiveComparer = new CaseInsensitiveStringComparer();
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
                else if (i.Namespace == TagNamespace.LibHitomi_Limit)
                {
                    if(int.TryParse(i.Query, out int temp)) limit = temp;
                    continue;
                }
                else if (i.Namespace == TagNamespace.LibHitomi_Offset)
                {
                    if (int.TryParse(i.Query, out int temp)) offset = temp;
                    continue;
                }
                else
                    throw new InvalidQueryException();

                // http://stackoverflow.com/a/1197004
                if (i.isForArrayNamespace)
                {
                    result = result.FindAll(gallery => (i.QueryType == QueryMatchType.NA ? 
                             getPropertyValueByName<string[]>(gallery, ns).Length == 0 :
                             (i.QueryType == QueryMatchType.Equals ? 
                             getPropertyValueByName<string[]>(gallery, ns).Contains(i.Query, caseInsensitiveComparer) : 
                             getPropertyValueByName<string[]>(gallery, ns).Any(x => x.CaseInsensitiveContains(i.Query)))) ^ i.isExclusion);
                } else
                {
                    result = result.FindAll(gallery => (i.QueryType == QueryMatchType.NA ?
                             getPropertyValueByName<string>(gallery, ns) == "" || getPropertyValueByName<string>(gallery, ns) == null :
                             (i.QueryType == QueryMatchType.Contains ?
                             getPropertyValueByName<string>(gallery, ns).CaseInsensitiveContains(i.Query) :
                             getPropertyValueByName<string>(gallery, ns).CaseInsensitiveEquals(i.Query))) ^ i.isExclusion);
                }
            }
            if (offset > 0)
            {
                return result.Skip(offset);
            }
            if (limit > 0)
            {
                return result.Take(limit);
            }
            return result;
        }
        private T getPropertyValueByName<T>(Gallery gallery, string propName)
        {
            return (T)gallery.GetType().GetProperty(propName).GetValue(gallery);
        }
    }
}
