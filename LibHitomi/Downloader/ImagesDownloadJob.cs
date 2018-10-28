using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;

namespace LibHitomi.Downloader
{
    internal class ImagesDownloadJob : IDownloadJob
    {
        // Private variables
        private Gallery gallery = null;
        private bool isCompleted = false;
        private bool isStarted = false;
        private int imageLimit;
        private int notDownloadedImages;
        private string directoryPath;
        private Thread[] threads;
        private ConcurrentBag<string> urls = new ConcurrentBag<string>();
        // Properties
        public Gallery Gallery { get { return gallery; } }
        public bool IsCompleted { get { return isCompleted; } }
        public bool IsStarted { get { return isStarted; } }
        public bool IsDownloading { get { return isStarted && !isCompleted; } }
        public int JobId { get; set; } = -1;
        // Events
        public event DownloadCompletedDelegate DownloadCompleted;
        public event DownloadProgressDelegate DownloadProgress;
        // Private methods
        private void downloadImageUrls()
        {
            System.Threading.Thread.CurrentThread.Name = "Url Download Thread - " + gallery.Id;
            foreach (string i in gallery.getImageUrls())
                urls.Add(i);
            notDownloadedImages = urls.Count;
            DownloadProgress(this, ProgressEventTypes.SetProrgessBarMaximum, urls.Count);
            DownloadProgress(this, ProgressEventTypes.SetProgressBarValue, 0);
            for (int i = 0; i < imageLimit; i++)
            {
                threads[i].Start(i == 0);
            }
        }
        private void downloadImage(object _waitForOthers)
        {
            bool waitForOthers = (bool)_waitForOthers;
            System.Threading.Thread.CurrentThread.Name = "Image Download Thread " + (waitForOthers ? "(Sacred)" : "") + "- " + gallery.id;
            while (true)
            {
                if (!urls.TryTake(out string url))
                    break;
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                string filePath = url.Split('/').Last();
                foreach (char i in Path.GetInvalidFileNameChars())
                    filePath = filePath.Replace(i, '_');
                filePath = Path.Combine(directoryPath, filePath);

                HttpWebRequest wreq = RequestHelper.CreateRequest(url);
                using (WebResponse wres = wreq.GetResponse())
                using (Stream str = wres.GetResponseStream())
                using (FileStream fstr = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    str.CopyTo(fstr);

                DownloadProgress(this, ProgressEventTypes.IncreaseProgressBar, null);
                Interlocked.Decrement(ref notDownloadedImages);
            }
            if (waitForOthers)
            {
                while (true)
                {
                    if (notDownloadedImages == 0 && urls.Count.Equals(0))
                        break;
                }
                isCompleted = true;
                for(int i = 0; i < threads.Length; i++)
                {
                    if(threads[i].ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    {
                        threads[i].Abort();
                        threads[i] = null;
                    }
                }
                DownloadCompleted(this);
            }
        }
        // Public methods
        public void Initialize(Gallery gallery, int imageLimit, string directory)
        {
            if (this.gallery == null)
                this.gallery = gallery;
            else
                throw new InvalidOperationException("이미 초기화된 객체입니다.");
            this.imageLimit = imageLimit;
            this.directoryPath = directory;
            this.threads = new Thread[imageLimit];
            foreach (char i in Path.GetInvalidPathChars())
                directoryPath = directoryPath.Replace(i, '_');
        }
        public void StartDownload()
        {
            if (isStarted)
                throw new InvalidOperationException("이미 다운로드를 시작했습니다");
            isStarted = true;
            for (int i = 0; i < imageLimit; i++)
                threads[i] = new Thread(downloadImage);
            Thread thr = new Thread(downloadImageUrls);
            thr.Start();
        }
    }
}
