using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods;

namespace SnesBonus {
	public class Scraper {

		public static readonly Dictionary<string, string[]> CachedResults = new Dictionary<string, string[]>();

		private static DateTime _lastScrapeTime;

		static Scraper(){
			RomsFolder = Properties.SettingsHelper.RomsFolder;
			ImageFolder = Properties.SettingsHelper.ImageFolder;
		}

		private static readonly string RomsFolder;
		private static readonly string ImageFolder;

		private readonly List<Models.Game> _games = new List<Models.Game>();

		protected async void OnInitialized(EventArgs e) {
			foreach (var game in _games.ToList()) {
				var localFilePath = ImageFolder + game.ImagePath.CleanFileName();
				if (System.IO.File.Exists(localFilePath))
					game.ImagePath = localFilePath;
				else {
					game.ImagePath = await DownloadThumbnail(game.ImagePath);
				}
			}
		}

		public event Action ScrapeBegin;
		public event Action<Models.Game> ScrapeEnd;

		public async void ScrapeDir() {
			var gameNameRegex = new Regex("[a-z0-9-' \\.]+", RegexOptions.IgnoreCase);

			string[] files;
			if (System.IO.Directory.Exists(RomsFolder))
				files = System.IO.Directory.GetFiles(RomsFolder);
			else
				files = new string[0];

			var fileNames = files
				.Select(p => p.Split('\\').Last())
				.Select(p => p.Replace("." + p.Split('.').Last(), ""))
				.Select(p => gameNameRegex.Match(p).Value.Trim())
				.ToList();

			fileNames.RemoveRange(0, _games.Count);

			foreach (var gameName in fileNames) {
				if (ScrapeBegin != null) ScrapeBegin();
				var gather = await GetSearchResult(gameName);
				var detail = await GetGameDetails(gather.First());
				if (ScrapeEnd != null) ScrapeEnd(detail);
				_games.Add(detail);
				await Task.Delay(Properties.Settings.Default.ScraperTimeout);
			}
		}

		public async void ScrapeSingle(string file) {
			var gameNameRegex = new Regex("[a-z0-9-' \\.]+", RegexOptions.IgnoreCase);

			var funcs = new Func<string, string>[]{
				p => p.Split('\\').Last(),
				p => p.Replace("." + p.Split('.').Last(), ""),
				p => gameNameRegex.Match(p).Value.Trim()
			};

			var cleanName = funcs.Aggregate(file, (p, f) => f(p));

			file = gameNameRegex.Match(file).Value.Trim();

			var timeDifference = DateTime.Now - _lastScrapeTime;
			if (timeDifference.TotalMilliseconds > Properties.Settings.Default.ScraperTimeout)
				return;
				//await Task.Delay();

			_lastScrapeTime = DateTime.Now;


			if (ScrapeBegin != null) ScrapeBegin();
			var gather = await GetSearchResult(file);
			var detail = await GetGameDetails(gather.First());
			if (ScrapeEnd != null) ScrapeEnd(detail);
			_games.Add(detail);
			await Task.Delay(Properties.Settings.Default.ScraperTimeout);
		}

		public static async Task<string> DownloadThumbnail(string url) {
			var request = new RequestHelper.RequestHandler<System.IO.Stream>(url) {
				ResponseContentType = RequestHelper.HttpContentType.Binary
			};
			await request.SendAwait();
			var resp = request.Response();
			byte[] byteArr;

			using (var memStream = new System.IO.MemoryStream()) {
				resp.CopyTo(memStream);
				byteArr = memStream.ToArray();
			}

			System.IO.File.WriteAllBytes(ImageFolder + url.CleanFileName(), byteArr);

			return ImageFolder + url.CleanFileName();
		}

		public static async Task<IEnumerable<string>> GetSearchResult(string gameName){
			if (CachedResults.ContainsKey(gameName))
				return CachedResults[gameName];

			var request = new RequestHelper.RequestHandler<string>(string.Format(
				"http://www.gamefaqs.com/search/index.html?platform=63&game={0}&developer=&publisher=&res=1",
				gameName.Replace(" ", "%20")
			));
			await request.SendAwait();

			var doc = CsQuery.CQ.CreateDocument(request.Response());

			const string prefixUrl = "http://www.gamefaqs.com/";

			var result = doc["td.rtitle > a"].Select(p => prefixUrl + p.GetAttribute("href")).ToArray();
			CachedResults[gameName] = result;
			return result;
		}

		public static async Task<Models.Game> GetGameDetails(string url) {
			var request = new RequestHelper.RequestHandler<string>(url);

			await request.SendAwait();

			var doc = CsQuery.CQ.CreateDocument(request.Response());

			var newGame = new Models.Game();

			newGame.Title = doc[".page-title > a:nth-child(1)"].FirstElement().InnerText;
			newGame.Description = doc[".desc"].FirstElement().InnerText;
			newGame.ImagePath = doc["img.boxshot"].FirstElement().GetAttribute("src");
			newGame.MetacriticScore = doc[".mygames_stats_rate > a:nth-child(1)"].FirstElement().InnerText;
			newGame.Publisher = doc[".pod_gameinfo > div:nth-child(2) > ul:nth-child(1) > li:nth-child(3) > a:nth-child(1)"].FirstElement().InnerText;
			newGame.ReleaseDate = doc[".pod_gameinfo > div:nth-child(2) > ul:nth-child(1) > li:nth-child(4) > a:nth-child(2)"].FirstElement().InnerText;
			newGame.Location = url;

			return newGame;
		}

	}
}
