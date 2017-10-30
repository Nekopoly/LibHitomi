using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader.EventArgs
{
    public class DownloadGalleryCompeletedEventArgs
    {
        public Gallery Gallery { get; set; }
        public int JobId { get; set; }
        internal DownloadGalleryCompeletedEventArgs(Gallery gallery, int jobId)
        {
            Gallery = gallery;
            JobId = jobId;
        }
    }
}
