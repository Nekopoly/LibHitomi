using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader.EventArgs
{
    public class DownloadGalleryProgressEventArgs
    {
        public Gallery Gallery { get; set; }
        public ProgressEventTypes ProgressType { get; set; }
        public object Parameter { get; set; }
        public int JobId { get; set; }
        internal DownloadGalleryProgressEventArgs(Gallery gallery, ProgressEventTypes evtType, object param, int jobId)
        {
            Gallery = gallery;
            ProgressType = evtType;
            Parameter = param;
            JobId = jobId;
        }
    }
}
