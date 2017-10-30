using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader.EventArgs
{
    public class GalleryAddedEventArgs
    {
        public Gallery Gallery { get; set; }
        public int JobId { get; set; }
        internal GalleryAddedEventArgs(Gallery gallery, int jobId)
        {
            Gallery = gallery;
            JobId = jobId;
        }
    }
}
