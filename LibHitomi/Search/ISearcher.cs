using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Search
{
    /// <summary>
    /// 히토미 갤러리를 검색하는 인터페이스입니다.
    /// </summary>
    public interface ISearcher
    {
        IEnumerable<Gallery> Search(IEnumerable<Gallery> galleries, string query);
    }
}
