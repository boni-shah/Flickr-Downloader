using System;

namespace Flickr_Set_Downloader.Classes
{
    public class Constants
    {
        public const String Application_Corrupt_Error = "The Application has been Corrupted!!";
        public const String Registry_read_Error = "Error reading values from the Registry";
        public const String Registry_write_Error = "Error writing values to the Registry";
        public const String Resetting_Data_msg = "Resetting all data......\nError code : ";

        public const String OneInstanceAllowedmsg = "Only one Instance allowed. Application is already running.";
        public const String UserErrormsg = "User Error!!";

        public const string FlickrBaseURL = @"https://www.flickr.com";
        public const string FlickrPhotoBaseURL = FlickrBaseURL + @"/photos/";

        public const string ErrorMsg_InvalidURL = "Please Enter a valid Flickr URL or UserName.\ne.g. formats : abcdefgh, https://www.flickr.com/photos/abcdefgh, https://www.flickr.com/photos/abcdefgh/sets/";
        public const string ErrorMsg_Caption = "Uh-Oh!!!.";
        public const string ErrorMsg_URLLoadException = "There has been an error trying to load the set. The Exception is : ";
        public const string InfoMsg_NoDataFound = "No Data Available";
        public const string InfoMsg_LoadingData = "Loading Data. Please Wait...";
        public const string InfoMsg_ScanGoingOn = "Whoa there, Tiger!!!! You already have a scan going on.";
        public const String ScenarioSolution_Homepage = @"http://obinshah.wordpress.com";
        public const String ScenarioSolution_TEDTagpage = @"http://obinshah.wordpress.com/category/my-utilities/";

        public const String NotFound = "Not Found.";
        public const int Username_Length = 32;
        public const int ParallelThreadCount = 5;
    }
}