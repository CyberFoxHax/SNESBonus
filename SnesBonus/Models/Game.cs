using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace SnesBonus.Models{
	public sealed class Game  {
		private static readonly string Path = System.Environment.CurrentDirectory;

		public string	Title			{ get; set; }
        public string	MetacriticScore	{ get; set; }
		public string	Publisher		{ get; set; }
		public string	ReleaseDate		{ get; set; }
		public string	ImagePath		{ get; set; }
		public string	Location		{ get; set; }
		public string	FilePath		{ get; set; }
		public string	Description		{ get; set; }

        [ScriptIgnore]
        public string FullFilePath {
            get { return DeserializePath(FilePath); }
            set { FilePath = SerializePath(value); }
        }

        [ScriptIgnore]
        public string FullImagePath {
            get { return ImagePath == null ? null : DeserializePath(ImagePath); }
            set { ImagePath = SerializePath(value); }
        }

        public Game CopyTo(Game p){
			p.Title				= Title				;
			p.MetacriticScore	= MetacriticScore	;
			p.Publisher			= Publisher			;
			p.ReleaseDate		= ReleaseDate		;
		//	p.ImagePath			= ImagePath			;
			p.Location			= Location			;
		//	p.FilePath			= FilePath			;
			p.Description		= Description		;
			return this;
		}


		private static readonly Regex NumberRegex = new Regex("[0-9]+", RegexOptions.IgnoreCase);
		public static string NumberToRomanNumerals(string input){
			var match = NumberRegex.Match(input).Value;
			var matchAsNumber = int.Parse(match);
			switch (matchAsNumber){
				case  1: return input.Replace(match, "I");
				case  2: return input.Replace(match, "II");
				case  3: return input.Replace(match, "III");
				case  4: return input.Replace(match, "IV");
				case  5: return input.Replace(match, "V");
				case  6: return input.Replace(match, "VI");
				case  7: return input.Replace(match, "VII");
				case  8: return input.Replace(match, "VIII");
				case  9: return input.Replace(match, "IX");
				case 10: return input.Replace(match, "X");
			} // FUCKIT XD
			return null;
		}

		internal bool ImageIsLocal {
			get { return ImagePath != null && ImagePath.StartsWith("http") == false; }
		}

		internal bool IsScraped {
			get {
				return new[]{
					Title,
					MetacriticScore,
					Publisher,
					ReleaseDate,
					ImagePath,
					Location,
					FilePath,
					Description
				}.Any(string.IsNullOrEmpty) == false;
			}
		}

		public string CleanGameName() {
			return CleanGameName(FilePath);
		}

		private static readonly Regex GameNameRegex = new Regex("[a-z0-9-' \\.]+", RegexOptions.IgnoreCase);
		public static string CleanGameName(string file) {
			file = file.Split('\\').Last();
			file = file.Replace("." + file.Split('.').Last(), "");
			file = GameNameRegex.Match(file).Value.Trim();
			return file;
		}

		public static List<Game> LoadGamesFromJson() {
			var path = SettingsHelper.GamesDb;
			var imageFolder = SettingsHelper.ImageFolder;

			List<Game> games = null;
			if (System.IO.File.Exists(path))
				games = CsQuery.Utility.JSON.ParseJSON<List<Game>>(System.IO.File.ReadAllText(path));

			if (games == null)
				return null;

			foreach (var game in games) {
                game.FilePath = SerializePath(game.FilePath);
                if(game.ImagePath != null)
                    game.ImagePath = SerializePath(game.ImagePath);
                var localFilePath = imageFolder + game.ImagePath.CleanFileName();
				if (System.IO.File.Exists(localFilePath))
					game.ImagePath = localFilePath;
			}

			return games;
		}

	    public static void SaveGamesToJson(List<Game> games) {
            System.IO.File.WriteAllText(SettingsHelper.GamesDb, Lib.JsonHelper.FormatJson(CsQuery.Utility.JSON.ToJSON(games)));
        }

        private static string DeserializePath(string inStr) {
            return inStr.Replace("{this}", Path);
        }

        private static string SerializePath(string inStr) {
            return inStr.Replace(Path, "{this}");
        }
    }
}