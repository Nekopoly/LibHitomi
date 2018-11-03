using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibHitomi.Downloader.EventArgs;
using System.Threading;

namespace LibHitomi.Downloader
{
    public class Downloader
    {
        public event GalleryDownloadProgressDelegate DownloadGalleryProgress = (a, b) => { };
        private DownloadDirectoryGenerator directoryGenerator;
        private DownloadFilenameGenerator filenameGenerator;
        int jobId = 0;
        public Downloader(DownloadDirectoryGenerator directoryGenerator, DownloadFilenameGenerator filenameGenerator) : base()
        {
            this.directoryGenerator = directoryGenerator;
            this.filenameGenerator = filenameGenerator;
        }

        private void onGalleryDownloadProgress(object sender, ProgressEventTypes evtType, object param)
        {
            DownloadGalleryProgress(this, new DownloadGalleryProgressEventArgs((sender as IDownloadJob).Gallery, evtType, param, (sender as IDownloadJob).JobId));
        }
        
        public async Task DownloadAsync(Gallery gallery)
        {
            IDownloadJob job;
            if (gallery.Type == "anime")
                job = new AnimeDownloadJob();
            else
                job = new ImagesDownloadJob();
            job.DownloadProgress += onGalleryDownloadProgress;
            job.Initialize(gallery, directoryGenerator(gallery), filenameGenerator);
            job.JobId = Interlocked.Increment(ref jobId);
            await job.DownloadAsync();
        }
    }
}
