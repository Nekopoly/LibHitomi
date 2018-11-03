using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHitomi.Downloader
{
    internal delegate void DownloadProgressDelegate(object sender, ProgressEventTypes evtType, object param);
    public enum ProgressEventTypes
    {
        /// <summary>
        /// ProgressBar의 컨트롤의 스타일을 Marquee로 설정해야 할지의 여부가 bool형의 매개변수로 전달됩니다.
        /// </summary>
        ToggleProgressMarquee,
        /// <summary>
        /// ProgressBar의 Value 속성값을 올려야 할때 발생합니다. 매개변수가 없을 시 1로 간주됩니다.
        /// </summary>
        IncreaseProgressBar,
        /// <summary>
        /// ProgressBar의 Maximum 속성값을 전달된 int형의 매개변수로 설정해야 할 때 발생합니다.
        /// </summary>
        SetProrgessBarMaximum,
        /// <summary>
        /// ProgressBar의 Value 속성값을 전달된 int형의 매개변수로 설정해야 할 때 발생합니다.
        /// </summary>
        SetProgressBarValue
    }
    internal interface IDownloadJob
    {
        void Initialize(Gallery gallery, string directory, DownloadFilenameGenerator filenameGenerator);
        Task DownloadAsync();
        int JobId { get; set; }
        bool IsStarted { get; }
        bool IsDownloading { get; }
        bool IsCompleted { get; }
        Gallery Gallery { get; }
        event DownloadProgressDelegate DownloadProgress;
    }
}
