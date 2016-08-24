using System.Windows;
using System.Linq;

namespace SnesBonus.Views {
	public partial class ListEditor {
		private void AppOnRomsDirChanged() {
			var romsFolder = Properties.SettingsHelper.RomsFolder;

			string[] files;
			if (System.IO.Directory.Exists(romsFolder))
				files = System.IO.Directory.GetFiles(romsFolder);
			else
				return;

			var existingNames = _games.Select(p => p.FilePath).ToArray();

			var newFiles = (
				from file in files
				where existingNames.Contains(file) == false
				select new Models.Game {
					FilePath = file,
					Title = file
				}
			).ToList();

			_games.AddRange(newFiles);
			Dispatcher.Invoke(RefreshGrid);
		}

		private static void DataGridOnLoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e) {
			var game = e.Row.DataContext as Models.Game;
			if (game == null)
				return;

			if (System.IO.File.Exists(game.FilePath) == false)
				e.Row.Background = System.Windows.Media.Brushes.OrangeRed;
			else if (System.IO.File.Exists(game.ImagePath) == false)
				e.Row.Background = System.Windows.Media.Brushes.Orange;
			else if(game.IsScraped == false)
				e.Row.Background = System.Windows.Media.Brushes.DeepSkyBlue;
			else
				e.Row.Background = System.Windows.Media.Brushes.White;
		}

		private void AppOnGamesDbChanged() {
			Dispatcher.Invoke(ReloadJsonFromDisk);
		}

		protected override void OnClosed(System.EventArgs e){
			if (_games != null) _games.RemoveAll(p => p == null);
			base.OnClosed(e);
		}

		private void DataGrid_OnSelected(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			LblGameName.Content = game.Title;
			if (string.IsNullOrEmpty(game.ImagePath) == false)
				ImgGameCover.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(game.ImagePath));
		}

		private void BtnMoveUp_Click(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			var newIndex = Move(_games, game, -1);
			RefreshGrid();
			DataGrid.SelectedIndex = newIndex;
		}

		private void BtnMoveDown_Click(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			var newIndex = Move(_games, game, +1);
			RefreshGrid();
			DataGrid.SelectedIndex = newIndex;
		}

		private void BtnRescrape_Click(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			var scraper = new Scraper();
			scraper.QueueGame(game);
		}

		private void BtnEditRaw_Click(object sender, RoutedEventArgs e) {
			var file = Properties.SettingsHelper.GamesDb;

			// todo format json before opening it

			//var jsonData = Utils.LoadGamesFromJson();
			//System.IO.File.WriteAllText(file, CsQuery.Utility.JSON.ToJSON(jsonData));

			var processInfo = new System.Diagnostics.ProcessStartInfo(file) { Verb = "openas" };
			try {
				System.Diagnostics.Process.Start(processInfo);
			}
			catch (System.Exception) {
				processInfo.Verb = null;
				System.Diagnostics.Process.Start(processInfo);
			}
		}
	}
}
