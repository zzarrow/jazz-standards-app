using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JazzChartApp
{
    public class SongItem
    {
        public String Name { get; set; }
        public String Filepath { get; set; }

        private const String CHARTS_DIR_NAME = "chart_files";

        //Comparator for SongItems (alphabetical based on underlying Name field)
        public static int CompareSongs(SongItem a, SongItem b)
        {
            //a is greater = 1, b is greater = -1, equal = 0
            return a.Name.CompareTo(b.Name);
        }

        public static String ParseSongName(String _fullName)
        {
            //Input format: JazzChartApp.chart_files.Blue Moon.pdf
            //Output format: Blue Moon
            int endPoint = _fullName.LastIndexOf('.'); //Non-inclusive
            int startPoint = _fullName.Substring(0, endPoint).LastIndexOf(CHARTS_DIR_NAME + ".") + CHARTS_DIR_NAME.Length + 1; //Inclusive
            //Also covers the case for when a song title contains a .

            return _fullName.Substring(startPoint, endPoint - startPoint);
        }

        //Constructor takes in the filepath, derives and stores the song name for the interface to bind to
        public SongItem(String _filepath)
        {
            Filepath = _filepath;
            Name = ParseSongName(Filepath);
        }
    }
}
