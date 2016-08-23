namespace SnesBonus.Properties {
	public static class SettingsHelper{

		private static readonly string Path = System.Environment.CurrentDirectory;

		private static string ParseOut(string inStr){
			return inStr.Replace("{this}", Path);
		}

		public static string GamesDb		{ get { return ParseOut(Settings.Default.GamesDb); } }
		public static string RomsFolder		{ get { return ParseOut(Settings.Default.RomsFolder); } }
		public static string ImageFolder	{ get { return ParseOut(Settings.Default.ImageFolder); } }
		public static string ExecutableFile { get { return ParseOut(Settings.Default.ExecutableFile); } }
	}
}
