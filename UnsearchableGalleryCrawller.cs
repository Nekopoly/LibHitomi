using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LibHitomi
{
    public enum CrawlProgressTypes
    {
        /// <summary>
        /// 허탕쳤습니다. (foundGallery 매개변수에는 null이 전달됩니다.)
        /// </summary>
        NotFound,
        /// <summary>
        /// 찾았습니다.
        /// </summary>
        FoundUnsearchable,
        /// <summary>
        /// 찾았는데 Anime라서 생략했습니다.
        /// </summary>
        AnimeSkipped
    }
    public delegate void CrawlProgressDelegate(object sender, UnsearchableGalleryCrawlProgressEventArgs e);
    public delegate void CrawlCompletedDelegate(Gallery[] galleries);
    // 참고 : npm hitomi.la 묘듈에서는 Unsearchable, Hidden 등으로 용어에 혼란이 있음.
    //        이 라이브러리에서는 Unsearchable로 용어를 통일함.
    /// <summary>
    /// 히토미 사이트내에 이미지는 존재하나 접근할 수 없는 갤러리를 크롤링합니다.
    /// 클래스 내에서 멀티 쓰레드를 사용합니다. (안 쓰면 느려서 속터져요)
    /// </summary>
    public class UnsearchableGalleryCrawller
    {
        private Gallery[] galleries = null;
        private List<Gallery> resultGalleries = new List<Gallery>();
        private int triedGalleriesCount = 0, targetGalleriesCount = 0;
        private bool isCrawlling = false;
        private Thread[] threads;
        private GalleryBlockParser parser = new GalleryBlockParser();
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private Queue<int> numberQueue = new Queue<int>();
        /// <summary>
        /// 크롤링이 완료될 시 발생하는 이벤트입니다.
        /// </summary>
        public event CrawlCompletedDelegate CrawlCompleted;
        /// <summary>
        /// 크롤링 진행시 발생하는 이벤트입니다.
        /// </summary>
        public event CrawlProgressDelegate CrawlProgress;
        /// <summary>
        /// 사용할 쓰레드 갯수입니다.
        /// </summary>
        public int ThreadCount { get; set; } = 30;
        /// <summary>
        /// 크롤링과정에서 anime 유형의 갤러리도 크롤링할지의 여부입니다.
        /// </summary>
        public bool includeAnimes { get; set; } = false;
        /// <summary>
        /// 검색할 수 있는 갤러리 목록을 설정합니다.
        /// </summary>
        /// <param name="galleries">검색 가능한 갤러리 목록</param>
        public void SetSearchableGalleries(Gallery[] galleries)
        {
            this.galleries = galleries;
        }
        /// <summary>
        /// 시작합니다.
        /// </summary>
        /// <param name="startGalleryId">대입할 갤러리 번호의 최소값입니다</param>
        /// <param name="endGalleryId">대입할 갤러리 번호의 최대값입니다.</param>
        public void Start(int startGalleryId, int endGalleryId)
        {
            if (isCrawlling)
            {
                throw new InvalidOperationException("Already crawlling!");
            }
            isCrawlling = true;
            numberQueue.Clear();
            resultGalleries.Clear();
            triedGalleriesCount = 0;
            List<int> searchableIds = new List<int>(galleries.Select(gallery => gallery.id));
            for (int i = startGalleryId; i <= endGalleryId; i++)
            {
                numberQueue.Enqueue(i);
            }
            numberQueue = new Queue<int>(numberQueue.Except(searchableIds));
            targetGalleriesCount = numberQueue.Count;
            initThreads();
        }
        private void initThreads()
        {
            stopwatch.Restart();
            threads = new Thread[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                bool isLoyal = i == ThreadCount - 1;
                threads[i] = new Thread(threadFunc);
                threads[i].Name = $"Unsearchable Gallery Crawlling Thread" + (isLoyal ? " - Loyal" : "");
                threads[i].Start(isLoyal);
            }
        }
        private void raiseProgressEvent(CrawlProgressTypes type, Gallery gallery=null)
        {
            int totalSuccesses;
            lock (resultGalleries)
            {
                totalSuccesses = resultGalleries.Count;
            }
            CrawlProgress(this, new UnsearchableGalleryCrawlProgressEventArgs()
            {
                TotalIdCount = targetGalleriesCount,
                CrawlledIdCount = triedGalleriesCount,
                ElapsedTime = stopwatch.ElapsedMilliseconds,
                EventType = type,
                FoundGallery = gallery,
                FoundUnserachableGalleryCount = totalSuccesses
            });
        }
        private void threadFunc(object _isLoyalThread)
        {
            bool isLoyalThread = (bool)_isLoyalThread;
            while (true)
            {
                int number;
                lock (numberQueue)
                {
                    if (numberQueue.Count > 0)
                        number = numberQueue.Dequeue();
                    else
                        break;
                }
                Gallery gallery;
                bool isParsed = parser.TryParse(number, out gallery);
                if (isParsed)
                {
                    if (includeAnimes || (!includeAnimes && gallery.type != "anime"))
                    {
                        lock(resultGalleries)
                        {
                            resultGalleries.Add(gallery);
                        }
                        raiseProgressEvent(CrawlProgressTypes.FoundUnsearchable, gallery);
                    }
                    else {
                        raiseProgressEvent(CrawlProgressTypes.AnimeSkipped, gallery);
                    }
                        
                } else
                {
                    raiseProgressEvent(CrawlProgressTypes.NotFound, null);
                }
                Interlocked.Increment(ref triedGalleriesCount);
            }
            if (isLoyalThread)
            {
                while (triedGalleriesCount != targetGalleriesCount)
                {

                }
                CrawlCompleted(resultGalleries.ToArray());
                triedGalleriesCount = 0;
                targetGalleriesCount = 0;
                resultGalleries.Clear();
                numberQueue.Clear();
                isCrawlling = false;
            }
        }
    }
}
