using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace Flickr_Set_Downloader
{
    public class FlickrSet
    {
        public string SetDisplayPic { get; set; }
        public string SetLink { get; set; }
        public string SetTitle { get; set; }
        public string SetCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class FlickrSetItem
    {
        public int id { get; set; }
        public string FilePath { get; set; }
        public string SetId { get; set; }
        public string SetName { get; set; }
        public string Title { get; set; }
        public DownloadStatus _status;
        public string Status { get { return _status.GetDescription(); } }
        public bool IsStartStopButtonVisible
        {
            get
            {
                if (_status == DownloadStatus.DownloadCancelled || _status == DownloadStatus.DownloadFailed || _status == DownloadStatus.Downloading) return true;
                return false;
            }
        }

        public string StartStopButtonText
        {
            get
            {
                if (_status == DownloadStatus.DownloadCancelled || _status == DownloadStatus.DownloadFailed) return "4";
                else if (_status == DownloadStatus.Downloading) return "g";
                return string.Empty;
            }
        }

        public int StartStopButtonTextSize
        {
            get
            {
                if (_status == DownloadStatus.DownloadCancelled || _status == DownloadStatus.DownloadFailed) return 30;
                else if (_status == DownloadStatus.Downloading) return 20;
                return 0;
            }
        }

        public Brush ProgressbarColor
        {
            get
            {
                if (_status == DownloadStatus.DownloadCancelled) return Brushes.Red;
                else if (_status == DownloadStatus.DownloadCompleted) return Brushes.Green;
                else if (_status == DownloadStatus.Downloading) return Brushes.Green;
                else if (_status == DownloadStatus.DownloadFailed) return Brushes.Red;
                else if (_status == DownloadStatus.DownloadNotStarted) return Brushes.Green;
                return Brushes.Red;
            }
        }

    }

    public static class EnumExtension
    {
        /// <summary>
        /// Gets the first [Description("")] Attribute of the enum.
        /// value.ToString() if there is none defined
        /// </summary>
        /// <param name="value">the enum value</param>
        /// <returns>the description</returns>
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }

            return value.ToString();
        }
    }
}
