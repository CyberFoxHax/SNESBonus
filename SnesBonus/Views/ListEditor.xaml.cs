using System.Collections.Generic;

namespace SnesBonus.Views {
	public partial class ListEditor {
		public ListEditor() {
			InitializeComponent();
			App.GamesDbChanged += AppOnGamesDbChanged;
			App.RomsDirChanged += AppOnRomsDirChanged;
			DataGrid.LoadingRow += DataGridOnLoadingRow;
		}

		~ListEditor() {
			App.GamesDbChanged -= AppOnGamesDbChanged;
			App.RomsDirChanged -= AppOnRomsDirChanged;
		}

		private Scraper _scraper;
		private List<Models.Game> _games;

		public static ListEditor OpenEditor(List<Models.Game> games, Scraper scraper) {
			var editor = new ListEditor {
				DataGrid = { ItemsSource = games },
				_games = games,
				_scraper = scraper
			};
			return editor;
		}
	}
}
