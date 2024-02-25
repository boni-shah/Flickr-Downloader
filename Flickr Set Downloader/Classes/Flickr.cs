using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace Flickr_Set_Downloader
{
    public class Photolist : INotifyPropertyChanged
    {
        public string name { get; set; }
        public string is_video { get; set; }
        public string id { get; set; }
        public string src { get; set; }
        public string photo_url { get; set; }
        public string full_name { get; set; }

        private bool _IsSelected = false;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (!_IsSelected && value)
                {
                    _IsSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
                else if (_IsSelected && !value)
                {
                    _IsSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public URLlist sq;
        public URLlist q;
        public URLlist t;
        public URLlist s;
        public URLlist n;
        public URLlist m;
        public URLlist z;
        public URLlist c;
        public URLlist l;
        public URLlist o;
        public URLlist h;
        public URLlist k;

        public Photolist() { }

        public Photolist(string name, string id, string src, string photo_url, string full_name, string is_video) { }

        public Photolist(JToken rowobj)
        {
            name = (string)rowobj["name"];
            id = (string)rowobj["id"];
            src = (string)rowobj["src"];
            photo_url = @"https://www.flickr.com" + (string)rowobj["photo_url"];
            full_name = (string)rowobj["full_name"];
            is_video = (string)rowobj["is_video"];

            sq = new URLlist(rowobj["sizes"]["sq"],name,id);
            q = new URLlist(rowobj["sizes"]["q"],name,id);
            t = new URLlist(rowobj["sizes"]["t"],name,id);
            s = new URLlist(rowobj["sizes"]["s"],name,id);
            n = new URLlist(rowobj["sizes"]["n"],name,id);
            m = new URLlist(rowobj["sizes"]["m"],name,id);
            z = new URLlist(rowobj["sizes"]["z"],name,id);
            c = new URLlist(rowobj["sizes"]["c"],name,id);
            l = new URLlist(rowobj["sizes"]["l"],name,id);
            o = new URLlist(rowobj["sizes"]["o"],name,id);
            h = new URLlist(rowobj["sizes"]["h"],name,id);
            k = new URLlist(rowobj["sizes"]["k"],name,id);
        }

        public void MarkObjectSelected() { _IsSelected = true; }
    }

    public class URLlist
    {
        public string name { get; set; }
        public string id { get; set; }
        public string file { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public string width { get; set; }
        public string height { get; set; }


        public URLlist() { }
        public URLlist(JToken rowobj, string fname, string fid)
        {
            if (rowobj != null)
            {
                file = (string)rowobj["file"];
                label = (string)rowobj["label"];
                url = (string)rowobj["url"];
                width = (string)rowobj["width"];
                height = (string)rowobj["height"];
                name = fname;
                id = fid;
            }
        }
    }

    public class Pagelist
    {
        public string pagenumber { get; set; }
        public string url { get; set; }
    }
}