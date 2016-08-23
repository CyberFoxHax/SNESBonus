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

			if (_games == null) return;
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
			Dispatcher.Invoke(RefreshGamesList);
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
			System.IO.File.WriteAllText(GamesDb, CsQuery.Utility.JSON.ToJSON(_games));
		}

		private void ReloadList_Click(object sender, System.Windows.RoutedEventArgs e){
			LoadGamesList();
			RefreshGamesList();
		}

		private void EditIndexes_Click(object sender, System.Windows.RoutedEventArgs e){
			var editor = ListEditor.OpenEditor(_games);
			editor.Closed += delegate{
				RefreshGamesList();
				Show();
			};
			Hide();
			editor.Show();
		}

		private void ChangeDirectories_Click(object sender, System.Windows.RoutedEventArgs e){
			new Settings().Show();
		}
	}
}
