using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NYoutubeDL;
using NYoutubeDL.Models;

namespace GetTube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubeDLP youtubeDl = new();
        private DownloadInfo? videoInfo;

        public MainWindow()
        {
            InitializeComponent();
            youtubeDl.YoutubeDlPath = "C:/Users/da/Downloads/ytdl.exe";
            youtubeDl.Options.GeneralOptions.ExtractorDescriptions = true;
        }

        private void EventFlaticon(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://www.flaticon.com"}") { CreateNoWindow = true });
        }

        private void EventSource(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/ahmadkabdullah/GetTube"}") { CreateNoWindow = true });
        }

        async private void EventGetVideo(object sender, RoutedEventArgs e)
        {
            // reset the text
            videoInfo = null;
            varVidInfo.Opacity = 0.2;
            varStatus.Content = "Fetching video info...";
            varVidTitle.Text = "Title";

            // get the link
            youtubeDl.VideoUrl = varVideoURL.Text;

            // get info using the link
            videoInfo = await youtubeDl.GetDownloadInfoAsync();

            // if info was not found
            if (videoInfo == null)
            {
                varStatus.Content = "Could not retrieve video info...";
            } 
            // otherwise when info was found
            else
            {
                varVidInfo.Opacity = 0.9;
                varVidTitle.Text = videoInfo.Title;
            }
        }

        private void DownloadVideoLow(object sender, RoutedEventArgs e)
        {
            if (videoInfo == null)
            {
                return;
            }
        }

        private void DownloadAudio(object sender, RoutedEventArgs e)
        {
            if (videoInfo == null)
            {
                return;
            }
        }

        private void DownloadVideoMedium(object sender, RoutedEventArgs e)
        {
            if (videoInfo == null)
            {
                return;
            }
        }

        private void DownloadVideoHigh(object sender, RoutedEventArgs e)
        {
            if (videoInfo == null)
            {
                return;
            }
        }
    }
}
