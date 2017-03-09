namespace SnesBonus {
	public static class SettingsHelper{
		private static readonly string Path = System.Environment.CurrentDirectory;
		private static readonly string JsonPath = Path + "\\Settings.json";

		private class SettingsModel{
			public string GamesDb { get; set; }
			public string RomsFolder { get; set; }
			public string ImageFolder { get; set; }
			public string ExecutableFile { get; set; }
			public int ScraperTimeout { get; set; }
			public bool? AutoScrape { get; set; }
			public System.DateTime BanTimestamp { get; set; }
		}

		public static void Save() {
			var saveObj = new SettingsModel{
				GamesDb = _gamesDb,
				RomsFolder = _romsFolder,
				ImageFolder = _imageFolder,
				ExecutableFile = _executableFile,
				ScraperTimeout = _scraperTimeout,
				AutoScrape = _autoScrape,
				BanTimestamp = _banTimestamp
			};

			System.IO.File.WriteAllText(JsonPath, Lib.JsonHelper.FormatJson(CsQuery.Utility.JSON.ToJSON(saveObj)));
		}

		public static void Load(){
			SettingsModel res;
			if (System.IO.File.Exists(JsonPath))
				res = CsQuery.Utility.JSON.ParseJSON<SettingsModel>(System.IO.File.ReadAllText(JsonPath));
			else{
				Reset();
				return;
			}

			_gamesDb		= res.GamesDb;
			_romsFolder		= res.RomsFolder;
			_imageFolder	= res.ImageFolder;
			_executableFile = res.ExecutableFile;
			_scraperTimeout = res.ScraperTimeout;
			_autoScrape		= res.AutoScrape;
			_banTimestamp   = res.BanTimestamp;
		}

		public static void Reset() {
			System.IO.File.Delete(JsonPath);
			_gamesDb		= @"{this}\GamesDB.json";
			_romsFolder		= @"{this}\Roms\";
			_imageFolder	= @"{this}\Thumbnails\";
			_executableFile = @"{this}\snes9x.exe";
			_scraperTimeout = 50000;
			_autoScrape		= true;
			_banTimestamp   = default(System.DateTime);
		}

        private static string ParseOut(string inStr) {
            return inStr.Replace("{this}", Path);
        }

        private static string ParseIn(string inStr) {
            return inStr.Replace(Path, "{this}");
        }

        private static string _gamesDb;
		private static string _romsFolder;
		private static string _imageFolder;
		private static string _executableFile;
		private static int _scraperTimeout;
		private static bool? _autoScrape;
		private static System.DateTime _banTimestamp;

		public static string GamesDb
		{
			get { return ParseOut(_gamesDb); }
			set { _gamesDb = ParseIn(value); }
		}

		public static string RomsFolder
		{
			get { return ParseOut(_romsFolder); }
			set { _romsFolder = ParseIn(value); }
		}

		public static string ImageFolder
		{
			get { return ParseOut(_imageFolder); }
			set { _imageFolder = ParseIn(value); }
		}

		public static string ExecutableFile
		{
			get { return ParseOut(_executableFile); }
			set { _executableFile = ParseIn(value); }
		}

		public static int ScraperTimeout
		{
			get { return _scraperTimeout; }
			set { _scraperTimeout = value; }
		}

		public static bool? AutoScrape
		{
			get { return _autoScrape; }
			set { _autoScrape = value; }
		}

		public static System.DateTime BanTimestamp
		{
			get { return _banTimestamp; }
			set { _banTimestamp = value; }
		}
	}
}
