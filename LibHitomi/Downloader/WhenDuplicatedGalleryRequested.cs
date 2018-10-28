using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader
{
    /// <summary>
    /// 중복된 갤러리(이미 완료했거나, 진행중인 갤러리)의 다운로드를 시도할때의 행동을 나타냅니다.
    /// </summary>
    public enum WhenTriedDuplicatedGallery
    {
        /// <summary>
        /// 아무것도 하지 않습니다.
        /// </summary>
        DoNothing,
        /// <summary>
        /// TriedDuplicatedGalleryException 예외를 발생합니다.
        /// </summary>
        ThrowException,
        /// <summary>
        /// 무시하고 다운로드하지 않습니다.
        /// </summary>
        IgnoreAndDoNotDownload
    }
}
