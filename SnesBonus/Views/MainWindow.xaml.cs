using System;
using System.Collections.Generic;

namespace SnesBonus.Views {
	public partial class MainWindow{

		private static readonly string GamesDb = SettingsHelper.GamesDb;
		private static readonly string ImageFolder = SettingsHelper.ImageFolder;
		private static readonly string ProcessFile = SettingsHelper.ExecutableFile;

		private List<Models.Game> _games = new List<Models.Game>();
		private readonly Scraper _scraper = new Scraper();

		public MainWindow() {
			if (System.IO.Directory.Exists(ImageFolder) == false)
				System.IO.Directory.CreateDirectory(ImageFolder);

			LoadGamesList();
			InitializeComponent();

			DataGrid.Items.Clear();
			DataGrid.ItemsSource = _games;
			DataGrid.Focus();

			App.GamesDbChanged += AppOnGamesDbChanged;
			App.RomsDirChanged += AppOnRomsDirChanged;

			AppOnRomsDirChanged();
			_scraper.ScrapeEnd += ScraperOnScrapeEnd;
			_scraper.StartScraping();

			DataGrid.Focus();
		}

		public new void Hide() {
			_scraper.ScrapeEnd -= ScraperOnScrapeEnd;
			App.GamesDbChanged -= AppOnGamesDbChanged;
			App.RomsDirChanged -= AppOnRomsDirChanged;
			base.Hide();
		}

		public new void Show() {
			_scraper.ScrapeEnd -= ScraperOnScrapeEnd;
			App.GamesDbChanged -= AppOnGamesDbChanged;
			App.RomsDirChanged -= AppOnRomsDirChanged;

			_scraper.ScrapeEnd += ScraperOnScrapeEnd;
			App.GamesDbChanged += AppOnGamesDbChanged;
			App.RomsDirChanged += AppOnRomsDirChanged;

			base.Show();
			RefreshGamesList();
		}

		public void LoadGamesList(){
			_games = Models.Game.LoadGamesFromJson();
		}

		private void RefreshGamesList(){
			DataGrid.ItemsSource = null;
			DataGrid.ItemsSource = _games;
		}

		private System.Diagnostics.Process _snesProcess;
		private void OpenGame(){
			var game = DataGrid.SelectedItem as Models.Game;
			if (game == null) return;

			var processStartInfo = new System.Diagnostics.ProcessStartInfo(
				ProcessFile,
				string.Format("\"{0}\"", game.FullFilePath) + " -fullscreen"
			);

			var process = System.Diagnostics.Process.Start(processStartInfo);
			if (process == null) return;
			_snesProcess = process;
			AppDomain.CurrentDomain.UnhandledException += OnException;
			process.EnableRaisingEvents = true;
			Hide();
			var mousePos = new Win32Point();
			GetCursorPos(ref mousePos);
			SetCursorPos((int) System.Windows.SystemParameters.PrimaryScreenWidth, 100);
            Environment.Exit(0);
			process.Exited += (o, args) =>{
				Dispatcher.Invoke(Show);
				SetCursorPos(mousePos.X, mousePos.Y);
				AppDomain.CurrentDomain.UnhandledException -= OnException;
			};
		}

		[System.Runtime.InteropServices.DllImport("User32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		internal struct Win32Point {
			public int X;
			public int Y;
		};

		~MainWindow(){
			App.GamesDbChanged -= AppOnGamesDbChanged;
		}
	}
}
