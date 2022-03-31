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
using WPFLocalizeExtension;
using WPFLocalizeExtension.Engine;
using System.Globalization;
using Microsoft.Win32;
using Syroot.Windows.IO;
using YoutubeDLSharp;

namespace GetTube
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string? SelectedTheme;
		private string? SelectedLanguage;

		private string? DownloadsFolder;

		public MainWindow()
		{
			InitializeComponent();

			// check for config's existence
			RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GetTube");
			if (key == null)
			{
				// if first run, create config for reading
				RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GetTube", true);
				newKey.SetValue("Theme", "Light");
				newKey.SetValue("Language", "en-US");
				newKey.Close();
			}

			// read the configuration anyways
			ReadConfig();

			// modify UI according to config
			SetUILang(SelectedLanguage);
			SetUITheme(SelectedTheme);

			// set the downloads folder
			DownloadsFolder = new KnownFolder(KnownFolderType.Downloads).Path;
		}

		private void ReadConfig()
		{
			RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GetTube");
			SelectedLanguage = key.GetValue("Language").ToString();
			SelectedTheme = key.GetValue("Theme").ToString();
			key.Close();
		}

		private void UpdateConfig()
		{
			RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GetTube", true);
			key.SetValue("Language", SelectedLanguage);
			key.SetValue("Theme", SelectedTheme);
			key.Close();
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

			// TODO fetch video info

			// say info was found
			varStatus.Content = Properties.Resources.Found;

			// TODO set video info on ui

			// make buttons clickable
			videoBtn.Click += DownloadVideo;
			audioBtn.Click += DownloadAudio;

			// make section appear
			varVidInfo.Opacity = 0.9;
			varStatus.Content = Properties.Resources.WaitingFormat;
		}

		// Download Options

		async private void DownloadAudio(object sender, RoutedEventArgs e)
		{
			// TODO download the audio

			varStatus.Content = Properties.Resources.DownloadedAudio;
		}

		async private void DownloadVideo(object sender, RoutedEventArgs e)
		{
			// TODO download the audio

			varStatus.Content = Properties.Resources.DownloadedVideo;
		}

		// Pressing Language and Color Buttons

		private void EventLang(object sender, RoutedEventArgs e)
		{
			switch (SelectedLanguage)
			{
				case "en-US":
					SetUILang("ar-IQ");
					secStatus.Content = Properties.Resources.ToKurdish;
					break;
				case "ar-IQ":
					SetUILang("en-US");
					secStatus.Content = Properties.Resources.ToEnglish;
					break;
			}
		}

		private void EventColor(object sender, RoutedEventArgs e)
		{
			// switch theme
			switch (SelectedTheme)
			{
				case "Dark":
					SetUITheme("Light");
					secStatus.Content = Properties.Resources.ToLight;
					break;
				case "Light":
					SetUITheme("Dark");
					secStatus.Content = Properties.Resources.ToDark;
					break;
			}
		}

		private void SetUILang(string language)
		{
			// set another language
			switch (language)
			{
				case "ar-IQ":
					varVidAuthor.FontFamily =
						varVidDuration.FontFamily =
						varVidTitle.FontFamily =
						videoBtn.FontFamily =
						audioBtn.FontFamily =
						secStatus.FontFamily =
						varStatus.FontFamily = new FontFamily("NRT BOLD");
					SelectedLanguage = "ar-IQ";
					break;

				case "en-US":
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

			// update configuration
			UpdateConfig();
		}

		private void SetUITheme(string theme)
		{
			// used colors
			Brush bgCol, fgCol, fgColB;

			// switch theme
			switch (theme)
			{
				default:
				case "Light":
					// light theme colors
					bgCol = Brushes.WhiteSmoke;
					fgCol = new SolidColorBrush(Color.FromArgb(0xFF, 20, 20, 20));
					fgColB = Brushes.Gray;
					SelectedTheme = "Light";
					break;
				case "Dark":
					// dark theme colors
					bgCol = new SolidColorBrush(Color.FromArgb(0xFF, 30, 30, 30));
					fgCol = Brushes.WhiteSmoke;
					fgColB = Brushes.Gray;
					SelectedTheme = "Dark";
					break;
			}

			// finally set all the colors
			container.Background = bgCol;
			varVideoURL.Background = bgCol;
			varVideoURL.Foreground = fgCol;
			varStatus.Foreground = fgCol;
			secStatus.Foreground = fgColB;

			// update configuration
			UpdateConfig();
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

	}
}

class Utility
{
	static public string RemoveSpecialCharacters(string input)
	{
		string output = input;
		var charsToRemove = new string[] { ",", "'", "\"" };
		foreach (var c in charsToRemove)
			output = output.Replace(c, string.Empty);
		return output;
	}
}
