using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
    public abstract class QueryStringParser
    {
        public abstract IEnumerable<QueryEntry> Parse(string query);
    }
}
