using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHitomi.Downloader.EventArgs;

namespace LibHitomi.Downloader
{
    public delegate void GalleryDownloadStartedDelegate(object sender, GalleryDownloadStartedEventArgs e);
    public delegate void GalleryDownloadProgressDelegate(object sender, DownloadGalleryProgressEventArgs e);
    public delegate void GalleryDownloadCompletedDelegate(object sender, DownloadGalleryCompeletedEventArgs e);
    public delegate void EveryGalleriesDownloadCompletedDelegate(object sender);
    public delegate void GalleryAddedDelegate(object sender, Gallery gallery);
}
