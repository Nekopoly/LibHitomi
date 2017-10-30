using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader
{
    public class TriedDuplicatedGalleryException : Exception
    {
        private Gallery gallery;
        public Gallery Gallery { get { return gallery; } }
        internal TriedDuplicatedGalleryException(Gallery gallery) : base("이미 다운로드됐거나, 다운로드 중인 작품을 시도했습니다.")
        {
            this.gallery = gallery;
        }
    }
}
