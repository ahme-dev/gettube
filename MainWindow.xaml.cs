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

namespace GetTube;

public partial class MainWindow : Window
{
	private string? SelectedTheme;
	private string? SelectedLanguage;
	private string? DownloadsFolder;

	public MainWindow()
	{
		InitializeComponent();

		// deal with configuration
		ReadConfig();

		// modify UI according to config
		SetUILang(SelectedLanguage);
		SetUITheme(SelectedTheme);

		// set the downloads folder
		DownloadsFolder = new KnownFolder(KnownFolderType.Downloads).Path;
	}

	// Configuration management

	private void ReadConfig()
	{
		RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GetTube");
		if (key == null)
		{
			CreateConfig();
			ReadConfig();
			return;
		}
		SelectedLanguage = key.GetValue("Language").ToString();
		SelectedTheme = key.GetValue("Theme").ToString();
		key.Close();
	}
	private void CreateConfig()
	{
		RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\GetTube", true);
		newKey.SetValue("Theme", "Light");
		newKey.SetValue("Language", "en-US");
		newKey.Close();
	}

	private void UpdateConfig()
	{
		RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GetTube", true);
		key.SetValue("Language", SelectedLanguage);
		key.SetValue("Theme", SelectedTheme);
		key.Close();
	}

	// Source and Thanks Buttons

	private void EventFlaticon(object sender, RoutedEventArgs e)
	{
		uiSecStatus.Content = Properties.Resources.Flaticon;
	}

	private void EventSource(object sender, RoutedEventArgs e)
	{
		uiSecStatus.Content = Properties.Resources.Github;
		Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/ahmadkabdullah/GetTube"}") { CreateNoWindow = true });
	}

	// Fetching Info And Downloading 

	async private void EventGetVideo(object sender, RoutedEventArgs e)
	{
		// delete the info of the previous video
		ResetVideoInfoUI();

		// say fetching and do fetch
		uiStatus.Content = Properties.Resources.Fetching;

		// TODO fetch video info

		// say info was found
		uiStatus.Content = Properties.Resources.Found;

		// TODO set video info on ui

		// make buttons clickable
		uiVideoBtn.Click += DownloadVideo;
		uiAudioBtn.Click += DownloadAudio;

		// make section appear
		uiVidBox.Opacity = 0.9;
		uiStatus.Content = Properties.Resources.WaitingFormat;
	}

	async private void DownloadAudio(object sender, RoutedEventArgs e)
	{
		// TODO download the audio

		uiStatus.Content = Properties.Resources.DownloadedAudio;
	}

	async private void DownloadVideo(object sender, RoutedEventArgs e)
	{
		// TODO download the audio

		uiStatus.Content = Properties.Resources.DownloadedVideo;
	}

	// Language and Color Buttons

	private void EventLang(object sender, RoutedEventArgs e)
	{
		if (SelectedLanguage == "en-US")
			SetUILang("ar-IQ");
		else
			SetUILang("en-US");
	}

	private void EventColor(object sender, RoutedEventArgs e)
	{
		if (SelectedTheme == "Dark")
			SetUITheme("Light");
		else
			SetUITheme("Dark");
	}

	// Changing the UI

	private void SetUILang(string language)
	{
		if (language == "ar-IQ")
		{
			uiVidAuthor.FontFamily =
				uiVidDuration.FontFamily =
				uiVidTitle.FontFamily =
				uiVideoBtn.FontFamily =
				uiAudioBtn.FontFamily =
				uiSecStatus.FontFamily =
				uiStatus.FontFamily = new FontFamily("NRT BOLD");
			SelectedLanguage = "ar-IQ";
			uiSecStatus.Content = Properties.Resources.ToKurdish;
		}
		else
		{
			uiVidAuthor.FontFamily =
				uiVidDuration.FontFamily =
				uiVidTitle.FontFamily =
				uiVideoBtn.FontFamily =
				uiAudioBtn.FontFamily =
				uiSecStatus.FontFamily =
				uiStatus.FontFamily = new FontFamily("Fira Sans");
			SelectedLanguage = "en-US";
			uiSecStatus.Content = Properties.Resources.ToEnglish;
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

		if (theme == "Light")
		{

			// light theme colors
			bgCol = Brushes.WhiteSmoke;
			fgCol = new SolidColorBrush(Color.FromArgb(0xFF, 20, 20, 20));
			fgColB = Brushes.Gray;
			SelectedTheme = "Light";
			uiSecStatus.Content = Properties.Resources.ToLight;
		}
		else
		{
			// dark theme colors
			bgCol = new SolidColorBrush(Color.FromArgb(0xFF, 30, 30, 30));
			fgCol = Brushes.WhiteSmoke;
			fgColB = Brushes.Gray;
			SelectedTheme = "Dark";
			uiSecStatus.Content = Properties.Resources.ToDark;
		}

		// finally set all the colors
		uiContainer.Background = bgCol;
		uiURLBox.Background = bgCol;
		uiURLBox.Foreground = fgCol;
		uiStatus.Foreground = fgCol;
		uiSecStatus.Foreground = fgColB;

		// update configuration
		UpdateConfig();
	}

	private void ResetVideoInfoUI()
	{
		// reset elements after new link is given
		uiVidBox.Opacity = 0.2;
		uiVidTitle.Text = Properties.Resources.VidTitle;
		uiVidAuthor.Text = Properties.Resources.VidAuthor;
		uiVidDuration.Text = Properties.Resources.VidDuration;

		// make buttons not clickable
		uiVideoBtn.Click -= DownloadVideo;
		uiAudioBtn.Click -= DownloadAudio;
	}
}

class GTUtility
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
