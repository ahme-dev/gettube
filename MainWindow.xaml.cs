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
            secStatus.Content = Properties.Resources.Flaticon;
        }

        private void EventSource(object sender, RoutedEventArgs e)
        {
            secStatus.Content = Properties.Resources.Github;
            Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/ahmadkabdullah/GetTube"}") { CreateNoWindow = true });
        }

        // Get the video
        
        async private void EventGetVideo(object sender, RoutedEventArgs e)
        {
            // delete the info of the previous video
            ResetVideoInfoUI();

            // say fetching and do fetch
            varStatus.Content = Properties.Resources.Fetching;

            YoutubeExplode.Videos.Video? video;

            try
            {
                video = await youtube.Videos.GetAsync(varVideoURL.Text);
            }
            catch
            {
                video = null;
            }

            // if video not found 
            if (video == null)
            {
                varStatus.Content = Properties.Resources.NotFound;
            }
            // otherwise if found
            else
            {
                // say info was found
                varStatus.Content = Properties.Resources.Found;

                // set the values for preview from url
                varVidTitle.Text = video.Title;
                varVidAuthor.Text = video.Author.Title;
                varVidDuration.Text = video.Duration.ToString();
                
                // get a list of streams
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(varVideoURL.Text);

                // make buttons clickable
                videoBtn.Click += DownloadVideo;
                audioBtn.Click += DownloadAudio;

                // make section appear
                varVidInfo.Opacity = 0.9;
                varStatus.Content = Properties.Resources.WaitingFormat;
            }
        }

        // Download Options

        async private void DownloadAudio(object sender, RoutedEventArgs e)
        {
            // Get audio stream
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // Stop button from working
            varStatus.Content = Properties.Resources.Downloading;
            audioBtn.Click -= DownloadAudio;

            // Download and notify after
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
            varStatus.Content = Properties.Resources.DownloadedAudio;
        }

        async private void DownloadVideo(object sender, RoutedEventArgs e)
        {
            // Get mixed stream 
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            // Stop button from working
            varStatus.Content = Properties.Resources.Downloading;
            videoBtn.Click -= DownloadVideo;

            // Download and notify after
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{varVidTitle.Text}.{streamInfo.Container}");
            varStatus.Content = Properties.Resources.DownloadedVideo;
        }

        private void EventLang(object sender, RoutedEventArgs e)
        {
            SwitchUILang();

            switch (SelectedLanguage)
            {
                case "en-US":
                    secStatus.Content = Properties.Resources.ToEnglish;
                    break;
                case "ar-IQ":
                    secStatus.Content = Properties.Resources.ToKurdish;
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
                    secStatus.Content = Properties.Resources.ToDark;
                    break;
                case "Light":
                    secStatus.Content = Properties.Resources.ToLight;
                    break;
            }
        }

        // to be run after a new link is given
        private void ResetVideoInfoUI()
        {
            varVidInfo.Opacity = 0.2;
            varVidTitle.Text = Properties.Resources.VidTitle;
            varVidAuthor.Text = Properties.Resources.VidAuthor;
            varVidDuration.Text = Properties.Resources.VidDuration;

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
