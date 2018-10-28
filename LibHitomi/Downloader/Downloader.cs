using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibHitomi.Downloader.EventArgs;

namespace LibHitomi.Downloader
{
    [Obsolete("ContinuousDownloader 클래스를 사용하세요.")]
    public class Downloader
    {
        public event GalleryDownloadCompletedDelegate DownloadGalleryCompleted;
        public event GalleryDownloadProgressDelegate DownloadGalleryProgress;
        public event EveryGalleriesDownloadCompletedDelegate DownloadEveryGalleriesCompleted;
        private List<Gallery> galleries = new List<Gallery>();
        private List<IDownloadJob> jobs = new List<IDownloadJob>();
        private int galleryLimit;
        private bool isStarted = false;
        public Downloader(IEnumerable<Gallery> galleries, string rootDirectory, int imageLimit, int galleryLimit) : base()
        {
            this.galleryLimit = galleryLimit;
            this.galleries.AddRange(galleries);
            foreach (Gallery gallery in galleries)
            {
                IDownloadJob job;
                string dir = $"{gallery.Id} - {gallery.name}";
                foreach (char i in Path.GetInvalidFileNameChars())
                    dir = dir.Replace(i, '_');
                dir = Path.Combine(rootDirectory, dir);
                if (gallery.type == "anime")
                {
                    job = new AnimeDownloadJob();
                }else
                {
                    job = new ImagesDownloadJob();
                }
                job.DownloadCompleted += onGalleryDownloadCompleted;
                job.DownloadProgress += onGalleryDownloadProgress;
                job.Initialize(gallery, imageLimit, dir);
                jobs.Add(job);
            }

            this.galleryLimit = galleryLimit;
        }

        private void onGalleryDownloadProgress(object sender, ProgressEventTypes evtType, object param)
        {
            DownloadGalleryProgress(this, new DownloadGalleryProgressEventArgs((sender as IDownloadJob).Gallery, evtType, param, (sender as IDownloadJob).JobId));
        }

        private void onGalleryDownloadCompleted(object sender)
        {
            DownloadGalleryCompleted(this, new DownloadGalleryCompeletedEventArgs((sender as IDownloadJob).Gallery, (sender as IDownloadJob).JobId));
            bool isEverytingCompleted = true;
            foreach (IDownloadJob job in jobs)
            {
                if (!job.IsStarted)
                {
                    job.StartDownload();
                    break;
                }
            }
            foreach (IDownloadJob job in jobs)
            {
                if (!job.IsCompleted)
                    isEverytingCompleted = false;
            }
            if(isEverytingCompleted)
                DownloadEveryGalleriesCompleted(this);
        }

        public Gallery[] Galleries { get { return galleries.ToArray(); } }
        public void StartDownload()
        {
            if (isStarted)
                throw new InvalidOperationException("이미 다운로드가 시작됐습니다");
            isStarted = true;
            for (int i = 0; i < galleryLimit; i++)
                if(i < jobs.Count)
                    jobs[i].StartDownload();
        }
    }
}
