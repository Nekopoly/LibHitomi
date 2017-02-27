using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi
{
    /// <summary>
    /// 문자열 형태의 검색어를 받아 갤러리 중에서 특정 조건에 부합하는 갤러리를 찾아냅니다.
    /// </summary>
    public class SimpleSearcher
    {
        /// <summary>
        /// 검색을 수행합니다.
        /// </summary>
        /// <param name="galleries">검색 대상인 갤러리들</param>
        /// <param name="query">문자열로 된 검색어</param>
        /// <returns></returns>
        public Gallery[] Search(List<Gallery> galleries, string query)
        {
            string[] splitted = query.Trim().Split(' ');
            List<Gallery> result = new List<Gallery>(galleries);
            foreach (string i in splitted)
            {
                string ns = i.Split(':')[0].ToLower();
                string match = i.Split(':')[1].ToLower().Replace('_', ' ');

                if (ns == "tag" || ns == "male" || ns == "female")
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
                                return true;
                        }
                        return false;
                    }
                    else if (ns == "Name")
                    {
                        string name = gallery.Name.ToLower();
                        return name.Contains(match.ToLower());
                    } else if (ns == "Language" || ns == "Type")
                    {
                        return (string)gallery.GetType().GetProperty(ns).GetValue(gallery) == match.ToLower();
                    } else
                    {
                        throw new Exception("알 수 없는 네임스페이스입니다");
                    }
                }));
            }
            return result.ToArray();
        }
    }
}
