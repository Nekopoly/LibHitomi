using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi
{
    public class UnsearchableGalleryCrawlProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 크롤링할 ID의 총 갯수입니다. 크롤링된 ID와 안 된 ID 둘 다 포함합니다.
        /// </summary>
        public int TotalIdCount { get; internal set; }
        /// <summary>
        /// 크롤링된 ID의 총 갯수입니다.
        /// </summary>
        public int CrawlledIdCount { get; internal set; }
        /// <summary>
        /// 지금까지 발견한 검색불가 갤러리의 총 갯수입니다.
        /// </summary>
        public int FoundUnserachableGalleryCount { get; internal set; }
        /// <summary>
        /// 이번에 발견한 검색불가 갤러리입니다.
        /// </summary>
        public Gallery FoundGallery { get; internal set; }
        /// <summary>
        /// 크롤링 시작부터 지금까지의 시간입니다. 단위는 ms입니다.
        /// </summary>
        public long ElapsedTime { get; internal set; }
        /// <summary>
        /// 이벤트 유형입니다.
        /// </summary>
        public CrawlProgressTypes EventType { get; internal set; }

        internal UnsearchableGalleryCrawlProgressEventArgs() { }
    }
}
