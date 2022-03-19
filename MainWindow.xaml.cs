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
using WPFLocalizeExtension.Engine;
using System.Globalization;

namespace GetTube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubeClient youtube = new();
        private StreamManifest? streamManifest;
        private string SelectedTheme;
        private string SelectedLanguage;

        public MainWindow()
        {
            InitializeComponent();

            // set theme and language
            SwitchUILang();
            SwitchUITheme();

            // retrieve configuration

            // set language at start up
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = new CultureInfo(SelectedLanguage);
        }

        private void EventFlaticon(object sender, RoutedEventArgs e)
        {
            secStatus.Content = "Thanks to flaticon.com for the icons";
        }

        private void EventSource(object sender, RoutedEventArgs e)
        {
            secStatus.Content = "You're checking the source code, oh no!";
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

        private void EventLang(object sender, RoutedEventArgs e)
        {
            SwitchUILang();

            switch (SelectedLanguage)
            {
                case "en-US":
                    secStatus.Content = "Language is now English!";
                    break;
                case "ar-IQ":
                    secStatus.Content = "سڵاو لە کوردزمان";
                    break;
            }
        }

        private void EventColor(object sender, RoutedEventArgs e)
        {
            SwitchUITheme();

            // switch theme
            switch (SelectedTheme)
            {
                case "Dark":
                    secStatus.Content = "It's getting dark out here!";
                    break;
                case "Light":
                    secStatus.Content = "It's bright again";
                    break;
            }
        }

        // to be run after a new link is given
        private void ResetVideoInfoUI()
        {
            varVidInfo.Opacity = 0.2;
            varVidTitle.Text = "{lex:}";
            varVidAuthor.Text = "Author";
            varVidDuration.Text = "Duration";

            // make buttons not clickable
            videoBtn.Click -= DownloadVideo;
            audioBtn.Click -= DownloadAudio;
        }
        
        private void SwitchUILang()
        {
            // set another language
            switch (SelectedLanguage)
            {
                case "en-US":
                    varVidAuthor.FontFamily =
                        varVidDuration.FontFamily =
                        varVidTitle.FontFamily =
                        videoBtn.FontFamily =
                        audioBtn.FontFamily = 
                        secStatus.FontFamily =
                        varStatus.FontFamily = new FontFamily("NRT BOLD");
                    SelectedLanguage = "ar-IQ";
                        break;

                default:
                case "ar-IQ":
                    varVidAuthor.FontFamily =
                        varVidDuration.FontFamily =
                        varVidTitle.FontFamily =
                        videoBtn.FontFamily =
                        audioBtn.FontFamily =
                        secStatus.FontFamily =
                        varStatus.FontFamily = new FontFamily("Fira Sans");
                    SelectedLanguage = "en-US";
                        break;
                }

                // switch to set language
                LocalizeDictionary.Instance.Culture = new CultureInfo(SelectedLanguage);
        }

        private void SwitchUITheme()
        {
            // used colors
            Brush bgCol, fgCol, fgColB;

            // switch theme
            switch (SelectedTheme)
            {
                default:
                case "Dark":
                    // light theme colors
                    bgCol = Brushes.WhiteSmoke;
                    fgCol = Brushes.Black;
                    fgColB = Brushes.Gray;
                    SelectedTheme = "Light";
                    break;
                case "Light":
                    // dark theme colors
                    bgCol = new SolidColorBrush(Color.FromArgb(0xFF, 30, 30, 30));
                    fgCol = Brushes.WhiteSmoke;
                    fgColB = new SolidColorBrush(Color.FromArgb(0xFF, 140, 140, 140));
                    SelectedTheme = "Dark";
                    break;
            }

            // finally set all the colors
            varVideoURL.Background = bgCol;
            varVideoURL.Foreground = fgCol;
            container.Background = bgCol;
            varStatus.Foreground = fgCol;
            secStatus.Foreground = fgColB;
        }
    }
}
