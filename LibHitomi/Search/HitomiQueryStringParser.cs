using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
    /// <summary>
    /// 검색 쿼리 처리 방식을 나타냅니다.
    /// </summary>
    enum QueryMatchType
    {
        /// <summary>
        /// 일치
        /// </summary>
        Equals,
        /// <summary>
        /// 포함
        /// </summary>
        Contains,
        /// <summary>
        /// N/A(지정되지 않음)
        /// </summary>
        NA
    }
    /// <summary>
    /// 태그 네임스페이스를 나타냅니다.
    /// </summary>
    enum TagNamespace
    {
        // 내부적으로 tag:로 처리됨.
        //Male,
        //Female,
        Tag,
        Artist,
        Group,
        Series,
        Character,
        Language,
        Name,
        Type,
        /// <summary>
        /// 검색결과 갯수 제한 쿼리
        /// </summary>
        LibHitomi_Limit,
        /// <summary>
        /// 검색결과 오프셋
        /// </summary>
        LibHitomi_Offset
    }
    /// <summary>
    /// 검색 쿼리 데이터입니다.
    /// </summary>
    struct QueryEntry
    {
        /// <summary>
        /// 태그 네임스페이스를 나타냅니다.
        /// </summary>
        public TagNamespace Namespace { get; set; }
        /// <summary>
        /// 검색 방식을 나타냅니다.
        /// </summary>
        public QueryMatchType QueryType { get; set; }
        /// <summary>
        /// 검색 대상이 배열 형식인지 문자열 형식인지를 나타냅니다.
        /// </summary>
        public bool isForArrayNamespace
        {
            get
            {
                return !(this.Namespace == TagNamespace.Name || this.Namespace == TagNamespace.Language || this.Namespace == TagNamespace.Type);
            }
        }
        /// <summary>
        /// 검색할 값을 나타냅니다.
        /// </summary>
        public string Query;
        /// <summary>
        /// 제외 여부를 나타냅니다.
        /// </summary>
        public bool isExclusion { get; set; }
    }
    class HitomiQueryStringParser
    {
        public IEnumerable<QueryEntry> Parse(string query)
        {
            string[] splitted = query.Trim().Split(' ');
            for (int i = 0; i < splitted.Length; i++)
            {
                string j = splitted[i];
                if (!j.Contains(':'))
                    continue;
                while(i < splitted.Length - 1 && !splitted[i + 1].Contains(':'))
                {
                    j += " " + splitted[++i];
                }
                QueryEntry entry = new QueryEntry();
                entry.isExclusion = false;
                string[] k = j.Split(':');
                string ns = k[0].ToLower();
                string match = k[1].ToLower().Replace('_', ' ');
                entry.Query = match;
                if (ns.StartsWith("-"))
                {
                    entry.isExclusion = true;
                    ns = ns.Substring(1);
                }
                if (ns == "male" || ns == "female")
                {
                    match = ns + ":" + match;
                    entry.Namespace = TagNamespace.Tag;
                }
                else if (ns == "tag")
                    entry.Namespace = TagNamespace.Tag;
                else if (ns == "artist")
                    entry.Namespace = TagNamespace.Artist;
                else if (ns == "group" || ns == "circle")
                    entry.Namespace = TagNamespace.Group;
                else if (ns == "series" || ns == "parody")
                    entry.Namespace = TagNamespace.Series;
                else if (ns == "character")
                    entry.Namespace = TagNamespace.Character;
                else if (ns == "language")
                    entry.Namespace = TagNamespace.Language;
                else if (ns == "name" || ns == "title")
                    entry.Namespace = TagNamespace.Name;
                else if (ns == "type")
                    entry.Namespace = TagNamespace.Type;
                else if (ns == "limit")
                    entry.Namespace = TagNamespace.LibHitomi_Limit;
                else if (ns == "offset")
                    entry.Namespace = TagNamespace.LibHitomi_Offset;
                else
                    continue;
                if (match == "")
                    entry.QueryType = QueryMatchType.NA;
                else if (entry.Namespace == TagNamespace.Name)
                {
                    entry.QueryType = QueryMatchType.Contains;
                } else
                {
                    entry.QueryType = QueryMatchType.Equals;
                }
                yield return entry;
            }
        }
    }
}
