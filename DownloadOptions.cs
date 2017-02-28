using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace LibHitomi
{
    /// <summary>
    /// 서브도메인을 설정합니다.
    /// </summary>
    public static class DownloadOptions
    {
        /// <summary>
        /// 갤러리 목록 다운로드 주소의 서브도메인입니다.
        /// </summary>
        public static string JsonSubdomain { get; set; } = "ltn";
        /// <summary>
        /// 이미지 주소를 생성할 때 사용할 서브도메인입니다
        /// </summary>
        public static string ImageSubdomain { get; set; } = "a";
        /// <summary>
        /// 썸네일 주소를 생성할 때 사용할 서브도메인입니다
        /// </summary>
        public static string ThumbnailSubdomain { get; set; } = "btn";
    }
    internal static class RequestHelper
    {
        /// <summary>
        /// 히토미와 관련된 주소를 생성합니다
        /// </summary>
        /// <param name="subdomain">서브도메인</param>
        /// <param name="path">경로, 예를 들자면 http://example.com/asdf에서 /asdf 부분입니다.</param>
        /// <returns></returns>
        internal static string CreateUrl(string subdomain, string path)
        {
            return subdomain == "" ? $"https://hitomi.la{path}" : $"https://{subdomain}.hitomi.la{path}";
        }
        internal static HttpWebRequest CreateRequest(string uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri) as HttpWebRequest;
            req.Accept = "*/*";
            req.KeepAlive = true;
            req.Method = "GET";
            req.Pipelined = true;
            req.Referer = "https://hitomi.la";
            req.ServicePoint.ConnectionLimit = 10000;
            req.ServicePoint.Expect100Continue = false;
            req.UserAgent = "Mozilla/5.0 (compatible)";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return req;
        }
        internal static HttpWebRequest CreateRequest(string subdomain, string path)
        {
            return CreateRequest(CreateUrl(subdomain, path));
        }
    }
}
