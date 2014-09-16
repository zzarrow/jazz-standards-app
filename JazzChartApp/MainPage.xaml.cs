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
using System.Diagnostics;
using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace JazzChartApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const String STR_FILTER_DEFAULT = "Type here to filter by name...";
        private const int SPLASH_SCREEN_DURATION = 1;

        //Splash screen controllers
        private Popup _popup;
        private int _ticks;
        private DispatcherTimer _dispatcherTimer;

        private SongItem MostRecentlyViewed;

        //Handler attached to the dispatcherTimer
        private void CheckTicks(object sender, EventArgs e)
        {
            _ticks++;
            if (_ticks != SPLASH_SCREEN_DURATION) return;
        
            _popup.IsOpen = false;
            _dispatcherTimer.Stop();
        }

        void displaySplashScreen()
        {
            _popup = new Popup();
            var img = new Image { Source = new BitmapImage(new Uri("SplashScreenImage.jpg", UriKind.Relative)) };
            _popup.Child = img;
            _popup.IsOpen = true;

            _dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 2) };
            _dispatcherTimer.Tick += CheckTicks; //Attaches the handler
            _dispatcherTimer.Start();
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            displaySplashScreen();
            Loaded += new RoutedEventHandler(MainPage_Loaded);

            txtFilter.Text = STR_FILTER_DEFAULT;
        }

        void PopulateListBasedOnFilter(String _filter = "") //default = no filter
        {
            if (_filter.Equals(STR_FILTER_DEFAULT)) //Don't filter based on the default instruction text
                _filter = "";            
            
            List<SongItem> songs = new List<SongItem>();

            var test = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string key in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (!key.ToLower().Contains(".gif")) //Throw out the other (system) resources we get from the assembly
                    continue;
                if ((_filter.Equals("")) || (SongItem.ParseSongName(key).ToLower().Contains(_filter.ToLower())))
                    songs.Add(new SongItem(key)); //SongItem constructor will parse out the title
            }

            //If there's no filter, we don't need to sort further, and can update the UI and halt here
            if (_filter.Equals(""))
            {
                songs.Sort(SongItem.CompareSongs);
                SongList.ItemsSource = songs; //Update UI

                //SongList.UpdateLayout();
                //SongList.SelectedIndex = 0;
                //SongList.UpdateLayout();

                return;
            }

            //Now, we want the songs that start with the filter text to appear first.
            //Let's strip them out of the list, sort the remaining items, then combine lists and pass to the UI
            
            List<SongItem> BestResults = new List<SongItem>();
            List<SongItem> OtherResults = new List<SongItem>(); //Need to create 2 new lists because we can't modify a collection while iterating
            if (!_filter.Equals(""))
            {
                foreach(SongItem s in songs)
                {
                    if (s.Name.ToLower().StartsWith(_filter.ToLower()))
                        BestResults.Add(s);
                    else
                        OtherResults.Add(s);
                }
            }

            //Sort both alphabetically
            BestResults.Sort(SongItem.CompareSongs);
            OtherResults.Sort(SongItem.CompareSongs);

            //Concat the lists
            BestResults.AddRange(OtherResults);
            SongList.ItemsSource = BestResults; //Update UI

            if (BestResults.Count == 0)
            {
                //No songs found - Display message
                txtNotFound1.Visibility = System.Windows.Visibility.Visible;
                txtNotFound2.Visibility = System.Windows.Visibility.Visible;
                txtNotFound3.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtNotFound1.Visibility = System.Windows.Visibility.Collapsed;
                txtNotFound2.Visibility = System.Windows.Visibility.Collapsed;
                txtNotFound3.Visibility = System.Windows.Visibility.Collapsed;

                SongList.UpdateLayout();
                SongList.SelectedIndex = 0;
                SongList.UpdateLayout();
                UpdateLayout();
            }
            SongList.UpdateLayout();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateListBasedOnFilter(); //default parm = no filter
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!txtFilter.Text.Equals(STR_FILTER_DEFAULT)) //Fixes keyoard display bug on song selection
                PopulateListBasedOnFilter(txtFilter.Text);
        }

        private void txtFilter_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //The first time the user taps the filter textbox, clear our default instruction text so they don't have to backspace it
            if (txtFilter.Text.Equals(STR_FILTER_DEFAULT))
                txtFilter.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SongItem data = (sender as Button).DataContext as SongItem;            
            Debug.WriteLine("View song: " + data.Name);
            MostRecentlyViewed = data;
            NavigationService.Navigate(new Uri("/ViewSong.xaml?song=" + data.Name + "&path=" + data.Filepath, UriKind.Relative));
            txtFilter.Text = STR_FILTER_DEFAULT; //Clear the filter when the user selects a song
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //This code will fix it so that when the user comes back to the song list, it is scrolled to the song
            //they were just viewing.
            base.OnNavigatedTo(e);
            if ((e.NavigationMode == System.Windows.Navigation.NavigationMode.Back) && (MostRecentlyViewed != null))
            {
                SongList.UpdateLayout();
                SongList.SelectedItem = MostRecentlyViewed;
                SongList.UpdateLayout();
            }
        }
    }
}