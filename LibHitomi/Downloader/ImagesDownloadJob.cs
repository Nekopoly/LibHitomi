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
        private string directoryPath;
        private DownloadFilenameGenerator filenameGenerator;
        // Properties
        public Gallery Gallery { get; private set; } = null;
        public bool IsCompleted { get; private set; } = false;
        public bool IsStarted { get; private set; } = false;
        public bool IsDownloading { get { return IsStarted && !IsCompleted; } }
        public int JobId { get; set; } = -1;
        // Events
        public event DownloadProgressDelegate DownloadProgress;
        // Private methods
        private async Task downloadImage(string url, int order)
        {
            string origFilename = url.Split('/').Last(), filePath;
            filePath = Path.Combine(directoryPath, filenameGenerator(Gallery, origFilename, order));

            HttpWebRequest wreq = RequestHelper.CreateRequest(url);
            using (WebResponse wres = await wreq.GetResponseAsync())
            using (Stream str = wres.GetResponseStream())
            using (FileStream fstr = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                str.CopyTo(fstr);

            DownloadProgress(this, ProgressEventTypes.IncreaseProgressBar, null);
        }
        // Public methods
        public void Initialize(Gallery gallery, string directory, DownloadFilenameGenerator filenameGenerator)
        {
            if (this.Gallery == null)
                this.Gallery = gallery;
            else
                throw new InvalidOperationException("이미 초기화된 객체입니다.");
            this.directoryPath = directory;
            this.filenameGenerator = filenameGenerator;
            foreach (char i in Path.GetInvalidPathChars())
                directoryPath = directoryPath.Replace(i, '_');
        }
        public async Task DownloadAsync()
        {
            if (IsStarted)
                throw new InvalidOperationException("이미 다운로드를 시작했습니다");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            IsStarted = true;
            string[] urls = Gallery.getImageUrls();
            Task[] tasks = new Task[urls.Length];
            for (int i = 0; i < urls.Length; i++)
                tasks[i] = downloadImage(urls[i], i);
            DownloadProgress(this, ProgressEventTypes.SetProrgessBarMaximum, urls.Length);
            DownloadProgress(this, ProgressEventTypes.SetProgressBarValue, 0);
            await Task.WhenAll(tasks);
        }
    }
}
