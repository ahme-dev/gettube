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
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace GetTube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubeClient youtube = new();
        private StreamManifest? streamManifest;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EventFlaticon(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://www.flaticon.com"}") { CreateNoWindow = true });
        }

        private void EventSource(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/ahmadkabdullah/GetTube"}") { CreateNoWindow = true });
        }

        // Get the video

        async private void EventGetVideo(object sender, RoutedEventArgs e)
        {
            // delete the info of the previous video
            ResetVideoInfoUI();

            // say fetching and do fetch
            varStatus.Content = "Fetching information...";
            var video = await youtube.Videos.GetAsync(varVideoURL.Text);

            // if video not found 
            if (video == null)
            {
                varStatus.Content = "Video info not found...";
            }
            // otherwise if found
            else
            {
                // say info was found and make section appear
                varVidInfo.Opacity = 0.9;
                varStatus.Content = "Video info found. Waiting for quality selection...";

                // set the values for preview from url
                varVidTitle.Text = video.Title;
                varVidAuthor.Text = video.Author.Title;
                varVidDuration.Text = video.Duration.ToString();
                
                // get a list of streams
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(varVideoURL.Text);
            }
        }

        // Download Video Quality Options

        private void DownloadVideoLow(object sender, RoutedEventArgs e)
        {
        }

        async private void DownloadAudio(object sender, RoutedEventArgs e)
        {
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
        }

        private void DownloadVideoMedium(object sender, RoutedEventArgs e)
        {
        }

        async private void DownloadVideoHigh(object sender, RoutedEventArgs e)
        {
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
        }

        private void ResetVideoInfoUI()
        {
                varVidInfo.Opacity = 0.2;
                varVidTitle.Text = "Title";
                varVidAuthor.Text = "Author";
                varVidDuration.Text = "Duration";
        }
    }
}
