using System.ComponentModel;

namespace Flickr_Set_Downloader
{
    public enum DownloadStatus
    {
        [Description("Queued")]
        DownloadNotStarted = 0,
        [Description("Downloading...")]
        Downloading = 1,
        [Description("Completed")]
        DownloadCompleted = 2,
        [Description("Failed")]
        DownloadFailed = 3,
        [Description("Cancelled")]
        DownloadCancelled = 4
    }

    public enum SetRequestType
    {
        [Description("Invalid Request")]
        None = 0,
        [Description("List all available Flickr sets for a user")]
        Listr = 1,
        [Description("List all available images in a Flickr set")]
        Viewr = 2
    }

    public enum PhotoSizeEnum
    {
        [Description("Square (Square 75)")]
        sq = 0,
        [Description("Large Square (Square 150)")]
        q = 1,
        [Description("Thumbnail")]
        t = 2,
        [Description("Small (Small 240)")]
        s = 3,
        [Description("Small 320")]
        n = 4,
        [Description("Medium (Medium 500)")]
        m = 5,
        [Description("Medium 640")]
        z = 6,
        [Description("Medium 800")]
        c = 7,
        [Description("Large (Large 1024)")]
        l = 8,
        [Description("Original")]
        o = 9,
        [Description("Large 1600")]
        h = 10,
        [Description("Large 2048")]
        k = 11
    }
}
