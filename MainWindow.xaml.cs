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
using WPFLocalizeExtension;

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
                varStatus.Content = "Video info not found!";
            }
            // otherwise if found
            else
            {
                varStatus.Content = "Video info found!";

                // set the values for preview from url
                varVidTitle.Text = video.Title;
                varVidAuthor.Text = video.Author.Title;
                varVidDuration.Text = video.Duration.ToString();
                
                // get a list of streams
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(varVideoURL.Text);

                // make buttons clickable
                videoBtn.Click += DownloadVideo;
                audioBtn.Click += DownloadAudio;

                // say info was found and make section appear
                varVidInfo.Opacity = 0.9;
                varStatus.Content = "Waiting for quality selection...";
            }
        }

        // Download Options

        async private void DownloadAudio(object sender, RoutedEventArgs e)
        {
            // Get audio stream
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // Stop button from working
            varStatus.Content = "Now downloading...";
            audioBtn.Click -= DownloadAudio;

            // Download and notify after
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
            varStatus.Content = "Audio was downloaded!";
        }

        async private void DownloadVideo(object sender, RoutedEventArgs e)
        {
            // Get mixed stream 
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            // Stop button from working
            varStatus.Content = "Now downloading...";
            videoBtn.Click -= DownloadVideo;

            // Download and notify after
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
            varStatus.Content = "Video was downloaded!";
        }

        private void ResetVideoInfoUI()
        {
                varVidInfo.Opacity = 0.2;
                varVidTitle.Text = "Title";
                varVidAuthor.Text = "Author";
                varVidDuration.Text = "Duration";

                // make buttons not clickable
                videoBtn.Click -= DownloadVideo;
                audioBtn.Click -= DownloadAudio;
        }
    }
}
