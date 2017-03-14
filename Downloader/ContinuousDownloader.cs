using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LibHitomi.Downloader
{
    /// <summary>
    /// 지속적으로 진행되는 다운로더입니다. 언제나 다운로드할 작품을 추가할 수 있습니다.
    /// </summary>
    public class ContinuousDownloader
    {
        public event GalleryAddedDelegate GalleryAdded;
        public event GalleryDownloadProgressDelegate DownloadGalleryProgress;
        public event GalleryDownloadCompletedDelegate DownloadGalleryCompleted;
        public event GalleryDownloadStartedDelegate DownloadGalleryStarted;
        private Queue<IDownloadJob> jobs = new Queue<IDownloadJob>();
        private Thread jobStarterThread;
        private int galleryLimit, imageLimit;
        private string saveDirectory;
        private bool isStarted = false;
        private int processingJobs = 0;

        private void startJob(IDownloadJob job)
        {
            job.DownloadCompleted += startAnotherJobWhenFinished;
            job.DownloadCompleted += JobDownloadCompleted;
            job.DownloadProgress += JobDownloadProgress;
            DownloadGalleryStarted(this, new EventArgs.GalleryDownloadStartedEventArgs(job.Gallery, job.JobId));
            job.StartDownload();
        }

        private void startAnotherJobWhenFinished(object sender)
        {
            processingJobs--;
        }

        private void JobDownloadProgress(object sender, ProgressEventTypes evtType, object param)
        {
            IDownloadJob job = sender as IDownloadJob;
            DownloadGalleryProgress(this, new EventArgs.DownloadGalleryProgressEventArgs(job.Gallery, evtType, param, job.JobId));
        }

        private void JobDownloadCompleted(object sender)
        {
            IDownloadJob job = sender as IDownloadJob;
            DownloadGalleryCompleted(this, new EventArgs.DownloadGalleryCompeletedEventArgs(job.Gallery, job.JobId));
        }

        private void startJobs()
        {
            int maxJobId = 1;
            while (true)
            {
                if (jobs.Count == 0)
                    continue;
                IDownloadJob job = jobs.Dequeue();
                if (job == null)
                    continue;
                System.Diagnostics.Debug.WriteLine("Setting jobid to " + maxJobId);
                job.JobId = maxJobId++;
                bool jobExecuted = false;
                while(!jobExecuted)
                {
                    if(processingJobs < galleryLimit)
                    {
                        startJob(job);
                        processingJobs++;
                        jobExecuted = true;
                    }
                }
            }
        }

        /// <summary>
        /// ContinuousDownloader 클래스 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="directory">다운로드된 갤러리들이 저장될 디렉토리입니다.</param>
        /// <param name="galleryLimit">동시에 다운로드할 갤러리 갯수입니다.</param>
        /// <param name="imageLimit">한개의 갤러리당 동시에 다운로드할 이미지 갯수입니다.</param>
        public ContinuousDownloader(string directory, int galleryLimit, int imageLimit)
        {
            this.galleryLimit = galleryLimit;
            this.imageLimit = imageLimit;
            saveDirectory = directory;

            jobStarterThread = new Thread(startJobs);
        }

        /// <summary>
        /// 다운로드할 갤러리를 추가합니다.
        /// </summary>
        /// <param name="gallery">추가할 갤러리입니다.</param>
        public void AddGallery(Gallery gallery)
        {
            AddGalleries(new Gallery[] { gallery });
        }

        /// <summary>
        /// 다운로드할 갤러리들을 추가합니다.
        /// </summary>
        /// <param name="galleries">추가할 갤러리들입니다.</param>
        public void AddGalleries(IEnumerable<Gallery> galleries)
        {
            GalleryAdded(this, galleries.ToArray());
            foreach (Gallery gallery in galleries)
            {
                IDownloadJob job;
                if (gallery.type == "anime")
                {
                    job = new AnimeDownloadJob();
                }
                else
                {
                    job = new ImagesDownloadJob();
                }
                string subdir = $"{gallery.Id} - {gallery.Name}";
                foreach (char i in Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()))
                {
                    subdir = subdir.Replace(i, '_');
                }
                job.Initialize(gallery, imageLimit, Path.Combine(saveDirectory, subdir));
                jobs.Enqueue(job);
            }
        }

        /// <summary>
        /// 이 다운로더가 시작됐는지의 여부입니다.
        /// </summary>
        public bool IsStarted { get { return isStarted; } }

        /// <summary>
        /// 다운로드를 시작합니다.
        /// </summary>
        public void Start()
        {
            if (isStarted)
                return;
            isStarted = true;
            if (!jobStarterThread.IsAlive)
                jobStarterThread.Start();
        }
    }
}
