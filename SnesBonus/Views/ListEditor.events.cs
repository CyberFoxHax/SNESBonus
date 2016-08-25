using System.Windows;
using System.Linq;
using System;

namespace SnesBonus.Views {
	public partial class ListEditor {
		private void AppOnRomsDirChanged() {
			var newGames = Utils.CheckRomsDirForNew(ref _games);
			if(newGames.Any()){
				Dispatcher.Invoke(RefreshGrid);
				if (Properties.Settings.Default.AutoScrape)
					_scraper.QueueGames(newGames);
			}
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

		private void TimerOnElapsed(object s, System.Timers.ElapsedEventArgs elapsedEventArgs) {
			Func<TimeSpan> calcRemainder;
			bool isBanned;
			if (Properties.Settings.Default.BanTimestamp != default(DateTime)){
				isBanned = true;
				calcRemainder = () =>
					DateTime.Now - Properties.Settings.Default.BanTimestamp.AddHours(24);
			}
			else if (Scraper.LastScrapeTime != default(DateTime)){
				isBanned = false;
				calcRemainder = () =>
					DateTime.Now - Scraper.LastScrapeTime.AddMilliseconds(Properties.Settings.Default.ScraperTimeout);
			}
			else{
				isBanned = false;
				calcRemainder = () => new TimeSpan(0);
			}
			Dispatcher.Invoke(() =>{
				LblBanMsg.Visibility     = isBanned			? Visibility.Visible : Visibility.Collapsed;
				LblTimeoutMsg.Visibility = isBanned==false	? Visibility.Visible : Visibility.Collapsed;

				var remainder = calcRemainder();
				if(remainder.TotalMilliseconds>0)
					LblTimer.Content = "Ready";
				else
					LblTimer.Content = remainder.ToString().Split('.')[0];
			});
		}

		protected override void OnClosed(EventArgs e){
			if (_games != null) _games.RemoveAll(p => p == null);
			_timer.Stop();
			_timer.Dispose();
			DataGrid.IsReadOnly = true;
			base.OnClosed(e);
		}

		private void DataGrid_OnSelected(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			LblGameName.Content = game.Title;
			if (string.IsNullOrEmpty(game.ImagePath) == false)
				ImgGameCover.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(game.ImagePath));
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

		private async void BtnRescrape_Click(object sender, RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			IsEnabled = false;

			var newResult = await Scraper.GetSearchResult(game.Title);
			if (newResult == null && Properties.Settings.Default.BanTimestamp != default(DateTime)){
				IsEnabled = true;
				return;
			}
			var pickerResult = await ResultPicker.AwaitPick(newResult);
			if (pickerResult != null){
				var result = await Scraper.GetGameDetails(pickerResult.Href);

				var imagePath = Properties.SettingsHelper.ImageFolder + result.ImagePath.CleanFileName();
				if (System.IO.File.Exists(imagePath))
					game.ImagePath = imagePath;
				else if (string.IsNullOrEmpty(result.ImagePath) == false)
					game.ImagePath = await Scraper.DownloadImage(result.ImagePath);

				result.CopyTo(game);
				RefreshGrid();
			}

			IsEnabled = true;
		}

		private void BtnEditRaw_Click(object sender, RoutedEventArgs e) {
			var file = Properties.SettingsHelper.GamesDb;

			var text = CsQuery.Utility.JSON.ToJSON(Models.Game.LoadGamesFromJson());
			System.IO.File.WriteAllText(file, Lib.JsonHelper.FormatJson(text));

			var processInfo = new System.Diagnostics.ProcessStartInfo(file) { Verb = "openas" };
			try {
				System.Diagnostics.Process.Start(processInfo);
			}
			catch (Exception) {
				processInfo.Verb = null;
				System.Diagnostics.Process.Start(processInfo);
			}
		}

		private void BanMsg_OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e){
			System.Diagnostics.Process.Start("http://gamefaqs.com/");
		}

		private void ScraperOnScrapeBegin(){
			Dispatcher.Invoke(() =>{
				IsEnabled = false;
				DataGrid.CommitEdit();
			});
		}

		private void ScraperOnScrapeEnd(Models.Game game) {
			Dispatcher.Invoke(() =>{
				RefreshGrid();
				IsEnabled = true;
			});
			App.GamesDbChanged -= AppOnGamesDbChanged;
			System.IO.File.WriteAllText(Properties.SettingsHelper.GamesDb, CsQuery.Utility.JSON.ToJSON(_games));
			App.GamesDbChanged += AppOnGamesDbChanged;
		}
	}
}
