using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SnesBonus.Models{
	public sealed class Game  {
		public string	Title			{ get; set; }
		public string	MetacriticScore	{ get; set; }
		public string	Publisher		{ get; set; }
		public string	ReleaseDate		{ get; set; }
		public string	ImagePath		{ get; set; }
		public string	Location		{ get; set; }
		public string	FilePath		{ get; set; }
		public string	Description		{ get; set; }

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
			get { return ImagePath.StartsWith("http") == false; }
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
			var path = Properties.SettingsHelper.GamesDb;
			var imageFolder = Properties.SettingsHelper.ImageFolder;

			List<Game> games = null;
			if (System.IO.File.Exists(path))
				games = CsQuery.Utility.JSON.ParseJSON<List<Game>>(System.IO.File.ReadAllText(path));

			if (games == null)
				return null;

			foreach (var game in games) {
				var localFilePath = imageFolder + game.ImagePath.CleanFileName();
				if (System.IO.File.Exists(localFilePath))
					game.ImagePath = localFilePath;
			}

			return games;
		}
	}
}