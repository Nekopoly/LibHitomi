using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader.EventArgs
{
    public class GalleryDownloadStartedEventArgs
    {
        public Gallery Gallery { get; set; }
        public int JobId { get; set; }

        internal GalleryDownloadStartedEventArgs(Gallery gallery, int jobId)
        {
            Gallery = gallery;
            JobId = jobId;
        }
    }
}
