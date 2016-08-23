using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Windows;

namespace SnesBonus.Views {
	public partial class Settings {
		public Settings() {
			InitializeComponent();
			TxtGamesDb.Text = Properties.Settings.Default.GamesDb;
			TxtRomsFolder.Text = Properties.Settings.Default.RomsFolder;
			TxtImagesFolder.Text = Properties.Settings.Default.ImageFolder;
			TxtExecutable.Text = Properties.Settings.Default.ExecutableFile;
			TxtScraperInterval.Text = Properties.Settings.Default.ScraperTimeout + "";
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e){
			Close();
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e){
			Properties.Settings.Default.GamesDb = TxtGamesDb.Text;
			Properties.Settings.Default.RomsFolder = TxtRomsFolder.Text;
			Properties.Settings.Default.ImageFolder = TxtImagesFolder.Text;
			Properties.Settings.Default.ExecutableFile = TxtExecutable.Text;

			int intout;
			if (int.TryParse(TxtScraperInterval.Text, out intout))
				Properties.Settings.Default.ScraperTimeout = intout;

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
				InitialDirectory = System.IO.Path.GetDirectoryName(Properties.SettingsHelper.ExecutableFile)
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
				InitialDirectory = System.IO.Path.GetDirectoryName(Properties.SettingsHelper.GamesDb)
			};
			if (diag.ShowDialog() == true)
				TxtGamesDb.Text = ParsePath(diag.FileNames.First());
			Focus();
		}

		private void RomsFolderBrowse_Click(object sender, RoutedEventArgs e) {
			var diag = new CommonOpenFileDialog {
				IsFolderPicker = true,
				InitialDirectory = Properties.SettingsHelper.RomsFolder
			};
			diag.AddPlace(Environment.CurrentDirectory, 0);
			if (diag.ShowDialog() == CommonFileDialogResult.Ok)
				TxtRomsFolder.Text = ParsePath(diag.FileNames.First());
			Focus();
		}

		private void ImagesFolderBrowse_Click(object sender, RoutedEventArgs e){
			var diag = new CommonOpenFileDialog{
				IsFolderPicker = true,
				InitialDirectory = Properties.SettingsHelper.ImageFolder
			};
			diag.AddPlace(Environment.CurrentDirectory, 0);
			if (diag.ShowDialog() == CommonFileDialogResult.Ok)
				TxtImagesFolder.Text = ParsePath(diag.FileNames.First());
			Focus();
		}
	}
}
