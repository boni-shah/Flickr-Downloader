using Flickr_Set_Downloader.Classes;
using Flickr_Set_Downloader.Handler;
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Flickr_Set_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SetDownloaderWindow
    {
        #region Variables, Initialisations and Declarations

            List<FlickrSet> FlickrSetslst = new List<FlickrSet>();
            private ObservableCollection<FlickrSetItem> FlickrSetItemlst = new ObservableCollection<FlickrSetItem>();

            static List<Photolist> PhtList = new List<Photolist>();
            static List<URLlist> URLlst = new List<URLlist>();

            static List<string> errlines = new List<string>();
            static List<string> successlines = new List<string>();

            About Aboutbox;
            Settings SettingsWindow;
            ApplicationSettings ObjApplicationSettings = new ApplicationSettings();

            private BackgroundWorker _SetListrworker;
            private BackgroundWorker _SetViewrworker;
            private BackgroundWorker _downloadListLoaderWorker;

            public string CurrentSetListrURL;
            ParallelDownloader downloader = new ParallelDownloader(Constants.ParallelThreadCount);

            public SetDownloaderWindow()
            {
                InitializeComponent();

                RegHandler.InitialiseReg(ObjApplicationSettings);
                if (ObjApplicationSettings.RunCount % 10 == 0)
                    Process.Start(new ProcessStartInfo(Constants.ScenarioSolution_Homepage));

                ObjApplicationSettings.RunCount++;
                ObjApplicationSettings.LastRunDate = DateTime.Today.ToShortDateString();
                if (String.IsNullOrEmpty(ObjApplicationSettings.Download_Folder) || ObjApplicationSettings.Download_Folder.Equals(Constants.NotFound))
                    SettingsWindow_Show();

                ServicePointManager.DefaultConnectionLimit = 10;
                URLtxt.Focus();

                _SetListrworker = new BackgroundWorker {WorkerSupportsCancellation = true};
                _SetListrworker.DoWork += _SetListrworker_DoWork;
                _SetListrworker.RunWorkerCompleted += _SetListrworker_RunCompleted;

                _downloadListLoaderWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
                _downloadListLoaderWorker.DoWork += _downloadListLoaderWorker_DoWork;
                _downloadListLoaderWorker.RunWorkerCompleted += _downloadListLoaderWorker_RunCompleted;

                _SetViewrworker = new BackgroundWorker {WorkerSupportsCancellation = true};
                _SetViewrworker.DoWork += _SetViewrworker_DoWork;
                _SetViewrworker.RunWorkerCompleted += _SetViewrworker_RunCompleted;
                
                ImageDownloadList.ItemsSource = FlickrSetItemlst;
            }
        #endregion

        #region BackgroundWorker Tasks
            void _SetListrworker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                FlickrSetGrid.ItemsSource = FlickrSetslst;
                FlickrSetListrGridLblMsgLbl.Content = Constants.InfoMsg_NoDataFound;
            }

            void _SetListrworker_DoWork(object sender, DoWorkEventArgs e)
            {
                LoadallFlickrSetsforUserfrom(e.Argument as String);
            }

            void _downloadListLoaderWorker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                Dispatcher.Invoke(StopWait);
                Dispatcher.Invoke(() => System.Windows.Data.CollectionViewSource.GetDefaultView(FlickrSetItemlst).Refresh());
            }

            void _downloadListLoaderWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                var ArgumentObj = e.Argument as Argument;
                Dispatcher.Invoke(StartWait);
                if (ArgumentObj != null)
                {
                    var _CurrentPhotoSize = (PhotoSizeEnum)Enum.Parse(typeof(PhotoSizeEnum), ObjApplicationSettings.Size, true);
                    var ImagesintheSet = GetalltheImagespresentintheSet(ArgumentObj.SetLink, false);

                    foreach (var Flickrobj in ImagesintheSet)
                    {
                        var FlickrSetItem = FlickrSetItemlst.SingleOrDefault(x => x.Id == Flickrobj.id && x.PhotoSizeDescription == _CurrentPhotoSize.GetDescription());

                        if (FlickrSetItem != null && FlickrSetItem.PhotoSizeDescription == _CurrentPhotoSize.GetDescription()) continue;

                        var ImagetobeAdded = ReturnImageofSpecificSize(Flickrobj, _CurrentPhotoSize);
                        var title = Flickrobj.name;
                        if (FlickrSetItemlst.Any(x => x.Id == Flickrobj.id))
                            title += " (" + _CurrentPhotoSize.GetDescription() + ")";
                        var ItemStatus = ImagetobeAdded.url ==null? DownloadStatus.DownloadFailed: DownloadStatus.DownloadNotStarted;
                            
                        var flickrobj = Flickrobj;
                        Dispatcher.Invoke(() => FlickrSetItemlst.Add(new FlickrSetItem
                        {
                            Status = ItemStatus,
                            FilePath = ImagetobeAdded.url ?? "",
                            Title = title,
                            Id = flickrobj.id,
                            PhotoSizeDescription = _CurrentPhotoSize.GetDescription(),
                            SetId = ArgumentObj.SetId,
                            SetName = ArgumentObj.SetName,
                            IsIndeterminate = ItemStatus != DownloadStatus.DownloadFailed
                        }));
                    }
                    QueueDownloadFiles();
                }
            }

            void _SetViewrworker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                FlickrSetViewrGrid.ItemsSource = PhtList;
                FlickrSetViewrGridLblMsgLbl.Content = Constants.InfoMsg_NoDataFound;
            }

            void _SetViewrworker_DoWork(object sender, DoWorkEventArgs e)
            {
                var ImagesPresentintheSet = GetalltheImagespresentintheSet(e.Argument as String, true);

                foreach (var v in ImagesPresentintheSet)
                {
                    if (FlickrSetItemlst.Any((x => x.Id == v.id))) v.MarkObjectSelected();
                    PhtList.Add(v);
                }
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
                    MessageBox.Show(Constants.InfoMsg_ScanGoingOn);
                    return;
                }

                var URL = URLtxt.Text;

                if (!string.IsNullOrEmpty(URL) && URL.StartsWith("http") && !URL.StartsWith("https")) URL = URL.Replace("http", "https");

                if (string.IsNullOrEmpty(URL) || (!URL.StartsWith(Constants.FlickrPhotoBaseURL) && URL.Length > Constants.Username_Length))
                    MessageBox.Show(Constants.ErrorMsg_InvalidURL, "Error");
                else
                {
                    bool flag = false;

                    var SetType = SetRequestType.None;
                    var UserURL = string.Empty;
                    AnalyseFlickrURL(ref URL, ref SetType, ref UserURL, ref flag);

                    if (CurrentSetListrURL != UserURL)
                    {
                        FlickrSetslst.Clear();
                        FlickrSetGrid.ItemsSource = null;
                        FlickrSetListrGridLblMsgLbl.Content = Constants.InfoMsg_LoadingData;
                    }

                    PhtList.Clear();
                    FlickrSetViewrGrid.ItemsSource = null;
                    FlickrSetViewrGridLblMsgLbl.Content = Constants.InfoMsg_LoadingData;

                    URLlst.Clear();

                    if (flag && SetType == SetRequestType.Listr && !string.IsNullOrEmpty(UserURL))
                    {
                        FlickrSetViewr.Header = "Flickr Set Viewr";
                        URLtxt.Text = UserURL;
                        _SetListrworker.RunWorkerAsync(UserURL);
                        FlickrSetListr.IsExpanded = true;
                        FlickrSetViewrGridLblMsgLbl.Content = Constants.InfoMsg_NoDataFound;
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
                    else MessageBox.Show(Constants.ErrorMsg_InvalidURL, "Error");
                }
            }

            private void CatchEnterKey(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                    GetFlickrSetsbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        
            private void GetFlickrSettusbtn_Click(object sender, RoutedEventArgs e)
            {

            }

            private void Settingsbtn_Click(object sender, RoutedEventArgs e)
            {
                SettingsWindow_Show();
            }

            private void Infobtn_Click(object sender, RoutedEventArgs e)
            {
                if (Aboutbox == null)
                {
                    Aboutbox = new About(this);
                    Aboutbox.Show();
                    Aboutbox.Closed += Aboutbox_Closed;
                }
                else
                    Aboutbox.Focus();
            }

            void Aboutbox_Closed(object sender, EventArgs e)
            {
                Aboutbox = null;
            }
        #endregion

        #region Flickr Set Listr Processing Functions
            private void LoadallFlickrSetsforUserfrom(string URL)
            {
                var document = new HtmlDocument();
                document.LoadHtml(DownloadHtmlPagefrom(URL));
                GenerateLinksofFlickrSetsfrom(document);
            }

            public void GenerateLinksofFlickrSetsfrom(HtmlDocument document)
            {
                HtmlNode node = document.DocumentNode.SelectSingleNode(".//div[@class='SetsContainer']");
                if (node != null)
                    foreach (HtmlNode type in node.SelectNodes(".//div[@class='Sets']"))
                    {
                        const string bgstring = "background-image: url(";
                        var SetDisplayPic = type.Attributes["style"].Value;
                        var start_pos = SetDisplayPic.IndexOf(bgstring) + bgstring.Length;
                        SetDisplayPic = SetDisplayPic.Substring(start_pos, SetDisplayPic.IndexOf(");") - start_pos);

                        HtmlNode Setobj = type.SelectSingleNode(".//a[@class='Seta']");
                        if (Setobj != null)
                        {
                            var SetTitle = Setobj.Attributes["title"].Value;
                            var SetId = Setobj.Attributes["data-setid"].Value;
                            var SetLink = @"https://www.flickr.com" + Setobj.Attributes["href"].Value;
                            var SetCount = Setobj.SelectSingleNode(".//p").InnerText.Replace("\r", "").Replace("\n", "").Replace("\t", "");

                            FlickrSetslst.Add(new FlickrSet { SetDisplayPic = SetDisplayPic, SetTitle = SetTitle, SetLink = SetLink, SetCount = SetCount, SetId = SetId});
                        }
                    }
            }
        #endregion

        #region Flickr Set Viewr Processing Functions
            /// <summary>
            /// Get all the images in the set
            /// </summary>
            /// <param name="url"></param>
            /// <param name="UpdateFlickrSetViewrHeader"></param>
            /// <example-Urls>  url = "https://www.flickr.com/photos/wolfgangwagner/sets/72157610762580279/"; //5 Images
            ///                 url = "https://www.flickr.com/photos/wolfgangwagner/sets/72157603692370825/";  //77 Images
            ///                 url = "https://www.flickr.com/photos/peopleofplatt/sets/72157624572361792/";  //846 Images
            /// </example-Urls>
            IEnumerable<Photolist> GetalltheImagespresentintheSet(string url, bool UpdateFlickrSetViewrHeader)
            {
                object HeaderText = null;
                var pagelist = new List<Pagelist>();
                var photolist = new List<Photolist>();

                if (UpdateFlickrSetViewrHeader)
                    Dispatcher.Invoke(() =>
                    {
                        HeaderText = FlickrSetViewr.Header;
                        FlickrSetViewr.Header = HeaderText + " - Analyzing";
                    });
                
                var document = GetHtmlDocumentfrom(url);
                photolist.AddRange(GenerateLinksforSetViewr(document));
                GetcountofPagesinAFlickrSet(document, url, pagelist);
                for (var i = 0; i < pagelist.Count(); i++)
                {
                    int CurrentPageCount = i + 2; //one for already completed page and other for index starting from 0
                    if (UpdateFlickrSetViewrHeader)
                        Dispatcher.Invoke(() => { FlickrSetViewr.Header = HeaderText + " - Analyzing " + CurrentPageCount + " of " + (pagelist.Count + 1) + " pages."; });
                    
                    var v = pagelist[i];
                    document = GetHtmlDocumentfrom(v.url);
                    photolist.AddRange(GenerateLinksforSetViewr(document));
                    GetcountofPagesinAFlickrSet(document, url, pagelist);
                }
                if (UpdateFlickrSetViewrHeader)
                    Dispatcher.Invoke(() => { FlickrSetViewr.Header = HeaderText; });
                return photolist;
                //foreach (var v in URLlst)
                //    DownloadFile(v.url, v.name, v.id);

                /* Reporting
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\ExportedData.txt", URLlst.Select(v => String.Format("{0} : ({1}) - {2} ({3} X {4}) {5}URL : {6}", v.name, v.id, v.label, v.height, v.width, Environment.NewLine, v.url)).ToArray(););
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\ExportedUrllst.txt", URLlst.Select(v => v.url).ToArray();                
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\Errorlst.txt", errlines.ToArray());
                    System.IO.File.WriteAllLines(@"C:\Users\IBM_ADMIN\Desktop\Successlst.txt", successlines.ToArray());
                */
            }

            private void GetcountofPagesinAFlickrSet(HtmlDocument document, string url, ICollection<Pagelist> PageList)
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
                        var GeneratedUrl = Constants.FlickrBaseURL + v.Url;
                        if (!PageList.Any(x => x.url == GeneratedUrl))
                            PageList.Add(new Pagelist { url = GeneratedUrl, pagenumber = v.Text });
                    }
                }
            }

            private void LoadFlickrSetfromURL(object sender, MouseButtonEventArgs e)
            {
                FlickrSetViewr.IsExpanded = true;
                var _Image = sender as Image;
                if (_Image != null)
                {
                    var item = _Image.DataContext as FlickrSet;
                    if (item != null)
                    {
                        URLtxt.Text = item.SetLink;
                        FlickrSetViewr.Header = "Flickr Set Viewr - " + item.SetTitle;
                        FlickrSetViewr.Tag = item.SetId + "," + item.SetTitle;
                    }
                }
                GetFlickrSetsbtn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            public List<Photolist> GenerateLinksforSetViewr(HtmlDocument document)
            {
                var PhotoList = new List<Photolist>();
                var JsonScriptTag = from scripts in document.DocumentNode.Descendants() where scripts.Name == "script" select new { Text = scripts.InnerText };
                foreach (var v in JsonScriptTag)
                    if (v.Text.Contains("Y.listData = "))
                    {
                        var startindex = v.Text.IndexOf("Y.listData = ") + ("Y.listData = ").Length;
                        var length = v.Text.IndexOf("\"container_class\":\"ju\"}") + ("\"container_class\":\"ju\"}").Length - startindex;
                        var jsonstring = v.Text.Substring(startindex, length);
                        JObject o = JObject.Parse(jsonstring);

                        for (var i = 0; i < o["rows"].Count(); i++)
                        {
                            var x = o["rows"][i];
                            for (var j = 0; j < x["row"].Count(); j++)
                            {
                                var y = x["row"][j];
                                var t = new Photolist(y);
                                PhotoList.Add(t);
                            }
                        }
                    }

                return PhotoList;
            }
        #endregion

        #region Static Utility Functions
            public static URLlist ReturnImageofSpecificSize(Photolist p, PhotoSizeEnum size)
            {
                switch (size)
                {
                    case PhotoSizeEnum.sq: return p.sq;
                    case PhotoSizeEnum.q: return p.q;
                    case PhotoSizeEnum.t: return p.t;
                    case PhotoSizeEnum.s: return p.s;
                    case PhotoSizeEnum.n: return p.n;
                    case PhotoSizeEnum.m: return p.m;
                    case PhotoSizeEnum.z: return p.z;
                    case PhotoSizeEnum.c: return p.c;
                    case PhotoSizeEnum.l: return p.l;
                    case PhotoSizeEnum.o: return p.o;
                    case PhotoSizeEnum.h: return p.h;
                    case PhotoSizeEnum.k: return p.k;
                }
                return p.o;
            }

            public static void AddImageofSpecificSizetoList(Photolist p, PhotoSizeEnum size)
            {
                switch (size)
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
                }
            }

            public static void AddLargestImagetoList(JToken d, string name, string id)
            {
                if (d["o"] == null) return;
                if (d["l"] != null)
                    URLlst.Add(Convert.ToInt32(d["o"]["height"]) > Convert.ToInt32(d["l"]["height"]) ? new URLlist(d["o"], name, id) : new URLlist(d["l"], name, id));
                else
                    URLlst.Add(new URLlist(d["o"], name, id));
            }
        #endregion

        #region Download List Functions
            private void AddtoDownloadListandStartDownload(object sender, MouseButtonEventArgs e)
            {
                if (!_downloadListLoaderWorker.IsBusy)
                {
                    var FlickrSetobj = (FlickrSet)((FrameworkElement)(sender)).DataContext;
                    var ArgumentObj = new Argument(FlickrSetobj.SetLink, FlickrSetobj.SetTitle, FlickrSetobj.SetId);
                    _downloadListLoaderWorker.RunWorkerAsync(ArgumentObj);
                    FlickrProgressr.IsExpanded = true;
                }
                else
                    MessageBox.Show("There is already a download list generation going on!!! Please wait for it to end.");
            }
        
            private void AddSingleImagetoDownloadListandStartDownload(object sender, MouseButtonEventArgs e)
            {
                var _CurrentPhotoSize = (PhotoSizeEnum)Enum.Parse(typeof(PhotoSizeEnum), ObjApplicationSettings.Size, true);
                var Flickrobj = (Photolist)((FrameworkElement)(sender)).DataContext;

                var FlickrSetItem = FlickrSetItemlst.SingleOrDefault(x => x.Id == Flickrobj.id && x.PhotoSizeDescription == _CurrentPhotoSize.GetDescription());

                if (FlickrSetItem == null || FlickrSetItem.PhotoSizeDescription != _CurrentPhotoSize.GetDescription())
                {
                    var ImagetobeAdded = ReturnImageofSpecificSize(Flickrobj, _CurrentPhotoSize);
                    var setInformation = FlickrSetViewr.Tag.ToString().Split(',');
                    var title = Flickrobj.name;
                    if (FlickrSetItemlst.Any(x => x.Id == Flickrobj.id))
                        title += " (" + _CurrentPhotoSize.GetDescription() + ")";
                    var ItemStatus = ImagetobeAdded.url == null ? DownloadStatus.DownloadFailed : DownloadStatus.DownloadNotStarted;

                        FlickrSetItemlst.Add(new FlickrSetItem
                        {
                            Status = ItemStatus,
                            FilePath = ImagetobeAdded.url ?? "",
                            Title = title,
                            Id = Flickrobj.id,
                            PhotoSizeDescription = _CurrentPhotoSize.GetDescription(),
                            SetId = setInformation[0],
                            SetName = setInformation[1],
                            IsIndeterminate = ItemStatus != DownloadStatus.DownloadFailed
                        });
                    QueueDownloadFiles();
                }
                else if (FlickrSetItem.Status != DownloadStatus.DownloadCompleted && FlickrSetItem.Status != DownloadStatus.Downloading)
                    FlickrSetItemlst.Remove(FlickrSetItem);
                else
                {
                    FlickrProgressr.IsExpanded = true;
                    ImageDownloadList.ScrollIntoView(FlickrSetItem);
                }
            }

            private void RemovefromDownloadList(object sender, RoutedEventArgs routedEventArgs)
            {
                var Flickrobj = (FlickrSetItem)((FrameworkElement)(sender)).DataContext;
                var FlickrSetItem = FlickrSetItemlst.SingleOrDefault(x => x.Id == Flickrobj.Id && x.PhotoSizeDescription == Flickrobj.PhotoSizeDescription);
                if (FlickrSetItem != null && (FlickrSetItem.Status == DownloadStatus.DownloadCompleted || FlickrSetItem.Status == DownloadStatus.Downloading))
                {
                    MessageBox.Show("This item has already been downloaded or is downloading, it cannot be removed.");
                    return;
                }               
                FlickrSetItemlst.Remove(FlickrSetItem);

                var PhtFlickrObj = PhtList.SingleOrDefault(x => x.id == Flickrobj.Id);
                if (PhtFlickrObj != null && Flickrobj.PhotoSizeDescription == ObjApplicationSettings.Size) PhtFlickrObj.IsSelected = false;
            }

            void Video_WC_DownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
            {
                var v = (ParallelWebClient) sender;
                v.item.IsIndeterminate = false;
                v.item.ProgressPercentage = e.ProgressPercentage;
            }

            void Video_WC_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                var v = (ParallelWebClient)sender;
                v.item.Status = DownloadStatus.DownloadCompleted;
                QueueDownloadFiles();
            }

            void QueueDownloadFiles()
            {
                var index = ParallelDownloader.GetFreeWebClient();
                var i = 0;

                while (i < FlickrSetItemlst.Count && index != -1)
                {
                    var v = FlickrSetItemlst[i];
                    if (v.Status == DownloadStatus.DownloadNotStarted)
                    {
                        downloader.InitialiseWebClientandBeginDownload(index, v, Video_WC_DownloadProgressChanged, Video_WC_DownloadFileCompleted);
                        index = ParallelDownloader.GetFreeWebClient();
                    }
                    i++;
                }
            }
        #endregion

        #region LoadingAnimationFunction
            private void StartWait()
            {
                LoadingAdorner.IsAdornerVisible = true;
                ImageDownloadList.IsEnabled = false;
            }

            private void StopWait()
            {
                LoadingAdorner.IsAdornerVisible = false;
                ImageDownloadList.IsEnabled = true;
            }
        #endregion

        #region Download Related Functions
            private static string DownloadHtmlPagefrom(string uri)
            {
                try
                {
                    var client = new WebClient();
                    string htmlCode = client.DownloadString(uri);
                    client.Dispose();
                    return htmlCode;
                }
                catch (Exception e) { MessageBox.Show(Constants.ErrorMsg_URLLoadException + e.Message, Constants.ErrorMsg_Caption); }
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
                catch (Exception e) { MessageBox.Show(Constants.ErrorMsg_URLLoadException + e.Message, Constants.ErrorMsg_Caption); }
                return new HtmlDocument();
            }

            void DownloadFile(string URL, string Providedname, string id)
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
            private static void AnalyseFlickrURL(ref string URL, ref SetRequestType SetType, ref string UserURL, ref bool flag)
            {
                flag = false;
                SetType = SetRequestType.None;

                try
                {
                    if (!URL.Contains("/")) //not a url, assume it to be a username 
                    {
                        URL = Constants.FlickrPhotoBaseURL + URL + "/sets/";
                        UserURL = URL;
                        SetType = SetRequestType.Listr;
                        flag = true;
                    }
                    else if (URL.StartsWith(Constants.FlickrPhotoBaseURL)) //could be a single set or list
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
                            var URLString = URL.Substring(URL.IndexOf(Constants.FlickrPhotoBaseURL) + Constants.FlickrPhotoBaseURL.Length);
                            var UserName = URLString.Substring(0, URLString.IndexOf("/"));
                            const string SetConstantString = "/sets/";
                            const string inSetConstantString = "/in/set-";

                            UserURL = Constants.FlickrPhotoBaseURL + UserName + "/sets/";

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
                                URL = UserURL;
                                SetType = SetRequestType.Listr;
                                flag = true;
                            }
                        }
                    }
                }
                catch (Exception) { UserURL = URL = string.Empty; }
            }
        #endregion

        #region Settings Related Functions
            private void SettingsWindow_Show()
            {
                if (SettingsWindow == null)
                {
                    SettingsWindow = new Settings(ObjApplicationSettings);
                    SettingsWindow.Closed += SettingsWindow_Closed;
                    SettingsWindow.ShowDialog();
                }
                else
                    SettingsWindow.Focus();
            }

            void SettingsWindow_Closed(object sender, EventArgs e)
            {
                if (SettingsWindow.Save_Succesful)
                {
                    ObjApplicationSettings.Download_Folder = SettingsWindow.download_folder;
                    ObjApplicationSettings.Size = SettingsWindow.size;
                    RegHandler.write_to_registry(ObjApplicationSettings);
                }
                SettingsWindow = null;
            }
        #endregion

        #region Internal Classes
            internal class Argument
            {
                public string SetLink;
                public string SetName;
                public string SetId;

                public Argument(string setLink, string setName, string setId)
                {
                    SetLink = setLink;
                    SetName = setName;
                    SetId = setId;
                }
            }
        #endregion

        #region Window Events
            private void App_Closing(object sender, CancelEventArgs e)
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 1)
                {
                    if (_SetListrworker.IsBusy)
                        _SetListrworker.CancelAsync();
                    if (_SetViewrworker.IsBusy)
                        _SetViewrworker.CancelAsync();
                    if (_downloadListLoaderWorker.IsBusy)
                        _downloadListLoaderWorker.CancelAsync();
                    RegHandler.write_to_registry(ObjApplicationSettings);
                }
            }
        #endregion
    }
}