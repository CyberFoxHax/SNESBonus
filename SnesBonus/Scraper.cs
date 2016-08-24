using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods;

namespace SnesBonus {
	public class Scraper {

		private static DateTime _lastScrapeTime;
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
					game.ImagePath = await DownloadThumbnail(game.ImagePath);
			}
		}

		private async void Loop(){
			if (DateTime.Now < _lastScrapeTime.AddMilliseconds(ScrapeTimeout)){
				var timeToNext = ScrapeTimeout - (DateTime.Now - _lastScrapeTime).TotalMilliseconds;
				await Task.Delay((int) timeToNext);
			}

			while (_waitingGames.Any()){
				var game = _waitingGames.First();

				if (ScrapeBegin != null)
					ScrapeBegin();

				var gather = await GetSearchResult(game.CleanGameName());
				var dists = gather.Select(p => new {
					Value = p,
					Dist = Utils.LevenshteinDistance(game.Title, p.Text)
				}).ToArray();
				var best = dists.Min(p => p.Dist);
				var target = dists.First(p => p.Dist == best);

				var result = await GetGameDetails(target.Value.Href);

				if (System.IO.File.Exists(ImageFolder + result.ImagePath.CleanFileName()) == false && string.IsNullOrEmpty(result.ImagePath) == false)
					result.ImagePath = await DownloadThumbnail(result.ImagePath);

				result.FilePath = game.FilePath;

				if (ScrapeEnd != null)
					ScrapeEnd(result.CopyTo(game));

				_waitingGames.Remove(game);

				_lastScrapeTime = DateTime.Now;
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

		private static async Task<string> DownloadThumbnail(string url) {
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
			public string Text { get; set; }
		}

		private static async Task<IEnumerable<SearchResult>> GetSearchResult(string gameName){
			if (CachedSearchResults.ContainsKey(gameName))
				return CachedSearchResults[gameName];

			var request = new RequestHelper.RequestHandler<string>(string.Format(
				"http://www.gamefaqs.com/search/index.html?platform=63&game={0}&developer=&publisher=&res=1",
				gameName.Replace(" ", "%20")
			));
			await request.SendAwait();

			var doc = CsQuery.CQ.CreateDocument(request.Response());

			const string prefixUrl = "http://www.gamefaqs.com/";

			var res = doc["td.rtitle > a"].Select(p => new SearchResult{
				Href = prefixUrl + p.GetAttribute("href"),
				Text = p.InnerText
			}).ToArray();

			CachedSearchResults[gameName] = res;
			return res;
		}

		private static async Task<Models.Game> GetGameDetails(string url) {
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

			newGame.ReleaseDate = newGame.ReleaseDate.Replace("&#187;", "");

			return newGame;
		}

	}
}
