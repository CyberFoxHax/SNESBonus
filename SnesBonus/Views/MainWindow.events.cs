using System;
using System.Linq;

namespace SnesBonus.Views {
	public partial class MainWindow {

		private void AppOnRomsDirChanged() {
			var romsFolder = Properties.SettingsHelper.RomsFolder;

			string[] files;
			if (System.IO.Directory.Exists(romsFolder))
				files = System.IO.Directory.GetFiles(romsFolder);
			else
				return;

			Func<string, Models.Game> convert = p => 
				new Models.Game{
					FilePath = p,
					Title = Models.Game.CleanGameName(p)
				};

			System.Collections.Generic.List<Models.Game> newGames;
			if (_games != null) {
				var existingNames = _games.Select(p => p.FilePath).ToArray();
				newGames = (
					from file in files
					where existingNames.Contains(file) == false
					select convert(file)
					).ToList();
				_games.AddRange(newGames);
			}
			else{
				newGames = files.Select(convert).ToList();
				_games = newGames;
			}

			Dispatcher.Invoke(RefreshGamesList);
			_scraper.QueueGames(newGames);
		}

		private void AppOnGamesDbChanged() {
			LoadGamesList();
			RefreshGamesList();
		}

		private void EditMenu_OnClick(object sender, System.Windows.RoutedEventArgs e){
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;


			var editor = EditGame.OpenEditor(game);
			editor.GameSaved += EditGameOnGameSaved;
		}

		private async void DeleteMenu_OnClick(object sender, System.Windows.RoutedEventArgs e) {
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			if (await Confirm.AwaitResult(this) == false) return;
			_games.Remove(game);
			RefreshGamesList();
		}

		private void DataGrid_OnPick(object sender, EventArgs e) {
			var keyEvt = e as System.Windows.Input.KeyEventArgs;
			if (keyEvt != null && keyEvt.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.Enter) == false)
				return;

			OpenGame();
		}

		protected override void OnClosed(EventArgs e) {
			System.Windows.Application.Current.Shutdown();
			base.OnClosed(e);
		}

		private void SaveList_Click(object sender, System.Windows.RoutedEventArgs e){
			App.GamesDbChanged -= AppOnGamesDbChanged;
			System.IO.File.WriteAllText(GamesDb, CsQuery.Utility.JSON.ToJSON(_games));
			App.GamesDbChanged += AppOnGamesDbChanged;
		}

		private void ReloadList_Click(object sender, System.Windows.RoutedEventArgs e){
			LoadGamesList();
			RefreshGamesList();
		}

		private void OpenEditor_Click(object sender, System.Windows.RoutedEventArgs e){
			var editor = ListEditor.OpenEditor(_games, _scraper);
			editor.Closed += delegate{
				RefreshGamesList();
				Show();
			};
			Hide();
			editor.Show();
		}

		private void Settings_Click(object sender, System.Windows.RoutedEventArgs e){
			new Settings().Show();
		}

		private void ScraperOnScrapeEnd(Models.Game game) {
			Dispatcher.Invoke(RefreshGamesList);
		}
	}
}
