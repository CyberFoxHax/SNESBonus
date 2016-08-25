using System;
using System.Collections.Generic;

namespace SnesBonus.Views {
	public partial class ListEditor {
		public ListEditor() {
			InitializeComponent();
			App.GamesDbChanged += AppOnGamesDbChanged;
			App.RomsDirChanged += AppOnRomsDirChanged;
			DataGrid.LoadingRow += DataGridOnLoadingRow;
			Loaded += (sender, args) =>{
				_scraper.ScrapeBegin += ScraperOnScrapeBegin;
				_scraper.ScrapeEnd += ScraperOnScrapeEnd;
			};

			LblTimer.Content = null;
			LblBanMsg.Visibility = System.Windows.Visibility.Collapsed;
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
		}

		private readonly System.Timers.Timer _timer = new System.Timers.Timer(500);

		~ListEditor() {
			App.GamesDbChanged -= AppOnGamesDbChanged;
			App.RomsDirChanged -= AppOnRomsDirChanged;
			DataGrid.LoadingRow -= DataGridOnLoadingRow;
			_scraper.ScrapeBegin -= ScraperOnScrapeBegin;
			_scraper.ScrapeEnd -= ScraperOnScrapeEnd;
		}

		private Scraper _scraper;
		private List<Models.Game> _games;

		public static ListEditor GetInstance(List<Models.Game> games, Scraper scraper) {
			var editor = new ListEditor {
				DataGrid = { ItemsSource = games },
				_games = games,
				_scraper = scraper
			};
			return editor;
		}
	}
}
