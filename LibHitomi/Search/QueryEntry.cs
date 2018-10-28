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
    public enum QueryMatchType
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
    public enum TagNamespace
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
        /// Id 선택 쿼리
        /// </summary>
        LibHitomi_Id,
        /// <summary>
        /// 디버그용 쿼리
        /// </summary>
        LibHitomi_Debug,
        /// <summary>
        /// 전체집합 선택 쿼리
        /// </summary>
        LibHitomi_From,
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
    public struct QueryEntry
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
        public bool isForArrayNamespace { get
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
}
