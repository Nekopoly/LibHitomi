using System;
using System.Collections.Generic;
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
        private Queue<string> urls = new Queue<string>();
        // Properties
        public Gallery Gallery { get { return gallery; } }
        public bool IsCompleted { get { return isCompleted; } }
        public bool IsStarted { get { return isStarted; } }
        public bool IsDownloading { get { return isStarted && !isCompleted; } }
        // Events
        public event DownloadCompletedDelegate DownloadCompleted;
        public event DownloadProgressDelegate DownloadProgress;
        // Private methods
        private void downloadImageUrls()
        {
            foreach (string i in gallery.getImageUrls())
                urls.Enqueue(i);
            notDownloadedImages = urls.Count;
            DownloadProgress(this, ProgressEventTypes.SetProrgessBarMaximum, urls.Count);
            DownloadProgress(this, ProgressEventTypes.SetProgressBarValue, 0);
            for(int i = 0; i < imageLimit; i++)
            {
                threads[i].Start(i == 0);
            }
        }
        private void downloadImage(object _waitForOthers)
        {
            bool waitForOthers = (bool)_waitForOthers;
            while(true)
            {
                if (urls.Count == 0)
                    break;
                string url = urls.Dequeue();
                if (url == null)
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
                notDownloadedImages--;
            }
            if (waitForOthers)
            {
                while (notDownloadedImages != 0 || urls.Count != 0)
                {
                    
                }
                isCompleted = true;
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
