using System;
using System.Collections.Generic;
using System.Linq;

namespace SnesBonus {
	internal static class Utils {

		public static string CleanFileName(this string fileName) {
			return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
		}

		public static int LevenshteinDistance(string s, string t) {
			var length = s.Length;
			var num2 = t.Length;
			var numArray = new int[length + 1, num2 + 1];

			if (length == 0) return num2;
			if (num2 == 0) return length;
			var num3 = 0;
			while (num3 <= length)
				numArray[num3, 0] = num3++;
			var num4 = 0;
			while (num4 <= num2)
				numArray[0, num4] = num4++;
			for (var i = 1; i <= length; i++)
				for (var j = 1; j <= num2; j++) {
					var num7 = t[j - 1] == s[i - 1] ? 0 : 1;
					numArray[i, j] = Math.Min(Math.Min(numArray[i - 1, j] + 1, numArray[i, j - 1] + 1), numArray[i - 1, j - 1] + num7);
				}
			return numArray[length, num2];
		}

		public static List<Models.Game> CheckRomsDirForNew(ref List<Models.Game> games){
			var romsFolder = Properties.SettingsHelper.RomsFolder;

			string[] files;
			if (System.IO.Directory.Exists(romsFolder))
				files = System.IO.Directory.GetFiles(romsFolder);
			else
				return null;

			Func<string, Models.Game> convert = p =>
				new Models.Game {
					FilePath = p,
					Title = Models.Game.CleanGameName(p)
				};

			List<Models.Game> newGames;
			if (games != null) {
				var existingNames = games.Select(p => p.FilePath.ToLower()).ToArray();
				newGames = (
					from file in files
					where existingNames.Contains(file.ToLower()) == false
					select convert(file)
				).ToList();
				games.AddRange(newGames);
			}
			else {
				newGames = files.Select(convert).ToList();
				games = newGames;
			}

			return newGames;
		}
	}
}
