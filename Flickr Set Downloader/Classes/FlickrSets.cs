using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Flickr_Set_Downloader
{
    public class FlickrSet
    {
        public string SetDisplayPic { get; set; }
        public string SetLink { get; set; }
        public string SetTitle { get; set; }
        public string SetCount { get; set; }
        public string SetId { get; set; }
        public bool IsSelected { get; set; }
    }

    public class FlickrSetItem : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public string SetId { get; set; }
        public string SetName { get; set; }
        public string Title { get; set; }
        public string PhotoSizeDescription { get; set; }

        private DownloadStatus _status;
        public DownloadStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("IsTextBoxVisible");
                NotifyPropertyChanged("IsProgressBarVisible");
                NotifyPropertyChanged("StatusDescription");
            }
        }

        public string StatusDescription
        {
            get { return _status.GetDescription(); }
        }

        public Visibility IsTextBoxVisible
        {
            get
            {
                return Status != DownloadStatus.Downloading? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsProgressBarVisible
        {
            get
            {
                return Status == DownloadStatus.Downloading ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool IsStartStopButtonVisible
        {
            get
            {
                return false; //Status == DownloadStatus.DownloadCancelled || Status == DownloadStatus.DownloadFailed || Status == DownloadStatus.Downloading;
            }
        }

        public string StartStopButtonText
        {
            get
            {
                return "";
                if (Status == DownloadStatus.DownloadCancelled || Status == DownloadStatus.DownloadFailed) return "4";
                return Status == DownloadStatus.Downloading ? "g" : string.Empty;
            }
        }

        public int StartStopButtonTextSize
        {
            get
            {
                return 0;
                if (Status == DownloadStatus.DownloadCancelled || Status == DownloadStatus.DownloadFailed) return 30;
                return Status == DownloadStatus.Downloading ? 20 : 0;
            }
        }

        public Brush ProgressbarColor
        {
            get
            {
                return Brushes.Green;
                switch (Status)
                {
                    case DownloadStatus.DownloadCancelled:
                    case DownloadStatus.DownloadFailed:
                        return Brushes.Red;
                    case DownloadStatus.DownloadCompleted:
                    case DownloadStatus.Downloading:
                    case DownloadStatus.DownloadNotStarted:
                        return Brushes.Green;
                }
                return Brushes.Red;
            }
        }

        private int _ProgressPercentage;
        public int ProgressPercentage
        {
            get { return _ProgressPercentage; }
            set
            {
                _ProgressPercentage = value;
                NotifyPropertyChanged("ProgressPercentage");
            }
        }

        private bool _IsIndeterminate;
        public bool IsIndeterminate {
            get { return _IsIndeterminate; }
            set
            {
                _IsIndeterminate = value;
                NotifyPropertyChanged("IsIndeterminate"); 
            }
        }

        public FlickrSetItem() { 
            Status = DownloadStatus.DownloadNotStarted;
            ProgressPercentage = 0;
            IsIndeterminate = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (null != handler)
                handler(this, new PropertyChangedEventArgs(propertyName));
            
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