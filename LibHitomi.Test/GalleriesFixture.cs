using System;
using System.Collections.Generic;
using System.Text;
using LibHitomi.GalleryList;

namespace LibHitomi.Test
{
    public class GalleriesFixture
    {
        public List<Gallery> galleries { get; private set; }
        public GalleriesFixture()
        {
            ListDownloader listDownloader = new ListDownloader();
            var listDownloadTask = listDownloader.Download();
            listDownloadTask.Wait();
            galleries = new List<Gallery>(listDownloadTask.Result);
        }
    }
}
