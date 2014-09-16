using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Reflection;
using System.IO;
using System.Text;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
using Microsoft.Phone.Shell;

namespace JazzChartApp
{
    public partial class ViewSong : PhoneApplicationPage
    {
        private String name;
        private String path;

        private void SaveToIsoStore(string fileName, byte[] data)
        {
            string strBaseDir = string.Empty;
            string delimStr = "/";
            char[] delimiter = delimStr.ToCharArray();
            string[] dirsPath = fileName.Split(delimiter);

            //Get the IsoStore.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            //Re-create the directory structure.
            for (int i = 0; i < dirsPath.Length - 1; i++)
            {
                strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
                isoStore.CreateDirectory(strBaseDir);
            }

            //Remove the existing file.
            if (isoStore.FileExists(fileName))
            {
                isoStore.DeleteFile(fileName);
            }

            //Write the file.
            using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(fileName)))
            {
                bw.Write(data);
                bw.Close();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            name = NavigationContext.QueryString["song"];
            path = NavigationContext.QueryString["path"];

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            Stream sr = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            using (BinaryReader br = new BinaryReader(sr))
            {
                byte[] data = br.ReadBytes((int)sr.Length);
                SaveToIsoStore("CurrentSong.gif", data);
                SaveToIsoStore("CurrentSong.html", System.Text.Encoding.UTF8.GetBytes("<!doctype html> <html xmlns=\"http://www.w3.org/1999/xhtml\"><head><meta name=\"Viewport\" content=\"width=device-width\" /></head><body><img height=\"100%\" width=\"100%\" src=\"CurrentSong.gif\"></body></html>"));
            }
            
            webBrowser1.Navigate(new Uri("CurrentSong.html", UriKind.Relative));
            //Old: webBrowser1.NavigateToString("<html><body><img src=\"current_song.gif\"></body></html>");
        }

        public ViewSong()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); //Using normal navigation relaunches splash screen.  This avoids that.
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Enabled;
        }
    }
}