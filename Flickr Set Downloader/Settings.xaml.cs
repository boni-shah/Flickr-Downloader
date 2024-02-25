using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Flickr_Set_Downloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public const String NotFound = "Not Found.";
        public const String UserErrormsg = "User Error!!";
        public const String ValidPathErrormsg = "Please Select a Valid Folder Path!!";

        internal FolderBrowserDialog downloadfolderbrowser = new FolderBrowserDialog();

        public String download_folder = NotFound;
        public String size = String.Empty;
        public bool Save_Succesful = false;

        public Settings(ApplicationSettings objApplicationSettings)
        {
            InitializeComponent();

            download_folder = objApplicationSettings.Download_Folder;
            if (!download_folder.Equals(NotFound)) DownloadFolderpath.Text = download_folder;

            Size_combo.SelectedValue = size = objApplicationSettings.Size ?? "l";
            Totalrun_value_lbl.Content = objApplicationSettings.RunCount;
        }

        private void DownloadFolderbtn_Click(object sender, RoutedEventArgs e)
        {
            if (downloadfolderbrowser.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
                DownloadFolderpath.Text = downloadfolderbrowser.SelectedPath;
        }

        private void SaveSettingsbtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(DownloadFolderpath.Text) || !DownloadFolderpath.Text.Contains("\\"))
                System.Windows.MessageBox.Show(ValidPathErrormsg, UserErrormsg);
            else
            {
                download_folder = DownloadFolderpath.Text;
                size = ((ComboBoxItem)Size_combo.SelectedItem).Tag.ToString();
                Save_Succesful = true;
                Close();
            }
        }
    }
}