using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Xml;
using Debug = System.Diagnostics.Debug;

namespace LibHitomi
{
    public delegate void ListDownloadCompletedDelegate(List<Gallery> result);
    public delegate void ListDownloadProgress(ListDownloadProgressType progressType, int? data);
    public enum ListDownloadProgressType
    {
        /// <summary>
        /// 조각 갯수를 받았을 때 발생합니다. data 매개변수로 조각 갯수가 전달됩니다.
        /// </summary>
        GotTotalChunkCount,
        /// <summary>
        /// 조각을 다운로드하기 시작했을 때 발생합니다. data 매개변수로 다운로드를 시작한 조각 번호가 전달됩니다. (조각 번호는 0부터 시작합니다.)
        /// </summary>
        DownloadingChunkStarted,
        /// <summary>
        /// 조각을 다운로드했을 때 발생합니다. data 매개변수로 다운로드한 조각 번호가 전달됩니다. (조각 번호는 0부터 시작합니다.)
        /// </summary>
        DownloadedChunk,
        /// <summary>
        /// 조각들을 다 받고 마무리 작업이 시작할 때 발생합니다. data 매개변수는 전달되지 않습니다.
        /// </summary>
        FinishingStarted,
        /// <summary>
        /// 수동추가할 갤러리들을 불러들이기 시작할 때 발생합니다. data 매개변수는 전달되지 않습니다.
        /// </summary>
        LoadingExtraGalleries,
        /// <summary>
        /// 갤러리들이 수동추가될 때 발생합니다. data 매개변수는 전달되지 않습니다.
        /// </summary>
        LoadedExtraGalleries,
        /// <summary>
        /// 수동추가할 갤러리가 없을 때 발생합니다. data 매개변수는 전달되지 않습니다.
        /// </summary>
        HasNoExtraGalleries,
    }
    /// <summary>
    /// 갤러리 목록 전체를 다운로드합니다.
    /// </summary>
    public class ListDownloader : ListDownloaderBase
    {
        private Dictionary<int, Gallery[]> chunks = new Dictionary<int, Gallery[]>();
        private bool isDownloading = false;
        private int chunkCnt = 0;
        public ListDownloader()
        {
            ListDownloadCompleted += (a) => { };
            ListDownloadProgress += (a, b) => { };
        }
        private Gallery[] getChunk(int i, bool raiseEvent = false)
        {
            if (raiseEvent) ListDownloadProgress(ListDownloadProgressType.DownloadingChunkStarted, i);
            HttpWebRequest wreq = RequestHelper.CreateRequest(DownloadOptions.JsonSubdomain, $"/galleries{i}.json");
            using (WebResponse wres = wreq.GetResponse())
            using (Stream str = wres.GetResponseStream())
            using (StreamReader sre = new StreamReader(str))
            using (JsonReader reader = new JsonTextReader(sre))
            {
                JsonSerializer serializer = new JsonSerializer();
                Gallery[] result = serializer.Deserialize<Gallery[]>(reader);
                if (raiseEvent) ListDownloadProgress(ListDownloadProgressType.DownloadedChunk, i);
                return result;
            }
        }
        private Gallery[] loadExtraGalleries()
        {
            using (FileStream fstr = new FileStream(ExtraGalleriesPath, FileMode.Open, FileAccess.Read))
            using (StreamReader sre = new StreamReader(fstr))
            using (JsonReader reader = new JsonTextReader(sre))
            {
                JsonSerializer serializer = new JsonSerializer();
                Gallery[] result = serializer.Deserialize<Gallery[]>(reader);
                for(int i = 0; i < result.Length; i++)
                {
                    result[i].GalleryCrawlMethod = GalleryCrawlMethod.AddedManually;
                }
                return result;
            }
        }
        private List<Gallery> finishChunksJob()
        {
            Debug.WriteLine("Finishing Thread #" + Thread.CurrentThread.ManagedThreadId + " Started");
            ListDownloadProgress(ListDownloadProgressType.FinishingStarted, null);
            List<Gallery> list = new List<Gallery>();
            for (var i = 0; i < chunkCnt; i++)
            {
                list.AddRange(chunks[i]);
            }
            Debug.WriteLine("Every chunks were added into list");
            if(LoadExtraGalleries)
            {
                Debug.WriteLine("Loading extra galleries");
                ListDownloadProgress(ListDownloadProgressType.LoadingExtraGalleries, null);
                List<int> searchableIds = new List<int>(list.Select(gallery => gallery.id));
                list.AddRange(loadExtraGalleries().Where((Gallery extraGallery) => !searchableIds.Contains(extraGallery.id)));
                ListDownloadProgress(ListDownloadProgressType.LoadedExtraGalleries, null);
            } else
            {
                ListDownloadProgress(ListDownloadProgressType.HasNoExtraGalleries, null);
            }
            Debug.WriteLine("Made a list");
            for(int i = 0; i < list.Count; i++)
            {
                list[i].UnNull();
            }
            Debug.WriteLine("Unnulled, Completed and Finished!");
            return list;
        }
        private async Task downloadChunkJob(object _index)
        {
            await Task.Run(() =>
            {
                int index = (int)_index;
                Debug.WriteLine("Thread #" + Thread.CurrentThread.ManagedThreadId + ", Working with " + index + "st chunk(zero-based)");
                Gallery[] chunk = getChunk(index, true);
                chunks.Add(index, chunk);
                Debug.WriteLine("Thread #" + Thread.CurrentThread.ManagedThreadId + "," + index + "st chunk(zero-based) has " + chunk.Length + " galleries and it's added");
                return;
            });
        }
        /// <summary>
        /// 목록 다운로드가 완료됐을 때 발생합니다.
        /// </summary>
        public event ListDownloadCompletedDelegate ListDownloadCompleted;
        /// <summary>
        /// 목록 다운로드가 진행중일때 발생합니다.
        /// </summary>
        public event ListDownloadProgress ListDownloadProgress;
        /// <summary>
        /// 추가적으로 갤러리들을 파일에서 불러올지의 여부입니다.
        /// </summary>
        public bool LoadExtraGalleries { get; set; } = false;
        /// <summary>
        /// 추가적으로 추가할 갤러리 파일의 경로입니다.
        /// </summary>
        public string ExtraGalleriesPath { get; set; } = "";
        /// <summary>
        /// 갤러리 목록 다운로드를 시작합니다. 여러개의 쓰레드를 사용하며 완료시 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="throwErrorIfAlreadyDownloading">이미 다운로드하고 있을 시 오류를 반환할 지의 여부입니다.</param>
        public async void StartDownload(bool throwErrorIfAlreadyDownloading = true)
        {
            if(isDownloading && throwErrorIfAlreadyDownloading)
            {
                throw new Exception("Already downloading a gallery list!");
            } else if(isDownloading)
            {
                return;
            } else
            {
                isDownloading = true;
            }
            Debug.WriteLine("Starting to download gallery list");
            chunkCnt = getJsonCount();
            ListDownloadProgress(ListDownloadProgressType.GotTotalChunkCount, chunkCnt);
            Debug.WriteLine("Gallery Json Chunk Count : " + chunkCnt);
            chunks.Clear();
            List<Task> tasks = new List<Task>();
            for(var i = 0; i < chunkCnt; i++)
            {
                tasks.Add(downloadChunkJob(i));
            }
            await Task.WhenAll(tasks.ToArray());
            List<Gallery> result = null;
            await Task.Factory.StartNew(finishChunksJob).ContinueWith((Task<List<Gallery>> Task) => { result = Task.Result; });
            ListDownloadCompleted(result);
        }
    }
}
