using Microsoft.Win32;
using Syroot.Windows.IO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WPFLocalizeExtension.Engine;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace GetTube;

public partial class MainWindow : Window
{
	private string? SelectedTheme;
	private string? SelectedLanguage;
	private readonly string? DownloadsFolder;

	private YoutubeDL? ytClient;
	private string? ytUrl;
	private RunResult<VideoData>? ytRes;
	private Progress<DownloadProgress>? ytProg;
	private CancellationTokenSource? ytCancel;
	private bool ytCanCancel;

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

		// setup downloading with youtubedlsharp
		SetupDownloader();
	}

	// Setting downloading lib

	private void SetupDownloader()
	{
		// make downloader client
		ytClient = new();

		// set downloader paths
		ytClient.YoutubeDLPath = DownloadsFolder + "\\" + "youtube-dl.exe";
		ytClient.FFmpegPath = DownloadsFolder + "\\" + "ffmpeg.exe";
		ytClient.OutputFolder = DownloadsFolder;

		// set cancellation
		ytCancel = new CancellationTokenSource();

		// set downloader prog element
		ytProg = new(
			p => uiStatus.Content =
			Properties.Resources.Downloading + " " +
			Utils.FloatToStr(p.Progress));
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

	// Fetching Info And Downloading 

	async private void EventGetVideo(object sender, RoutedEventArgs e)
	{
		// delete the info of the previous video
		ClearUIInfo();

		// say fetching and fetch info of new video
		uiStatus.Content = Properties.Resources.Fetching;
		ytRes = await ytClient.RunVideoDataFetch(uiURLBox.Text);

		// check for no info found
		if (ytRes == null)
		{
			uiStatus.Content = Properties.Resources.NotFound;
			return;
		}

		// save link
		ytUrl = uiURLBox.Text;

		// say info was found and set on ui
		uiStatus.Content = Properties.Resources.Found;
		ResetUIInfo();

		// make buttons clickable
		uiVideoBtn.Click += DownloadVideo;
		uiAudioBtn.Click += DownloadAudio;

		// make section appear
		uiVidBox.Opacity = 0.9;
		uiStatus.Content = Properties.Resources.WaitingFormat;
	}

	async private void DownloadAudio(object sender, RoutedEventArgs e)
	{
		// cancel previous download
		if (ytCanCancel)
			ytCancel.Cancel();

		// able to cancel task
		ytCanCancel = true;

		// download while reporting progress
		var result = await ytClient.RunAudioDownload(
			ytUrl,
			AudioConversionFormat.Mp3,
			progress: ytProg,
			ct: ytCancel.Token);

		// report finishing
		if (result.Success)
			uiStatus.Content = Properties.Resources.DownloadedAudio;

		// can't cancel task
		ytCanCancel = false;
	}

	async private void DownloadVideo(object sender, RoutedEventArgs e)
	{
		// cancel previous download
		if (ytCanCancel)
			ytCancel.Cancel();

		// able to cancel task
		ytCanCancel = true;

		// download while reporting progress
		var result = await ytClient.RunVideoDownload(
			ytUrl,
			progress: ytProg,
			ct: ytCancel.Token);

		// report finishing
		if (result.Success)
			uiStatus.Content = Properties.Resources.DownloadedVideo;

		// can't cancel task
		ytCanCancel = false;
	}

	// Bottom bar events

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

	private void EventFlaticon(object sender, RoutedEventArgs e)
	{
		uiSecStatus.Content = Properties.Resources.Flaticon;
	}

	private void EventSource(object sender, RoutedEventArgs e)
	{
		uiSecStatus.Content = Properties.Resources.Github;
		Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/ahmadkabdullah/GetTube"}") { CreateNoWindow = true });
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
			uiSecStatus.Content = Properties.Resources.ToEnglish;
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
			uiSecStatus.Content = Properties.Resources.ToKurdish;
		}

		// switch to set language
		LocalizeDictionary.Instance.Culture = new CultureInfo(SelectedLanguage);

		// update configuration
		UpdateConfig();

		// reset the ui info as it was cleared by new culture
		ResetUIInfo();
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

	private void ClearUIInfo()
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

	private void ResetUIInfo()
	{
		if (ytRes == null)
			return;

		uiVidTitle.Text = ytRes.Data.Title;
		uiVidAuthor.Text = ytRes.Data.Uploader;
		uiVidDuration.Text = ytRes.Data.Duration.ToString();
	}
}

class Utils
{
	public static string RemoveSpecialCharacters(string input)
	{
		string output = input;
		var charsToRemove = new string[] { ",", "'", "\"" };
		foreach (var c in charsToRemove)
			output = output.Replace(c, string.Empty);

		return output;
	}

	public static string FloatToStr(float num)
	{
		var str = string.Format("{0:N5}", num);
		if (str.Length <= 1)
			return str;
		else
			return str[2].ToString() + str[3].ToString() + "." + str[4].ToString();
	}
}
