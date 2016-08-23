using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SnesBonus.RequestHelper.PostData{
	public class UrlEncodedPostData : IPostData {
		public UrlEncodedPostData() : this(new Dictionary<string, string>()){}
		public UrlEncodedPostData(Dictionary<string, string> formParams) {
			Dict = formParams;
		}

		private static string UrlEncodeParameters(Dictionary<string, string> parameters) {
			return string.Join("&", parameters.Select(p => p.Key + "=" + p.Value));
		}

		public Dictionary<string, string> Dict { get; set; }

		public Stream GetStream() {
			if (Dict == null || !Dict.Any()) return null;
			var stringData = UrlEncodeParameters(Dict);
			var buffer = Encoding.UTF8.GetBytes(stringData);

			var postData = new MemoryStream();
			postData.Write(buffer, 0, buffer.Length);

			return postData;
		}

		public static implicit operator JsonPostData(UrlEncodedPostData p){
			return new JsonPostData(p.Dict);
		}


		public string ContentType {
			get { return "application/x-www-form-urlencoded;charset=UTF-8"; }
		}
	}
}