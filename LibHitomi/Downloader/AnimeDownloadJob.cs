using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;

namespace LibHitomi.Downloader
{
    internal class AnimeDownloadJob : IDownloadJob
    {
        // Private variables
        private Gallery gallery = null;
        private bool isCompleted = false;
        private bool isStarted = false;
        private string directoryPath;
        private Thread thread;
        private DownloadFilenameGenerator filenameGenerator;
        // Properties
        public Gallery Gallery { get { return gallery; } }
        public bool IsCompleted { get { return isCompleted; } }
        public bool IsStarted { get { return isStarted; } }
        public bool IsDownloading { get { return isStarted && !isCompleted; } }
        public int JobId { get; set; } = -1;
        // Events
        public event DownloadProgressDelegate DownloadProgress;
        // Private methods
        private async Task downloadAnime()
        {
            string url = gallery.getDownloadableVideoUrl();
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            string filePath = filenameGenerator(gallery, gallery.VideoFilename, -1);
            filePath = Path.Combine(directoryPath, filePath);

            HttpWebRequest wreq = RequestHelper.CreateRequest(url);
            using (WebResponse wres = await wreq.GetResponseAsync())
            using (Stream str = wres.GetResponseStream())
            using (FileStream fstr = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                int received = 0;
                long contentLength = wres.ContentLength, totalReceived = 0;
                const int bufferSize = 81920;
                byte[] buffer = new byte[bufferSize];
                while ((received = str.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fstr.Write(buffer, 0, received);
                    totalReceived += received;
                    DownloadProgress(this, ProgressEventTypes.SetProgressBarValue, (int)((double)totalReceived / contentLength * 100000.0));
                }
            }
            isCompleted = true;
        }
        // Public methods
        public void Initialize(Gallery gallery, string directory, DownloadFilenameGenerator filenameGenerator)
        {
            if (this.gallery == null)
                this.gallery = gallery;
            else
                throw new InvalidOperationException("이미 초기화된 객체입니다.");
            foreach (char i in Path.GetInvalidPathChars())
                directory = directory.Replace(i, '_');
            this.directoryPath = directory;
            this.filenameGenerator = filenameGenerator;
        }
        public async Task DownloadAsync()
        {
            if (isStarted)
                throw new InvalidOperationException("이미 다운로드를 시작했습니다");
            isStarted = true;
            DownloadProgress(this, ProgressEventTypes.SetProrgessBarMaximum, 100000);
            await downloadAnime();
        }
    }
}
