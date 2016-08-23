using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SnesBonus.Views {
	public partial class MainWindow{

		private static readonly string GamesDb = Properties.SettingsHelper.GamesDb;
		private static readonly string ImageFolder = Properties.SettingsHelper.ImageFolder;
		private static readonly string ProcessFile = Properties.SettingsHelper.ExecutableFile;

		private List<Models.Game> _games = new List<Models.Game>();

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
		}

		public void LoadGamesList(){
			_games = Utils.LoadGamesFromJson();
		}

		private void EditGameOnGameSaved(Models.Game game){
			RefreshGamesList();
		}

		private void RefreshGamesList(){
			Dispatcher.Invoke(() =>{
				DataGrid.ItemsSource = null;
				DataGrid.ItemsSource = _games;
			});
		}

		private System.Diagnostics.Process _snesProcess;
		private void OpenGame(){
			var processStartInfo = new System.Diagnostics.ProcessStartInfo(
				ProcessFile,
				string.Format("\"{0}\"", ((Models.Game)DataGrid.SelectedItem).FilePath) + " -fullscreen"
			);

			var process = System.Diagnostics.Process.Start(processStartInfo);
			if (process == null) return;
			_snesProcess = process;
			AppDomain.CurrentDomain.UnhandledException += OnException;
			process.EnableRaisingEvents = true;
			Hide();
			process.Exited += (o, args) =>{
				Dispatcher.Invoke(Show);
				AppDomain.CurrentDomain.UnhandledException -= OnException;
			};
		}

		private void OnException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs){
			_snesProcess.Kill();
		}

		~MainWindow(){
			App.GamesDbChanged -= AppOnGamesDbChanged;
		}
	}
}
