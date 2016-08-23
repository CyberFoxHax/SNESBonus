using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SnesBonus {
	internal static class Utils {
		public static string CleanFileName(this string fileName) {
			return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
		}

		public static List<Models.Game> LoadGamesFromJson(){
			var path = Properties.SettingsHelper.GamesDb;
			var imageFolder = Properties.SettingsHelper.ImageFolder;

			List<Models.Game> games = null;
			if (System.IO.File.Exists(path))
				games = CsQuery.Utility.JSON.ParseJSON<List<Models.Game>>(System.IO.File.ReadAllText(path));

			if (games == null)
				return null;

			foreach (var game in games){
				var localFilePath = imageFolder + game.ImagePath.CleanFileName();
				if (System.IO.File.Exists(localFilePath))
					game.ImagePath = localFilePath;
			}

			return games;
		}
	}
}
