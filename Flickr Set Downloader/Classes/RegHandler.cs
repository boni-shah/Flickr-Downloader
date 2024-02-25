using Flickr_Set_Downloader.Classes;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Flickr_Set_Downloader.Handler
{
    public static class RegHandler
    {
        private static RegistryKey baseRegistryKey = Registry.CurrentUser;
        private const String Subkey = "Software\\OUtilities\\FlickrDownloadr";
        private const String Welcome_msg = "They say that Sharing knowledge is the greatest of all callings and this is my contribution.\n\nHi,\nThis is your first time using this utility on this machine(or you have deleted the relevant registry keys), so you will have to fill the settings form that you will see next. Don't worry it's a one time thing only.\n\n And ya, thanks for using this app. Hope it is as useful to you as it is to me :).\n\n\nWarning: This computer program is protected by copyright law and international treaties. Unauthorized reproduction or distribution of this program, or any portion of it, may result in severe civil and criminal penalties, and will be prosecuted under the maximum extent possible under law.";
        
        public static void InitialiseReg(ApplicationSettings ObjApplicationSettings)
        {
            RegistryKey rk = baseRegistryKey;
            RegistryKey sk = rk.OpenSubKey(Subkey);
            try
            {
                if (sk == null)
                    MessageBox.Show(Welcome_msg, "Welcome to the Family");
                else
                    read_from_registry(ObjApplicationSettings);
            }
            catch (Exception e)
            {
                MessageBox.Show(Constants.Resetting_Data_msg + e.Message, Constants.Application_Corrupt_Error);
                reset_data();
            }
        }

        #region Registry Operations
            public static void write_to_registry(ApplicationSettings ObjApplicationSettings)
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk = rk.CreateSubKey(Subkey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                try
                {
                    sk.SetValue("Created by", "Obin Shah");
                    sk.SetValue("Download Folder Location", ObjApplicationSettings.Download_Folder);
                    sk.SetValue("Last Run Date", ObjApplicationSettings.LastRunDate);
                    sk.SetValue("No. of Runs", ObjApplicationSettings.RunCount);
                    sk.SetValue("Error Count", ObjApplicationSettings.ErrorCount);
                    sk.SetValue("Size", ObjApplicationSettings.Size);
                }
                catch (Exception e)
                {
                    MessageBox.Show(Constants.Resetting_Data_msg + e.Message, Constants.Registry_write_Error);
                    reset_data();
                }
                if (sk != null) sk.Close();
                rk.Close();
            }

            private static void read_from_registry(ApplicationSettings ObjApplicationSettings)
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk = rk.OpenSubKey(Subkey, true);
                try
                {
                    ObjApplicationSettings.Download_Folder = sk.GetValue("Download Folder Location").ToString();
                    ObjApplicationSettings.LastRunDate = sk.GetValue("Last Run Date").ToString();
                    ObjApplicationSettings.RunCount = (int)sk.GetValue("No. of Runs");
                    ObjApplicationSettings.ErrorCount = (int)sk.GetValue("Error Count");
                    ObjApplicationSettings.Size = sk.GetValue("Size").ToString();
                }
                catch (Exception e)
                {
                    MessageBox.Show(Constants.Resetting_Data_msg + e.Message, Constants.Registry_read_Error);
                    reset_data();
                }
                if (sk != null) sk.Close();
                rk.Close();
            }
        #endregion

        #region Utility Functions
            private static void reset_data()
            {
                RegistryKey rk = baseRegistryKey;
                rk.DeleteSubKey(Subkey);
                rk.Close();
            }
        #endregion
    }
}