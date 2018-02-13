using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader
{
    public class TriedDuplicatedGalleryException : Exception
    {
        internal TriedDuplicatedGalleryException() : base("이미 다운로드됐거나, 다운로드 중인 작품을 시도했습니다.")
        {
        }
    }
}
