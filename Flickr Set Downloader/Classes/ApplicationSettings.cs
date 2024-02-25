using Flickr_Set_Downloader.Classes;
using System;

namespace Flickr_Set_Downloader
{
    public class ApplicationSettings
    {
        public int RunCount { get; set; }

        public string Size { get; set; }

        public string Download_Folder { get; set; }

        public int ErrorCount { get; set; }

        public string LastRunDate { get; set; }

        public ApplicationSettings()
        {
            Download_Folder = Constants.NotFound;
            Size = "l";
            LastRunDate = DateTime.Today.ToShortDateString();
            RunCount = 0;
            ErrorCount = 0;
        }
    }
}