using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods.Internal;

namespace SnesBonus {
	public class Scraper {

		public static DateTime LastScrapeTime { get; private set; }
		private static readonly string ImageFolder = Properties.SettingsHelper.ImageFolder;
		private static readonly int ScrapeTimeout = Properties.Settings.Default.ScraperTimeout;
		public static readonly Dictionary<string, SearchResult[]> CachedSearchResults = new Dictionary<string, SearchResult[]>();

		public event Action ScrapeBegin;
		public event Action<Models.Game> ScrapeEnd;
		private readonly List<Models.Game> _waitingGames = new List<Models.Game>();
		private System.Threading.Thread _scraperThread;

		public void StartScraping(){
			if (_scraperThread != null) return;
			_scraperThread = new System.Threading.Thread(Loop);
			_scraperThread.Start();
		}

		protected async void DownloadImages(IEnumerable<Models.Game> games) {
			foreach (var game in games.ToList()) {
				var localFilePath = ImageFolder + game.ImagePath.CleanFileName();
				if (System.IO.File.Exists(localFilePath))
					game.ImagePath = localFilePath;
				else
					game.ImagePath = await DownloadImage(game.ImagePath);
			}
		}

		private async void Loop(){
			if (DateTime.Now < LastScrapeTime.AddMilliseconds(ScrapeTimeout)){
				var timeToNext = ScrapeTimeout - (DateTime.Now - LastScrapeTime).TotalMilliseconds;
				await Task.Delay((int) timeToNext);
			}

			while (_waitingGames.Any()){
				var game = _waitingGames.First();

				if (ScrapeBegin != null)
					ScrapeBegin();

				var gather = await GetSearchResult(game.CleanGameName());
				if (gather == null){
					await Task.Delay(ScrapeTimeout);
					continue;
				}

				string href;
				if (false){
					var searchResults = gather as SearchResult[] ?? gather.ToArray();
					var dists = searchResults.Select(p => new{
						Value = p,
						Dist = Utils.LevenshteinDistance(game.Title, p.Title)
					}).Concat(searchResults.Select(p => new{
						Value = p,
						Dist = Utils.LevenshteinDistance(Models.Game.NumberToRomanNumerals(game.Title), p.Title)
					})).ToArray();

					var best = dists.Min(p => p.Dist);
					var target = dists.First(p => p.Dist == best);
					href = target.Value.Href;
				}
				else
					href = gather.First().Href;

				var result = await GetGameDetails(href);
				if (result == null) {
					await Task.Delay(ScrapeTimeout);
					continue;
				}

				var imagePath = Properties.SettingsHelper.ImageFolder + result.ImagePath.CleanFileName();
				if (System.IO.File.Exists(imagePath))
					game.ImagePath = imagePath;
				else if (string.IsNullOrEmpty(result.ImagePath) == false)
					game.ImagePath = await DownloadImage(result.ImagePath);

				result.FilePath = game.FilePath;

				if (ScrapeEnd != null)
					ScrapeEnd(result.CopyTo(game));

				_waitingGames.Remove(game);

				await Task.Delay(ScrapeTimeout);
			}
			_scraperThread = null;
		}

		public void QueueGame(Models.Game game) {
			_waitingGames.Add(game);
			StartScraping();
		}

		public void QueueGames(IEnumerable<Models.Game> games) {
			_waitingGames.AddRange(games);
			StartScraping();
		}

		public static async Task<string> DownloadImage(string url) {
			var request = new RequestHelper.RequestHandler<System.IO.Stream>(url.Replace("_thumb", "_front")) {
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


		public class SearchResult{
			public string Href { get; set; }
			public string Title { get; set; }
		}

		public static bool IsBlocked(CsQuery.CQ doc){
			return doc[".page-title"].FirstElement().InnerText == "Blocked IP Address";
		}

		public static async Task<IEnumerable<SearchResult>> GetSearchResult(string gameName, bool cache=true){
			if (cache && CachedSearchResults.ContainsKey(gameName))
				return CachedSearchResults[gameName];

			var request = new RequestHelper.RequestHandler<string>(string.Format(
				"http://www.gamefaqs.com/search/index.html?platform=63&game={0}&developer=&publisher=&res=1",
				gameName.Replace(" ", "%20")
			));
			await request.SendAwait();
			LastScrapeTime = DateTime.Now;

			var doc = CsQuery.CQ.CreateDocument(request.Response());

			var blocked = IsBlocked(doc);
			if (blocked){
				if (Properties.Settings.Default.BanTimestamp == default(DateTime)){
					Properties.Settings.Default.BanTimestamp = DateTime.Now;
					Properties.Settings.Default.Save();
				}
				return null;
			}

			const string prefixUrl = "http://www.gamefaqs.com/";

			var res = doc["td.rtitle > a"].Select(p => new SearchResult{
				Href = prefixUrl + p.GetAttribute("href"),
				Title = p.InnerText
			}).ToArray();

			CachedSearchResults[gameName] = res;
			return res;
		}

		public static async Task<Models.Game> GetGameDetails(string url) {
			var request = new RequestHelper.RequestHandler<string>(url);
			LastScrapeTime = DateTime.Now;

			await request.SendAwait();

			var doc = CsQuery.CQ.CreateDocument(request.Response());

			var blocked = IsBlocked(doc);
			if (blocked) {
				if (Properties.Settings.Default.BanTimestamp == default(DateTime)){
					Properties.Settings.Default.BanTimestamp = DateTime.Now;
					Properties.Settings.Default.Save();
				}
				return null;
			}

			var newGame = new Models.Game();

			newGame.Title = doc[".page-title > a:nth-child(1)"].FirstElement().InnerText;
			newGame.Description = doc[".desc"].FirstElement().InnerText;
			newGame.ImagePath = doc["img.boxshot"].FirstElement().GetAttribute("src");
			newGame.MetacriticScore = doc[".mygames_stats_rate > a:nth-child(1)"].FirstElement().InnerText;
			newGame.Publisher = doc[".pod_gameinfo > div:nth-child(2) > ul:nth-child(1) > li:nth-child(3) > a:nth-child(1)"].FirstElement().InnerText;
			newGame.ReleaseDate = doc[".pod_gameinfo > div:nth-child(2) > ul:nth-child(1) > li:nth-child(4) > a:nth-child(2)"].FirstElement().InnerText;
			newGame.Location = url;

			newGame.ReleaseDate = newGame.ReleaseDate.Replace("&#187;", "");

			return newGame;
		}

	}
}
