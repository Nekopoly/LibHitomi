using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
    public class HitomiQueryStringParser : QueryStringParser
    {
        public override IEnumerable<QueryEntry> Parse(string query)
        {
            string[] splitted = query.Trim().Split(' ');
            foreach (string i in splitted)
            {
                if (!i.Contains(':'))
                    continue;
                QueryEntry entry = new QueryEntry();
                bool isExclusion = false;
                string ns = i.Split(':')[0].ToLower();
                string match = i.Split(':')[1].ToLower().Replace('_', ' ');
                if (ns.StartsWith("-"))
                {
                    isExclusion = true;
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
#if SupportIdSelectQuery
                else if (ns == "id")
                    entry.Namespace = TagNamespace.LibHitomi_Id;
#endif
#if DEBUG && SupportDebugQuery
                else if (ns == "debug")
                    entry.Namespace = TagNamespace.LibHitomi_Debug;
#endif
                else if (ns == "from")
                    entry.Namespace = TagNamespace.LibHitomi_From;
                else if (ns == "limit")
                    entry.Namespace = TagNamespace.LibHitomi_Limit;
                else
                    continue;
                entry.isExclusion = isExclusion;
                entry.Query = match;
                if (match == "~~na~~")
                    entry.QueryType = QueryMatchType.NA;
                else if(entry.Namespace == TagNamespace.Name)
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
