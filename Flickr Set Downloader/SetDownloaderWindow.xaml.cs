using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace Flickr_Set_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SetDownloaderWindow : Window
    {
        #region Variables, Initialisations and Declarations

            List<FlickrSet> FlickrSetslst = new List<FlickrSet>();
            private ObservableCollection<FlickrSetItem> FlickrSetItemlst;

            static List<Pagelist> pgList = new List<Pagelist>();
            static List<Photolist> PhtList = new List<Photolist>();
            static List<URLlist> URLlst = new List<URLlist>();

            static List<string> errlines = new List<string>();
            static List<string> successlines = new List<string>();

            WebClient webClient; 
            Stopwatch sw = new Stopwatch();

            public const string FlickrBaseURL = @"https://www.flickr.com";
            public const string FlickrPhotoBaseURL = FlickrBaseURL + @"/photos/";

            private BackgroundWorker _SetListrworker;
            private BackgroundWorker _SetViewrworker;
            private BackgroundWorker _loaderWorker;

            public const string ErrorMsg_InvalidURL = "Please Enter a valid Flickr URL or UserName.\ne.g. formats : abcdefgh, https://www.flickr.com/photos/abcdefgh, https://www.flickr.com/photos/abcdefgh/sets/";
            public const string ErrorMsg_Caption = "Uh-Oh!!!.";
            public const string ErrorMsg_URLLoadException = "There has been an error trying to load the set. The Exception is : ";
            public const string InfoMsg_NoDataFound = "No Data Available";
            public const string InfoMsg_LoadingData = "Loading Data. Please Wait...";
            public const string InfoMsg_ScanGoingOn = "Whoa there, Tiger!!!! You already have a scan going on.";
            public const int Username_Length = 32;

            public string CurrentSetListrURL;

            public SetDownloaderWindow()
            {
                InitializeComponent();
                URLtxt.Focus();

                _SetListrworker = new BackgroundWorker();
                _SetListrworker.DoWork += new DoWorkEventHandler(_SetListrworker_DoWork);
                _SetListrworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_SetListrworker_RunCompleted);

                _loaderWorker = new BackgroundWorker();
                _loaderWorker.DoWork += new DoWorkEventHandler(_loaderWorker_DoWork);
                _loaderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_loaderWorker_RunCompleted);

                _SetViewrworker = new BackgroundWorker();
                _SetViewrworker.DoWork += new DoWorkEventHandler(_SetViewrworker_DoWork);
                _SetViewrworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_SetViewrworker_RunCompleted);

                FlickrSetItemlst = new ObservableCollection<FlickrSetItem>()
                 {
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #1", id=1},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadFailed, FilePath="https://example.com/12345", Title="Download #2", id=2},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCompleted, FilePath="https://example.com/12345", Title="Download #3", id=3},
                     new FlickrSetItem(){_status = DownloadStatus.Downloading, FilePath="https://example.com/12345", Title="Download #4", id=4},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCancelled, FilePath="https://example.com/12345", Title="Download #5", id=5},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #11", id=1},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCancelled, FilePath="https://example.com/12345", Title="Download #12", id=2},
                     new FlickrSetItem(){_status = DownloadStatus.Downloading, FilePath="https://example.com/12345", Title="Download #13", id=3},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCompleted, FilePath="https://example.com/12345", Title="Download #14", id=4},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #15", id=5},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCancelled, FilePath="https://example.com/12345", Title="Download #21", id=1},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadFailed, FilePath="https://example.com/12345", Title="Download #22", id=2},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #23", id=3},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCancelled, FilePath="https://example.com/12345", Title="Download #24", id=4},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCompleted, FilePath="https://example.com/12345", Title="Download #25", id=5},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #31", id=1},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadCompleted, FilePath="https://example.com/12345", Title="Download #32", id=2},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadFailed, FilePath="https://example.com/12345", Title="Download #33", id=3},
                     new FlickrSetItem(){_status = DownloadStatus.DownloadNotStarted, FilePath="https://example.com/12345", Title="Download #34", id=4},
                     new FlickrSetItem(){_status = DownloadStatus.Downloading, FilePath="https://example.com/12345", Title="Download #35", id=5},
                 };
                ListBox1.ItemsSource = FlickrSetItemlst;
            }
        #endregion

		#region BackgroundWorker Tasks
            void _SetListrworker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
			{
				FlickrSetGrid.ItemsSource = FlickrSetslst;
                FlickrSetListrGridLblMsgLbl.Content = InfoMsg_NoDataFound;
			}

            void _SetListrworker_DoWork(object sender, DoWorkEventArgs e)
			{
				LoadallFlickrSetsforUserfrom(e.Argument as String);
			}

            void _loaderWorker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                this.Dispatcher.Invoke((Action)(() => { StopWait();}));
            }

            void _loaderWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                this.Dispatcher.Invoke((Action)(() => { StartWait(); }));
                System.Threading.Thread.Sleep(5000);
            }

            void _SetViewrworker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                FlickrSetViewrGrid.ItemsSource = PhtList;
                FlickrSetViewrGridLblMsgLbl.Content = InfoMsg_NoDataFound;
            }

            void _SetViewrworker_DoWork(object sender, DoWorkEventArgs e)
            {
                LoadallImagesfromaFlickrSet(e.Argument as String);
            }
		#endregion

        #region Click Events
            public void HandleRequestNavigate(object sender, RoutedEventArgs e)
            {
                string navigateUri = ((Hyperlink)sender).NavigateUri.ToString();
                Process.Start(new ProcessStartInfo(navigateUri));
                e.Handled = true;
            }

            private void LoadDataButton_Click(object sender, RoutedEventArgs e)
            {
                if (_SetListrworker.IsBusy || _SetViewrworker.IsBusy)
                {
                    MessageBox.Show(InfoMsg_ScanGoingOn);
                    return;
                }

                var URL = URLtxt.Text;
                
                if (!string.IsNullOrEmpty(URL) && URL.StartsWith("http") && !URL.StartsWith("https")) URL = URL.Replace("http", "https");

                if (string.IsNullOrEmpty(URL) || (!URL.StartsWith(FlickrPhotoBaseURL) && URL.Length > Username_Length))
                    MessageBox.Show(ErrorMsg_InvalidURL, "Error");
                else
                {
                    bool flag = false;

                    SetRequestType SetType = SetRequestType.None;
                    string UserURL = string.Empty;
                    AnalyseFlickrURL(ref URL, ref SetType, ref UserURL, ref flag);

                    if (CurrentSetListrURL != UserURL)
                    {
                        FlickrSetslst.Clear();
                        FlickrSetGrid.ItemsSource = null;
                        FlickrSetListrGridLblMsgLbl.Content = InfoMsg_LoadingData;
                    }

                    PhtList.Clear();
                    FlickrSetViewrGrid.ItemsSource = null;
                    FlickrSetViewrGridLblMsgLbl.Content = InfoMsg_LoadingData;

                    URLlst.Clear();

                    if (flag && SetType == SetRequestType.Listr && !string.IsNullOrEmpty(UserURL))
                    {
                        URLtxt.Text = UserURL;
                        _SetListrworker.RunWorkerAsync(UserURL);
                        FlickrSetListr.IsExpanded = true;
                        FlickrSetViewrGridLblMsgLbl.Content = InfoMsg_NoDataFound;
                        CurrentSetListrURL = UserURL;
                    }
                    else if (flag && SetType == SetRequestType.Viewr && !string.IsNullOrEmpty(UserURL) && !string.IsNullOrEmpty(URL))
                    {
                        URLtxt.Text = UserURL;
                        _SetViewrworker.RunWorkerAsync(URL);
                        FlickrSetViewr.IsExpanded = true;
                        if (CurrentSetListrURL != UserURL)
                            _SetListrworker.RunWorkerAsync(UserURL);
                        CurrentSetListrURL = UserURL;
                    }
                    else MessageBox.Show(ErrorMsg_InvalidURL, "Error");
                }
            }

            private void CatchEnterKey(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == System.Windows.Input.Key.Return)
                {
                    GetFlickrSetsbtn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                }
            }

            private void GetFlickrSettusbtn_Click(object sender, RoutedEventArgs e)
            {

            }
        #endregion

        #region Flickr Set Listr Processing Functions
            private void LoadallFlickrSetsforUserfrom(string URL)
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(DownloadHtmlPagefrom(URL));
                GenerateLinksofFlickrSetsfrom(document);
            }

            /* Sample Flickr Set HTML Markup
             <div class="SetsContainer" id="SetsCenterContainer" style="width:1430px">
              <div class="Sets" style="background-image: url (https://farm6.staticflickr.com/5043/5303366278_d161d8bfa7_m.jpg);">
                 <div class="SetCase" data-setid="72157623805923609">
                     <div class="setLinkDiv">
                         <a class="setLink" href="/photos/peopleofplatt/sets/72157623805923609/">
                             <img class="setThumb" src="https://l.yimg.com/g/images/spaceball.gif.v999.1385300100?images/spaceball.gif.33" width="430" height="500">
                         </a>
                     </div>
                     <div class="setText">
                         <a class="Seta" data-setid="72157623805923609" title="peculiar snapshots" href="/photos/peopleofplatt/sets/72157623805923609/" style="height:100%">
                             <h4>peculiar<wbr> snapshots</h4>
                             <p> 705 photos </p>
                         </a>
                     </div>
                 </div>
             </div>
           */
            public void GenerateLinksofFlickrSetsfrom(HtmlDocument document)
            {
                HtmlNode node = document.DocumentNode.SelectSingleNode(".//div[@class='SetsContainer']");
                if (node != null)
                {
                    foreach (HtmlNode type in node.SelectNodes(".//div[@class='Sets']"))
                    {
                        string bgstring = "background-image: url(";
                        var SetDisplayPic = type.Attributes["style"].Value;
                        int start_pos = SetDisplayPic.IndexOf(bgstring) + bgstring.Length;
                        SetDisplayPic = SetDisplayPic.Substring(start_pos, SetDisplayPic.IndexOf(");") - start_pos);

                        HtmlNode Setobj = type.SelectSingleNode(".//a[@class='Seta']");
                        if (Setobj != null)
                        {
                            var SetTitle = Setobj.Attributes["title"].Value;
                            var SetLink = @"https://www.flickr.com" + Setobj.Attributes["href"].Value;
                            var SetCount = Setobj.SelectSingleNode(".//p").InnerText.Replace("\r", "").Replace("\n", "").Replace("\t", "");

                            FlickrSetslst.Add(new FlickrSet { SetDisplayPic = SetDisplayPic, SetTitle = SetTitle, SetLink = SetLink, SetCount = SetCount });
                        }
                    }
                }
            }
        #endregion

        #region Flickr Set Viewr Processing Functions
            public static void LoadallImagesfromaFlickrSet(string url)
            {
                //url = "https://www.flickr.com/photos/wolfgangwagner/sets/72157610762580279/"; //5 Images
                //url = "https://www.flickr.com/photos/wolfgangwagner/sets/72157603692370825/";  //77 Images
                // url = "https://www.flickr.com/photos/peopleofplatt/sets/72157624572361792/";  //846 Images
                var document = GetHtmlDocumentfrom(url);
                GenerateLinksforSetViewr(document);
                GetcountofPagesinAFlickrSet(document, url);
                for (int i = 0; i < pgList.Count(); i++)
                {
                    var v = pgList[i];
                    document = GetHtmlDocumentfrom(v.url);
                    GenerateLinksforSetViewr(document);
                    GetcountofPagesinAFlickrSet(document, url); //CHECK : whether url is correct here
                }

                //foreach (var v in URLlst)
                //    DownloadFile(v.url, v.name, v.id);

                /* Reporting
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\ExportedData.txt", URLlst.Select(v => String.Format("{0} : ({1}) - {2} ({3} X {4}) {5}URL : {6}", v.name, v.id, v.label, v.height, v.width, Environment.NewLine, v.url)).ToArray(););
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\ExportedUrllst.txt", URLlst.Select(v => v.url).ToArray();                
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\Errorlst.txt", errlines.ToArray());
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\Successlst.txt", successlines.ToArray());
                */
            }

            public static void GetcountofPagesinAFlickrSet(HtmlDocument document, string url)
            {
                string[] items = url.Split('/');
                var index = items[items.Length - 2];

                var linksOnPage = from lnks in document.DocumentNode.Descendants()
                                  where lnks.Name == "a" && lnks.Attributes["href"] != null && lnks.InnerText.Trim().Length > 0
                                  select new { Url = lnks.Attributes["href"].Value, Text = lnks.InnerText };

                foreach (var v in linksOnPage)
                {
                    if (v.Url != "#" && v.Url.Contains(index + "/page"))
                    {
                        var GeneratedUrl = FlickrBaseURL + v.Url;
                        if (!pgList.Any(x => x.url == GeneratedUrl))
                            pgList.Add(new Pagelist { url = GeneratedUrl, pagenumber = v.Text });
                    }
                }
            }

            public static void GenerateLinksforSetViewr(HtmlDocument document)
            {
                var JsonScriptTag = from scripts in document.DocumentNode.Descendants() where scripts.Name == "script" select new { Text = scripts.InnerText };
                foreach (var v in JsonScriptTag)
                    if (v.Text.Contains("Y.listData = "))
                    {
                        var startindex = v.Text.IndexOf("Y.listData = ") + ("Y.listData = ").Length;
                        var length = v.Text.IndexOf("\"container_class\":\"ju\"}") + ("\"container_class\":\"ju\"}").Length - startindex;
                        String jsonstring = v.Text.Substring(startindex, length);
                        JObject o = JObject.Parse(jsonstring);   
                        
                        for (int i = 0; i < o["rows"].Count(); i++)
                        {
                            var x = o["rows"][i];
                            for (int j = 0; j < x["row"].Count(); j++)
                            {
                                var y = x["row"][j];
                                var t = new Photolist(y);
                                PhtList.Add(t);
                                //AddImageofSpecificSizetoList(t,PhotoSizeEnum.m);
                            }
                        }
                    }
            }

            public static void AddLargestImagetoList(JToken d, string name, string id)
            {
                if (d["o"] != null)
                {
                    if (d["l"] != null)
                    {
                        if (Convert.ToInt32(d["o"]["height"]) > Convert.ToInt32(d["l"]["height"]))
                            URLlst.Add(new URLlist(d["o"], name, id));
                        else
                            URLlst.Add(new URLlist(d["l"], name, id));
                    }
                    else
                        URLlst.Add(new URLlist(d["o"], name, id));
                }   
                //Console.WriteLine("{0} - {1} - {2} - {3} - {4} - {5} - {6} - {7} - {8} - {9}", 
                //                                      d["sq"], d["q"] , d["t"] , d["s"] , d["n"] , d["m"] , d["z"] , d["c"] , d["l"] , d["o"]);
            }

            public static void AddImageofSpecificSizetoList(Photolist p, PhotoSizeEnum size)
            {
                switch(size)
                {
                    case PhotoSizeEnum.sq: URLlst.Add(p.sq); break;
                    case PhotoSizeEnum.q: URLlst.Add(p.q); break;
                    case PhotoSizeEnum.t: URLlst.Add(p.t); break;
                    case PhotoSizeEnum.s: URLlst.Add(p.s); break;
                    case PhotoSizeEnum.n: URLlst.Add(p.n); break;
                    case PhotoSizeEnum.m: URLlst.Add(p.m); break;
                    case PhotoSizeEnum.z: URLlst.Add(p.z); break;
                    case PhotoSizeEnum.c: URLlst.Add(p.c); break;
                    case PhotoSizeEnum.l: URLlst.Add(p.l); break;
                    case PhotoSizeEnum.o: URLlst.Add(p.o); break;
                    case PhotoSizeEnum.h: URLlst.Add(p.h); break;
                    case PhotoSizeEnum.k: URLlst.Add(p.k); break;
                    default: break;
                }
            }

            public static void DownloadFile(string URL, string Providedname, string id)
            {
                var TempfileName = Path.GetTempFileName();   //string URL = "https://bitbucket.org/obinshah/ted-talks-downloader/downloads/TED Downloader V 3.5 - TEDinator.exe";
                string fileName = Path.GetFileName(URL).Replace("%20", " ");
                if (!string.IsNullOrEmpty(Providedname) && !string.IsNullOrEmpty(id))
                    fileName = Providedname + " - " + id + Path.GetExtension(fileName);
                string ActualFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\IM - Copy\\" + fileName.Replace(":", "-").Replace("/", "(out of)");

                Console.WriteLine("Trying to Download {0}", fileName);
                try
                {
                    if (!File.Exists(ActualFilePath))
                    {
                        using (WebClient updateDownloader = new WebClient()) { updateDownloader.DownloadFile(new Uri(URL), TempfileName); }
                        File.Move(TempfileName, ActualFilePath);
                        File.Delete(TempfileName);
                        Console.WriteLine("File Downloaded : {0}", fileName);
                        successlines.Add(String.Format("{0}", fileName));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to Download : {0}", fileName);
                    errlines.Add(String.Format("{0}", fileName));
                    Console.WriteLine(ex.Message);
                }
                return;


                /* For later Checking 
                var list = new[] { "https://google.com", "https://yahoo.com", "https://stackoverflow.com" }; 
                var tasks = Parallel.ForEach(list, new ParallelOptions{MaxDegreeOfParallelism = 10}, s =>{
                    using (var client = new WebClient())
                    {
                        Console.WriteLine("starting to download {0}", s);
                        string result = client.DownloadString((string)s);
                        Console.WriteLine("finished downloading {0}", s);
                    }});
                */
            }

        #endregion

        #region Window Functions
            private void AddtoDownloadListandStartDownload(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                if (!_loaderWorker.IsBusy) 
                { 
                    _loaderWorker.RunWorkerAsync();
                    FlickrProgressr.IsExpanded = true;
                }

                //var Flickrobj = (Photolist)((System.Windows.FrameworkElement)(sender)).DataContext;
                //if (!Flickrobj.IsSelected) Flickrobj.IsSelected = true;
                //objFlickr_Downloader = (Flickr_Downloader.MainWindow)IsWindowOpen<Window>("FlickrSetViewer");

                //if (objFlickr_Downloader != null)
                //    objFlickr_Downloader.Focus();
                //else
                //{
                //    objFlickr_Downloader = new Flickr_Downloader.MainWindow();
                //    objFlickr_Downloader.Show();
                //}
            }

            private void LoadFlickrSetfromURL(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                FlickrSetViewr.IsExpanded = true;
                System.Windows.Controls.Image _Image = sender as System.Windows.Controls.Image;
                FlickrSet item = _Image.DataContext as FlickrSet;
                URLtxt.Text = item.SetLink;

                GetFlickrSetsbtn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));                
            }

            private void RemovefromDownloadList(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                MessageBox.Show("Removed");
            }
        #endregion

        #region Static Functions
            public static T IsWindowOpen<T>(string name = null) where T : Window
            {
                var windows = Application.Current.Windows.OfType<T>();
                return string.IsNullOrEmpty(name) ? windows.FirstOrDefault() : windows.FirstOrDefault(w => w.Name.Equals(name));
            }
        #endregion

        #region LoadingAnimationFunction
            private void StartWait()
            {
                LoadingAdorner.IsAdornerVisible = true;
                ListBox1.IsEnabled = false;
            }

            private void StopWait()
            {
                LoadingAdorner.IsAdornerVisible = false;
                ListBox1.IsEnabled = true;
            }
        #endregion

        #region Progressbar
            public void DownloadFile(string urlAddress, string location)
            {
                webClient = new WebClient();                
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with https://)
                Uri URL = urlAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("https://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }

            // The event that will fire whenever the progress of the WebClient is changed
            private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                //// Calculate download speed and output it to labelSpeed.
                //labelSpeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

                //// Update the progressbar percentage only when the value is not the same.
                //progressBar.Value = e.ProgressPercentage;

                //// Show the percentage on our label.
                //labelPerc.Text = e.ProgressPercentage.ToString() + "%";

                //// Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
                //labelDownloaded.Text = string.Format("{0} MB's / {1} MB's", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            }

            // The event that will trigger when the WebClient is completed
            private void Completed(object sender, AsyncCompletedEventArgs e)
            {
                // Reset the stopwatch.
                sw.Reset();

                if (e.Cancelled == true)
                {
                    MessageBox.Show("Download has been canceled.");
                }
                else
                {
                    MessageBox.Show("Download completed!");
                }
            }
        #endregion

        #region Web Functions

            private static string DownloadHtmlPagefrom(string uri)
            {
                try
                {
                    WebClient client = new WebClient();
                    string htmlCode = client.DownloadString(uri);
                    client.Dispose();
                    return htmlCode;
                }
                catch (Exception e) { MessageBox.Show(ErrorMsg_URLLoadException + e.Message, ErrorMsg_Caption); }
                return string.Empty;
            }

            private static HtmlDocument GetHtmlDocumentfrom(string url)
            {
                try
                {
                    var webGet = new HtmlWeb();
                    var document = webGet.Load(url);
                    return document;
                }
                catch (Exception e) { MessageBox.Show(ErrorMsg_URLLoadException + e.Message, ErrorMsg_Caption); }
                return null;
            }

        #endregion

        #region Flickr Relevant Functions
        
        /// <param name="URL"></param>
        /// <param name="SetType"></param>
        /// <param name="UserURL"></param>
        /// <param name="flag"></param>
        /// <example>  
        ///     User / Set Listr URL : https://www.flickr.com/photos/wcowperthwaite/ (done), https://www.flickr.com/photos/asd/sets/ (done)
        ///     Single Set/ Set Viewr URL : https://www.flickr.com/photos/asd/sets/114424/ (done)
        ///     Photo URL : https://www.flickr.com/photos/asd/4543110/in/set-114424 (done), https://www.flickr.com/photos/wcowperthwaite/12035298853/
        ///     Photo sizes URL : https://www.flickr.com/photos/asd/4541731/sizes/z/ (lists all sizes - done), https://www.flickr.com/photos/asd/4541731/sizes/sq/ (square 75x75 - done)
        ///                       https://www.flickr.com/photos/asd/4541731/sizes/s/ (small 240x240 - done) 
        ///</example>
            private void AnalyseFlickrURL(ref string URL, ref SetRequestType SetType, ref string UserURL, ref bool flag)
            {
                flag = false;
                SetType = SetRequestType.None;

                try
                {
                    if (!URL.Contains("/")) //not a url, assume it to be a username 
                    {
                        URL = FlickrPhotoBaseURL + URL + "/sets/";
                        UserURL = URL;
                        SetType = SetRequestType.Listr;
                        flag = true;
                    }
                    else if (URL.StartsWith(FlickrPhotoBaseURL)) //could be a single set or list
                    {
                        if (!URL.EndsWith("/")) URL += "/";
                        if (URL.EndsWith("/sets/")) //already in proper set list url format
                        {
                            flag = true;
                            UserURL = URL;
                            SetType = SetRequestType.Listr;
                        }
                        else
                        {
                            var URLString = URL.Substring(URL.IndexOf(FlickrPhotoBaseURL) + FlickrPhotoBaseURL.Length);
                            var UserName = URLString.Substring(0, URLString.IndexOf("/"));
                            var SetConstantString = "/sets/";
                            var inSetConstantString = "/in/set-";

                            UserURL = FlickrPhotoBaseURL + UserName + "/sets/";

                            if (URLString.Contains(SetConstantString))
                            {                                
                                var StartingIndex = URLString.IndexOf(SetConstantString) + SetConstantString.Length;
                                var SetID = URLString.Substring(StartingIndex);

                                URL = UserURL + SetID.Remove(SetID.IndexOf('/')) + "/";
                                SetType = SetRequestType.Viewr;
                                flag = true;
                            }
                            else if (URLString.Contains(inSetConstantString))
                            {
                                var StartingIndex = URLString.IndexOf(inSetConstantString) + inSetConstantString.Length;
                                var SetID = URLString.Substring(StartingIndex);

                                URL = UserURL + SetID.Remove(SetID.IndexOf('/')) + "/";
                                SetType = SetRequestType.Viewr;
                                flag = true;
                            }
                            else //if (URLString.Count(x => x == '/') == 1 || URLString.Contains("/sizes/"))
                            {                                
                                URL =  UserURL;
                                SetType = SetRequestType.Listr;
                                flag = true;
                            }
                        }
                    }
                }
                catch (Exception) { UserURL = URL = string.Empty; }
            }
        #endregion
    }
}