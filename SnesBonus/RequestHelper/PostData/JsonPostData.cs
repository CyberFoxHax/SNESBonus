using System.IO;
using System.Text;

namespace SnesBonus.RequestHelper.PostData{
	public class JsonPostData : IPostData{
		public JsonPostData(){}
		public JsonPostData(object serializeObject): this() {
			SerializeObject = serializeObject;
		}

		public object SerializeObject { get; set; }

		public Stream GetStream() {
			if (SerializeObject == null) return null;
			var stringData = CsQuery.Utility.JSON.ToJSON(SerializeObject);
			var buffer = Encoding.UTF8.GetBytes(stringData);

			var postData = new MemoryStream();
			postData.Write(buffer, 0, buffer.Length);

			return postData;
		}


		public string ContentType {
			get { return "application/json;charset=UTF-8"; }
		}
	}
}