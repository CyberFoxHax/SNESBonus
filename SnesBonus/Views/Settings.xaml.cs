using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Windows;

namespace SnesBonus.Views {
	public partial class Settings {
		private static readonly string Path = System.Environment.CurrentDirectory;

	    private static string ParseIn(string inStr) {
	        return inStr.Replace(Path, "{this}");
	    }

		public Settings() {
            InitializeComponent();
			TxtGamesDb.Text			= ParseIn(SettingsHelper.GamesDb);
			TxtRomsFolder.Text		= ParseIn(SettingsHelper.RomsFolder);
			TxtImagesFolder.Text	= ParseIn(SettingsHelper.ImageFolder);
			TxtExecutable.Text		= ParseIn(SettingsHelper.ExecutableFile);
			TxtScraperInterval.Text = SettingsHelper.ScraperTimeout + "";
			BoolAutoScrape.IsChecked= SettingsHelper.AutoScrape;
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e){
			Close();
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e){
			SettingsHelper.GamesDb			= TxtGamesDb.Text;
			SettingsHelper.RomsFolder		= TxtRomsFolder.Text;
			SettingsHelper.ImageFolder		= TxtImagesFolder.Text;
			SettingsHelper.ExecutableFile	= TxtExecutable.Text;
			SettingsHelper.AutoScrape		= BoolAutoScrape.IsChecked==true;
			SettingsHelper.Save();
			int intout;
			if (int.TryParse(TxtScraperInterval.Text, out intout))
                SettingsHelper.ScraperTimeout = intout;

			Properties.Settings.Default.Save();
			Close();
		}

		private static string ParsePath(string str){
			return str.Replace(Environment.CurrentDirectory, "{this}");
		}

		private static readonly List<FileDialogCustomPlace> CustomPlaces = new List<FileDialogCustomPlace>{
				new FileDialogCustomPlace(Environment.CurrentDirectory)
			};

		private void ExecutableBrowse_Click(object sender, RoutedEventArgs e){
			var diag = new OpenFileDialog{
				Filter = "Bacon|*.exe",
				CustomPlaces = CustomPlaces,
				InitialDirectory = System.IO.Path.GetDirectoryName(SettingsHelper.ExecutableFile)
			};
			if (diag.ShowDialog() == true){
				TxtExecutable.Text = ParsePath(diag.FileNames.First());
			}
			Focus();
		}

		private void GamesDbBrowse_Click(object sender, RoutedEventArgs e) {
			var diag = new OpenFileDialog{
				Filter = "bacon|*.json",
				CustomPlaces = CustomPlaces,
				InitialDirectory = System.IO.Path.GetDirectoryName(SettingsHelper.GamesDb)
			};
			if (diag.ShowDialog() == true)
				TxtGamesDb.Text = ParsePath(diag.FileNames.First());
			Focus();
		}

		private void RomsFolderBrowse_Click(object sender, RoutedEventArgs e) {
			var diag = new CommonOpenFileDialog {
				IsFolderPicker = true,
				InitialDirectory = SettingsHelper.RomsFolder
			};
			diag.AddPlace(Environment.CurrentDirectory, 0);
			if (diag.ShowDialog() == CommonFileDialogResult.Ok)
				TxtRomsFolder.Text = ParsePath(diag.FileNames.First());
			Focus();
		}

		private void ImagesFolderBrowse_Click(object sender, RoutedEventArgs e){
			var diag = new CommonOpenFileDialog{
				IsFolderPicker = true,
				InitialDirectory = SettingsHelper.ImageFolder
			};
			diag.AddPlace(Environment.CurrentDirectory, 0);
			if (diag.ShowDialog() == CommonFileDialogResult.Ok)
				TxtImagesFolder.Text = ParsePath(diag.FileNames.First());
			Focus();
		}

		private void ThisFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e){
			System.Diagnostics.Process.Start(Environment.CurrentDirectory);
		}
	}
}
