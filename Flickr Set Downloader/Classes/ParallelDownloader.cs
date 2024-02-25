using System;
using System.ComponentModel;
using System.Net;

namespace Flickr_Set_Downloader
{
    public class ParallelWebClient : WebClient
    {
        public FlickrSetItem item;
        public int Index = 0;

        public ParallelWebClient(int index) { item = new FlickrSetItem(); Index = index; }

        public ParallelWebClient(FlickrSetItem v, int index) { item = v; Index = index; }

        public void BeginAsyncDownload()
        {
            var url = new Uri(item.FilePath);
            item.Status = DownloadStatus.Downloading;
            DownloadFileAsync(url, item.Title + ".jpg");  //TODO : need path here
        }
    }

    public class ParallelDownloader
    {
        private static ParallelWebClient[] _downloader;

        public ParallelDownloader() { _downloader = new[] { new ParallelWebClient(0) }; }

        public ParallelDownloader(int parallelThreadCount)
        {
            _downloader = new ParallelWebClient[parallelThreadCount];
            for (int i = 0; i < parallelThreadCount; i++)
                _downloader[i] = new ParallelWebClient(i);
        }

        public static int GetFreeWebClient()
        {
            for (var i = 0; i < _downloader.Length; i++)
            {
                var v = _downloader[i];
                if (v != null && !v.IsBusy) return i;
            }
            return -1;
        }

        public void InitialiseWebClientandBeginDownload(int index, FlickrSetItem flickrSetItem,
                            DownloadProgressChangedEventHandler WC_DownloadProgressChanged, AsyncCompletedEventHandler WC_DownloadFileCompleted)
        {
            _downloader[index] = null;
            _downloader[index] = new ParallelWebClient(flickrSetItem, index);
            _downloader[index].DownloadProgressChanged += WC_DownloadProgressChanged;
            _downloader[index].DownloadFileCompleted += WC_DownloadFileCompleted;
            _downloader[index].BeginAsyncDownload();
        }
    }
}