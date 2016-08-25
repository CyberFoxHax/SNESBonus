using System.IO;

namespace SnesBonus {
	public partial class App{
		public App(){
			{
				var file = SnesBonus.Properties.SettingsHelper.GamesDb;
				if (File.Exists(file) == false)
					File.CreateText(file).Close();

				var watcher = new FileSystemWatcher {
					NotifyFilter = NotifyFilters.LastWrite,
					Path = Path.GetDirectoryName(file),
					Filter = Path.GetFileName(file)
				};
				watcher.Changed += GamesDbWatcher_OnChanged;
				watcher.EnableRaisingEvents = true;
			}
			{
				var dir = SnesBonus.Properties.SettingsHelper.RomsFolder;
				if (Directory.Exists(dir) == false)
					Directory.CreateDirectory(dir);

				var watcher = new FileSystemWatcher{
					Path = dir,
					NotifyFilter =
						NotifyFilters.LastWrite
					|	NotifyFilters.FileName
					|	NotifyFilters.DirectoryName,
					Filter = "*.*"
				};
				watcher.Changed += RomsDirWatcher_OnChanged;
				watcher.Created += RomsDirWatcher_OnChanged;
				watcher.Deleted += RomsDirWatcher_OnChanged;
				watcher.Renamed += RomsDirWatcher_OnChanged;
				watcher.EnableRaisingEvents = true;
			}

#if DEBUG
			SnesBonus.Properties.Settings.Default.Reset();
#endif
		}


		public static event System.Action RomsDirChanged;
		private static async void RomsDirWatcher_OnChanged(object sender, FileSystemEventArgs e) {
			if (RomsDirChanged == null) return;

			await System.Threading.Tasks.Task.Delay(100);
			lock (RomsDirChanged) {
				RomsDirChanged();
			}
		}

		public static event System.Action GamesDbChanged;
		private static async void GamesDbWatcher_OnChanged(object sender, FileSystemEventArgs fileSystemEventArgs){
			if (GamesDbChanged == null) return;

			await System.Threading.Tasks.Task.Delay(100);
			GamesDbChanged();
		}
	}
}
