using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace LibHitomi
{
    /// <summary xml:lang="ko">
    /// 서브도메인을 설정합니다.
    /// </summary>
    /// <summary>
    /// Sets subdomains globally
    /// </summary>
    public static class DownloadOptions
    {
        /// <summary xml:lang="ko">
        /// 갤러리 목록 다운로드 주소의 서브도메인입니다.
        /// </summary>
        /// <summary>
        /// Subdomain used when downloading gallery list
        /// </summary>
        public static string JsonSubdomain { get; set; } = "ltn";
        /// <summary xml:lang="ko">
        /// 갤러리블록 다운로드시의 서브도메인입니다.
        /// </summary>
        /// <summary>
        /// Subdomain used when getting galleryblock.
        /// </summary>
        public static string GalleryBlockSubdomain { get; set; } = "ltn";
        /// <summary xml:lang="ko">
        /// 썸네일 주소를 생성할 때 사용할 서브도메인입니다
        /// </summary>
        /// <summary>
        /// Subodmain used when generating thumbnail url
        /// </summary>
        public static string ThumbnailSubdomain { get; set; } = "btn";
        /// <summary xml:lang="ko">
        /// 동영상 스트리밍 주소를 생성할 때 서용할 서브도메인입니다.
        /// </summary>
        /// <summary>
        /// Subodmain used when generating video url for streaming
        /// </summary>
        public static string VideoStreamingSubdomain { get; set; } = "streaming";
        /// <summary xml:lang="ko">
        /// 다운로드나 갤러리 목록 조회등에 사용할 프록시입니다.
        /// </summary>
        /// <summary>
        /// Proxy used when downloading or downloading gallery list
        /// </summary>
        public static WebProxy DefaultProxy { get; set; } = null;
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
            req.ServicePoint.ConnectionLimit = int.MaxValue;
            req.ServicePoint.Expect100Continue = false;
            req.Accept = "*/*";
            req.KeepAlive = true;
            req.Method = "GET";
            req.Pipelined = true;
            req.Referer = "https://hitomi.la";
            req.ServicePoint.ConnectionLimit = 10000;
            req.ServicePoint.Expect100Continue = false;
            req.UserAgent = "Mozilla/5.0 (compatible)";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            if(DownloadOptions.DefaultProxy != null)
            {
                req.Proxy = DownloadOptions.DefaultProxy;
            }
            return req;
        }
        private static int frontendCount = 0;
        internal static string GalleryIdToImageSubdoamin(int galleryId)
        {
            if (frontendCount == 0)
            {
                HttpWebRequest wreq = CreateRequest(CreateUrl(DownloadOptions.JsonSubdomain, "/common.js"));
                using (HttpWebResponse wres = wreq.GetResponse() as HttpWebResponse)
                using (Stream str = wres.GetResponseStream())
                using (StreamReader sre = new StreamReader(str))
                {
                    Regex frontendPattern = new System.Text.RegularExpressions.Regex(@"var\s?number_of_frontends\s?=\s?([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    frontendCount = int.Parse(frontendPattern.Match(sre.ReadToEnd()).Groups[1].Value);
                }
            }
            const string subdomainBase = "a";
            int first = galleryId % 10;
            if (first == 1) first = 0; // 왜 이렇게 스크립트를 짜놨지...?
            return Char.ConvertFromUtf32(97 + (first % frontendCount)) + subdomainBase;
        }
        internal static HttpWebRequest CreateRequest(string subdomain, string path)
        {
            return CreateRequest(CreateUrl(subdomain, path));
        }
    }
}
